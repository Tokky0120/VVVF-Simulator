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

namespace VVVF_Simulator.GUI.TrainAudio.Pages.Motor
{
    /// <summary>
    /// TrainAudio_MotorSound_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainAudio_MotorSound_Setting_Page : Page
    {
        Yaml_TrainSound_Data train_Harmonic_Data;
        bool no_update = true;
        public TrainAudio_MotorSound_Setting_Page(Yaml_TrainSound_Data train_Harmonic_Data)
        {
            InitializeComponent();
            this.train_Harmonic_Data = train_Harmonic_Data;

            var motor = train_Harmonic_Data.Motor_Specification;
            SR.Text = motor.R_s.ToString();
            RR.Text = motor.R_r.ToString();
            SI.Text = motor.L_s.ToString();
            RI.Text = motor.L_r.ToString();
            MI.Text = motor.L_m.ToString();
            PL.Text = motor.NP.ToString();
            D.Text = motor.DAMPING.ToString();
            RIM.Text = motor.INERTIA.ToString();
            SF.Text = motor.STATICF.ToString();

            Motor_Harmonics_List.ItemsSource = train_Harmonic_Data.Sine_Harmonics;
            if (train_Harmonic_Data.Sine_Harmonics.Count > 0)
            {
                Motor_Harmonics_List.SelectedIndex = 0;
                Motor_Harmonic_Edit_Frame.Navigate(new TrainAudio_Harmonic_Setting_Page((Yaml_TrainSound_Data.Harmonic_Data)Motor_Harmonics_List.SelectedItem,Motor_Harmonics_List));
            }
                

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
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;

            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;
            double d = parse_d(tb);
            var motor = train_Harmonic_Data.Motor_Specification;
            if (tag.Equals("SR")) motor.R_s = d;
            else if (tag.Equals("RR")) motor.R_r = d;
            else if (tag.Equals("SI")) motor.L_s = d;
            else if (tag.Equals("RI")) motor.L_r = d;
            else if (tag.Equals("MI")) motor.L_m = d;
            else if (tag.Equals("PL")) motor.NP = d;
            else if (tag.Equals("D")) motor.DAMPING = d;
            else if (tag.Equals("RIM")) motor.INERTIA = d;
            else if (tag.Equals("SF")) motor.STATICF = d;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Yaml_TrainSound_Data.Harmonic_Data)Motor_Harmonics_List.SelectedItem;
            if (item == null) return;
            Motor_Harmonic_Edit_Frame.Navigate(new TrainAudio_Harmonic_Setting_Page(item, Motor_Harmonics_List));
        }

        private void Update_ListView()
        {
            Motor_Harmonics_List.ItemsSource = train_Harmonic_Data.Sine_Harmonics;
            Motor_Harmonics_List.Items.Refresh();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object tag = mi.Tag;

            if (tag.Equals("Add"))
            {
                train_Harmonic_Data.Sine_Harmonics.Add(new Yaml_TrainSound_Data.Harmonic_Data());
                Update_ListView();
            }
            else if (tag.Equals("Remove"))
            {
                if (Motor_Harmonics_List.SelectedIndex < 0) return;
                Motor_Harmonic_Edit_Frame.Navigate(null);
                train_Harmonic_Data.Sine_Harmonics.RemoveAt(Motor_Harmonics_List.SelectedIndex);
                Update_ListView();  
            }
            else if (tag.Equals("Clone"))
            {
                if (Motor_Harmonics_List.SelectedIndex < 0) return;
                Yaml_TrainSound_Data.Harmonic_Data harmonic_Data = (Yaml_TrainSound_Data.Harmonic_Data)Motor_Harmonics_List.SelectedItem;
                train_Harmonic_Data.Sine_Harmonics.Add(harmonic_Data.Clone());
                Update_ListView();
            }
        }
    }
}
