using System;
using System.Collections.Generic;

namespace LunarLander.audio; 

public static class RP2A03_API {
    
    #region Note Structs

    public struct PulseNote {
        public int channel { get; private set; }
        public int note { get; private set; }
        public int octave { get; private set; }
        public int duration { get; private set; }
        public int envelope { get; private set; }
        public bool cv { get; private set; }
        public int duty { get; private set; }

        public PulseNote(int channel, int note, int octave, int duration = 1, int envelope = 15, bool cv = true,
            int duty = 2) {
            this.channel = channel;
            this.note = note;
            this.octave = octave;
            this.duration = duration;
            this.envelope = envelope;
            this.cv = cv;
            this.duty = duty;
        }
    }
    #endregion
    
    #region Helper Functions
    /// /////////////////////////////////////////////////////////////////////
    /// Private Helper methods
    /// /////////////////////////////////////////////////////////////////////
    private static void write(int address, int value, int start, int end) {
        byte old = RP2A03.read(address);
        byte mask = (byte)(((1 << (end - start)) - 1) << start);
        byte new_data = (byte)((old & ~mask) | ((value << start) & mask));
        RP2A03.write(address, new_data);
    }
    private static void writeFlag(int address, int bit, bool value) {
        write(address, value ? 1 : 0, bit, bit + 1);
    }

    private static int getClosest(int value, IReadOnlyList<int> values) {
        int closest = 0;
        int closestDistance = int.MaxValue;
        for (int i = 0; i < values.Count; i++) {
            int distance = Math.Abs(value - values[i]);
            if (distance < closestDistance) {
                closest = i;
                closestDistance = distance;
            }
        }
        return closest;
    }

    
    private static byte getLCIFromDuration(int millis) {
        double lengthCount = (1000 / 120.0) * millis;
        int closestIndex = 0;
        int closestDistance = int.MaxValue;
        for (int i = 0; i < RP2A03.lengthTable.Length; i++) {
            if (lengthCount - RP2A03.lengthTable[i] < closestDistance) {
                closestIndex = i;
                closestDistance = (int)(lengthCount - RP2A03.lengthTable[i]);
            }
        }
        return (byte)closestIndex;
    }

    private static double getFrequency(int note, int octave) {
        return 440 * Math.Pow(2, ((note - 9) / 12.0) + (octave - 4)); // 12 notes per octave, A4 is 440Hz, A is 9th note
        //    C    = 0
        // C# / Db = 1
        //    D    = 2
        // D# / Eb = 3
        //    E    = 4
        //    F    = 5
        // F# / Gb = 6
        //    G    = 7
        // G# / Ab = 8
        //    A    = 9
        // A# / Bb = 10
        //    B    = 11
    }
    #endregion

    #region ChannelFlags
    /// /////////////////////////////////////////////////////////////////////
    /// Enable / Disable Channels
    /// /////////////////////////////////////////////////////////////////////
    public static void setPulse1(bool enabled) {
        writeFlag(0x15, 0, enabled);
    }
    
    public static void setPulse2(bool enabled) {
        writeFlag(0x15, 1, enabled);
    }
    
    public static void setTriangle(bool enabled) {
        writeFlag(0x15, 2, enabled);
    }
    
    public static void setNoise(bool enabled) {
        writeFlag(0x15, 3, enabled);
    }
    
    public static void setDmc(bool enabled) {
        writeFlag(0x15, 4, enabled);
    }
    #endregion
    
    #region PulseAPI_L1
    /// /////////////////////////////////////////////////////////////////////
    /// Pulse Channel L1 API Methods
    /// /////////////////////////////////////////////////////////////////////
    public static void setPulseDuty(int channel, byte value) {
        write(0x00 + (channel == 0 ? 0 : 4), value, 6, 8);
    }
    
    public static void setPulseLengthCounterHalt(int channel, bool value) {
        writeFlag(0x00 + (channel == 0 ? 0 : 4), 5, value);
    }
    
    public static void setPulseCV(int channel, bool enabled) {
        writeFlag(0x00 + (channel == 0 ? 0 : 4), 4, enabled);
    }
    
    public static void setPulseEnvelope(int channel, byte value) {
        write(0x00 + (channel == 0 ? 0 : 4), value, 0, 4);
    }
    
    public static void setPulseSweepEnabled(int channel, bool enabled) {
        writeFlag(0x01 + (channel == 0 ? 0 : 4), 7, enabled);
    }
    
    public static void setPulseSweepPeriod(int channel, byte value) {
        write(0x01 + (channel == 0 ? 0 : 4), value, 4, 8);
    }

    public static void setPulseSweepNegate(int channel, bool negate) {
        writeFlag(0x01 + (channel == 0 ? 0 : 4), 3, negate);
    }
    
    public static void setPulseSweepShift(int channel, byte value) {
        write(0x01 + (channel == 0 ? 0 : 4), value, 0, 3);
    }

