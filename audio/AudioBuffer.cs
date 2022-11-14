using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    
    private static readonly BiQuadFilter pass1;
    private static readonly BiQuadFilter pass2;
    private static readonly BiQuadFilter pass3;
    
    private static bool writeLock;

    public static bool running { get; private set; } = false;
    
    static AudioBuffer() {

        pass1 = BiQuadFilter.HighPassFilter(48000, 90, 1);
        pass2 = BiQuadFilter.HighPassFilter(48000, 440, 1);
        pass3 = BiQuadFilter.LowPassFilter(48000, 14000, 1);
        
        Thread t = new Thread(run);
        t.Start();
        t.IsBackground = true;
    }

    public static void write(IReadOnlyList<byte> buffer) {
        writeLock = true;
        for (int i = 0; i < buffer.Count; i++) {
            AudioBuffer.buffer[writeHead++] = buffer[i];
            if(writeHead > AudioBuffer.buffer.Length - 1) writeHead = 0;
        }
        writeLock = false;
    }

    public static void write(short sample) {
        writeLock = true;
        buffer[writeHead++] = (byte) (sample & 0xFF);
        buffer[writeHead++] = (byte) ((sample >> 8) & 0xFF);
        if(writeHead > buffer.Length - 1) writeHead = 0;
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
            if (!LunarLander.running) {
                running = false;
                return;
            }
            const long durationTicks = (long)(1000000000 * (samplesPerRead / 48000.0));
            if (writeLock) {
                Thread.Sleep(1);
            }
            // wait for enough data to be available
            while (bytesAvailable() < samplesPerRead * 2) {
                Thread.Sleep(1);
            }
            // read 4800 bytes
            byte[] data = new byte[samplesPerRead * 2];
            for (int i = 0; i < samplesPerRead * 2; i++) {
                data[i] = buffer[readHead++];
                if (readHead == buffer.Length) {
                    readHead = 0;
                }
            }
            
            // convert bytes to shorts
            short[] shorts = new short[samplesPerRead];
            for (int i = 0; i < samplesPerRead; i++) {
                shorts[i] = (short)((data[i * 2 + 1] << 8) | data[i * 2]);
            }
            
            // convert shorts to floats and apply filters
            float[] floats = new float[samplesPerRead];
            for (int i = 0; i < samplesPerRead; i++) {
                float sample = shorts[i] / (float)0x7FFF;
                floats[i] = sample;
            }

            // Complex[] complex = new Complex[floats.Length];
            // for (int i = 0; i < floats.Length; i++) {
            //     complex[i] = new Complex();
            //     complex[i].X = floats[i];
            // }
            for(int i = 0; i < floats.Length; i++) {
                floats[i] = pass1.Transform(floats[i]);
                floats[i] = pass2.Transform(floats[i]);
                floats[i] = pass3.Transform(floats[i]);
            }
            

            // clamp floats to [-1, 1]
            for (int i = 0; i < samplesPerRead; i++) {
                floats[i] = Math.Min(Math.Max(floats[i], -1), 1);
            }
            
            // convert floats back to shorts
            for (int i = 0; i < samplesPerRead; i++) {
                shorts[i] = (short)(floats[i] * 0x7FFF);
            }
            
            // convert shorts back to bytes
            for (int i = 0; i < samplesPerRead; i++) {
                data[i * 2 + 1] = (byte)(shorts[i] >> 8);
                data[i * 2] = (byte)(shorts[i] & 0xFF);
            }

            // write to file
            // writer.BaseStream.Write(data, 0, data.Length);
            
            // copy to previous buffer for external use
            data.CopyTo(prevBuffer, 0);

            // play audio
            SoundEffect effect = new (data, 48000, AudioChannels.Mono);
            effect.Play();
            if (durationTicks - sw.ElapsedTicks > 1000000) {
                Thread.Sleep((int)Math.Floor((double)((int)durationTicks - (int)sw.ElapsedTicks - 500000) / 1000000)); // spin for at least 500us
            }
            // wait for audio to finish
            while (sw.ElapsedTicks < durationTicks - 1000000) {
                // spin
            }
            sw.Restart();
        }
    }
}