using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Audio.Train_Sound.Generate_Train_Audio.Train_Harmonic_Data;
using static VVVF_Simulator.Generation.Audio.Train_Sound.Generate_Train_Audio_Filter.NAudio_Filter;
using static VVVF_Simulator.Generation.Motor.Generate_Motor_Core;
using static VVVF_Simulator.My_Math;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;

namespace VVVF_Simulator.Generation.Audio.Train_Sound
{
    public class Generate_Train_Audio
    {
        // -------- TRAIN SOUND --------------

        public static Train_Harmonic_Data default_Train_Harmonic_Data { get; set; } = new Train_Harmonic_Data(192000);
        public class Train_Harmonic_Data
        {
            public int Gear1 = 14;//16;
            public int Gear2 = 99;//101;
            private int FinalSampleFreq = 192000;
            public Harmonic_Data[] Gear_Harmonics = new Harmonic_Data[]
            {
                new Harmonic_Data{harmonic = 16, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 101, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            };
            public Harmonic_Data[] Sine_Harmonics = new Harmonic_Data[]
            {

                new Harmonic_Data{harmonic = 1, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 5, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 7, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x60,min_val=0,max_val=0x60},disappear = 10000},
            };
            public SoundFilter[] Filteres = new SoundFilter[]
            {
                new(SoundFilter.FilterType.LowPassFilter,-1,1000,2)
            };
            public BiQuadFilter[,] NFilteres = new BiQuadFilter[0,0];

            public class SoundFilter
            {
                public FilterType filterType;
                public float Gain;
                public float Frequency;
                public float Q;
                public enum FilterType
                {
                    PeakingEQ, HighPassFilter, LowPassFilter, NotchFilter
                }

                public SoundFilter(FilterType filterType, float gain, float frequency, float q)
                {
                    this.filterType = filterType;
                    this.Gain = gain;
                    this.Frequency = frequency;
                    this.Q = q;
                }

                public SoundFilter Clone()
                {
                    return (SoundFilter)MemberwiseClone();
                }
            }
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

                public Harmonic_Data Clone()
                {
                    return (Harmonic_Data)MemberwiseClone();
                }
            }

