using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using System.IO.Pipes;

namespace LunarLander.audio; 

public static class AudioBuffer {
    private const int bufferSizeSeconds = 2;
    private static readonly byte[] buffer = new byte[bufferSizeSeconds * 48000 * 2];
    private const int samplesPerRead = 3600;
    private static readonly double[] window = new double[samplesPerRead];
    private static int writeHead = 0;
    private static int readHead = 0;

    private static readonly StreamWriter writer = new StreamWriter("/home/joe/audio_out", false);
    
    private static bool writeLock = false;
    
    static AudioBuffer() {
        // create a trapezoidal window for the audio buffer
        // this is to avoid clicks when the buffer wraps around
        int rampLength = 0;
        for (int i = 0; i < rampLength; i++) {
            window[i] = (double)i / rampLength;
            window[window.Length - i - 1] = (double)i / rampLength;
        }
        for(int i = rampLength; i < window.Length - rampLength; i++) {
            window[i] = 1;
        }
        Thread t = new Thread(run);
        t.Start();
        t.IsBackground = true;
    }

    public static void write(IEnumerable<byte> buffer) {
        writeLock = true;
        foreach (byte t in buffer) {
            AudioBuffer.buffer[writeHead++] = t;
            if(writeHead == AudioBuffer.buffer.Length) {
                writeHead = 0;
            }
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

    public static void run() {
        Thread.Sleep(100); // wait for audio buffer to populate somewhat
        var sw = Stopwatch.StartNew();
        sw.Start();
        while (true) {
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
            // apply window
            for (int i = 0; i < window.Length; i++) {
                int sample = (data[i * 2] << 8) | data[i * 2 + 1];
                sample -= 0x7FFF; // convert to signed
                sample *= (int)(window[i] * 0x7FFF);
                sample += 0x7FFF; // convert back to unsigned
                data[i * 2] = (byte)(sample >> 8);
                data[i * 2 + 1] = (byte)(sample & 0xFF);
            }
            // write to file
            writer.BaseStream.Write(data, 0, data.Length);

            // play audio
            SoundEffect effect = new (data, 48000, AudioChannels.Mono);
            effect.Play();
            // wait for audio to finish
            while (sw.ElapsedTicks < durationTicks - 1000000) {
                // spin
            }
            sw.Restart();
        }
    }
}