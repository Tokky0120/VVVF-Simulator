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
using VVVF_Simulator.GUI.MyUserControl;
using static VVVF_Simulator.GUI.MIDIConvert.MIDIConvert_Main;

namespace VVVF_Simulator.GUI.MIDIConvert
{
    /// <summary>
    /// MIDIConvert_Config.xaml の相互作用ロジック
    /// </summary>
    public partial class MIDIConvert_Config : Window
    {
        private Midi_Convert_Config config;
        public MIDIConvert_Config(Midi_Convert_Config config)
        {
            InitializeComponent();
            this.config = config;

            BtnMultiThread.SetEnabled(config.MultiThread);

        }

        private void EnableButton_OnClicked(object sender, EventArgs e)
        {
            EnableButton? enableButton = sender as EnableButton;
            if (enableButton == null) return;
            String? tag = enableButton.Tag.ToString();
            if(tag == null) return;

            if(tag.Equals("MultiThread")) config.MultiThread = enableButton.GetEnabled();
        }
    }
}
