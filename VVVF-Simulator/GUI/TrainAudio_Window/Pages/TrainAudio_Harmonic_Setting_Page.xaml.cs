using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator.GUI.TrainAudio_Window.Pages
{
    /// <summary>
    /// TrainAudio_Harmonic_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainAudio_Harmonic_Setting_Page : Page
    {
        Yaml_TrainSound_Data.Harmonic_Data Harmonic_Data;
        ListView ListView;
        bool no_update = true;
        public TrainAudio_Harmonic_Setting_Page(Yaml_TrainSound_Data.Harmonic_Data harmonic_Data, ListView listView)
        {
            Harmonic_Data = harmonic_Data;
            ListView = listView;
            InitializeComponent();

            Harmonic.Text = Harmonic_Data.harmonic.ToString();
            Disappear_Frequency.Text = Harmonic_Data.disappear.ToString();
            Range_Start_Frequency.Text = Harmonic_Data.range.start.ToString();
            Range_End_Frequency.Text = Harmonic_Data.range.end.ToString();
            Start_Frequency.Text = Harmonic_Data.amplitude.start.ToString();
            End_Frequency.Text = Harmonic_Data.amplitude.end.ToString();
            Start_Amplitude.Text = Harmonic_Data.amplitude.start_val.ToString();
            End_Amplitude.Text = Harmonic_Data.amplitude.end_val.ToString();
            Max_Amplitude.Text = Harmonic_Data.amplitude.max_val.ToString();
            Min_Amplitude.Text = Harmonic_Data.amplitude.min_val.ToString();

            no_update = false;
        }

        private double parse_d(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return Double.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        public void TextBox_TextChanged(object sender,RoutedEventArgs e)
        {
            if (no_update) return;

            TextBox tb = (TextBox)sender;
            String name = tb.Name;
            double d = parse_d(tb);

            if (ListView != null)
                ListView.Items.Refresh();

            if (name.Equals("Harmonic")) Harmonic_Data.harmonic = d;
            else if (name.Equals("Disappear_Frequency")) Harmonic_Data.disappear = d;
            else if (name.Equals("Range_Start_Frequency")) Harmonic_Data.range.start = d;
            else if (name.Equals("Range_End_Frequency")) Harmonic_Data.range.end = d;
            else if (name.Equals("Start_Frequency")) Harmonic_Data.amplitude.start = d;
            else if (name.Equals("End_Frequency")) Harmonic_Data.amplitude.end = d;
            else if (name.Equals("Start_Amplitude")) Harmonic_Data.amplitude.start_val = d;
            else if (name.Equals("End_Amplitude")) Harmonic_Data.amplitude.end_val = d;
            else if (name.Equals("Max_Amplitude")) Harmonic_Data.amplitude.max_val = d;
            else if (name.Equals("Min_Amplitude")) Harmonic_Data.amplitude.min_val = d;
        }

    }
}
