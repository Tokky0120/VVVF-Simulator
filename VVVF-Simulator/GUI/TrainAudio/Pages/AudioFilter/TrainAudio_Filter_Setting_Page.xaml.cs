using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze.Yaml_TrainSound_Data.SoundFilter;

namespace VVVF_Simulator.GUI.TrainAudio.Pages.AudioFilter
{
    /// <summary>
    /// TrainAudio_Filter_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainAudio_Filter_Setting_Page : Page
    {
        Yaml_TrainSound_Data yaml_TrainSound_Data;

        public TrainAudio_Filter_Setting_Page(Yaml_TrainSound_Data train_Harmonic_Data)
        {
            yaml_TrainSound_Data = train_Harmonic_Data;

            InitializeComponent();

            filterType_Selector.ItemsSource = (FilterType[])Enum.GetValues(typeof(FilterType));
            Filter_DataGrid.ItemsSource = yaml_TrainSound_Data.Filteres;

        }
    }
}
