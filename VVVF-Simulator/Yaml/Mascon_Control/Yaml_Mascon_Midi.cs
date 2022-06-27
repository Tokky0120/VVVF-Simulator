using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NextMidi.Data;
using NextMidi.Data.Score;
using NextMidi.Data.Track;
using NextMidi.DataElement;
using NextMidi.Filing.Midi;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze.Yaml_Mascon_Data;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Midi.NoteEvent_Simple;

namespace VVVF_Simulator.Yaml.Mascon_Control
{
    public class Yaml_Mascon_Midi
    {
        public class NoteEvent_Simple
        {
            public NoteEvent_SimpleData On = new();
            public NoteEvent_SimpleData Off = new();
            public class NoteEvent_SimpleData
            {
                public Note_Event_Type type;
                public double time;
                public int note;
            }

            public enum Note_Event_Type
            {
                ON, OFF,
            }
        }

        public static Yaml_Mascon_Data? Convert(GUI.Mascon_Window.Generation_Mascon_Control_Midi.LoadData loadData)
        {
            //MIDIDataを変換
            MidiData midiData;
            try
            {
                midiData = MidiReader.ReadFrom(loadData.path);
            }
            catch
            {
                MessageBox.Show("This MIDI cannot be converted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            List<NoteEvent_Simple> converted_Constructs = GetTime_Line(midiData, loadData.track);
            Yaml_Mascon_Data mascon_Data = new();

            double total_time = 0;

            for(int j = 0; j < loadData.priority; j++)
            {
                double pre_event_time = 0;
                List<int> selected_Data = new();
                for (int i = 0; i < converted_Constructs.Count; i++)
                {
                    NoteEvent_Simple data = converted_Constructs[i];

                    if (data.On.time < pre_event_time) continue;

                    double initial_wait = data.On.time - pre_event_time;
                    double play_wait = data.Off.time - data.On.time;

                    pre_event_time = data.Off.time;
                    selected_Data.Add(i);

                    if (loadData.priority != j + 1) continue;

                    // set initial
                    mascon_Data.points.Add(new Yaml_Mascon_Data_Point() { rate = 0, duration = -1, brake = false, mascon_on = true, order = 4 * i });
                    // wait initial
                    mascon_Data.points.Add(new Yaml_Mascon_Data_Point() { rate = 0, duration = initial_wait, brake = false, mascon_on = true, order = 4 * i + 1 });
                    // set play
                    double frequency = 440 * Math.Pow(2, (data.On.note - 69) / 12.0) / loadData.division;
                    mascon_Data.points.Add(new Yaml_Mascon_Data_Point() { rate = frequency, duration = -1, brake = false, mascon_on = true, order = 4 * i + 2 });
                    // wait play
                    mascon_Data.points.Add(new Yaml_Mascon_Data_Point() { rate = 0, duration = play_wait, brake = false, mascon_on = true, order = 4 * i + 3 });

                    total_time += play_wait;
                    total_time += initial_wait;
                }

                for(int i = 0; i < selected_Data.Count; i++)
                {
                    converted_Constructs.RemoveAt(selected_Data[selected_Data.Count - i - 1]);
                }
            }

            return mascon_Data;
        }

        public static List<NoteEvent_Simple> GetTime_Line(MidiData midiData,int track_num)
        {
            List<NoteEvent_Simple> events = new List<NoteEvent_Simple>();
            TempoMap sc = new TempoMap(midiData);
            MidiTrack track = midiData.Tracks[track_num];

            foreach (var note in track.GetData<NoteEvent>())
            {
                NoteEvent_SimpleData Note_ON_Data = new()
                {
                    time = 0.001 * sc.ToMilliSeconds(note.Tick),
                    type = Note_Event_Type.ON,
                    note = note.Note
                };

                NoteEvent_SimpleData Note_OFF_Data = new()
                {
                    time = 0.001 * sc.ToMilliSeconds(note.Tick + note.Gate),
                    type = Note_Event_Type.OFF,
                    note = note.Note
                };

                NoteEvent_Simple note_event = new()
                {
                    On = Note_ON_Data,
                    Off = Note_OFF_Data
                };

                events.Add(note_event);
            }

            events.Sort((a, b) => Math.Sign(a.On.time - b.On.time));
            return events;
        }

    }
}
