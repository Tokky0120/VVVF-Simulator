using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Structs;

namespace VVVF_Simulator.GUI.Simulator.RealTime.Display
{
    /// <summary>
    /// RealTime_WaveForm_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_WaveForm_Window : Window
    {
        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {
            private BitmapFrame? _waveform;
            public BitmapFrame? waveform { get { return _waveform; } set { _waveform = value; RaisePropertyChanged(nameof(waveform)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        RealTime_Parameter realTime_Parameter;
        public RealTime_WaveForm_Window(RealTime_Parameter r)
        {
            realTime_Parameter = r;

            DataContext = view_model;
            InitializeComponent();
        }

        public void Start_Task()
        {
            Task.Run(() => {
                while (!realTime_Parameter.quit)
                {
                    set_WaveForm();
                    System.Threading.Thread.Sleep(16);
                }
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            });
        }

        private void set_WaveForm()
        {
            Yaml_VVVF_Sound_Data sound_data = realTime_Parameter.sound_data;
            VVVF_Values control = realTime_Parameter.control_values.Clone();

            control.set_Saw_Time(0);
            control.set_Sine_Time(0);

            control.set_Allowed_Random_Freq_Move(false);

            int image_width = 1200;
            int image_height = 450;
            int calculate_div = 3;
            int wave_height = 100;

            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
            Bitmap image = Generation.Video.WaveForm.Generate_WaveForm_UV.Get_WaveForm_Image(control, calculated_Values, image_width, image_height, wave_height, calculate_div, 2,0);

            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                view_model.waveform = data;
            }
        }
    }
}
