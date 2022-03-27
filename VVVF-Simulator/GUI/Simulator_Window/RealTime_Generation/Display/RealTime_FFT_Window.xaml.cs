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
using static VVVF_Simulator.Generation.Audio.Generate_RealTime;

namespace VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Display
{
    /// <summary>
    /// RealTime_FFT_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_FFT_Window : Window
    {
        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {

            private BitmapFrame? _hexagon;
            public BitmapFrame? hexagon { get { return _hexagon; } set { _hexagon = value; RaisePropertyChanged(nameof(hexagon)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public RealTime_FFT_Window()
        {
            InitializeComponent();
            DataContext = view_model;
        }

        public void Start_Task()
        {
            Task.Run(() => {
                while (!RealTime_Parameter.quit)
                {
                    update_control_stat();
                }
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            });
        }

        private void update_control_stat()
        {
            

            VVVF_Values control = RealTime_Parameter.control_values.Clone();
            Yaml_VVVF_Sound_Data ysd = RealTime_Parameter.sound_data;

            control.set_Sine_Time(0);
            control.set_Saw_Time(0);

            Bitmap image = Generation.Video.FFT.Generate_FFT.Get_FFT_Image(control,ysd);


            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                view_model.hexagon = data;
            }

            image.Dispose();
        }
    }
}
