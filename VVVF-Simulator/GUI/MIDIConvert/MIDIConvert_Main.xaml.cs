using NextMidi.Data;
using NextMidi.Data.Track;
using NextMidi.Filing.Midi;
using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace VVVF_Simulator.GUI.MIDIConvert
{
    /// <summary>
    /// MIDIConvert_Main.xaml の相互作用ロジック
    /// </summary>
    public partial class MIDIConvert_Main : Window
    {
        public MIDIConvert_Main()
        {
            InitializeComponent();
        }

        public bool Conversion(String midi_path)
        {
            //MIDIDataを変換
            MidiData midiData;
            try
            {
                midiData = MidiReader.ReadFrom(midi_path);
            }
            catch
            {
                MessageBox.Show("This MIDI cannot be converted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            TrackCollection tracks = midiData.Tracks;

            for(int i = 0; i < tracks.Count; i++)
            {
                int priority = 0;
                while (true)
                {
                    Mascon.Mascon_Control_Midi.LoadData loadData = new()
                    {
                        track = i,
                        division = 1,
                        path = midi_path,
                        priority = priority,
                    };
                    Yaml.Mascon_Control.Yaml_Mascon_Analyze.Yaml_Mascon_Data? ymd = Yaml.Mascon_Control.Yaml_Mascon_Midi.Convert(loadData);
                    if (ymd == null) return false;
                    if (ymd.points.Count == 0) break;
                }
            }

            return true;
        }
    }
}
