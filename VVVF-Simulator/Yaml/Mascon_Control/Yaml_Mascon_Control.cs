using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze.Yaml_Mascon_Data_Compiled;
using Yaml_Mascon_Data = VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Mascon_Data;

namespace VVVF_Simulator.Yaml.Mascon_Control
{
    public class Yaml_Mascon_Control
    {

        private static double Get_Freq_At(double time, double initial, Yaml_Mascon_Data_Compiled ymdc)
        {
            List<Yaml_Mascon_Data_Compiled_Point> SelectSource = ymdc.Points;

            int pos = SelectSource.Count / 2;
            int search = SelectSource.Count / 2;
            while (true)
            {
                search /= 2;
                if (SelectSource[pos].StartTime < time)
                    pos += search;
                else if (SelectSource[pos].StartTime > time)
                    pos -= search;

                if (search == 1)
                    break;
            }

            Yaml_Mascon_Data_Compiled_Point Selected = SelectSource[pos];
            
            bool time_f = Selected.StartTime <= time && time <= Selected.EndTime;

            double A_Frequency = (Selected.EndFrequency - Selected.StartFrequency) / (Selected.EndTime - Selected.StartTime);
            double Frequency = A_Frequency * (time - Selected.StartTime) + Selected.StartFrequency;

            return Frequency + initial;

        }

        public static bool Check_For_Freq_Change(VVVF_Values control, Yaml_Mascon_Data_Compiled ymdc, Yaml_Mascon_Data ysd, double add_time)
        {

            double current_time = control.Get_Generation_Current_Time();
            List<Yaml_Mascon_Data_Compiled_Point> select_source = ymdc.Points;

            Yaml_Mascon_Data_Compiled_Point? target = null;
            double time_temp = 0;
            double force_mascon_on_freq = -1;
            bool braking = false, mascon_on = false;

            for (int i = 0; i < select_source.Count; i++)
            {
                Yaml_Mascon_Data_Compiled_Point search = select_source[i];
                Yaml_Mascon_Data_Compiled_Point? next_search = i + 1 < select_source.Count ? select_source[i + 1] : null;
                Yaml_Mascon_Data_Compiled_Point? pre_search = i - 1 >= 0 ? select_source[i - 1] : null;

                braking = !search.IsAccel();
                mascon_on = search.IsMasconOn;
                force_mascon_on_freq = -1;

                if (!mascon_on && pre_search != null)
                    braking = !pre_search.IsAccel();

                if (next_search != null && control.is_Free_Running() && next_search.IsMasconOn)
                {
                    
                    double mascon_on_finish_freq = Get_Freq_At(time_temp, 0, ymdc);
                    double freq_per_sec,freq_go_to;
                    if (!next_search.IsAccel())
                    {
                        freq_per_sec = ysd.braking.on.freq_per_sec;
                        freq_go_to = ysd.braking.on.control_freq_go_to;
                    }
                    else
                    {
                        freq_per_sec = ysd.accelerating.on.freq_per_sec;
                        freq_go_to = ysd.accelerating.on.control_freq_go_to;
                    }

                    double target_freq = mascon_on_finish_freq > freq_go_to ? freq_go_to : mascon_on_finish_freq;
                    double need_time = target_freq  / freq_per_sec;
                    if (time_temp - need_time < control.Get_Generation_Current_Time())
                    {
                        mascon_on = true;
                        braking = !next_search.IsAccel();
                        force_mascon_on_freq = mascon_on_finish_freq;
                    }
                }

                if (time_temp > current_time)
                {
                    target = search;
                    break;
                }
            }

            double new_sine = Get_Freq_At(current_time, 0, ymdc);
            if (new_sine < 0) new_sine = 0;

            control.set_Braking(braking);
            control.set_Mascon_Off(!mascon_on);
            control.set_Free_Running(target != null && !target.IsMasconOn);

            if (!control.is_Free_Running())
            {
                double amp = new_sine == 0 ? 0 : control.get_Sine_Freq() / new_sine;
                control.set_Sine_Angle_Freq(new_sine * Math.PI * 2);
                if (control.is_Allowed_Sine_Time_Change())
                    control.multi_Sine_Time(amp);
            }else if (force_mascon_on_freq != -1)
            {
                double amp = force_mascon_on_freq == 0 ? 0 : control.get_Sine_Freq() / force_mascon_on_freq;
                control.set_Sine_Angle_Freq(force_mascon_on_freq * Math.PI * 2);
                if (control.is_Allowed_Sine_Time_Change())
                    control.multi_Sine_Time(amp);
            }

            

            //This is also core of controlling. This should never changed.
            if (!control.is_Mascon_Off()) // mascon on
            {
                if (!control.is_Free_Running())
                    control.set_Control_Frequency(control.get_Sine_Freq());
                else
                {
                    double freq_change = control.get_Free_Freq_Change() * add_time;
                    double final_freq = control.get_Control_Frequency() + freq_change;

                    if (control.get_Sine_Freq() <= final_freq)
                        control.set_Control_Frequency(control.get_Sine_Freq());
                    else
                        control.set_Control_Frequency(final_freq);
                }
            }
            else
            {
                double freq_change = control.get_Free_Freq_Change() * add_time;
                double final_freq = control.get_Control_Frequency() - freq_change;
                control.set_Control_Frequency(final_freq > 0 ? final_freq : 0);
            }

            control.Add_Generation_Current_Time(add_time);

            if (target == null) return false;
            return true;
        }
    }
}
