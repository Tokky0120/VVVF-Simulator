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
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Control_Data;

namespace VVVF_Simulator.GUI.VVVF_Window.Control_Settings.Common
{
    /// <summary>
    /// Control_Moving_Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Moving_Setting : UserControl
    {
        bool no_update = true;
        private ViewModel view_model = new();

        public class ViewModel : ViewModelBase
        {
            private Visibility _Exponential_Visibility = Visibility.Collapsed;
            public Visibility Exponential_Visibility { get { return _Exponential_Visibility; } set { _Exponential_Visibility = value; RaisePropertyChanged(nameof(Exponential_Visibility)); } }

            private Visibility _CurveRate_Visibility = Visibility.Collapsed;
            public Visibility CurveRate_Visibility { get { return _CurveRate_Visibility; } set { _CurveRate_Visibility = value; RaisePropertyChanged(nameof(CurveRate_Visibility)); } }

            public Yaml_Moving_Value Moving_Value { get; set; } = new Yaml_Moving_Value();

        };
        public class ViewModelBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Control_Moving_Setting(Yaml_Moving_Value target)
        {

            view_model.Moving_Value = target;
            DataContext = view_model;

            InitializeComponent();

            Move_Mode_Selector.ItemsSource = (Yaml_Moving_Value.Moving_Value_Type[])Enum.GetValues(typeof(Yaml_Moving_Value.Moving_Value_Type));
            set_Visibility();

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

        private void text_changed(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start"))
                view_model.Moving_Value.start = parse_d(tb);
            else if (tag.Equals("start_val"))
                view_model.Moving_Value.start_value = parse_d(tb);
            else if (tag.Equals("end"))
                view_model.Moving_Value.end = parse_d(tb);
            else if (tag.Equals("end_val"))
                view_model.Moving_Value.end_value = parse_d(tb);
            else if (tag.Equals("degree"))
                view_model.Moving_Value.degree = parse_d(tb);
            else if (tag.Equals("curve_rate"))
                view_model.Moving_Value.curve_rate = parse_d(tb);

        }

        private void Move_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (no_update) return;

            Yaml_Moving_Value.Moving_Value_Type selected = (Yaml_Moving_Value.Moving_Value_Type)Move_Mode_Selector.SelectedItem;
            view_model.Moving_Value.type = selected;
            set_Visibility();


        }

        private void _set_Visibility(int x, Visibility b)
        {
            if (x == 0) view_model.Exponential_Visibility = b;
            else if (x == 1) view_model.CurveRate_Visibility = b;
        }

        private void set_Visibility()
        {
            Yaml_Moving_Value.Moving_Value_Type selected = (Yaml_Moving_Value.Moving_Value_Type)Move_Mode_Selector.SelectedItem;

            Visibility[] visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Collapsed };

            if (selected == Yaml_Moving_Value.Moving_Value_Type.Proportional)
                visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Collapsed };
            else if (selected == Yaml_Moving_Value.Moving_Value_Type.Pow2_Exponential)
                visible_list = new Visibility[2] { Visibility.Visible, Visibility.Collapsed };
            else if(selected == Yaml_Moving_Value.Moving_Value_Type.Inv_Proportional)
                visible_list = new Visibility[2] { Visibility.Collapsed, Visibility.Visible };

            for (int i = 0;i  < visible_list.Length; i++)
            {
                _set_Visibility(i, visible_list[i]);
            }
        }
    }
}
