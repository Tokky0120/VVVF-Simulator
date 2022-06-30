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

namespace VVVF_Simulator.GUI.MyUserControl
{
    /// <summary>
    /// EnableButton.xaml の相互作用ロジック
    /// </summary>
    public partial class EnableButton : UserControl
    {
        public EnableButton()
        {
            InitializeComponent();
        }

        private void setImg()
        {
            if (Enabled) StatImg.Source = new BitmapImage(new Uri("pack://application:,,,/GUI/Images/Enable_Button/B_Enabled.png", UriKind.Absolute));
            else StatImg.Source = new BitmapImage(new Uri("pack://application:,,,/GUI/Images/Enable_Button/B_Disabled.png", UriKind.Absolute));
        }

        public event EventHandler? OnClicked;

        private bool Enabled = false;
        private void StatImg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Enabled = !Enabled;
            setImg();

            if (OnClicked != null)
            {
                OnClicked(this, EventArgs.Empty);
            }

            
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            setImg();
        }

        public bool GetEnabled()
        {
            return Enabled;
        }
    }
}
