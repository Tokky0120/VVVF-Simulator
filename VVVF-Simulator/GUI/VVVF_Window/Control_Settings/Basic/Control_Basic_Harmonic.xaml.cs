using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.VVVF_Structs.Pulse_Mode;

namespace VVVF_Simulator.GUI.Pages.Control_Settings.Basic
{
    /// <summary>
    /// Control_Basic_Harmonic.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Basic_Harmonic : Window
    {
        View_Data vd = new View_Data();
        public class View_Data : ViewModelBase
        {
            public List<Pulse_Harmonic> _harmonic_data = new List<Pulse_Harmonic>();
            public List<Pulse_Harmonic> harmonic_data { get { return _harmonic_data; } set { _harmonic_data = value; RaisePropertyChanged(nameof(harmonic_data)); } }
        }
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //
        //Harmonic Presets
        public enum Preset_Harmonics
        {
            THI, SVM, DPM1, DPM2, DPM3, DPM4, Square_Fourier
        }

        public List<Pulse_Harmonic> Get_Preset_Harmonics(Preset_Harmonics harmonic)
        {
            switch (harmonic)
            {
                case Preset_Harmonics.THI:
                    return new List<Pulse_Harmonic>() { 
                        new Pulse_Harmonic() { amplitude = 0.2, harmonic = 3 } 
                    };
                case Preset_Harmonics.SVM:
                    return new List<Pulse_Harmonic>() { 
                        new Pulse_Harmonic() { amplitude = 0.25, harmonic = 3 , type = Pulse_Harmonic.Pulse_Harmonic_Type.Saw} 
                    };
                case Preset_Harmonics.DPM1:
                    return new List<Pulse_Harmonic>() {
                        new Pulse_Harmonic() { amplitude = -0.05, harmonic = 3 },
                        new Pulse_Harmonic() { amplitude = 0.2, harmonic = 3, type = Pulse_Harmonic.Pulse_Harmonic_Type.Square }
                    };
                case Preset_Harmonics.DPM2:
                    return new List<Pulse_Harmonic>() {
                        new Pulse_Harmonic() { amplitude = -0.05, harmonic = 3, initial_phase = 1.57079633, type = Pulse_Harmonic.Pulse_Harmonic_Type.Saw},
                        new Pulse_Harmonic() { amplitude = 0.2, harmonic = 3, type = Pulse_Harmonic.Pulse_Harmonic_Type.Square }
                    };
                case Preset_Harmonics.DPM3:
                    return new List<Pulse_Harmonic>() {
                        new Pulse_Harmonic() { amplitude = -0.05, harmonic = 3, initial_phase = -1.57079633, type = Pulse_Harmonic.Pulse_Harmonic_Type.Saw},
                        new Pulse_Harmonic() { amplitude = 0.2, harmonic = 3, type = Pulse_Harmonic.Pulse_Harmonic_Type.Square }
                    };
                case Preset_Harmonics.DPM4: //case Preset_Harmonics.DPM4:
                    return new List<Pulse_Harmonic>() {
                        new Pulse_Harmonic() { amplitude = 0.05, harmonic = 3, type = Pulse_Harmonic.Pulse_Harmonic_Type.Saw},
                        new Pulse_Harmonic() { amplitude = 0.2, harmonic = 3, type = Pulse_Harmonic.Pulse_Harmonic_Type.Square }
                    };
                default:
                    List<Pulse_Harmonic> harmonics = new();
                    for (int i = 0; i < 10; i++)
                    {
                        harmonics.Add(new Pulse_Harmonic() { amplitude = 1.0 / (2 * i + 3), harmonic = 2 * i + 3 });
                    }
                    return harmonics;


            }
        }


        //
        //
        //


        Pulse_Mode target;
        private bool no_update = true;
        public Control_Basic_Harmonic(Pulse_Mode data)
        {

            vd.harmonic_data = data.pulse_harmonics;
            DataContext = vd;
            target = data;

            InitializeComponent();

            Preset_Selector.ItemsSource = (Preset_Harmonics[])Enum.GetValues(typeof(Preset_Harmonics));
            Preset_Selector.SelectedIndex = 0;

            no_update = false;
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (no_update) return;
            target.pulse_harmonics = vd.harmonic_data;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Harmonic_Editor_Unloaded(object sender, RoutedEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;

            Preset_Harmonics selected = (Preset_Harmonics)Preset_Selector.SelectedItem;
            List<Pulse_Harmonic> harmonics = Get_Preset_Harmonics(selected);

            if (tag.Equals("Add"))
                vd.harmonic_data.AddRange(harmonics);
            else if (tag.Equals("Set"))
                vd.harmonic_data = harmonics;

            Harmonic_Editor.Items.Refresh();

        }
    }
}