    public static void setPulseTimer(int channel, short value) {
        write(2 + (channel == 0 ? 0 : 4), value & 0xFF, 0, 8);
        write(3 + (channel == 0 ? 0 : 4), (value >> 8) & 0x07, 0, 3);
    }
    
    public static void setPulseLengthCounter(int channel, byte value) {
        write(0x03 + (channel == 0 ? 0 : 4), value & 0x1F, 3, 8);
    }
    #endregion
    
    #region PulseAPI_L2
    /// /////////////////////////////////////////////////////////////////////
    /// Pulse Channel L2 API Methods
    /// /////////////////////////////////////////////////////////////////////
    public static void pulsePlayNote(int channel, double frequency, int durationMillis, byte envelope = 15, bool cv = false, byte duty = 2) {
        setPulseTimer(channel, (short)(RP2A03.clockRate / (16 * frequency) - 1));
        setPulseDuty(channel, duty);
        setPulseCV(channel, cv);
        setPulseEnvelope(channel, envelope);
        if (durationMillis == -1) {
            setPulseLengthCounterHalt(channel, true);
        } else {
            setPulseLengthCounterHalt(channel, false);
            setPulseLengthCounter(channel, getLCIFromDuration(durationMillis));
        }
    }
    
    public static void pulsePlayNote(int channel, int note, int octave, int durationMillis, byte envelope = 15, bool cv = false, byte duty = 2) {
        double frequency = getFrequency(note, octave);
        pulsePlayNote(channel, frequency, durationMillis, envelope, cv, duty);
    }

    public static void pulsePlayNote(PulseNote note) {
            pulsePlayNote(note.channel, note.note, note.octave, note.duration, (byte)note.envelope, note.cv, (byte)note.duty);
    }
    #endregion

    #region TriangleAPI_L1
    /// /////////////////////////////////////////////////////////////////////
    /// Triangle Channel L1 API Methods
    /// /////////////////////////////////////////////////////////////////////
    public static void setTriangleLengthCounterHalt(bool value) {
        writeFlag(0x08, 7, value);
    }

    public static void setTriangleLinearCounter(byte value) {
        write(0x08, value, 0, 7);
    }
    
    public static void setTriangleTimer(short value) {
        write(0x0A, value & 0xFF, 0, 8);
        write(0x0B, (value >> 8) & 0x07, 0, 3);
    }
    
    public static void setTriangleLengthCounter(byte value) {
        write(0x0B, value & 0x1F, 3, 8);
    }
    #endregion
    
    #region TriangleAPI_L2

    /// /////////////////////////////////////////////////////////////////////
    /// Triangle Channel L2 API Methods
    /// /////////////////////////////////////////////////////////////////////
    public static void trianglePlayNote(double frequency, int durationMillis) {
            setTriangleTimer((short)(RP2A03.clockRate / (32 * frequency) - 1));
        if (durationMillis == -1) {
            setTriangleLengthCounterHalt(true);
        } else {
            setTriangleLengthCounterHalt(false);
            setTriangleLengthCounter(getLCIFromDuration(durationMillis));
            setTriangleLinearCounter((byte)(durationMillis / (1000.0 / 240)));
        }
    }
    
    public static void trianglePlayNote(int note, int octave, int durationMillis) {
        double frequency = getFrequency(note, octave);
        trianglePlayNote(frequency, durationMillis);
    }
    
    #endregion
    
    #region NoiseAPI_L1
    /// /////////////////////////////////////////////////////////////////////
    /// Noise Channel L1 API Methods
    /// /////////////////////////////////////////////////////////////////////
    public static void setNoiseLengthCounterHalt(bool value) {
        writeFlag(0x0C, 5, value);
    }

    public static void setNoiseCV(bool value) {
        writeFlag(0x0C, 4, value);
    }
    
    public static void setNoiseEnvelope(byte value) {
        write(0x0C, value, 0, 4);
    }
    
    public static void setNoiseMode(bool value) {
        writeFlag(0x0E, 7, value);
    }
    
    public static void setNoisePeriod(byte value) {
        write(0x0E, value, 0, 4);
    }
    
    public static void setNoiseLengthCounter(byte value) {
        write(0x0F, value & 0x1F, 3, 8);
    }
    #endregion
    
    #region NoiseAPI_L2
    /// /////////////////////////////////////////////////////////////////////
    /// Noise Channel L2 API Methods
    /// /////////////////////////////////////////////////////////////////////
    
    
    public static void noisePlayNote(int period, int durationMillis, byte envelope = 15, bool cv = false) {
        setNoisePeriod((byte)period);
        setNoiseCV(cv);
        setNoiseEnvelope(envelope);
        setNoiseMode(false);
        if (durationMillis == -1) {
            setNoiseLengthCounterHalt(true);
        } else {
            setNoiseLengthCounterHalt(false);
            setNoiseLengthCounter(getLCIFromDuration(durationMillis));
        }
    }

    #endregion
    
}
