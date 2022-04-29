using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VVVF_Simulator.Yaml.VVVF_Sound;
using VVVF_Simulator.VVVF_Window.Control_Settings;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data;
using VVVF_Simulator.GUI.UtilForm;
using System.ComponentModel;
using System.Media;
using System.Threading.Tasks;
using VVVF_Simulator.GUI.Util_Window;
using VVVF_Simulator.GUI.Mascon_Window;
using VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation;
using VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Display;
using VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Setting_Window;
using YamlDotNet.Core;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;
using VVVF_Simulator.GUI.TrainAudio_Window;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewData view_data = new ViewData();
        public class ViewData : ViewModelBase
        {
            private bool _blocking = false;
            public bool blocking
            {
                get
                {
                    return _blocking;
                }
                set
                {
                    _blocking = value;
                    RaisePropertyChanged(nameof(blocking));
                }
            }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            DataContext = view_data;
            InitializeComponent();

        }

        

        private void setting_button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name.Equals("settings_level"))
                setting_window.Navigate(new Uri("GUI/VVVF_Window/Settings/level_setting.xaml", UriKind.Relative));
            else if(name.Equals("settings_minimum"))
                setting_window.Navigate(new Uri("GUI/VVVF_Window/Settings/minimum_freq_setting.xaml", UriKind.Relative));
            else if(name.Equals("settings_mascon"))
                setting_window.Navigate(new Uri("GUI/VVVF_Window/Settings/mascon_off_setting.xaml", UriKind.Relative));
        }

        private void settings_edit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");

            var list_view = command[0].Equals("accelerate") ? accelerate_settings : brake_settings;
            var settings = command[0].Equals("accelerate") ? Yaml_VVVF_Manage.current_data.accelerate_pattern : Yaml_VVVF_Manage.current_data.braking_pattern;

            if (command[1].Equals("remove"))
            {
                if(list_view.SelectedIndex >= 0)
                    settings.RemoveAt(list_view.SelectedIndex);
            }
                
            else if (command[1].Equals("add"))
                settings.Add(new Yaml_Control_Data());
            else if (command[1].Equals("reset"))
                settings.Clear();

            list_view.Items.Refresh();
        }
        private void settings_load(object sender, RoutedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag.Equals("accelerate"))
            {
                update_Control_List_View();
                accelerate_selected_show();
            }
            else
            {
                update_Control_List_View();
                brake_selected_show();
            }
        }
        private void settings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView btn = (ListView)sender;
            object? tag = btn.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;


            if(tag.Equals("accelerate"))
                accelerate_selected_show();
            else
                brake_selected_show();
        }
       
        private void accelerate_selected_show()
        {
            int selected = accelerate_settings.SelectedIndex;
            if (selected < 0) return;

            Yaml_VVVF_Sound_Data ysd = Yaml_VVVF_Manage.current_data;
            var selected_data = ysd.accelerate_pattern[selected];
            setting_window.Navigate(new Control_Setting_Page_Common(selected_data, this, ysd.level));

        }
        private void brake_selected_show()
        {
            int selected = brake_settings.SelectedIndex;
            if (selected < 0) return;

            Yaml_VVVF_Sound_Data ysd = Yaml_VVVF_Manage.current_data;
            var selected_data = ysd.braking_pattern[selected];
            setting_window.Navigate(new Control_Setting_Page_Common(selected_data, this, ysd.level));
        }

        public void update_Control_List_View()
        {
            accelerate_settings.ItemsSource = Yaml_VVVF_Manage.current_data.accelerate_pattern;
            brake_settings.ItemsSource = Yaml_VVVF_Manage.current_data.braking_pattern;
            accelerate_settings.Items.Refresh();
            brake_settings.Items.Refresh();
        }
        public void update_Control_Showing()
        {
            if (setting_tabs.SelectedIndex == 1)
            {
                accelerate_selected_show();
            }
            else if (setting_tabs.SelectedIndex == 2)
            {
                brake_selected_show();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split(".");
            if (command[0].Equals("brake"))
            {
                if (command[1].Equals("sort"))
                {
                    Yaml_VVVF_Manage.current_data.braking_pattern.Sort((a, b) => Math.Sign(b.from - a.from));
                    update_Control_List_View();
                    brake_selected_show();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = brake_settings.SelectedIndex;
                    if (selected < 0) return;

                    Yaml_VVVF_Sound_Data ysd = Yaml_VVVF_Manage.current_data;
                    var selected_data = ysd.braking_pattern[selected];
                    Yaml_VVVF_Manage.current_data.braking_pattern.Add(selected_data.Clone());
                    update_Control_List_View();
                    brake_selected_show();
                }
            }
            else if (command[0].Equals("accelerate"))
            {
                if (command[1].Equals("sort"))
                {
                    Yaml_VVVF_Manage.current_data.accelerate_pattern.Sort((a, b) => Math.Sign(b.from - a.from));
                    update_Control_List_View();
                    accelerate_selected_show();
                }
                else if (command[1].Equals("copy"))
                {
                    int selected = accelerate_settings.SelectedIndex;
                    if (selected < 0) return;

                    Yaml_VVVF_Sound_Data ysd = Yaml_VVVF_Manage.current_data;
                    Yaml_Control_Data selected_data = ysd.accelerate_pattern[selected];
                    Yaml_VVVF_Manage.current_data.accelerate_pattern.Add(selected_data.Clone());
                    update_Control_List_View();
                    brake_selected_show();
                }
            }
        }



        private String load_path = "";
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    Yaml_VVVF_Manage.load_Yaml(dialog.FileName);
                    MessageBox.Show("Load OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(YamlException ex)
                {
                    String error_message = "";
                    error_message += "Invalid yaml\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                load_path = dialog.FileName;
                update_Control_List_View();
                update_Control_Showing();

            }
            else if (tag.Equals("Save_As"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Path.GetFileName(load_path)
                };

                // ダイアログを表示する
                if (dialog.ShowDialog() == false) return;
                load_path = dialog.FileName;
                if (Yaml_VVVF_Manage.save_Yaml(dialog.FileName))
                    MessageBox.Show("Save OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tag.Equals("Save"))
            {
                String save_path = load_path;
                if(save_path.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = Path.GetFileName(load_path)
                    };

                    // ダイアログを表示する
                    if (dialog.ShowDialog() == false) return;
                    load_path = dialog.FileName;
                    save_path = load_path;
                }
                if (Yaml_VVVF_Manage.save_Yaml(save_path))
                    MessageBox.Show("Save OK.", "Great", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Error occurred on saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static class Generation_Params
        {
            public static List<double> Double_Values = new();
        }
        private void Generation_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            String[] command = tag_str.Split("_");


            view_data.blocking = true;

            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
            
            bool unblock = solve_Command(command, clone);

            if (!unblock) return;
            view_data.blocking = false;
            SystemSounds.Beep.Play();

        }
        private Boolean solve_Command(String[] command, Yaml_VVVF_Sound_Data data)
        {

            if (command[0].Equals("VVVF"))
            {
                if (command[1].Equals("WAV"))
                {
                    var dialog = new SaveFileDialog { Filter = "wav (*.wav)|*.wav" };
                    if (dialog.ShowDialog() == false) return true;
                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Audio.VVVF_Sound.Generate_VVVF_Audio.Export_VVVF_Sound(dialog.FileName, clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }
               
                else if(command[1].Equals("RealTime"))
                {
                    RealTime_Parameter parameter = new();
                    parameter.quit = false;

                    view_data.blocking = true;

                    RealTime_Mascon_Window mascon = new(parameter);
                    mascon.Show();
                    mascon.Start_Task();

                    RealTime_WaveForm_Window? wave_form = null;
                    RealTime_ControlStat_Window? stat_window = null;
                    RealTime_Hexagon_Window? hexagon_window = null;
                    RealTime_FFT_Window? fft_window = null;

                    if (Properties.Settings.Default.RealTime_VVVF_WaveForm_Show)
                    {
                        wave_form = new(parameter);
                        wave_form.Show();
                        wave_form.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Control_Show)
                    {
                        stat_window = new(
                            parameter,
                            (RealTime_ControlStat_Style)Properties.Settings.Default.RealTime_VVVF_Control_Style
                        );
                        stat_window.Show();
                        stat_window.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_Hexagon_Show)
                    {
                        hexagon_window = new(
                            parameter,
                            (RealTime_Hexagon_Style)Properties.Settings.Default.RealTime_VVVF_Hexagon_Style,
                            Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector
                        );
                        hexagon_window.Show();
                        hexagon_window.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_VVVF_FFT_Show)
                    {
                        fft_window = new RealTime_FFT_Window(parameter);
                        fft_window.Show();
                        fft_window.Start_Task();
                    }

                    Task task = Task.Run(() => {
                        try
                        {
                            bool do_clone = !Properties.Settings.Default.RealTime_VVVF_EditAllow;
                            Yaml_VVVF_Sound_Data data;
                            if (do_clone)
                                data = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            else
                                data = Yaml_VVVF_Manage.current_data;
                            Generation.Audio.VVVF_Sound.RealTime_VVVF_Audio.RealTime_VVVF_Generation(data, parameter);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        view_data.blocking = false;
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_VVVF_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    RealTime_Basic_Settings setting = new( RealTime_Basic_Settings.RealTime_Basic_Setting_Type.VVVF );
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Train"))
            {
                if (command[1].Equals("WAV"))
                {
                    
                    var dialog = new SaveFileDialog { Filter = "wav (*.wav)|*.wav" };
                    if (dialog.ShowDialog() == false) return true;

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Yaml_TrainSound_Data trainSound_Data_clone = Yaml_TrainSound_Data_Manage.current_data.Clone();
                            Generation.Audio.Train_Sound.Generate_Train_Audio.Export_Train_Sound(dialog.FileName, clone, trainSound_Data_clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });


                    
                }
                else if (command[1].Equals("RealTime"))
                {
                    RealTime_Parameter parameter = new();
                    parameter.quit = false;

                    RealTime_Mascon_Window mascon = new(parameter);
                    mascon.Show();
                    mascon.Start_Task();

                    RealTime_WaveForm_Window? wave_form = null;
                    RealTime_ControlStat_Window? stat_window = null;
                    RealTime_Hexagon_Window? hexagon_window = null;
                    RealTime_FFT_Window? fft_window = null;

                    if (Properties.Settings.Default.RealTime_Train_WaveForm_Show)
                    {
                        wave_form = new(parameter);
                        wave_form.Show();
                        wave_form.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Control_Show)
                    {
                        stat_window = new(
                            parameter,
                            (RealTime_ControlStat_Style)Properties.Settings.Default.RealTime_Train_Control_Style
                        );
                        stat_window.Show();
                        stat_window.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_Train_Hexagon_Show)
                    {
                        hexagon_window = new(
                            parameter,
                            (RealTime_Hexagon_Style)Properties.Settings.Default.RealTime_Train_Hexagon_Style,
                            Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector
                        );
                        hexagon_window.Show();
                        hexagon_window.Start_Task();
                    }

                    if (Properties.Settings.Default.RealTime_Train_FFT_Show)
                    {
                        fft_window = new RealTime_FFT_Window(parameter);
                        fft_window.Show();
                        fft_window.Start_Task();
                    }
                    

                    view_data.blocking = true;
                    Task task = Task.Run(() => {
                        try
                        {
                            bool do_clone = !Properties.Settings.Default.RealTime_Train_EditAllow;
                            Yaml_VVVF_Sound_Data data;
                            if (do_clone)
                                data = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            else
                                data = Yaml_VVVF_Manage.current_data;
                            Generation.Audio.Train_Sound.RealTime_Train_Audio.RealTime_Train_Generation(data , parameter);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        view_data.blocking = false;
                        SystemSounds.Beep.Play();
                    });
                    return Properties.Settings.Default.RealTime_Train_EditAllow;
                }
                else if (command[1].Equals("Setting"))
                {
                    RealTime_Basic_Settings setting = new( RealTime_Basic_Settings.RealTime_Basic_Setting_Type.Train );
                    setting.ShowDialog();
                }
            }
            else if (command[0].Equals("Control"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;
                if (command[1].Equals("Original"))
                {
                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.Control_Info.Generate_Control_Original.Generate_Control_Original_Video(dialog.FileName, clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

                else if (command[1].Equals("Original2"))
                {
                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.Control_Info.Generate_Control_Original2.Generate_Control_Original2_Video(dialog.FileName, clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

                    
            }
            else if (command[0].Equals("WaveForm"))
            {
                var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                if (dialog.ShowDialog() == false) return true;
                Task task = Task.Run(() => {
                    try
                    {
                        Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                        if (command[1].Equals("Original"))
                            Generation.Video.WaveForm.Generate_WaveForm_UV.Generate_UV_2(dialog.FileName, clone);
                        else if (command[1].Equals("Spaced"))
                            Generation.Video.WaveForm.Generate_WaveForm_UV.Generate_UV_1(dialog.FileName, clone);
                        else if (command[1].Equals("UVW"))
                            Generation.Video.WaveForm.Generate_WaveForm_UVW.generate_wave_UVW(dialog.FileName, clone);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    SystemSounds.Beep.Play();
                });
            }
            else if (command[0].Equals("Hexagon"))
            {
                MessageBoxResult result = MessageBox.Show("Enable zero vector circle?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question);
                bool circle = result == MessageBoxResult.Yes;

                if (command[1].Equals("Original"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.Hexagon.Generate_Hexagon_Original.Generate_Hexagon_Original_Video(dialog.FileName, clone, circle);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }
                else if (command[1].Equals("Explain"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new Double_Ask_Form("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.Hexagon.Generate_Hexagon_Explain.generate_wave_hexagon_explain(dialog.FileName, clone, circle, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }
                else if (command[1].Equals("OriginalImage"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new Double_Ask_Form("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.Hexagon.Generate_Hexagon_Original.Generate_Hexagon_Original_Image(dialog.FileName, clone, circle, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }
                
            }
            else if (command[0].Equals("FFT"))
            {
                if (command[1].Equals("Video"))
                {
                    var dialog = new SaveFileDialog { Filter = "mp4 (*.mp4)|*.mp4" };
                    if (dialog.ShowDialog() == false) return true;

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.FFT.Generate_FFT.Generate_FFT_Video(dialog.FileName, clone);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }else if (command[1].Equals("Image"))
                {
                    var dialog = new SaveFileDialog { Filter = "png (*.png)|*.png" };
                    if (dialog.ShowDialog() == false) return true;

                    Double_Ask_Form double_Ask_Dialog = new Double_Ask_Form("Enter the frequency.");
                    double_Ask_Dialog.ShowDialog();

                    Task task = Task.Run(() => {
                        try
                        {
                            Yaml_VVVF_Sound_Data clone = Yaml_VVVF_Manage.DeepClone(Yaml_VVVF_Manage.current_data);
                            Generation.Video.FFT.Generate_FFT.Generate_FFT_Image(dialog.FileName, clone, Generation_Params.Double_Values[0]);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SystemSounds.Beep.Play();
                    });
                }

            }
            return true;
        }

        private void Window_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("LCalc"))
            {
                Linear_Calculator lc = new Linear_Calculator();
                lc.Show();
            }else if(tag_str.Equals("AccelPattern")){
                view_data.blocking = true;
                Generation_Mascon_Control_Window gmcw = new();
                gmcw.ShowDialog();
                view_data.blocking = false;
            }
            else if (tag_str.Equals("TrainSoundSetting"))
            {
                view_data.blocking = true;
                Yaml_TrainSound_Data _TrainSound_Data = Yaml_TrainSound_Data_Manage.current_data;
                TrainAudio_Setting_Window tahw = new(_TrainSound_Data);
                tahw.ShowDialog();
                view_data.blocking = false;
            }
        }

        private void Util_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            if (tag_str.Equals("AutoVoltage"))
            {
                view_data.blocking = true;
                Task.Run(() =>
                {
                    MessageBox.Show("The settings which is not using `Linear` will be skipped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    bool result = Yaml_VVVF_Util.Auto_Voltage(Yaml_VVVF_Manage.current_data);
                    if(!result)
                        MessageBox.Show("Please check next things.\r\nAll of the amplitude mode are linear.\r\nAccel and Braking has more than 2 settings.\r\nFrom is grater or equal to 0", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    view_data.blocking = false;
                    SystemSounds.Beep.Play();
                });
                
            }else if (tag_str.Equals("FreeRunAmpZero"))
            {
                view_data.blocking = true;
                Task.Run(() =>
                {
                    bool result = Yaml_VVVF_Util.Set_All_FreeRunAmp_Zero(Yaml_VVVF_Manage.current_data);
                    if (!result)
                        MessageBox.Show("Something went wrong.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    view_data.blocking = false;
                    SystemSounds.Beep.Play();
                });
            }

            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
