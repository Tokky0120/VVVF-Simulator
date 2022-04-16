using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;
using static VVVF_Simulator.Generation.Audio.Train_Sound.Generate_Train_Audio;
using static VVVF_Simulator.Generation.Audio.Train_Sound.Generate_Train_Audio_Filter.NAudio_Filter;
using static VVVF_Simulator.Generation.Motor.Generate_Motor_Core;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;

namespace VVVF_Simulator.Generation.Audio.Train_Sound
{
    public class RealTime_Train_Audio
    {
        //---------- TRAIN SOUND --------------

        private static int RealTime_Train_Generation_Calculate(BufferedWaveProvider provider, Yaml_VVVF_Sound_Data sound_data, VVVF_Values control, RealTime_Parameter realTime_Parameter)
        {
            while (true)
            {
                int bufsize = 20;

                int v = RealTime_CheckForFreq(control, realTime_Parameter, bufsize);
                if (v != -1) return v;

                byte[] add = new byte[bufsize];

                for (int i = 0; i < bufsize; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    add[i] = Get_Train_Sound(control, sound_data , realTime_Parameter.Motor, realTime_Parameter.Train_Sound_Data);
                }

                provider.AddSamples(add, 0, bufsize);
                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_Train_BuffSize) ;
            }
        }

        public static void RealTime_Train_Generation(Yaml_VVVF_Sound_Data ysd, RealTime_Parameter realTime_Parameter)
        {
            int sample_freq = 192000;
            realTime_Parameter.quit = false;
            realTime_Parameter.sound_data = ysd;

            realTime_Parameter.Motor = new Motor_Data() { 
                SIM_SAMPLE_FREQ = sample_freq ,
                motor_Specification = realTime_Parameter.Train_Sound_Data.Motor_Specification.Clone(),
            };

            Yaml_TrainSound_Data thd = realTime_Parameter.Train_Sound_Data;

            VVVF_Values control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            realTime_Parameter.control_values = control;
            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(sample_freq, 8, 1));
                var equalizer = new Equalizer(bufferedWaveProvider.ToSampleProvider(), thd.Get_NFilters());

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(equalizer);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = RealTime_Train_Generation_Calculate(bufferedWaveProvider, ysd, control, realTime_Parameter);
                }
                catch
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();

                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();

                    throw;
                }

                wavPlayer.Stop();
                wavPlayer.Dispose();

                mmDevice.Dispose();
                bufferedWaveProvider.ClearBuffer();

                if (stat == 0) break;
            }


        }
    }
}
