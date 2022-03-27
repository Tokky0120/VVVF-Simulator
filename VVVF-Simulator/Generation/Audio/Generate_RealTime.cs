using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.Generation.Audio.Generate_Audio_Core;
using static VVVF_Simulator.My_Math;
using VVVF_Simulator.Yaml.VVVF_Sound;
using NAudio.Dsp;
using static VVVF_Simulator.Generation.NAudio_Filter;

namespace VVVF_Simulator.Generation.Audio
{
    public class Generate_RealTime
    {
        // ---------- COMMON ---------------
        public static class RealTime_Parameter
        {
            public static double change_amount { get; set; } = 0;
            public static Boolean braking { get; set; } = false;
            public static Boolean quit { get; set; } = false;
            public static Boolean reselect { get; set; } = false;
            public static Boolean free_run { get; set; } = false;

            public static VVVF_Values control_values { get; set; } = new();
            public static Yaml_VVVF_Sound_Data sound_data { get; set; } = new();
        }

        public static int realtime_check_for_freq(VVVF_Values control)
        {
            control.set_Braking(RealTime_Parameter.braking);
            control.set_Mascon_Off(RealTime_Parameter.free_run);

            double change_amo = RealTime_Parameter.change_amount;

            double sin_new_angle_freq = control.get_Sine_Angle_Freq();
            sin_new_angle_freq += change_amo;
            if (sin_new_angle_freq < 0) sin_new_angle_freq = 0;

            if (!control.is_Free_Running())
            {
                if (control.is_Allowed_Sine_Time_Change())
                {
                    if (sin_new_angle_freq != 0)
                    {
                        double amp = control.get_Sine_Angle_Freq() / sin_new_angle_freq;
                        control.multi_Sine_Time(amp);
                    }
                    else
                        control.set_Sine_Time(0);
                }

                control.set_Control_Frequency(control.get_Sine_Freq());
                control.set_Sine_Angle_Freq(sin_new_angle_freq);
            }


            if (RealTime_Parameter.quit) return 0;
            else if (RealTime_Parameter.reselect) return 1;

            if (!control.is_Mascon_Off()) // mascon on
            {
                if (!control.is_Free_Running())
                    control.set_Control_Frequency(control.get_Sine_Freq());
                else
                {
                    double freq_change = control.get_Free_Freq_Change() * 1.0/192000 * 20.0;
                    double final_freq = control.get_Control_Frequency() + freq_change;

                    if (control.get_Sine_Freq() <= final_freq)
                    {
                        control.set_Control_Frequency(control.get_Sine_Freq());
                        control.set_Free_Running(false);
                    }
                    else
                    {
                        control.set_Control_Frequency(final_freq);
                        control.set_Free_Running(true);
                    }
                }
            }
            else
            {
                double freq_change = control.get_Free_Freq_Change() * 1.0 / 192000 * 20.0;
                double final_freq = control.get_Control_Frequency() - freq_change;
                control.set_Control_Frequency(final_freq > 0 ? final_freq : 0);
                control.set_Free_Running(true);
            }

            return -1;
        }

        // --------- VVVF SOUND ------------
        private static int realtime_vvvf_sound_calculate(BufferedWaveProvider provider, Yaml_VVVF_Sound_Data sound_data, VVVF_Values control)
        {
            while (true)
            {
                int v = realtime_check_for_freq(control);
                if (v != -1) return v;

                byte[] add = new byte[20];

                for (int i = 0; i < 20; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    byte sound_byte = Get_VVVF_Sound(control, sound_data);

                    add[i] = sound_byte;
                }


                int bufsize = 20;

                provider.AddSamples(add, 0, bufsize);
                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_VVVF_BuffSize) ;
            }
        }
        public static void realtime_vvvf_sound(Yaml_VVVF_Sound_Data ysd)
        {
            RealTime_Parameter.quit = false;
            RealTime_Parameter.sound_data = ysd;

            VVVF_Values control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            RealTime_Parameter.control_values = control;

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
                    stat = realtime_vvvf_sound_calculate(bufferedWaveProvider, ysd, control);
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


        //---------- TRAIN SOUND --------------
  
        private static int realtime_train_sound_calculate(BufferedWaveProvider provider, Yaml_VVVF_Sound_Data sound_data, VVVF_Values control)
        {
            while (true)
            {
                int v = realtime_check_for_freq(control);
                if (v != -1) return v;

                byte[] add = new byte[20];

                for (int i = 0; i < 20; i++)
                {
                    control.add_Sine_Time(1.0 / 192000.0);
                    control.add_Saw_Time(1.0 / 192000.0);
                    control.Add_Generation_Current_Time(1.0 / 192000.0);

                    add[i] = Get_Train_Sound(control, sound_data);
                }


                int bufsize = 20;

                provider.AddSamples(add, 0, bufsize);
                while (provider.BufferedBytes > Properties.Settings.Default.RealTime_Train_BuffSize) ;
            }
        }
        public static void realtime_train_sound(Yaml_VVVF_Sound_Data ysd)
        {
            RealTime_Parameter.quit = false;
            RealTime_Parameter.sound_data = ysd;

            VVVF_Values control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            RealTime_Parameter.control_values = control;
            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(192000, 8, 1));
                var equalizer = new Equalizer(bufferedWaveProvider.ToSampleProvider(), Get_Filter(192000));

                var mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                IWavePlayer wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 50);

                wavPlayer.Init(equalizer);
                wavPlayer.Play();

                

                int stat;
                try
                {
                    stat = realtime_train_sound_calculate(bufferedWaveProvider, ysd, control);
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
