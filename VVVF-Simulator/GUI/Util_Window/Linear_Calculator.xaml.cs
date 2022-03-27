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
using System.Windows.Shapes;

namespace VVVF_Simulator.GUI.Util_Window
{
    /// <summary>
    /// Linear_Calculator.xaml の相互作用ロジック
    /// </summary>
    public partial class Linear_Calculator : Window
    {
        public Linear_Calculator()
        {
            InitializeComponent();

            set_Calc(a_textbox);
            set_Calc(x_textbox);
            set_Calc(b_textbox);

            no_update = false;
        }

        private double a = 0, x = 0, b = 0;
        private bool no_update = true;

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

        private void copy_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, ans_textbox.Text);
            }
            catch { }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            set_Calc(tb);
        }

        private void set_Calc(TextBox tb)
        {
            Object tag = tb.Tag;
            double d = parse_d(tb);
            if (tag.Equals("A")) a = d;
            else if (tag.Equals("X")) x = d;
            else b = d;

            ans_textbox.Text = (a * x + b).ToString();
        }
    }
}
