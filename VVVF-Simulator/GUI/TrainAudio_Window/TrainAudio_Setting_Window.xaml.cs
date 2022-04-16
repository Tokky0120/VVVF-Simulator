using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VVVF_Simulator.GUI.TrainAudio_Window.Pages;
using VVVF_Simulator.GUI.TrainAudio_Window.Pages.AudioFilter;
using VVVF_Simulator.GUI.TrainAudio_Window.Pages.Gear;
using VVVF_Simulator.GUI.TrainAudio_Window.Pages.Motor;
using YamlDotNet.Core;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator.GUI.TrainAudio_Window
{
    /// <summary>
    /// TrainAudio_Harmonic_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainAudio_Setting_Window : Window
    {
        private Yaml_TrainSound_Data train_Harmonic_Data;
        public TrainAudio_Setting_Window(Yaml_TrainSound_Data thd)
        {
            train_Harmonic_Data = thd;
            InitializeComponent();

        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid btn = (Grid)sender;
            Object tag = btn.Tag;

            String image_path = "GUI/Images/TrainAudio_Settings/save_normal.png";

            if (tag.Equals("GearSound"))
                image_path = "GUI/Images/TrainAudio_Settings/gear_over.png";
            else if(tag.Equals("MotorSound"))
                image_path = "GUI/Images/TrainAudio_Settings/motor_over.png";
            else if(tag.Equals("Filter"))
                image_path = "GUI/Images/TrainAudio_Settings/filter_over.png";
            else if(tag.Equals("Load"))
                image_path = "GUI/Images/TrainAudio_Settings/load_over.png";
            else if(tag.Equals("Save"))
                image_path = "GUI/Images/TrainAudio_Settings/save_over.png";

            image_path = "pack://application:,,,/" + image_path;

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(image_path));
            btn.Background = imageBrush;

        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid btn = (Grid)sender;
            Object tag = btn.Tag;

            String image_path = "GUI/Images/TrainAudio_Settings/save_normal.png";

            if (tag.Equals("GearSound"))
                image_path = "GUI/Images/TrainAudio_Settings/gear_normal.png";
            else if (tag.Equals("MotorSound"))
                image_path = "GUI/Images/TrainAudio_Settings/motor_normal.png";
            else if (tag.Equals("Filter"))
                image_path = "GUI/Images/TrainAudio_Settings/filter_normal.png";
            else if (tag.Equals("Load"))
                image_path = "GUI/Images/TrainAudio_Settings/load_normal.png";
            else if (tag.Equals("Save"))
                image_path = "GUI/Images/TrainAudio_Settings/save_normal.png";

            image_path = "pack://application:,,,/" + image_path;

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(image_path));
            btn.Background = imageBrush;
        }

        private String load_path = "audio_setting.yaml";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Grid btn = (Grid)sender;
            Object tag = btn.Tag;

            if (tag.Equals("GearSound"))
            {
                Setting_Page_Frame.Navigate(new TrainAudio_Gear_Setting_Page(train_Harmonic_Data));
            }
            else if (tag.Equals("MotorSound"))
            {
                Setting_Page_Frame.Navigate(new TrainAudio_MotorSound_Setting_Page(train_Harmonic_Data));
            }
            else if (tag.Equals("Filter"))
            {
                Setting_Page_Frame.Navigate(new TrainAudio_Filter_Setting_Page(train_Harmonic_Data));
            }
            else if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    Setting_Page_Frame.Navigate(null);
                    train_Harmonic_Data = Yaml_TrainSound_Data_Manage.load_Yaml(dialog.FileName);
                    MessageBox.Show("Load OK.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(YamlException ex)
                {
                    String error_message = "";
                    error_message += "Invalid yaml\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                    

                load_path = dialog.FileName;
            }
            else if (tag.Equals("Save"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Path.GetFileName(load_path)
                };
                if (dialog.ShowDialog() == false) return;

                if (Yaml_TrainSound_Data_Manage.save_Yaml(dialog.FileName))
                    MessageBox.Show("Save OK.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
