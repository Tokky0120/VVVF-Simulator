using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VVVF_Simulator.Generation.Audio.Generate_Audio_Core.Harmonic_Data;
using static VVVF_Simulator.Generation.NAudio_Filter;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.My_Math;
using NAudio.Dsp;
using static VVVF_Simulator.VVVF_Structs;
using VVVF_Simulator.Yaml.VVVF_Sound;

namespace VVVF_Simulator.Generation.Audio
{
    public class Generate_Audio_Core
    {
        // -------- VVVF SOUND -------------
        public static byte Get_VVVF_Sound(VVVF_Values control,Yaml_VVVF_Sound_Data sound_data)
        {
            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
            Wave_Values value = calculate_values(control, calculated_Values, 0);

            double pwm_value = value.U - value.V;
            byte sound_byte = 0x80;
            if (pwm_value == 2) sound_byte += 0x40;
            else if (pwm_value == 1) sound_byte += 0x20;
            else if (pwm_value == -1) sound_byte -= 0x20;
            else if (pwm_value == -2) sound_byte -= 0x40;

            return sound_byte;
        }

        // -------- TRAIN SOUND --------------
        public class Harmonic_Data
        {
            public double harmonic { get; set; } = 0;
            public Harmonic_Data_Amplitude amplitude { get; set; } = new Harmonic_Data_Amplitude();
            public double disappear { get; set; } = 0;

            public class Harmonic_Data_Amplitude
            {
                public double start { get; set; } = 0;
                public double start_val { get; set; } = 0;
                public double end { get; set; } = 0;
                public double end_val { get; set; } = 0;
                public double min_val { get; set; } = 0;
                public double max_val { get; set; } = 0;
            }

        }

        public static Harmonic_Data[] motor_harmonics = new Harmonic_Data[]
        {
            
            new Harmonic_Data{harmonic = 1, amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x00,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            new Harmonic_Data{harmonic = 3, amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x00,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            new Harmonic_Data{harmonic = 5, amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x00,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            new Harmonic_Data{harmonic = 7, amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x00,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            

        };

        public static double gear1 = 12.5, gear2 = gear1 * 5;
        public static Harmonic_Data[] gear_harmonics = new Harmonic_Data[] {

            new Harmonic_Data{harmonic = gear1, amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x20,end=60,end_val=0x40,min_val=0,max_val=0x40},disappear = 10000},
            new Harmonic_Data{harmonic = gear2,amplitude = new Harmonic_Data_Amplitude{start=0,start_val=0x20,end=60,end_val=0x40,min_val=0,max_val=0x40},disappear = 8800},

        };

        public static BiQuadFilter[,] Get_Filter(float sample_rate)
        {

            BiQuadFilter[,] filters = new BiQuadFilter[,]
            {
                {

                    BiQuadFilter.PeakingEQ(sample_rate,10,0.8f,-20),
                    BiQuadFilter.PeakingEQ(sample_rate,100,0.8f,-2),
                    BiQuadFilter.PeakingEQ(sample_rate,800,0.8f,-4),
                    BiQuadFilter.PeakingEQ(sample_rate,1200,0.8f,3),
                    BiQuadFilter.PeakingEQ(sample_rate,2400,0.8f,3),
                    BiQuadFilter.PeakingEQ(sample_rate,4800,0.8f,-13),
                    BiQuadFilter.PeakingEQ(sample_rate,5000,0.8f,-13),
                    BiQuadFilter.PeakingEQ(sample_rate,9600,0.8f,-13),

                    BiQuadFilter.LowPassFilter(sample_rate,8000,0.1f),
                    

                }            
            };

            return filters;
        }
        public static byte Get_Train_Sound(VVVF_Values control, Yaml_VVVF_Sound_Data sound_data)
        {
            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
            Wave_Values value = calculate_values(control, calculated_Values, 0);

            
            double pwm_sound_val = 0;
            double pwm_value = value.U - value.V;


            if (pwm_value == 2) pwm_sound_val += 0x40;
            else if (pwm_value == 1) pwm_sound_val += 0x20;
            else if (pwm_value == -1) pwm_sound_val -= 0x20;
            else if (pwm_value == -2) pwm_sound_val -= 0x40;

            double sound_val = 0, total_sound_count = 0;

            // MOTOR HARMONICS
            for (int harmonic = 0; harmonic < motor_harmonics.Length; harmonic++)
            {
                Harmonic_Data harmonic_data = motor_harmonics[harmonic];

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = harmonic_data.amplitude.start_val + (harmonic_data.amplitude.end_val - harmonic_data.amplitude.start_val) / (harmonic_data.amplitude.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > harmonic_data.amplitude.max_val) amplitude = harmonic_data.amplitude.max_val;
                if (amplitude < harmonic_data.amplitude.min_val) amplitude = harmonic_data.amplitude.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * amplitude_disappear * (control.get_Control_Frequency() == 0 ? 0 : 1);
                sound_val += Math.Round(sine_val);
                total_sound_count++;
            }

            // GEAR HARMONICS
            for (int harmonic = 0; harmonic < gear_harmonics.Length; harmonic++)
            {
                Harmonic_Data harmonic_data = gear_harmonics[harmonic];

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = harmonic_data.amplitude.start_val + (harmonic_data.amplitude.end_val - harmonic_data.amplitude.start_val) / (harmonic_data.amplitude.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > harmonic_data.amplitude.max_val) amplitude = harmonic_data.amplitude.max_val;
                if (amplitude < harmonic_data.amplitude.min_val) amplitude = harmonic_data.amplitude.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * amplitude_disappear;
                sound_val += Math.Round(sine_val);
                total_sound_count++;
            }

            double[] saw_harmonics = new double[] { 3 };
            for (int harmonic = 0; harmonic < saw_harmonics.Length; harmonic++)
            {
                double saw_val = sin(control.get_Saw_Time() * control.get_Saw_Angle_Freq() * harmonic);
                saw_val *= 0x20 * (control.get_Control_Frequency() == 0 ? 0 : 1);
                sound_val += Math.Round(saw_val);
                total_sound_count++;
            }

            int pre_sound_byte = (int)Math.Round(sound_val / total_sound_count + pwm_sound_val + 0xFF / 2);
            byte sound_byte = (byte)(pre_sound_byte);
            return sound_byte;

        }
    }
}
