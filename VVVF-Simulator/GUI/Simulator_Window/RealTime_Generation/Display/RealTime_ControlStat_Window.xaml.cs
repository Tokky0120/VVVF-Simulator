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
using static VVVF_Simulator.Generation.Video.Control_Info.Generate_Control_Common;

namespace VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Display
{
    /// <summary>
    /// RealTime_ControlStat_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_ControlStat_Window : Window
    {
        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {

            private int _height = 100;
            public int height { get { return _height; } set { _height = value; RaisePropertyChanged(nameof(height)); } }

            private int _width = 100;
            public int width { get { return _width; } set { _width = value; RaisePropertyChanged(nameof(width)); } }

            private BitmapFrame? _control_stat;
            public BitmapFrame? control_stat { get { return _control_stat; } set { _control_stat = value; RaisePropertyChanged(nameof(control_stat)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        RealTime_ControlStat_Style style;
        RealTime_Parameter realTime_Parameter;
        int image_width = 960;
        int image_height = 1620;
        public RealTime_ControlStat_Window(RealTime_Parameter r,RealTime_ControlStat_Style style)
        {
            realTime_Parameter = r;

            DataContext = view_model;
            InitializeComponent();
            this.style = style;

            view_model.height = image_height;
            view_model.width = image_width;
        }

        public void Start_Task()
        {
            Task.Run(() => {
                while (!realTime_Parameter.quit)
                {
                    update_control_stat();

                }
                Dispatcher.Invoke((Action)(() =>
                {
                    Close();
                }));
            });
        }

        private Pre_Voltage_Data pre_Voltage_Data = new Pre_Voltage_Data(false, 0);
        private void update_control_stat()
        {
            Bitmap image;

            if(style == RealTime_ControlStat_Style.Original)
            {
                VVVF_Values control = realTime_Parameter.control_values.Clone();
                image = Generation.Video.Control_Info.Generate_Control_Original.Get_Control_Original_Image(
                    control,
                    realTime_Parameter.control_values.get_Sine_Freq() == 0
                );
            }
            else
            {
                VVVF_Values control = realTime_Parameter.control_values.Clone();
                image = Generation.Video.Control_Info.Generate_Control_Original2.Get_Control_Original2_Image(
                    control,
                    true,
                    realTime_Parameter.sound_data,
                    pre_Voltage_Data,
                    false
                );
            }

            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                view_model.control_stat = data;
            }
        }
    }

    public enum RealTime_ControlStat_Style { 
        Original, Original_2
    }
}
