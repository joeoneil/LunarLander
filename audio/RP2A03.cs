using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace LunarLander.audio; 

public static class RP2A03 {
    
    public static readonly byte[] lengthTable = { 
    //   0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F    
        0x0A, 0xFE, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06, 0xA0, 0x08, 0x3C, 0x0A, 0x0E, 0x0C, 0x1A, 0x0E, // 00-0F
        0x0C, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16, 0xC0, 0x18, 0x48, 0x1A, 0x10, 0x1C, 0x20, 0x1E  // 10-1F
    };

    public static readonly short[] timerPeriodTable = {
        4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068,
    };

    public static readonly bool[][] dutyTable = {
        new[] {
            false, false, false, false, false, false, false, true,
        },
        new[] {
            false, false, false, false, false, false, true, true,
        },
        new[] {
            false, false, false, false, true, true, true, true,
        },
        new[] {
            true, true, true, true, true, true, false, false,
        },
    };

    private static readonly byte[] registers = new byte[0x20]; // round to 32 bytes even though not all of them are used
    
    public const double clockRate = 1789773.0; // 1.789773 MHz

    private class Divider {
        public byte count { get; private set; }
        private byte period { get; set; }
        private bool loop { get; set; }

        public Divider(byte period, bool loop) {
            this.period = period;
            this.loop = loop;
            this.count = period;
        }

        public void load(byte period) {
            this.period = period;
            this.count = period;
        }
        
        public bool clock() {
            if (count == 0) {
                if (loop) count = period;
                return true;
            }
            count--;
            return false;
        }

        public void setLoop(bool loop) {
            this.loop = loop;
        }
    }
    private class LengthCounter {

        private Divider divider { get; set; }
        private bool halt { get; set; }

        public LengthCounter(byte period, bool halt) {
            this.divider = new Divider(period, false);
            this.halt = halt;
        }
        
        public void load(int index) {
            divider.load(lengthTable[index]);
        }
        
        public void load(byte period) {
            divider.load(period);
        }
        
        public void setHalt(bool halt) {
            this.halt = halt;
        }
        
        public void setLoop(bool loop) {
            divider.setLoop(loop);
        }
        
        public bool clock() {
            return !halt && divider.clock();
        }

        public byte get() {
            return divider.count;
        }
    }
    private class Envelope {
        private Divider divider;
        private byte envelope { get; set; }
        private byte decay { get; set; }
        private bool loop { get; set; }
        private bool constant { get; set; }
        private bool start { get; set; }
        
        public Envelope(byte envelope, bool loop, bool constant) {
            this.divider = new Divider(envelope, true);
            this.envelope = envelope;
            this.decay = 0;
            this.loop = loop;
            this.constant = constant;
            this.start = true;
        }
        
        public void load(byte envelope) {
            this.envelope = envelope;
            this.start = true;
        }
        
        public void setLoop(bool loop) {
            this.loop = loop;
        }
        
        public void setConstant(bool constant) {
            this.constant = constant;
        }
        
        public void clock() {
            // Console.WriteLine("Decay = " + decay + ", div counter = " + divider.count);
            if (start) {
                decay = 0xF;
                divider.load(envelope);
                start = false;
            } else if (divider.clock()) {
                if (decay > 0) decay--;
                else if (loop) decay = 0xF;
            }
        }
        
        public byte get() {
            return constant ? envelope : decay;
            // return 15;
        }
    }
    private class LFSR {
        bool[] bits = new bool[15];
        bool mode { get; set; }

        public LFSR(bool mode) {
            this.mode = mode;
            for (int i = 0; i < bits.Length; i++) bits[i] = false;
            bits[14] = true;
        }

        public void clock() {
            bool feedback = bits[0] ^ (mode ? bits[6] : bits[1]);
            // shift bits 1 to the right
            for (int i = 0; i < bits.Length - 1; i++) {
                bits[i] = bits[i + 1];
            }
            bits[14] = feedback;
        }
        
        public void setMode(bool mode) {
            this.mode = mode;
        }

        public bool get() {
            return bits[0];
        }

