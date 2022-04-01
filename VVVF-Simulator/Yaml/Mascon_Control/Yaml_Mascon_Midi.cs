using System;
using System.Collections.Generic;
using System.Linq;
using NextMidi.Data;
using NextMidi.Data.Score;
using NextMidi.Data.Track;
using NextMidi.DataElement;
using NextMidi.Filing.Midi;

namespace VVVF_Simulator.Yaml.Mascon_Control
{
    public class Yaml_Mascon_Midi
    {
        public class Converted_Construct
        {
            public Note_Event_Type type;
            public int tick;
            public int tempo;
            public int note;
        }

        public enum Note_Event_Type
        {
            ON,OFF,
        }
        public List<Converted_Construct> GetTime_Line(MidiData midiData)
        {
            List<Converted_Construct> events = new List<Converted_Construct>();
            List<Converted_Construct> converted_Constructs = new List<Converted_Construct>();
            TempoMap sc = new TempoMap(midiData);
            for (int i = 0; i < midiData.Tracks.Count; i++)
            {
                MidiTrack track = midiData.Tracks[i];
                foreach (var note in track.GetData<NoteEvent>())
                {
                    Converted_Construct Note_ON_Data = new Converted_Construct
                    {
                        tick = note.Tick,
                        tempo = sc.GetTempo(note.Tick),
                        type = Note_Event_Type.ON,
                        note = note.Note
                    };
                    events.Add(Note_ON_Data);
                    Converted_Construct Note_OFF_Data = new Converted_Construct
                    {
                        tick = note.Tick + note.Gate,
                        tempo = sc.GetTempo(note.Tick + note.Gate),
                        type = Note_Event_Type.OFF,
                        note = note.Note
                    };
                    events.Add(Note_OFF_Data);
                }
            }
            if (events.Count == 0) return null;//もしも、何もイベントがなかったらnullを返す
            while (true)
            {
                int min_tick = Int32.MaxValue, found = 0;
                for (int i = 0; i < events.Count; i++)
                {
                    Converted_Construct converted_Construct = events[i];
                    int tick = converted_Construct.tick;
                    if (min_tick > tick)
                    {
                        min_tick = tick;
                        found = i;
                    }
                }
                converted_Constructs.Add(events[found]);
                events.RemoveAt(found);
                if (events.Count == 0)
                {
                    break;
                }
            }

            return converted_Constructs;
        }
    }
}
