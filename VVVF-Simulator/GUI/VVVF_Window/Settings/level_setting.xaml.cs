using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VVVF_Simulator.Yaml.VVVF_Sound;


namespace VVVF_Simulator.VVVF_Window.Settings
{
    /// <summary>
    /// level_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class level_setting : Page
    {
        public level_setting()
        {
            InitializeComponent();

            if (Yaml_VVVF_Manage.current_data.level == 2)
                level_image.Source = new BitmapImage(new Uri("../../Images/VVVF_Settings/2-level.png", UriKind.Relative));
            else
                level_image.Source = new BitmapImage(new Uri("../../Images/VVVF_Settings/3-level.png", UriKind.Relative));

        }

        private void level_button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("2"))
                level_image.Source = new BitmapImage(new Uri("../../Images/VVVF_Settings/2-level.png", UriKind.Relative));
            else
                level_image.Source = new BitmapImage(new Uri("../../Images/VVVF_Settings/3-level.png", UriKind.Relative));

            Yaml_VVVF_Manage.current_data.level = Int32.Parse(tag);
        }
    }
}
