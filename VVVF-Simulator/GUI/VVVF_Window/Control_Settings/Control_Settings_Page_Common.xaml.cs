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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VVVF_Simulator;
using VVVF_Simulator.VVVF_Window.Control_Settings.Basic;
using VVVF_Simulator.VVVF_Window.Control_Settings.Dipolar;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.VVVF_Structs.Pulse_Mode;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data;

namespace VVVF_Simulator.VVVF_Window.Control_Settings
{
    /// <summary>
    /// Level_3_Page_Control_Common_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Setting_Page_Common : Page
    {
        private ViewModel viewModel = new();
        private class ViewModel : ViewModelBase
        {
            private Visibility _basic;
            public Visibility basic { get { return _basic; } set { _basic = value; RaisePropertyChanged(nameof(basic)); } }

            private Visibility _freerun;
            public Visibility freerun { get { return _freerun; } set { _freerun = value; RaisePropertyChanged(nameof(freerun)); } }

            private Visibility _amp_def;
            public Visibility amp_def { get { return _amp_def; } set { _amp_def = value; RaisePropertyChanged(nameof(amp_def)); } }

            private Visibility _amp_free_on;
            public Visibility amp_free_on { get { return _amp_free_on; } set { _amp_free_on = value; RaisePropertyChanged(nameof(amp_free_on)); } }

            private Visibility _amp_free_off;
            public Visibility amp_free_off { get { return _amp_free_off; } set { _amp_free_off = value; RaisePropertyChanged(nameof(amp_free_off)); } }

            private Visibility _dipolar;
            public Visibility dipolar { get { return _dipolar; } set { _dipolar = value; RaisePropertyChanged(nameof(dipolar)); } }

            private Visibility _async;
            public Visibility async { get { return _async; } set { _async = value; RaisePropertyChanged(nameof(async)); } }
        }
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Set_Visibility(Pulse_Mode_Names mode , int level)
        {
            viewModel.basic = Visibility.Visible;
            viewModel.freerun = Visibility.Visible;
            viewModel.amp_def = Visibility.Visible;
            viewModel.amp_free_on = Visibility.Visible;
            viewModel.amp_free_off = Visibility.Visible;

            if (level == 3 && mode == Pulse_Mode_Names.Async)
                viewModel.dipolar = Visibility.Visible;
            else
                viewModel.dipolar = Visibility.Collapsed;

            if (mode == Pulse_Mode_Names.Async)
                viewModel.async = Visibility.Visible;
            else
                viewModel.async = Visibility.Collapsed;
        }

        public Control_Setting_Page_Common(Yaml_Control_Data ycd, MainWindow mainWindow, int level)
        {
            InitializeComponent();
            DataContext = viewModel;
            Set_Visibility(ycd.pulse_Mode.pulse_name, level);

            Control_Basic.Navigate(new Control_Basic(ycd, mainWindow, level));
            Control_When_FreeRun.Navigate(new Control_When_FreeRun(ycd, mainWindow));

            Control_Amplitude_Default.Navigate(new Control_Amplitude(ycd.amplitude_control.default_data, Control_Amplitude_Content.Default, mainWindow));

            Control_Amplitude_FreeRun_On.Navigate(new Control_Amplitude(ycd.amplitude_control.free_run_data.mascon_on, Control_Amplitude_Content.Free_Run_On, mainWindow));
            Control_Amplitude_FreeRun_Off.Navigate(new Control_Amplitude(ycd.amplitude_control.free_run_data.mascon_off, Control_Amplitude_Content.Free_Run_Off, mainWindow));

            Control_Dipolar.Navigate(new Control_Dipolar(ycd, mainWindow));

            Control_Async.Navigate(new Control_Async(ycd, mainWindow));            
        }
    }
}
