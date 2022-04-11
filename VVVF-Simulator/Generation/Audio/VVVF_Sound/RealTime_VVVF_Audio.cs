using NAudio.CoreAudioApi;
using NAudio.Wave;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Generate_RealTime_Common;

namespace VVVF_Simulator.Generation.Audio.VVVF_Sound
{
    public class RealTime_VVVF_Audio
    {


        // --------- VVVF SOUND ------------
        private static int RealTime_VVVF_Generation_Calculate(BufferedWaveProvider provider, Yaml_VVVF_Sound_Data sound_data, VVVF_Values control, RealTime_Parameter realTime_Parameter)
        {
            while (true)
            {
                int bufsize = 20;

                int v = RealTime_CheckForFreq(control , realTime_Parameter, bufsize);
                if (v != -1) return v;

                byte[] add = new byte[bufsize];

                for (int i = 0; i < bufsize; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    byte sound_byte = Generate_VVVF_Audio.Get_VVVF_Sound(control, sound_data);

                    add[i] = sound_byte;
                }

                provider.AddSamples(add, 0, bufsize);
                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_VVVF_BuffSize) ;
            }
        }
        public static void RealTime_VVVF_Generation(Yaml_VVVF_Sound_Data ysd, RealTime_Parameter realTime_Parameter)
        {
            realTime_Parameter.quit = false;
            realTime_Parameter.sound_data = ysd;

            VVVF_Values control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            realTime_Parameter.control_values = control;

            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(192000, 8, 1));

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 50);

                wavPlayer.Init(bufferedWaveProvider);
                wavPlayer.Play();



                int stat;
                try
                {
                    stat = RealTime_VVVF_Generation_Calculate(bufferedWaveProvider, ysd, control, realTime_Parameter);
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