            public void Set_NFilteres(int SampleFreq)
            {
                FinalSampleFreq = SampleFreq;
                BiQuadFilter[,] nFilteres = new BiQuadFilter[1, Filteres.Length];
                for(int i = 0; i < Filteres.Length; i++)
                {
                    SoundFilter sf = Filteres[i];
                    BiQuadFilter bqf;
                    switch (sf.filterType)
                    {
                        case SoundFilter.FilterType.PeakingEQ :
                            {
                                bqf = BiQuadFilter.PeakingEQ(SampleFreq, sf.Frequency, sf.Q, sf.Gain);
                                break;
                            }
                        case SoundFilter.FilterType.HighPassFilter:
                            {
                                bqf = BiQuadFilter.HighPassFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                        case SoundFilter.FilterType.LowPassFilter:
                            {
                                bqf = BiQuadFilter.LowPassFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                        default: //case SoundFilter.FilterType.NotchFilter:
                            {
                                bqf = BiQuadFilter.NotchFilter(SampleFreq, sf.Frequency, sf.Q);
                                break;
                            }
                    }
                    nFilteres[0, i] = bqf;
                }

                NFilteres = nFilteres;

            }
            public void Set_Calculated_Gear_Harmonics()
            {
                List<Harmonic_Data> Gear_Harmonics_List = new List<Harmonic_Data>();
                Harmonic_Data.Harmonic_Data_Amplitude hda = new Harmonic_Data.Harmonic_Data_Amplitude { start = 0, start_val = 0x0, end = 20, end_val = 0x60, min_val = 0, max_val = 0x60 };

                double gear_rate = Gear2 / Gear1;

                // basic harmonics
                for (int i = 0; i < 2; i++)
                {
                    int odd = 2 * i + 1;

                    double harmonic_1 = Gear1 * 120 / 4 / 60.0 / odd;
                    double harmonic_2 = Gear2 * 120 / 4 / 60.0 / odd;

                    Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = harmonic_1, amplitude = hda, disappear = 10000 });
                    Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = harmonic_2, amplitude = hda, disappear = 10000 });

                }


                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = Gear1 * 120 / 4 / 60.0 / 3 * gear_rate / 5.0, amplitude = hda, disappear = 10000 });
                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = Gear2 * 120 / 4 / 60.0 / 3 * gear_rate / 5.0, amplitude = hda, disappear = 10000 });


                Gear_Harmonics = Gear_Harmonics_List.ToArray();
            }
            
            public Train_Harmonic_Data Clone()
            {
                var cloned = (Train_Harmonic_Data)MemberwiseClone();

                cloned.Gear_Harmonics = new List<Harmonic_Data>(Gear_Harmonics).ToArray();
                cloned.Sine_Harmonics = new List<Harmonic_Data>(Sine_Harmonics).ToArray();
                cloned.Filteres = new List<SoundFilter>(Filteres).ToArray();
                cloned.Set_NFilteres(cloned.FinalSampleFreq);

                return cloned;
            }

            public Train_Harmonic_Data(int SampleFreq)
            {
                Set_NFilteres(SampleFreq);
                Set_Calculated_Gear_Harmonics();
            }
        }

        public static byte Get_Train_Sound(VVVF_Values control, Yaml_VVVF_Sound_Data sound_data, Motor_Data motor, Train_Harmonic_Data train_Harmonic_Data)
        {

            double pwm_sound_val;
            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
            Wave_Values value = VVVF_Calculate.calculate_values(control, calculated_Values, 0);

            motor.motor_Param.sitamr = control.get_Video_Sine_Freq() * Math.PI * 2 * control.get_Sine_Time();
            motor.AynMotorControler(new Wave_Values() { U = value.W, V = value.V, W = value.U });
            motor.Asyn_Moduleabc();

            pwm_sound_val = motor.motor_Param.Te - (motor.motor_Param.pre_Te + motor.motor_Param.Te) / 2.0;
            pwm_sound_val *= 60;
            //pwm_sound_val += (value.U - value.V) * 2;
            pwm_sound_val = pwm_sound_val * 2 / 3.0;

            double sound_val = 0, total_sound_count = 0;

            // MOTOR HARMONICS
            for (int harmonic = 0; harmonic < train_Harmonic_Data.Sine_Harmonics.Length; harmonic++)
            {
                Harmonic_Data harmonic_data = train_Harmonic_Data.Sine_Harmonics[harmonic];

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

            // GEAR HARMONICS
            Harmonic_Data[] Gear_Harmonics = train_Harmonic_Data.Gear_Harmonics;
            for (int harmonic = 0; harmonic < Gear_Harmonics.Length; harmonic++)
            {
                Harmonic_Data harmonic_data = Gear_Harmonics[harmonic];

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

            int pre_sound_byte = (int)Math.Round(sound_val / total_sound_count / 2.0 + pwm_sound_val * 2 + 0xFF / 2);
            byte sound_byte = (byte)(pre_sound_byte);
            return sound_byte;

        }


        public static void Export_Train_Sound(String output_path, Yaml_VVVF_Sound_Data sound_data, Train_Harmonic_Data train_Harmonic_Data)
        {

            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            int sample_freq = 200000;

            BufferedWaveProvider wave_provider = new BufferedWaveProvider(new WaveFormat(sample_freq, 8, 1));
            wave_provider.BufferLength = 20000;

            Equalizer equalizer = new Equalizer(wave_provider.ToSampleProvider(), train_Harmonic_Data.NFilteres);
            IWaveProvider equal_wave_provider = equalizer.ToWaveProvider();
            WaveFileWriter writer = new WaveFileWriter(temp, equal_wave_provider.WaveFormat);


            bool loop = true;

            var motor = new Motor_Data();
            var motor_Param = motor.motor_Param;
            motor.SIM_SAMPLE_FREQ = sample_freq;
            motor_Param.TL = 0.0;

            

            while (loop)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                byte sound_byte = Get_Train_Sound(control, sound_data, motor , train_Harmonic_Data);

                wave_provider.AddSamples(new byte[] { sound_byte }, 0, 1);

                if (wave_provider.BufferedBytes == wave_provider.BufferLength)
                {
                    byte[] buffer = new byte[wave_provider.BufferedBytes];
                    int bytesRead = equal_wave_provider.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, bytesRead);
                }

                loop = Generate_Common.Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / sample_freq);

            }

            //last 
            if (wave_provider.BufferedBytes > 0)
            {
                byte[] buffer = new byte[wave_provider.BufferedBytes];
                int bytesRead = equal_wave_provider.Read(buffer, 0, buffer.Length);
                writer.Write(buffer, 0, bytesRead);
            }

            writer.Close();


            int outRate = 44800;
            using (var reader = new AudioFileReader(temp))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outRate);
                WaveFileWriter.CreateWaveFile16(output_path, resampler);
            }

            File.Delete(temp);

        }
    }
}