        public short getAll() {
            short acc = 0;
            for(int i = 0; i < bits.Length; i++) {
                if (bits[i]) acc |= (short)(1 << i);
            }
            return acc;
        }
    }
    private class Sweep {
        private bool channel { get; set; } // false: pulse 1, true: pulse 2
        private bool enabled { get; set; }
        private Divider divider { get; set; }
        private bool negate { get; set; }
        private byte shift { get; set; }
        
        public Sweep(bool enabled, byte period, bool negate, byte shift, bool channel) {
            this.channel = channel;
            this.enabled = enabled;
            this.divider = new Divider(period, true);
            this.negate = negate;
            this.shift = shift;
        }
        
        public void load(byte period) {
            divider.load(period);
        }
        
        public void setEnabled(bool enabled) {
            this.enabled = enabled;
        }
        
        public void setNegate(bool negate) {
            this.negate = negate;
        }
        
        public void setShift(byte shift) {
            this.shift = shift;
        }

        public short clock(short raw_period) {
            if (!enabled || !divider.clock()) return raw_period;
            short delta = (short)(raw_period >> shift);
            if (negate) delta = channel ? (short)-delta : (short)-(delta + 1);
            raw_period += delta;
            return raw_period;
        }
    }

    private class Sequencer {
        private byte[] sequence { get; set; }
        private byte index { get; set; }
        private byte length { get; set; }

        public Sequencer(byte[] sequence) {
            this.sequence = sequence;
            this.index = 0;
            this.length = (byte)sequence.Length;
        }

        public byte clock() {
            index++;
            if (index >= length) index = 0;
            return sequence[index];
        }

        public byte get() {
            return sequence[index];
        }
    }
    private class Pulse {
        private int channel { get; set; } // 0: pulse 1, 1: pulse 2
        private bool enabled { get; set; }

        private byte duty { get; set; }
        private short raw_period { get; set; }

        private double cycles;
        
        private LengthCounter lengthCounter { get; set; }
        private Envelope envelope { get; set; }
        private Sweep sweep { get; set; }
        private Sequencer sequencer { get; set; }

