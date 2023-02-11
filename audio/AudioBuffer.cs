using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using NAudio.Dsp;

namespace LunarLander.audio; 

public static class AudioBuffer {
    private const int bufferSizeSeconds = 2;
    private static readonly byte[] buffer = new byte[bufferSizeSeconds * 48000 * 2];
    private const int samplesPerRead = 2048;
    private static readonly byte[] prevBuffer = new byte[samplesPerRead * 2];
    private static int writeHead;
    private static int readHead;
    
    private static BiQuadFilter pass1;
    private static BiQuadFilter pass2;
    private static BiQuadFilter pass3;

    public static double gain { get; set; } = 1;

    private static bool writeLock;

    public static bool running { get; private set; }

    private static int _ = InitAudioBuffer();
    
    private static int InitAudioBuffer() {

        pass1 = BiQuadFilter.HighPassFilter(48000, 90, 1);
        pass2 = BiQuadFilter.HighPassFilter(48000, 440, 1);
        pass3 = BiQuadFilter.LowPassFilter(48000, 14000, 1);
        
        Thread t = new Thread(run);
        t.Start();
        t.IsBackground = true;

        return 0;
    }

    public static void write(IEnumerable<byte> buffer) {
        writeLock = true;
        foreach (byte t in buffer) {
            AudioBuffer.buffer[writeHead++] = t;
            if (writeHead > AudioBuffer.buffer.Length - 1) {
                writeHead = 0;
            }
        }
        writeLock = false;
    }

    public static void write(short sample) {
        writeLock = true;
        buffer[writeHead++] = (byte) (sample & 0xFF);
        buffer[writeHead++] = (byte) ((sample >> 8) & 0xFF);
        if (writeHead > buffer.Length - 1) {
            writeHead = 0;
        }
        writeLock = false;
    }
    
    private static int bytesAvailable() {
        int avail = writeHead - readHead;
        if(avail < 0) {
            avail += buffer.Length;
        }
        return avail;
    }
    
    public static byte[] getPrevBuffer() {
        return prevBuffer;
    }

    public static void run() {
        running = true;
        var sw = Stopwatch.StartNew();
        sw.Start();
        while (true) {
            const long durationTicks = (long)(1_000_000_000 * (samplesPerRead / 48_000.0));
            if (writeLock) {
                Thread.Sleep(1);
            }
            // wait for enough data to be available
            while (bytesAvailable() < samplesPerRead * 2) {
                if (!RP2A03.running) { // if the emulator is not running, exit
                    running = false;
                    return;
                }
                Thread.Sleep(1);
            }

            byte[] data = new byte[samplesPerRead * 2];
            short[] shorts = new short[samplesPerRead];
            float[] floats = new float[samplesPerRead];
            
            for (int i = 0; i < samplesPerRead * 2; i++) {
                data[i] = buffer[readHead++];
                if (readHead == buffer.Length) {
                    readHead = 0;
                }
            }
            
            for (int i = 0; i < samplesPerRead; i++) {
                shorts[i] = (short)((data[i * 2 + 1] << 8) | data[i * 2]);
                float sample = (float)gain * (shorts[i] / (float)0x7FFF);
                floats[i] = Math.Min(Math.Max(
                    pass3.Transform(
                    pass2.Transform(
                    pass1.Transform(sample))), -1), 1);
                shorts[i] = (short)(floats[i] * 0x7FFF);
                data[i * 2 + 1] = (byte)(shorts[i] >> 8);
                data[i * 2] = (byte)(shorts[i] & 0xFF);
            }
            
            // copy to previous buffer for external use
            data.CopyTo(prevBuffer, 0);

            // play audio
            SoundEffect effect = new (data, 48_000, AudioChannels.Mono);
            effect.Play();
            if (durationTicks - sw.ElapsedTicks > 1_000_000) {
                Thread.Sleep((int)Math.Floor((double)((int)durationTicks - (int)sw.ElapsedTicks - 500_000) / 1_000_000)); // spin for at least 500us
            }
            // wait for audio to finish
            while (sw.ElapsedTicks < durationTicks - 1_000_000) {
                // spin
            }
            sw.Restart();
        }
    }
}
