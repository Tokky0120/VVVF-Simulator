﻿using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static VVVF_Simulator.Generation.Motor.Generate_Motor_Core.Motor_Data;

namespace VVVF_Simulator.Yaml.TrainAudio_Setting
{
    public class Yaml_TrainSound_Analyze
    {
        public class Yaml_TrainSound_Data
        {
            private int FinalSampleFreq { get; set; } = 192000;
            public List<Harmonic_Data> Gear_Harmonics { get; set; } = new List<Harmonic_Data>()
            {
                new Harmonic_Data{harmonic = 14, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 99, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
            };
            public List<Harmonic_Data> Sine_Harmonics { get; set; } = new List<Harmonic_Data>()
            {
                new Harmonic_Data{harmonic = 1, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 5, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
                new Harmonic_Data{harmonic = 7, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude{start=0,start_val=0x10,end=60,end_val=0x30,min_val = 0,max_val=0x60},disappear = 10000},
            };
            public List<SoundFilter> Filteres { get; set; } = new List<SoundFilter>()
            {
                new(SoundFilter.FilterType.HighPassFilter,-1,50,2f),
                new(SoundFilter.FilterType.LowPassFilter,-1,900,2f),
            };

            public Motor_Specification Motor_Specification { get; set; } = new Motor_Specification();

            

            public class SoundFilter
            {
                public FilterType filterType { get; set; }
                public float Gain { get; set; }
                public float Frequency { get; set; }
                public float Q { get; set; }
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

                public SoundFilter() { }

                public SoundFilter Clone()
                {
                    return (SoundFilter)MemberwiseClone();
                }
            }
            public class Harmonic_Data
            {
                public double harmonic { get; set; } = 0;
                public Harmonic_Data_Amplitude amplitude { get; set; } = new();
                public Harmonic_Data_Range range { get; set; } = new();
                public double disappear { get; set; } = 0;

                public class Harmonic_Data_Amplitude
                {
                    public double start { get; set; } = 0;
                    public double start_val { get; set; } = 0;
                    public double end { get; set; } = 0;
                    public double end_val { get; set; } = 0;

                    public double min_val { get; set; } = 0;
                    public double max_val { get; set; } = 0x60;

                    public Harmonic_Data_Amplitude Clone()
                    {
                        return (Harmonic_Data_Amplitude)MemberwiseClone();
                    }
                }

                public class Harmonic_Data_Range
                {
                    public double start { get; set; } = 0;
                    public double end { get; set; } = -1;

                    public Harmonic_Data_Range Clone()
                    {
                        return (Harmonic_Data_Range)MemberwiseClone();
                    }
                }

                public Harmonic_Data Clone()
                {
                    var cloned = (Harmonic_Data)MemberwiseClone();

                    cloned.amplitude = amplitude.Clone();
                    cloned.range = range.Clone();

                    return cloned;
                }
            }


            private BiQuadFilter[,] NFilteres = new BiQuadFilter[0, 0];
            public void Set_NFilteres(int SampleFreq)
            {
                FinalSampleFreq = SampleFreq;
                BiQuadFilter[,] nFilteres = new BiQuadFilter[1, Filteres.Count];
                for (int i = 0; i < Filteres.Count; i++)
                {
                    SoundFilter sf = Filteres[i];
                    BiQuadFilter bqf;
                    switch (sf.filterType)
                    {
                        case SoundFilter.FilterType.PeakingEQ:
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
            public BiQuadFilter[,] Get_NFilters() { return NFilteres; }


            public void Set_Calculated_Gear_Harmonics(int Gear1, int Gear2)
            {
                List<Harmonic_Data> Gear_Harmonics_List = new List<Harmonic_Data>();

                Harmonic_Data.Harmonic_Data_Amplitude amp_Strong = new Harmonic_Data.Harmonic_Data_Amplitude { start = 0, start_val = 0x0, end = 40, end_val = 0x60, min_val = 0, max_val = 0x60 };
                Harmonic_Data.Harmonic_Data_Amplitude amp_Weak = new Harmonic_Data.Harmonic_Data_Amplitude { start = 0, start_val = 0x0, end = 40, end_val = 0x20, min_val = 0, max_val = 0x20 };

                double gear_rate = Gear2 / (double)Gear1;
                double motor_r = 120 / 4 / 60.0;

                // Sound From Gear 1
                // It has `Gear1` amount of teeth.
                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = motor_r * Gear1 * 3, amplitude = amp_Strong, disappear = -1 });
                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = motor_r * Gear1 * 2.5, amplitude = amp_Strong, disappear = -1 });
                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = motor_r * Gear1 * 1, amplitude = amp_Strong, disappear = -1 });

                Gear_Harmonics_List.Add(new Harmonic_Data { harmonic = motor_r * 86 * 2, amplitude = new Harmonic_Data.Harmonic_Data_Amplitude { start = 0, start_val = 0x0, end = 20, end_val = 0x60, min_val = 0, max_val = 0x30 }, disappear = -1 });

                Gear_Harmonics = new List<Harmonic_Data>(Gear_Harmonics_List);
            }
            public Yaml_TrainSound_Data Clone()
            {
                var cloned = (Yaml_TrainSound_Data)MemberwiseClone();

                cloned.Gear_Harmonics = new List<Harmonic_Data>(Gear_Harmonics);
                cloned.Sine_Harmonics = new List<Harmonic_Data>(Sine_Harmonics);
                cloned.Filteres = new List<SoundFilter>(Filteres);
                cloned.Set_NFilteres(cloned.FinalSampleFreq);

                return cloned;
            }
            public Yaml_TrainSound_Data(int SampleFreq, int Gear1, int Gear2)
            {
                Set_NFilteres(SampleFreq);
                Set_Calculated_Gear_Harmonics(Gear1, Gear2);
            }

            public Yaml_TrainSound_Data() { }
        }

        public class Yaml_TrainSound_Data_Manage
        {
            public static Yaml_TrainSound_Data current_data { get; set; } = new Yaml_TrainSound_Data(192000, 16, 101);

            public static bool save_Yaml(String path)
            {
                try
                {
                    using TextWriter writer = File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, current_data);
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static Yaml_TrainSound_Data load_Yaml(String path)
            {
                try
                {
                    var input = new StreamReader(path, Encoding.UTF8);
                    var deserializer = new Deserializer();
                    Yaml_TrainSound_Data deserializeObject = deserializer.Deserialize<Yaml_TrainSound_Data>(input);
                    input.Close();
                    return deserializeObject;
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