        public Pulse(int channel) {
            if (channel is not (0 or 1)) {
                throw new ArgumentException("Channel must be 0 or 1");
            }
            this.channel = channel;
            // dummy initialization
            this.duty = 0;
            this.raw_period = 0;
            this.lengthCounter = new LengthCounter(0, false);
            this.envelope = new Envelope(0, false, false);
            this.sweep = new Sweep(false, 0, false, 0, channel == 1);
            this.sequencer = new Sequencer(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            this.enabled = true;
            this.cycles = 0;
            // real initialization is in reset method
            this.reset();
        }

        private void reset() {
            this.duty = (byte)((registers[0x00 + channel * 4] & 0xC0) >> 6);
            this.raw_period = (short)((registers[0x03 + channel * 4] << 8) | (registers[0x02 + channel * 4] & 0x07));
            this.lengthCounter = new LengthCounter(
                lengthTable[registers[0x03 + channel * 4] >> 3],
                (registers[0x00 + channel * 4] & 0x20) != 0 
            );
            this.envelope = new Envelope(
                (byte)(registers[0x00 + channel * 4] & 0x0F),
                (registers[0x00 + channel * 4] & 0x20) != 0,
                (registers[0x00 + channel * 4] & 0x10) != 0
            );
            this.sweep = new Sweep(
                (registers[0x01 + channel * 4] & 0x80) != 0,
                (byte)((registers[0x01 + channel * 4] & 0x70) >> 4),
                (registers[0x01 + channel * 4] & 0x08) != 0,
                (byte)(registers[0x01 + channel * 4] & 0x07),
                channel == 1                        
            );
            this.enabled = readFlag(0x15, channel == 0 ? 0 : 1);
        }

        public void updateEnvelope() {
            // Called when $4000 or $4004 is written to
            envelope.load((byte)(registers[0x00 + channel * 4] & 0x0F));
            envelope.setLoop((registers[0x00 + channel * 4] & 0x20) != 0);
            envelope.setConstant((registers[0x00 + channel * 4] & 0x10) != 0);
        }
        
        public void updateSweep() {
            // Called when $4001 or $4005 is written to
            sweep.load((byte)((registers[0x01 + channel * 4] & 0x70) >> 4));
            sweep.setEnabled((registers[0x01 + channel * 4] & 0x80) != 0); 
            sweep.setNegate((registers[0x01 + channel * 4] & 0x08) != 0);
            sweep.setShift((byte)(registers[0x01 + channel * 4] & 0x07));
        }
        
        public void updatePeriod() {
            // Called when $4002 or $4006 is written to
            raw_period = (short)(((registers[0x03 + channel * 4] & 0x07) << 8) | (registers[0x02 + channel * 4]));
        }
        
        public void updateLengthCounter() {
            // Called when $4000, $4003, $4004, or $4007 is written to
            lengthCounter.setHalt(readFlag(channel == 0 ? 0x00 : 0x04, 5));
            lengthCounter.load(lengthTable[registers[0x03 + channel * 4] >> 3]);
        }
        
        public void setEnabled(bool enabled) {
            this.enabled = enabled;
        }

        public void clockHalfFrame() {
            lengthCounter.clock();
            raw_period = sweep.clock(raw_period);
            if (raw_period is < 8 or > 0x7FF) {
                this.enabled = false;
            }
        }
        
        public void clockQuarterFrame() {
            envelope.clock();
        }
        
        public byte get() {
            if (!enabled) return 0;
            if (lengthCounter.get() == 0) return 0;
            return raw_period is < 8 or > 0x7FF ? (byte)0 : envelope.get();
        }

        public byte[] getSamples() {
            if (!enabled) return new byte[200];
            if (lengthCounter.get() == 0) return new byte[200];
            if (raw_period is < 8 or > 0x7FF) return new byte[200];
            byte[] samples = new byte[200];
            for(int i = 0; i < 200; i++) {
                cycles += clockRate / 96000; // Pulse runs on the APU clock which is half the speed of the CPU clock
                if (cycles >= raw_period) {
                    cycles -= raw_period;
                    sequencer.clock();
                }
                samples[i] = (byte)(envelope.get() * (dutyTable[duty][sequencer.get()] ? 1 : 0));
            }
            return samples;
        }
    }
    private class Triangle {
        
        private bool enabled { get; set; }
        private short raw_period { get; set; }
        private double cycles { get; set; }

        private LengthCounter lengthCounter { get; set; }
        private LengthCounter linearCounter { get; set; }
        
        private Sequencer sequencer { get; set; }
        private byte[] samples { get; set; }
        
        public Triangle() {
            this.raw_period = 0;
            this.lengthCounter = new LengthCounter(0, false);
            this.linearCounter = new LengthCounter(0, false);
            this.enabled = true;
            this.sequencer = new Sequencer(new byte[] {
                0xF, 0xE, 0xD, 0xC, 0xB, 0xA, 0x9, 0x8, 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0,
                0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF,
            });
            this.samples = new byte[200];
            this.cycles = 0;
            this.reset();
        }
        
        private void reset() {
            this.raw_period = (short)(((registers[0x0B] & 0x07) << 8) | registers[0x0A]);
            this.lengthCounter = new LengthCounter(
                lengthTable[registers[0x0B] >> 3],
                (registers[0x06] & 0x80) != 0   
            );
            this.linearCounter = new LengthCounter(
                (byte)(registers[0x08] & 0x7F),
                (registers[0x06] & 0x80) != 0
            );
            this.enabled = readFlag(0x15, 2);
        }
        
        public void updatePeriod() {
            // Called when $400A is written to
            raw_period = (short)(((registers[0x0B] & 0x07) << 8) | registers[0x0A]);
        }
        
        public void updateLengthCounter() {
            // Called when $4008 or $400B is written to
            lengthCounter.setHalt(readFlag(0x08, 7));
            lengthCounter.load(lengthTable[registers[0x0B] >> 3]);
        }
        
        public void updateLinearCounter() {
            // Called when $4008 is written to
            linearCounter.setHalt(readFlag(0x08, 7));
            linearCounter.load((byte)(registers[0x08] & 0x7F));
        }
        
        public void setEnabled(bool enabled) {
            this.enabled = enabled;
        }
        
