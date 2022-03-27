using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
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

namespace VVVF_Simulator.GUI.Simulator_Window.RealTime_Generation.Setting_Window
{
    /// <summary>
    /// RealTime_Device_Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class RealTime_Device_Setting : Window
    {
        private ViewModel view_model = new ViewModel();
        public class ViewModel : ViewModelBase
        {

            private Visibility _Port_Visibility = Visibility.Visible;
            public Visibility Port_Visibility { get { return _Port_Visibility; } set { _Port_Visibility = value; RaisePropertyChanged(nameof(Port_Visibility)); } }
        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private RealTime_Mascon_Window main;
        public RealTime_Device_Setting(RealTime_Mascon_Window main)
        {
            this.main = main;
            DataContext = view_model;

            InitializeComponent();

            Mode_Selector.ItemsSource = (Device_Mode[])Enum.GetValues(typeof(Device_Mode));
            Mode_Selector.SelectedItem = main.current_mode;
            SetCOMPorts();
            Port_Selector.SelectedItem = main.current_port;
            SetVisibility(main.current_mode);
        }

        public void SetCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Port_Selector.ItemsSource = ports;
        }

        public void SetVisibility(Device_Mode mode)
        {
            if(mode == Device_Mode.KeyBoard)
            {
                view_model.Port_Visibility = Visibility.Hidden;
            }
            else if(mode == Device_Mode.PicoMascon)
            {
                view_model.Port_Visibility = Visibility.Visible;
            }
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("Mode"))
            {
                Device_Mode mode = (Device_Mode)cb.SelectedItem;
                main.current_mode = mode;
                SetVisibility(mode);
            }else if (tag.Equals("Port"))
            {
                string port = (string)cb.SelectedItem;
                main.current_port = port;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            main.Set_Config();
        }
    }

    public enum Device_Mode
    {
        KeyBoard, PicoMascon
    }
}
