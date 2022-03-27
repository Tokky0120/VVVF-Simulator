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
using static VVVF_Simulator.VVVF_Calculate;

namespace VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Display
{
    /// <summary>
    /// RealTime_Hexagon_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_Hexagon_Window : Window
    {
        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {

            private int _height = 100;
            public int height { get { return _height; } set { _height = value; RaisePropertyChanged(nameof(height)); } }

            private int _width = 100;
            public int width { get { return _width; } set { _width = value; RaisePropertyChanged(nameof(width)); } }

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


        private RealTime_Hexagon_Style style;
        private bool show_zero_vector;
        public RealTime_Hexagon_Window(RealTime_Hexagon_Style style, bool show_zero_vector)
        {
            InitializeComponent();

            this.style = style;
            this.show_zero_vector = show_zero_vector;

            view_model.height = 500;
            view_model.width = 500;

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
            Bitmap image = new Bitmap(100,100);

            VVVF_Values control = RealTime_Parameter.control_values.Clone();
            Yaml_VVVF_Sound_Data ysd = RealTime_Parameter.sound_data;

            control.set_Sine_Time(0);
            control.set_Saw_Time(0);

            if(style == RealTime_Hexagon_Style.Original)
            {
                int image_width = 1000;
                int image_height = 1000;
                int hex_div_seed = 10000;
                control.set_Allowed_Random_Freq_Move(false);
                image = Generation.Video.Hexagon.Generate_Hexagon_Original.Get_Hexagon_Original_Image(
                    control,
                    ysd, 
                    image_width, 
                    image_height,
                    hex_div_seed, 
                    2,
                    show_zero_vector,
                    false
                );
            }

            using (Stream st = new MemoryStream())
            {
                image.Save(st, ImageFormat.Bmp);
                st.Seek(0, SeekOrigin.Begin);
                var data = BitmapFrame.Create(st, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                view_model.hexagon = data;
            }
        }
    }

    public enum RealTime_Hexagon_Style
    {
        Original
    }
}
