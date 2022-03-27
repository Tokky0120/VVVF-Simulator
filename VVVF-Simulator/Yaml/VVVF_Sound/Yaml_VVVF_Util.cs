using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVF_Simulator.Yaml.VVVF_Sound
{
    public class Yaml_VVVF_Util
    {

        private static void Auto_Voltage_Task(Yaml_VVVF_Sound_Data ysd_x,bool brake,int i,int x, double max_freq)
        {
            List<Yaml_VVVF_Sound_Data.Yaml_Control_Data> ysd = brake ? ysd_x.braking_pattern : ysd_x.accelerate_pattern;
            var parameter = ysd[i].amplitude_control.default_data.parameter;

            parameter.disable_range_limit = false;

            double target_freq;
            if (ysd.Count == i + x)
                target_freq = ysd[i].from + 0.1 * x;
            else
                target_freq = ysd[i + x].from - 0.001 * x;
            if (x == 0) parameter.start_freq = target_freq;
            else parameter.end_freq = target_freq;

            parameter.max_amp = -1;
            parameter.cut_off_amp = -1;

            VVVF_Values control = new();
            control.reset_all_variables();
            control.reset_control_variables();
            control.set_Sine_Angle_Freq(target_freq * Math.PI * 2);
            control.set_Control_Frequency(target_freq);
            control.set_Mascon_Off(false);
            control.set_Free_Running(false);
            control.set_Braking(brake);
            control.set_Allowed_Random_Freq_Move(false);

            double desire_voltage = 1.0 / max_freq * target_freq * 100;

            int same_val_continue = 0; double pre_diff = 0;

            int amplitude_seed = -1;
            while(true)
            {
                amplitude_seed++;

                double try_amplitude = amplitude_seed / 1000.0;
                if (x == 0) parameter.start_amp = try_amplitude;
                else parameter.end_amp = try_amplitude;

                if (desire_voltage == 0) return;

                double voltage = Generation.Video.Control_Info.Generate_Control_Common.Get_Voltage_Rate(ysd_x, control, true);
                double diff = desire_voltage - voltage;

                if (Math.Abs(diff - pre_diff) > 2)
                    same_val_continue = 0;
                else
                    same_val_continue++;
                if (same_val_continue > 100) break;
                pre_diff = diff;

                Debug.WriteLine(String.Format("{0:F02},{1:F02},{2:F02},{3},{4},{5},{6}", diff, desire_voltage, voltage, amplitude_seed, i, x, target_freq));
                if (diff < 0.001)
                {
                    amplitude_seed -= 10;
                    if (amplitude_seed < 0)
                        amplitude_seed = 1000;
                }
                else if (diff < 0.2)
                {
                    if (x == 0)
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            var free_run_param = l == 0 ? ysd[i].amplitude_control.free_run_data.mascon_off.parameter : ysd[i].amplitude_control.free_run_data.mascon_on.parameter;
                            free_run_param.disable_range_limit = false;
                            free_run_param.start_amp = try_amplitude;
                            free_run_param.start_freq = target_freq;
                        }
                    }
                    break;
                }
                else
                {
                    if (voltage < 1) continue;
                    if (Math.Abs(diff) < 1) continue;
                    amplitude_seed = (int)Math.Round(desire_voltage / voltage * amplitude_seed);
                }

            }
        }
        public static bool Auto_Voltage(Yaml_VVVF_Sound_Data data)
        {
            var accel = data.accelerate_pattern;
            bool accel_has_settings = accel.Count > 1;
            bool allow_accel = true;
            for (int i = 0; i < accel.Count; i++)
            {
                bool flg_1 = accel[i].amplitude_control.default_data.mode.Equals(VVVF_Calculate.Amplitude_Mode.Linear);
                bool flg_2 = accel[i].from >= 0;
                if (flg_1 && flg_2) continue;
                allow_accel = false;
                break;
            }

            var brake = data.braking_pattern;
            bool brake_has_settings = brake.Count > 1;
            bool allow_brake = true;
            for (int i = 0; i < brake.Count; i++)
            {
                bool flg_1 = brake[i].amplitude_control.default_data.mode.Equals(VVVF_Calculate.Amplitude_Mode.Linear);
                bool flg_2 = brake[i].from >= 0;
                if (flg_1 && flg_2) continue;
                allow_brake = false;
                break;
            }

            if (!accel_has_settings || !allow_accel || !brake_has_settings || !allow_brake) return false;

            accel.Sort((a, b) => Math.Sign(a.from - b.from));
            brake.Sort((a, b) => Math.Sign(a.from - b.from));

            double accel_end_freq = accel[accel.Count - 1].from;
            double brake_end_freq = brake[brake.Count - 1].from;

            List<Task> tasks = new();

            for (int i = 0; i < accel.Count; i++)
            {
                for (int x = 0; x < 2; x++)
                {
                    int _i = i;
                    int _x = x;
                    Task t = Task.Run(() => Auto_Voltage_Task(data, false, _i,_x, accel_end_freq));
                    tasks.Add(t);
                }
            }

            for (int i = 0; i < brake.Count; i++)
            {
                for (int x = 0; x < 2; x++)
                {
                    int _i = i;
                    int _x = x;
                    Task t = Task.Run(() => Auto_Voltage_Task(data, true, _i, _x, brake_end_freq));
                    tasks.Add(t);
                }
            }



            Task.WaitAll(tasks.ToArray());

            accel.Sort((a, b) => Math.Sign(b.from - a.from));
            brake.Sort((a, b) => Math.Sign(b.from - a.from));

            return true;
        }

        public static bool Set_All_FreeRunAmp_Zero(Yaml_VVVF_Sound_Data data)
        {
            var accel = data.accelerate_pattern;
            for(int i = 0; i < accel.Count; i++)
            {
                accel[i].amplitude_control.free_run_data.mascon_off.parameter.start_amp = 0;
                accel[i].amplitude_control.free_run_data.mascon_off.parameter.start_freq = 0;
                accel[i].amplitude_control.free_run_data.mascon_on.parameter.start_amp = 0;
                accel[i].amplitude_control.free_run_data.mascon_on.parameter.start_freq = 0;
            }

            var brake = data.braking_pattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].amplitude_control.free_run_data.mascon_off.parameter.start_amp = 0;
                brake[i].amplitude_control.free_run_data.mascon_off.parameter.start_freq = 0;
                brake[i].amplitude_control.free_run_data.mascon_on.parameter.start_amp = 0;
                brake[i].amplitude_control.free_run_data.mascon_on.parameter.start_freq = 0;
            }

            return true;
        }
    }
}