        public void clockHalfFrame() {
            lengthCounter.clock();
            linearCounter.clock();
        }
        
        public void clockQuarterFrame() {
            // No-op
        }
        
        public byte[] getSamples() {
            if (!enabled) return new byte[200];
            if (linearCounter.get() == 0 || lengthCounter.get() == 0) return new byte[200];
            for (int i = 0; i < samples.Length; i++) {
                cycles += clockRate / 48000;
                while (cycles >= raw_period) {
                    cycles -= raw_period;
                    sequencer.clock();
                }
                samples[i] = sequencer.get();
            }
            return samples;
        }
    }
    private class Noise {
        private bool enabled { get; set; }
        private short timer_period { get; set; }
        private double cycles { get; set; }
        private double sampleCycles { get; set; }

        private LengthCounter lengthCounter { get; set; }
        private Envelope envelope { get; set; }
        private LFSR lfsr { get; set; }
        
        public Noise() {
            this.timer_period = 0;
            this.cycles = 0;
            this.sampleCycles = 0;
            this.lengthCounter = new LengthCounter(0, false);
            this.envelope = new Envelope(0, false, false);
            this.lfsr = new LFSR(false);
            this.enabled = true;
            this.reset();
        }

        public void reset() {
            this.timer_period = timerPeriodTable[registers[0x0E] & 0x0F];
            this.lengthCounter = new LengthCounter(
                lengthTable[registers[0x0F] >> 3],
                (registers[0x0C] & 0x20) != 0
            );
            this.envelope = new Envelope(
                (byte)(registers[0x0C] & 0x0F),
                (registers[0x0C] & 0x20) != 0,
                (registers[0x0C] & 0x10) != 0
            );
            this.lfsr = new LFSR(readFlag(0x0E, 7));
            this.enabled = readFlag(0x15, 3);
        }
        
        public void updateEnvelope() {
            // Called when $400C is written to
            envelope.load((byte)(registers[0x0C] & 0x0F));
            envelope.setLoop((registers[0x0C] & 0x20) != 0);
            envelope.setConstant((registers[0x0C] & 0x10) != 0);
            lengthCounter.setHalt((registers[0x0C] & 0x20) != 0);
        }
        
        public void updatePeriod() {
            // Called when $400E is written to
            lfsr.setMode((registers[0x0E] & 0x80) != 0);
            timer_period = timerPeriodTable[registers[0x0E] & 0x0F];
        }

        public void updateLFSR() {
            // Called when $400E is written to
            lfsr.setMode(readFlag(0x0E, 7));
        }
        
        public void updateLengthCounter() {
            // Called when or $400F is written to
            lengthCounter.load(lengthTable[registers[0x0F] >> 3]);
        }

        public void setEnabled(bool enabled) {
            this.enabled = enabled;
        }
        
        public void clockHalfFrame() {
            lengthCounter.clock();
        }
        
        public void clockQuarterFrame() {
            this.cycles += clockRate / 240;
            for(int i = 0; i < this.cycles; i++) {
                lfsr.clock();
            }
            this.cycles -= (int)this.cycles;
            envelope.clock();
        }

        public byte[] getSamples() {
            if (!enabled) return new byte[200];
            if (lengthCounter.get() == 0) return new byte[200];
            byte[] samples = new byte[200];
            for (int i = 0; i < samples.Length; i++) {
                sampleCycles += clockRate / 48000;
                while (sampleCycles >= timer_period) {
                    sampleCycles -= timer_period;
                    lfsr.clock();
                }
                samples[i] = (byte)(envelope.get() * (lfsr.get() ? 0 : 1));
            }
            return samples;
        }
    }

    private static Pulse pulse1 = new (0);
    private static Pulse pulse2 = new (1);
    private static Triangle triangle = new ();
    private static Noise noise = new ();

    public static byte read(int index) {
        return registers[index];
    }

    public static bool readFlag(int index, int bit) {
        return (registers[index] & (1 << bit)) != 0;
    }

