using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string? GetArgValue(string[] args,string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string comp_key = key + "=";
                if (args[i].StartsWith(comp_key))
                {
                    string value = args[i].Replace(comp_key, "");
                    return value;
                }
            }
            return null;
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = e.Args;
            string? test_mode = GetArgValue(args, "test");
            if (test_mode == null) return;

            string? yaml_path = GetArgValue(args, "yaml_path");
            string? export_path = GetArgValue(args, "export_path");

            if (export_path == null)
                return;

            if (yaml_path != null)
                Yaml_VVVF_Manage.load_Yaml(yaml_path);

        }
    }
}
