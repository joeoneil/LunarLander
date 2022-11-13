namespace LunarLander.audio; 

public class Song {
    private struct Note {
        public int note;
        public int octave;
        public readonly double start;
        public readonly double duration;
        
        public Note (int note, int octave, double start, double duration) {
            this.note = note;
            this.octave = octave;
            this.start = start;
            this.duration = duration;
        }
    }
    
    private Note[] notes;

    private Song(Note[] notes) {
        this.notes = notes;
    }
    
    public double noteStartTime(int note) {
        return notes[note].start;
    }
    
    public double noteDuration(int note) {
        return notes[note].duration;
    }
    
    public int notePitch(int note) {
        return notes[note].note;
    }
    
    public int noteOctave(int note) {
        return notes[note].octave;
    }

    private static double mario_tempo = (60.0 / 180.0);

    public static Song mario = new Song(new Note[] {
        // Measure 1
        new(4, 5, 0, 0.5 * mario_tempo),
        new(4, 5, 0.5 * mario_tempo, 0.5 * mario_tempo),
        new(4, 5, 1.5 * mario_tempo, 0.5 * mario_tempo),
        new(0, 5, 2.5 * mario_tempo, 0.5 * mario_tempo),
        new(4, 5, 3.0 * mario_tempo, 1.0 * mario_tempo),
        // Measure 2
        new(7, 5, 4.0 * mario_tempo, 1.0 * mario_tempo),
        new(7, 4, 6.0 * mario_tempo, 1.0 * mario_tempo),
        // Measure 3
    });
}