    public static void write(int index, byte data) {
        // Console.WriteLine("Write call at " + index.ToString("X2") + " with " + data.ToString("X2"));
        registers[index] = data;
        switch (index) {
            case 0: // $4000
                pulse1.updateEnvelope();
                break;
            case 1: // $4001
                pulse1.updateSweep();
                break;
            case 2: // $4002
                pulse1.updatePeriod();
                break;
            case 3: // $4003
                pulse1.updatePeriod();
                pulse1.updateLengthCounter();
                break;
            case 4: // $4004
                pulse2.updateEnvelope();
                break;
            case 5: // $4005
                pulse2.updateSweep();
                break;
            case 6: // $4006
                pulse2.updatePeriod();
                break;
            case 7: // $4007
                pulse2.updatePeriod();
                pulse2.updateLengthCounter();
                break;
            case 8: // $4008
                triangle.updateLinearCounter();
                break;
            case 10: // $400A
                triangle.updatePeriod();
                break;
            case 11: // $400B
                triangle.updatePeriod();
                triangle.updateLengthCounter();
                break;
            case 12: // $ 400C
                noise.updateEnvelope();
                break;
            case 14: // $400E
                noise.updateLFSR();
                noise.updatePeriod();
                break;
            case 15: // $400F
                noise.updateLengthCounter();
                break;
            case 21: // $4015
                pulse1.setEnabled(readFlag(21, 0));
                pulse2.setEnabled(readFlag(21, 1));
                triangle.setEnabled(readFlag(21, 2));
                noise.setEnabled(readFlag(21, 3));
                break;
        }
    }
    public static void writeFlag(int index, int bit, bool value) {
        byte data = registers[index];
        if (value) {
            data |= (byte)(1 << bit);
        } else {
            data &= (byte)~(1 << bit);
        }
        write(index, data);
    }
    
    private static void clockQuarterFrame() {
        pulse1.clockQuarterFrame();
        pulse2.clockQuarterFrame();
        triangle.clockQuarterFrame();
        noise.clockQuarterFrame();
    }
    
    private static void clockHalfFrame() {
        pulse1.clockHalfFrame();
        pulse2.clockHalfFrame();
        triangle.clockHalfFrame();
        noise.clockHalfFrame();
    }
    
    public static void start() {
        // create a separate thread for the RP2A03
        Thread t = new (Run);
        t.Start();
        t.IsBackground = true;
    }
    
    private static void Run() {
        const double f = 240; // Hz
        double durationTicks = Math.Round((1 / f) * Stopwatch.Frequency);
        var sw = Stopwatch.StartNew();
        int tick = 0;
        while (true) {
            bool mode = readFlag(0x17, 7);
            if (!mode) { // 4 step mode
                clockQuarterFrame();
                if (tick % 2 == 0) {
                    clockHalfFrame();
                }
            }
            else { // 5 step mode
                if (tick != 3) {
                    clockQuarterFrame();
                }
                if (tick is 1 or 4) {
                    clockHalfFrame();
                }
            }
            tick++;
            tick %= mode ? 5 : 4;
            // create audio samples
            byte[] p1 = pulse1.getSamples();
            byte[] p2 = pulse2.getSamples();
            byte[] t = triangle.getSamples();
            byte[] n = noise.getSamples();
            byte[] buffer = new byte[400];
            for (int i = 0; i < 200; i++) {
                double pulse = 95.88 / (8128.0 / (p1[i] + p2[i]) + 100);
                double tnd = 159.79 / (1.0 / (t[i] / 8227.0 + n[i] / 12241.0) + 100);
                double sample = pulse + tnd;
                short s = (short)(sample * 32767);
                buffer[2 * i] = (byte)(s & 0xFF);
                buffer[2 * i + 1] = (byte)(s >> 8);
            }
            // for (int i = 0; i < 200; i++) {
            //     // buffer is at 48kHz
            //     short val = (short)((Math.Sin((2 * Math.PI * 240) * (i / 48000.0)) * 0x7FFF) + 0x7FFF);
            //     buffer[2 * i] = (byte)(val & 0xFF);
            //     buffer[2 * i + 1] = (byte)(val >> 8);
            // }
            AudioBuffer.write(buffer);
            while(sw.ElapsedTicks < durationTicks) {
                // spin
            }
            sw.Reset();
            sw.Start();
        }
    }
}