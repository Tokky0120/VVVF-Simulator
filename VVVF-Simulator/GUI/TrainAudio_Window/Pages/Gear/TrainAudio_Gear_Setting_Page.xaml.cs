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
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator.GUI.TrainAudio_Window.Pages.Gear
{
    /// <summary>
    /// TrainAudio_Gear_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainAudio_Gear_Setting_Page : Page
    {
        Yaml_TrainSound_Data train_Harmonic_Data;
        public TrainAudio_Gear_Setting_Page(Yaml_TrainSound_Data train_Harmonic_Data)
        {
            this.train_Harmonic_Data = train_Harmonic_Data;
            InitializeComponent();

            Update_ListView();
        }


        private void Update_ListView()
        {
            Gear_Sound_List.ItemsSource = train_Harmonic_Data.Gear_Harmonics;
            Gear_Sound_List.Items.Refresh();

            var item = (Yaml_TrainSound_Data.Harmonic_Data)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new TrainAudio_Harmonic_Setting_Page(item, Gear_Sound_List));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object tag = mi.Header;

            if (tag.Equals("Add"))
            {
                train_Harmonic_Data.Gear_Harmonics.Add(new Yaml_TrainSound_Data.Harmonic_Data());
                Update_ListView();
            }
            else if (tag.Equals("Remove"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                Gear_Edit_Frame.Navigate(null);
                train_Harmonic_Data.Gear_Harmonics.RemoveAt(Gear_Sound_List.SelectedIndex);
                Update_ListView();
            }
            else if (tag.Equals("Copy"))
            {
                if (Gear_Sound_List.SelectedIndex < 0) return;
                Yaml_TrainSound_Data.Harmonic_Data harmonic_Data = (Yaml_TrainSound_Data.Harmonic_Data)Gear_Sound_List.SelectedItem;
                train_Harmonic_Data.Gear_Harmonics.Add(harmonic_Data.Clone());
                Update_ListView();
            }
            else if (tag.Equals("Calculate"))
            {
                TrainAudio_Gear_Get_Window taggw = new(16,101);
                taggw.ShowDialog();
                train_Harmonic_Data.Set_Calculated_Gear_Harmonics(taggw.Gear1, taggw.Gear2);
                Update_ListView();
            }
        }

        private void Gear_Sound_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (Yaml_TrainSound_Data.Harmonic_Data)Gear_Sound_List.SelectedItem;
            if (item == null) return;
            Gear_Edit_Frame.Navigate(new TrainAudio_Harmonic_Setting_Page(item, Gear_Sound_List));
        }
    }
}
