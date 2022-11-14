using System;
using System.Diagnostics;
using System.Threading;

namespace LunarLander.audio; 

public static class RP2A03 {
    
    #region state
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

    public static double testFreq = 240;

    public static double gain = 2;

    public static bool running { get; private set; } = false;
    
    #endregion

    #region subcomponents
    private class Divider {
        public short count { get; private set; }
        private short period { get; set; }
        private bool loop { get; set; }

        public Divider(short period, bool loop) {
            this.period = period;
            this.loop = loop;
            this.count = period;
        }

        public void load(short period) {
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
            return (byte)divider.count;
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
    #endregion
    
    #region channels
    private class Pulse {
        private int channel { get; set; } // 0: pulse 1, 1: pulse 2
        private bool enabled { get; set; }

        private byte duty { get; set; }
        private short raw_period { get; set; }
        
        private LengthCounter lengthCounter { get; set; }
        private Envelope envelope { get; set; }
        private Sweep sweep { get; set; }
        private Sequencer sequencer { get; set; }
        private Divider divider { get; set; }

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
            this.divider = new Divider(0, false);
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
            this.divider = new Divider(raw_period, true);
            this.enabled = readFlag(0x15, channel == 0 ? 0 : 1);
        }

        public void updateEnvelope() {
            // Called when $4000 or $4004 is written to
            this.duty = (byte)((registers[0x00 + channel * 4] & 0xC0) >> 6);
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
            divider.load(raw_period);
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

        public void clock() {
            if (divider.clock()) {
                sequencer.clock();
            }
        }
        
        public byte get() {
            if (!enabled) return 0;
            if (lengthCounter.get() == 0) return 0;
            if (raw_period is < 8 or > 0x7FF) return 0;
            return dutyTable[duty][sequencer.get()] ? envelope.get() : (byte)0;
        }
    }
    private class Triangle {
        
        private bool enabled { get; set; }
        private short raw_period { get; set; }
        private LengthCounter lengthCounter { get; set; }
        private LengthCounter linearCounter { get; set; }
        
        private Sequencer sequencer { get; set; }
        private Divider divider { get; set; }
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
            this.divider = new Divider(0, false);
            this.samples = new byte[200];
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
            this.divider = new Divider(raw_period, true);
            this.enabled = readFlag(0x15, 2);
        }
        
        public void updatePeriod() {
            // Called when $400A is written to
            raw_period = (short)(((registers[0x0B] & 0x07) << 8) | registers[0x0A]);
            this.divider = new Divider(raw_period, true);
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

        public void clock() {
            if (divider.clock()) {
                sequencer.clock();
            }
            if (divider.clock()) {
                sequencer.clock();
            }
        }

        public byte get() {
            if (!enabled) return 0;
            if (linearCounter.get() == 0 || lengthCounter.get() == 0) return 0;
            return sequencer.get();
        }
    }
    private class Noise {
        private bool enabled { get; set; }
        private short timer_period { get; set; }

        private LengthCounter lengthCounter { get; set; }
        private Divider divider { get; set; }
        private Envelope envelope { get; set; }
        private LFSR lfsr { get; set; }
        
        public Noise() {
            this.timer_period = 0;
            this.lengthCounter = new LengthCounter(0, false);
            this.envelope = new Envelope(0, false, false);
            this.lfsr = new LFSR(false);
            this.enabled = true;
            this.divider = new Divider(0, false);
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
            this.divider = new Divider(timer_period, true);
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
            divider.load(timer_period);
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
            envelope.clock();
        }

        public void clock() {
            if (divider.clock()) {
                lfsr.clock();
            }
        }

        public byte get() {
            if (!enabled) return 0;
            if (lengthCounter.get() == 0) return 0;
            return (byte)(envelope.get() * (lfsr.get() ? 0 : 1));
        }
    }
    #endregion

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
        Thread t = new (clock);
        t.Start();
        t.IsBackground = true;
    }

    private static void clock() {
        running = true;
        const double f = clockRate / 2;
        double durationTicks = Math.Round((1 / f) * Stopwatch.Frequency);
        var sw = Stopwatch.StartNew();
        int tick = 0;
        const double ticksPerSample = (1 / 48000.0) * (clockRate / 2);
        double nextSample = 0;
        while (true) {
            tick++;
            if (tick > 14915) {
                tick = 0;
                nextSample = 0;
            }
            switch (tick) {
                case 0:
                    clockQuarterFrame();
                    clockHalfFrame();
                    break;
                case 3729:
                    clockQuarterFrame();
                    break;
                case 7456:
                    clockQuarterFrame();
                    clockHalfFrame();
                    break;
                case 11185:
                    clockQuarterFrame();
                    break;
            }
            pulse1.clock();
            pulse2.clock();
            triangle.clock();
            noise.clock();
            if (tick > nextSample) {
                nextSample += ticksPerSample;
                byte p1 = pulse1.get();
                byte p2 = pulse2.get();
                byte t = triangle.get();
                byte n = noise.get();
                double pulse = 95.88 / (8128.0 / (p1 + p2) + 100);
                double tnd = 159.79 / (1.0 / (t / 8227.0 + n / 12241.0) + 100);
                double sample = (pulse + tnd) * gain;
                sample = Math.Max(-1, Math.Min(1, sample));
                short s = (short)(sample * 0x7FFF);
                AudioBuffer.write(s);
                if (!LunarLander.running) { // only check in sample loop to reduce performance impact
                    running = false;
                    return;
                }
            }
            while(durationTicks > sw.ElapsedTicks) {
                // spin
            }
            sw.Restart();
        }
    }
}