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

        private static int GetPointAtNum(double time, Yaml_Mascon_Data_Compiled ymdc)
        {
            List<Yaml_Mascon_Data_Compiled_Point> SelectSource = ymdc.Points;

            if (time < SelectSource.First().StartTime || SelectSource.Last().EndTime < time) return -1;

            int E_L = 0;
            int E_R = SelectSource.Count - 1;
            int Pos = (E_R - E_L) / 2 + E_L;
            while (true)
            {
                bool time_f = SelectSource[Pos].StartTime <= time && time <= SelectSource[Pos].EndTime;
                if (time_f) break;

                if (SelectSource[Pos].StartTime < time)
                    E_L = Pos + 1;
                else if (SelectSource[Pos].StartTime > time)
                    E_R = Pos - 1;

                Pos = (E_R - E_L) / 2 + E_L;
                
            }

            return Pos;
        }
        private static Yaml_Mascon_Data_Compiled_Point GetPointAtData(double time, Yaml_Mascon_Data_Compiled ymdc)
        {
            Yaml_Mascon_Data_Compiled_Point Selected = ymdc.Points[GetPointAtNum(time, ymdc)];
            return Selected;
        }
        private static double GetFreqAt(double time, double initial, Yaml_Mascon_Data_Compiled ymdc)
        {
            Yaml_Mascon_Data_Compiled_Point Selected = GetPointAtData(time,ymdc);

            double A_Frequency = (Selected.EndFrequency - Selected.StartFrequency) / (Selected.EndTime - Selected.StartTime);
            double Frequency = A_Frequency * (time - Selected.StartTime) + Selected.StartFrequency;

            return Frequency + initial;

        }

        public static bool Check_For_Freq_Change(VVVF_Values Control, Yaml_Mascon_Data_Compiled MasconDataCompiled, Yaml_Mascon_Data MasconChangeData, double TimeDelta)
        {
            double ForceOnFrequency;
            bool Braking, IsMasconOn;
            double CurrentTime = Control.Get_Generation_Current_Time();
            List<Yaml_Mascon_Data_Compiled_Point> SelectSource = MasconDataCompiled.Points;
            int DataAt = GetPointAtNum(CurrentTime,MasconDataCompiled);
            if (DataAt < 0) return false;
            Yaml_Mascon_Data_Compiled_Point Target = SelectSource[DataAt];
            Yaml_Mascon_Data_Compiled_Point? NextTarget = DataAt + 1 < SelectSource.Count ? SelectSource[DataAt + 1] : null;
            Yaml_Mascon_Data_Compiled_Point? PreviousTarget = DataAt - 1 >= 0 ? SelectSource[DataAt - 1] : null;

            Braking = !Target.IsAccel();
            IsMasconOn = Target.IsMasconOn;
            ForceOnFrequency = -1;

            if (!IsMasconOn && PreviousTarget != null)
                Braking = !PreviousTarget.IsAccel();

            if (NextTarget != null && Control.is_Free_Running() && NextTarget.IsMasconOn)
            {

                double MasconOnFrequency = GetFreqAt(Target.EndTime, 0, MasconDataCompiled);
                double FreqPerSec, FreqGoto;
                if (!NextTarget.IsAccel())
                {
                    FreqPerSec = MasconChangeData.braking.on.freq_per_sec;
                    FreqGoto = MasconChangeData.braking.on.control_freq_go_to;
                }
                else
                {
                    FreqPerSec = MasconChangeData.accelerating.on.freq_per_sec;
                    FreqGoto = MasconChangeData.accelerating.on.control_freq_go_to;
                }

                double TargetFrequency = MasconOnFrequency > FreqGoto ? FreqGoto : MasconOnFrequency;
                double RequireTime = TargetFrequency / FreqPerSec;
                if (Target.EndTime - RequireTime < Control.Get_Generation_Current_Time())
                {
                    IsMasconOn = true;
                    Braking = !NextTarget.IsAccel();
                    ForceOnFrequency = MasconOnFrequency;
                }
            }

            double NewSineFrequency = GetFreqAt(CurrentTime, 0, MasconDataCompiled);
            if (NewSineFrequency < 0) NewSineFrequency = 0;

            Control.set_Braking(Braking);
            Control.set_Mascon_Off(!IsMasconOn);
            Control.set_Free_Running(Target != null && !Target.IsMasconOn);

            if (!Control.is_Free_Running())
            {
                double SineTimeAmplitude = NewSineFrequency == 0 ? 0 : Control.get_Sine_Freq() / NewSineFrequency;
                Control.set_Sine_Angle_Freq(NewSineFrequency * Math.PI * 2);
                if (Control.is_Allowed_Sine_Time_Change())
                    Control.multi_Sine_Time(SineTimeAmplitude);
            }else if (ForceOnFrequency != -1)
            {
                double SineTimeAmplitude = ForceOnFrequency == 0 ? 0 : Control.get_Sine_Freq() / ForceOnFrequency;
                Control.set_Sine_Angle_Freq(ForceOnFrequency * Math.PI * 2);
                if (Control.is_Allowed_Sine_Time_Change())
                    Control.multi_Sine_Time(SineTimeAmplitude);
            }

            

            //This is also core of controlling. This should never changed.
            if (!Control.is_Mascon_Off()) // mascon on
            {
                if (!Control.is_Free_Running())
                    Control.set_Control_Frequency(Control.get_Sine_Freq());
                else
                {
                    double freq_change = Control.get_Free_Freq_Change() * TimeDelta;
                    double final_freq = Control.get_Control_Frequency() + freq_change;

                    if (Control.get_Sine_Freq() <= final_freq)
                        Control.set_Control_Frequency(Control.get_Sine_Freq());
                    else
                        Control.set_Control_Frequency(final_freq);
                }
            }
            else
            {
                double FreqChange = Control.get_Free_Freq_Change() * TimeDelta;
                double FinalFrequency = Control.get_Control_Frequency() - FreqChange;
                Control.set_Control_Frequency(FinalFrequency > 0 ? FinalFrequency : 0);
            }

            Control.Add_Generation_Current_Time(TimeDelta);

            if (Target == null) return false;
            return true;
        }
    }
}
