using Microsoft.Win32;
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

namespace VVVF_Simulator.GUI.Mascon_Window
{
    /// <summary>
    /// Generation_Mascon_Control_Midi.xaml の相互作用ロジック
    /// </summary>
    public partial class Generation_Mascon_Control_Midi : Window
    {

        bool no_update = true;
        string initial_path;
        public Generation_Mascon_Control_Midi(string? initial_path)
        {
            InitializeComponent();
            no_update = false;
            this.initial_path = initial_path == null ? "" : initial_path;
        }

        private double parse_d(TextBox tb, double minimum)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                double d = Double.Parse(tb.Text);
                if (d < minimum) throw new Exception();
                return d;
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return -1;
            }
        }

        private int parse_i(TextBox tb, int minimum)
        {
            try
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFFFFFFF") as Brush;
                int k = Int32.Parse(tb.Text);
                if (k < minimum) throw new Exception();
                return k;
            }
            catch
            {
                tb.Background = new BrushConverter().ConvertFrom("#FFfed0d0") as Brush;
                return 0;
            }
        }

        public class LoadData
        {
            public int track = 1;
            public int priority = 1;
            public double division = 1;
            public String path = "a";
        }
        public LoadData loadData = new();

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Track"))
            {
                int track = parse_i(tb, 1);
                loadData.track = track;
            }
            else if (tag.Equals("Priority"))
            {
                int priority = parse_i(tb, 1);
                loadData.priority = priority;
            }else if (tag.Equals("Division"))
            {
                double d = parse_d(tb, 1);
                loadData.division = d;
            }
        }

        private void Select_Path_Button_Click(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Midi (*.mid)|*.mid|All (*.*)|*.*",
                InitialDirectory = initial_path
            };
            if (dialog.ShowDialog() == false) return;

            String path = dialog.FileName;
            Path_Label.Text = path;
            loadData.path = path;
        }
    }
}
