using NextMidi.Data;
using NextMidi.Data.Track;
using NextMidi.Filing.Midi;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Generate_Common;
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

        public static bool Conversion(String midi_path, String output_path, int sample_freq)
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

            String? folder_path = Path.GetDirectoryName(output_path);
            if(folder_path == null)
            {
                MessageBox.Show("Selected Path is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            String file_name = Path.GetFileNameWithoutExtension(output_path);

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

                    GenerationBasicParameter generationBasicParameter = new(
                        ymd.GetCompiled(),
                        Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data),
                        new GenerationBasicParameter.ProgressData()
                    );

                    String task_description = "Generation of MIDI Sound part " + i + " of " + priority;

                    Task task = Task.Run(() =>
                    {
                        try
                        {
                            String export_path = folder_path + Path.PathSeparator + file_name + i + "-" + priority + ".wav";
                            Generation.Audio.VVVF_Sound.Generate_VVVF_Audio.Export_VVVF_Sound(generationBasicParameter, export_path, false, sample_freq);
                        }
                        catch
                        {
                            MessageBox.Show("Error on " + task_description, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });

                    MainWindow.TaskProgressData taskProgressData = new(task, generationBasicParameter.progressData, task_description);
                    MainWindow.taskProgresses.Add(taskProgressData);

                    priority++;
                }
            }

            return true;
        }
    }
}
