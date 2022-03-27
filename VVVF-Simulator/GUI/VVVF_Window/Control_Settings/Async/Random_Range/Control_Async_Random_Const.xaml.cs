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
using VVVF_Simulator;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Control_Data.Yaml_Async_Parameter.Yaml_Async_Parameter_Random;

namespace VVVF_Simulator.VVVF_Window.Control_Settings.Async.Random_Range
{
    /// <summary>
    /// Control_Async_Random_Const.xaml の相互作用ロジック
    /// </summary>
    public partial class Control_Async_Random_Const : UserControl
    {
        Yaml_Async_Parameter_Random_Value target;
        bool no_update = true;

        public Control_Async_Random_Const(Yaml_Async_Parameter_Random_Value data)
        {
            target = data;

            DataContext = data;

            InitializeComponent();

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

        private void const_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;

            double v = parse_d((TextBox)sender);
            target.const_value = v;
        }
    }
}
