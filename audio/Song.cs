using System;
using System.Data.Common;
using System.Diagnostics;
using LunarLander.audio;

public class Song {
    struct Pulse {
        public int channel;
        public int note;
        public int octave;
        public int envelope;
        public bool cv;
        
        public Pulse(int channel, int note, int octave, int envelope = 7, bool cv = true) {
            this.channel = channel;
            this.note = note;
            this.octave = octave;
            this.envelope = envelope;
            this.cv = cv;
        }
    }

    struct Triangle {
        public int note;
        public int octave;

        public Triangle(int note, int octave) {
            this.note = note;
            this.octave = octave;
        }
    }

    struct Noise {
        public int period;
        public int envelope;
        public bool cv;
        
        public Noise(int period, int envelope = 7, bool cv = true) {
            this.period = period;
            this.envelope = envelope;
            this.cv = cv;
        }
    }

    private class Optional<T> {
        private readonly T value;
        private readonly bool hasValue;

        public Optional(T value) {
            this.value = value;
            this.hasValue = true;
        }

        public Optional() {
            this.hasValue = false;
        }

        public T Value {
            get {
                if (!hasValue) {
                    throw new InvalidOperationException("Optional value is not set");
                }
                return value;
            }
        }

        public bool HasValue => hasValue;
    }
    
    private Optional<Pulse>[] pulse1;
    private Optional<Pulse>[] pulse2;
    private Optional<Triangle>[] triangle;
    private Optional<Noise>[] noise;

    private bool loop;
    private bool isPlaying;

    private double dt;
    private int songIndex;
    
    public Song(double length, double dt, bool loop) {
        this.dt = dt;
        this.loop = loop;
        int n = (int) (length / dt);
        pulse1 = new Optional<Pulse>[n];
        pulse2 = new Optional<Pulse>[n];
        triangle = new Optional<Triangle>[n];
        noise = new Optional<Noise>[n];
        for (int i = 0; i < n; i++) {
            pulse1[i] = new Optional<Pulse>();
            pulse2[i] = new Optional<Pulse>();
            triangle[i] = new Optional<Triangle>();
            noise[i] = new Optional<Noise>();
        }
    }
    
    public Song addPulse(int channel, int note, int octave, int envelope, bool cv, double t) {
        switch (channel) {
            case 0:
                pulse1[(int) (t / dt)] = new Optional<Pulse>(new Pulse(channel, note, octave, envelope, cv));
                break;
            case 1:
                pulse2[(int) (t / dt)] = new Optional<Pulse>(new Pulse(channel, note, octave, envelope, cv));
                break;
            default:
                throw new ArgumentException("Invalid channel");
        }

        return this;
    }

    public void update() {
        if (!isPlaying) {
            return;
        }
        if (songIndex >= pulse1.Length) {
            if (loop) songIndex = 0;
            else return;
        }
        
        if (pulse1[songIndex].HasValue) {
            Pulse pulse = pulse1[songIndex].Value;
            RP2A03_API.pulsePlayNote(0, pulse.note, pulse.octave, 1, (byte)pulse.envelope, pulse.cv);
        }
        
        if (pulse2[songIndex].HasValue) {
            Pulse pulse = pulse2[songIndex].Value;
            RP2A03_API.pulsePlayNote(1, pulse.note, pulse.octave, 1, (byte)pulse.envelope, pulse.cv);

        }
        
        if (triangle[songIndex].HasValue) {
            Triangle tri = triangle[songIndex].Value;
            RP2A03_API.trianglePlayNote(tri.note, tri.octave);
        }
        
        if (noise[songIndex].HasValue) {
            Noise n = noise[songIndex].Value;
            RP2A03_API.noisePlayNote(n.period, 1, (byte)n.envelope, n.cv);
        }
        
        songIndex++;
    }
    
    public void play() {
        isPlaying = true;
    }

    public void pause() {
        isPlaying = false;
    }
    
    public void stop() {
        isPlaying = false;
        reset();
    }
    public void reset() {
        songIndex = 0;
    }
 
    public static readonly Song song1 = new Song(5, (1.0 / 60), false)
        .addPulse(0, 0, 4, 15, false, 0.00)
        .addPulse(0, 4, 4, 15, false, 0.20)
        .addPulse(0, 7, 4, 15, false, 0.40);
}