using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Display;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Drawing.FontFamily;

namespace VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Setting_Window
{
    /// <summary>
    /// RealTime_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_Basic_Settings : Window
    {
        bool no_update = true;
        RealTime_Basic_Setting_Type setting_Type;

        public enum RealTime_Basic_Setting_Type
        {
            VVVF, Train
        }

        public RealTime_Basic_Settings(RealTime_Basic_Setting_Type type)
        {
            setting_Type = type;

            InitializeComponent();

            apply_data();
            DataContext = view_model;

            no_update = false;
        }

        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {

            private bool _Is_Language_Visible = true;
            public bool Is_Language_Visible { get { return _Is_Language_Visible; } set { _Is_Language_Visible = value; RaisePropertyChanged(nameof(Is_Language_Visible)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void apply_data()
        {

            control_design_selector.ItemsSource = (RealTime_ControlStat_Style[])Enum.GetValues(typeof(RealTime_ControlStat_Style));

            hexagon_design_selector.ItemsSource = (RealTime_Hexagon_Style[])Enum.GetValues(typeof(RealTime_Hexagon_Style));

            var prop = Properties.Settings.Default;
            if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
            {
                audio_buff_box.Text = prop.RealTime_VVVF_BuffSize.ToString();
                show_waveform_box.IsChecked = prop.RealTime_VVVF_WaveForm_Show;
                show_fft_box.IsChecked = prop.RealTime_VVVF_FFT_Show;
                realtime_edit_box.IsChecked = prop.RealTime_VVVF_EditAllow;

                show_control_stat.IsChecked = prop.RealTime_VVVF_Control_Show;
                control_design_selector.SelectedItem = (RealTime_ControlStat_Style)prop.RealTime_VVVF_Control_Style;

                show_hexagon.IsChecked = prop.RealTime_VVVF_Hexagon_Show;
                hexagon_design_selector.SelectedItem = (RealTime_Hexagon_Style)prop.RealTime_VVVF_Hexagon_Style;
                show_hexagon_zero_vector_check.IsChecked = prop.RealTime_VVVF_Hexagon_ZeroVector;


            }
            else
            {
                audio_buff_box.Text = prop.RealTime_Train_BuffSize.ToString();
                show_waveform_box.IsChecked = prop.RealTime_Train_WaveForm_Show;
                show_fft_box.IsChecked = prop.RealTime_Train_FFT_Show;
                realtime_edit_box.IsChecked = prop.RealTime_Train_EditAllow;

                show_control_stat.IsChecked = prop.RealTime_Train_Control_Show;
                control_design_selector.SelectedItem = (RealTime_ControlStat_Style)prop.RealTime_Train_Control_Style;

                show_hexagon.IsChecked = prop.RealTime_Train_Hexagon_Show;
                hexagon_design_selector.SelectedItem = (RealTime_Hexagon_Style)prop.RealTime_Train_Hexagon_Style;
                show_hexagon_zero_vector_check.IsChecked = prop.RealTime_Train_Hexagon_ZeroVector;
            }

        }
        private int parse_i(TextBox tb)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                return Int32.Parse(tb.Text);
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        private void box_Checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;
            CheckBox cb = (CheckBox)sender;
            Object tag = cb.Tag;

            Boolean is_checked = cb.IsChecked == true;

            if (tag.Equals("WaveForm"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_WaveForm_Show = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_WaveForm_Show = is_checked;
            }
                
            else if (tag.Equals("Edit"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_EditAllow = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_EditAllow = is_checked;
            }

            else if (tag.Equals("Control"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Show = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Show = is_checked;
            }

            else if (tag.Equals("Hexagon"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Show = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Show = is_checked;
            }
            else if (tag.Equals("HexagonZero"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector = is_checked;
            }
            else if (tag.Equals("FFT"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_FFT_Show = is_checked;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_FFT_Show = is_checked;
            }

        }

        private void audio_buff_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;


            int i = parse_i(audio_buff_box);
            if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                Properties.Settings.Default.RealTime_VVVF_BuffSize = i;
            else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                Properties.Settings.Default.RealTime_Train_BuffSize = i;

            
        }

        private void selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;
            if (tag.Equals("ControlDesign"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Style = (int)cb.SelectedItem;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Style = (int)cb.SelectedItem;
            }else if (tag.Equals("Language"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Language = (int)cb.SelectedItem;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Language = (int)cb.SelectedItem;
            }

            else if (tag.Equals("HexagonDesign"))
            {
                if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Style = (int)cb.SelectedItem;
                else if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Style = (int)cb.SelectedItem;

               
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            bool font_check = false;
            if (setting_Type.Equals(RealTime_Basic_Setting_Type.VVVF) && Properties.Settings.Default.RealTime_VVVF_Control_Show) font_check = true;
            if (setting_Type.Equals(RealTime_Basic_Setting_Type.Train) && Properties.Settings.Default.RealTime_Train_Control_Show) font_check = true;
            Properties.Settings.Default.Save();

            if (!font_check) return;
            var selected_style = Properties.Settings.Default.RealTime_VVVF_Control_Style;
            if(selected_style == (int)RealTime_ControlStat_Style.Original)
            {
                try
                {
                    Font[] fonts = new Font[]{
                        new Font(new FontFamily("Fugaz One"), 75, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel), //topic
                    };
                }
                catch
                {
                    MessageBox.Show(
                        "Required font is not installed\r\n\r\n" +
                        "Fugaz One\r\n",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );

                }
            }
            else if(selected_style == (int)RealTime_ControlStat_Style.Original_2)
            {
                try
                {
                    Font value_Font = new(new FontFamily("DSEG14 Modern"), 40, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                    Font unit_font = new(new FontFamily("Fugaz One"), 25, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                }
                catch
                {
                    MessageBox.Show(
                        "Required font is not installed\r\n\r\n" +
                        "Fugaz One\r\n" +
                        "DSEG14 Modern Italic\r\n",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }
            }
        }

        
    }
}
