using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Structs;

namespace VVVF_Simulator.Yaml.VVVF_Sound
{
    public class Yaml_VVVF_Sound_Data
    {
        private static String get_Value(Object? o)
        {
            if (o == null)
                return "null";
            String? str = o.ToString();
            if (str == null)
                return "null";
            else
                return str;
        }
        public int level { get; set; } = 2;
        public Yaml_Mascon_Data mascon_data { get; set; } = new Yaml_Mascon_Data();
        public Yaml_Min_Sine_Freq min_freq { get; set; } = new Yaml_Min_Sine_Freq();
        public List<Yaml_Control_Data> accelerate_pattern { get; set; } = new List<Yaml_Control_Data>();
        public List<Yaml_Control_Data> braking_pattern { get; set; } = new List<Yaml_Control_Data>();

        public override string ToString()
        {
            String final = "[\r\n";
            final += "level : " + get_Value(level) + "\r\n";
            final += "mascon_data : " + get_Value(mascon_data) + "\r\n";
            final += "min_freq : " + get_Value(min_freq) + "\r\n";
            final += "accelerate_pattern : [";

            for (int i = 0; i < accelerate_pattern.Count; i++)
            {
                final += get_Value(accelerate_pattern[i]) + "\r\n";
            }
            final += "]";

            final += "braking_pattern : [";
            for (int i = 0; i < braking_pattern.Count; i++)
            {
                final += get_Value(braking_pattern[i]) + "\r\n";
            }
            final += "]\r\n";
            final += "]";
            return final;
        }

        public class Yaml_Mascon_Data
        {
            public Yaml_Mascon_Data_On_Off braking { get; set; } = new Yaml_Mascon_Data_On_Off();
            public Yaml_Mascon_Data_On_Off accelerating { get; set; } = new Yaml_Mascon_Data_On_Off();

            public override string ToString()
            {
                String final;
                final = "[\r\n";
                final += "braking : " + get_Value(braking) + "\r\n";
                final += "accelerate : " + get_Value(accelerating) + "\r\n";
                final += "]";

                return final;
            }

            public class Yaml_Mascon_Data_On_Off
            {
                public Yaml_Mascon_Data_Single on { get; set; } = new Yaml_Mascon_Data_Single();
                public Yaml_Mascon_Data_Single off { get; set; } = new Yaml_Mascon_Data_Single();

                public override string ToString()
                {
                    String final;
                    final = "[\r\n";
                    final += "on : " + get_Value(on) + "\r\n";
                    final += "off : " + get_Value(off) + "\r\n";
                    final += "]";

                    return final;
                }

                public class Yaml_Mascon_Data_Single
                {
                    public double freq_per_sec { get; set; } = 60;
                    public double control_freq_go_to { get; set; } = 60;

                    public override string ToString()
                    {
                        String re = "[freq_per_sec : " + get_Value(freq_per_sec) + " , " + "control_freq_go_to : " + String.Format("{0:f3}", control_freq_go_to) + "]";
                        return re;
                    }
                }
            }


        }

        public class Yaml_Min_Sine_Freq
        {
            public double accelerate { get; set; } = -1.0;
            public double braking { get; set; } = -1.0;

            public override string ToString()
            {
                String final = "[";
                final += "accelerate : " + String.Format("{0:f3}", accelerate) + ",";
                final += "braking : " + String.Format("{0:f3}", braking);
                final += "]";
                return final;
            }
        }

        public class Yaml_Control_Data
        {
            public double from { get; set; } = -1;
            public double rotate_sine_from { get; set; } = -1;
            public double rotate_sine_below { get; set; } = -1;
            public bool enable_on_free_run { get; set; } = true;
            public bool enable_off_free_run { get; set; } = true;
            public bool enable_normal { get; set; } = true;

            // Null check !
            private Pulse_Mode _pulse_mode = new();
            public Pulse_Mode pulse_Mode
            {
                get { return _pulse_mode; }
                set { if (value != null) _pulse_mode = value; }
            }
            public Yaml_Free_Run_Condition when_freerun { get; set; } = new Yaml_Free_Run_Condition();
            public Yaml_Control_Data_Amplitude_Control amplitude_control { get; set; } = new Yaml_Control_Data_Amplitude_Control();
            public Yaml_Async_Parameter async_data { get; set; } = new Yaml_Async_Parameter();

            public Yaml_Control_Data Clone()
            {
                Yaml_Control_Data clone = (Yaml_Control_Data)MemberwiseClone();

                //Deep copy
                clone.when_freerun = when_freerun.Clone();
                clone.amplitude_control = amplitude_control.Clone();
                clone.async_data = async_data.Clone();
                clone.pulse_Mode = pulse_Mode.Clone();

                return clone;
            }
            public override string ToString()
            {
                String change_line = "\r\n";
                String final = "From : " + String.Format("{0:f3}", from) + change_line;
                final += "rotate_sine_from : " + get_Value(rotate_sine_from) + change_line;
                final += "rotate_sine_below : " + get_Value(rotate_sine_below) + change_line;
                final += "enable_on_free_run : " + get_Value(enable_on_free_run) + change_line;
                final += "enable_off_free_run : " + get_Value(enable_off_free_run) + change_line;
                final += "enable_normal : " + get_Value(enable_normal) + change_line;
                final += "PulseMode : " + get_Value(pulse_Mode) + change_line;
                final += "when_freerun : " + get_Value(when_freerun) + change_line;
                final += "amplitude_control : " + get_Value(amplitude_control) + change_line;
                final += "async_data : " + get_Value(async_data);
                return final;
            }


            public class Yaml_Moving_Value
            {
                public Moving_Value_Type type { get; set; } = Moving_Value_Type.Proportional;
                public double start { get; set; } = 0;
                public double start_value { get; set; } = 0;
                public double end { get; set; } = 1;
                public double end_value { get; set; } = 100;
                public double degree { get; set; } = 2;

                public double curve_rate { get; set; } = 0;

                public enum Moving_Value_Type
                {
                    Proportional, Pow2_Exponential , Inv_Proportional
                }

                public override string ToString()
                {
                    String final = "[";
                    final += "Type : " + type.ToString() + ",";
                    final += "Degree : " + String.Format("{0:f3}", degree) + ",";
                    final += "CurveRate : " + String.Format("{0:f3}", curve_rate) + ",";
                    final += "Start : " + String.Format("{0:f3}", start) + ",";
                    final += "Start_Val : " + String.Format("{0:f3}", start_value) + ",";
                    final += "End : " + String.Format("{0:f3}", end) + ",";
                    final += "End_Val : " + String.Format("{0:f3}", end_value) + "]";
                    return final;
                }

                public Yaml_Moving_Value Clone()
                {
                    Yaml_Moving_Value clone = (Yaml_Moving_Value)MemberwiseClone();
                    return clone;
                }
            }
            public class Yaml_Free_Run_Condition
            {
                public Yaml_Free_Run_Condition_Single on { get; set; } = new Yaml_Free_Run_Condition_Single();
                public Yaml_Free_Run_Condition_Single off { get; set; } = new Yaml_Free_Run_Condition_Single();

                public override string ToString()
                {
                    String re = "[ " +
                             "on : " + get_Value(on) + " , " +
                             "off : " + get_Value(off) + " , " +
                        "]";
                    return re;
                }

                public Yaml_Free_Run_Condition Clone()
                {
                    Yaml_Free_Run_Condition clone = (Yaml_Free_Run_Condition)MemberwiseClone();
                    clone.on = on.Clone();
                    clone.off = off.Clone();
                    return clone;
                }

                public class Yaml_Free_Run_Condition_Single
                {
                    public bool skip { get; set; } = false;
                    public bool stuck_at_here { get; set; } = false;
                    public override string ToString()
                    {
                        String re = "[ " +
                             "skip : " + get_Value(skip) + " , " +
                             "stuck_at_here : " + get_Value(stuck_at_here) + " , " +
                        "]";
                        return re;
                    }

                    public Yaml_Free_Run_Condition_Single Clone()
                    {
                        Yaml_Free_Run_Condition_Single clone = (Yaml_Free_Run_Condition_Single)MemberwiseClone();
                        return clone;
                    }
                }

            }

            public class Yaml_Async_Parameter
            {
                private Yaml_Async_Parameter_Random _random_data = new();
                public Yaml_Async_Parameter_Random random_data { get { return _random_data; } set { if (value != null) _random_data = value; } }

                public Yaml_Async_Parameter_Carrier_Freq carrier_wave_data { get; set; } = new();
                public Yaml_Async_Parameter_Dipolar dipoar_data { get; set; } = new();

                public override string ToString()
                {
                    String re = "[ " +
                             "random_data : " + get_Value(random_data) + " , " +
                             "dipoar_data : " + get_Value(dipoar_data) + " , " +
                             "carrier_wave_data : " + get_Value(carrier_wave_data) +
                        "]";
                    return re;
                }

                public Yaml_Async_Parameter Clone()
                {
                    Yaml_Async_Parameter clone = (Yaml_Async_Parameter)MemberwiseClone();
                    clone.random_data = random_data.Clone();
                    clone.carrier_wave_data = carrier_wave_data.Clone();
                    clone.dipoar_data = dipoar_data.Clone();
                    return clone;
                }

                public class Yaml_Async_Parameter_Random
                {
                    public Yaml_Async_Parameter_Random_Value random_range { get; set; } = new ();
                    public Yaml_Async_Parameter_Random_Value random_interval { get; set; } = new ();

                    public override string ToString()
                    {
                        String final = "[\r\n";
                        final += "Random_Range : " + get_Value(random_range) + "\r\n";
                        final += "Random_Interval : " + get_Value(random_interval) + "\r\n";
                        final += "]";
                        return final;
                    }

                    public Yaml_Async_Parameter_Random Clone()
                    {
                        Yaml_Async_Parameter_Random clone = (Yaml_Async_Parameter_Random)MemberwiseClone();
                        clone.random_range = random_range.Clone();
                        clone.random_interval = random_interval.Clone();
                        return clone;
                    }

                    public class Yaml_Async_Parameter_Random_Value
                    {
                        public Yaml_Async_Parameter_Random_Value_Mode value_mode { get; set; }
                        public double const_value { get; set; } = 0;
                        public Yaml_Moving_Value moving_value { get; set; } = new Yaml_Moving_Value();
                        public override string ToString()
                        {
                            String final = "[\r\n";
                            final += "value_mode : " + value_mode.ToString() + "\r\n";
                            final += "const_value : " + String.Format("{0:f3}", const_value) + "\r\n";
                            final += "moving_value : " + get_Value(moving_value) + "\r\n";
                            final += "]";
                            return final;
                        }

                        public Yaml_Async_Parameter_Random_Value Clone()
                        {
                            Yaml_Async_Parameter_Random_Value clone = (Yaml_Async_Parameter_Random_Value)MemberwiseClone();
                            clone.moving_value = this.moving_value.Clone();
                            return clone;
                        }

                        public enum Yaml_Async_Parameter_Random_Value_Mode
                        {
                            Const, Moving
                        }
                    }


                }
                public class Yaml_Async_Parameter_Carrier_Freq
                {
                    public Yaml_Async_Carrier_Mode carrier_mode { get; set; }
                    public double const_value { get; set; } = -1.0;
                    public Yaml_Moving_Value moving_value { get; set; } = new Yaml_Moving_Value();
                    public Yaml_Async_Parameter_Carrier_Freq_Vibrato vibrato_value { get; set; } = new Yaml_Async_Parameter_Carrier_Freq_Vibrato();
                    public Yaml_Async_Parameter_Carrier_Freq_Table carrier_table_value { get; set; } = new Yaml_Async_Parameter_Carrier_Freq_Table();

                    public override string ToString()
                    {
                        String final = "[\r\n";
                        final += "carrier_mode : " + carrier_mode.ToString() + "\r\n";
                        final += "const_value : " + String.Format("{0:f3}", const_value) + "\r\n";
                        final += "moving_value : " + get_Value(moving_value) + "\r\n";
                        final += "vibrato_value : " + get_Value(vibrato_value) + "\r\n";
                        final += "carrier_table_value : " + get_Value(carrier_table_value) + "\r\n";
                        final += "]";
                        return final;
                    }

                    public Yaml_Async_Parameter_Carrier_Freq Clone()
                    {
                        Yaml_Async_Parameter_Carrier_Freq clone = (Yaml_Async_Parameter_Carrier_Freq)MemberwiseClone();
                        clone.moving_value = moving_value.Clone();
                        clone.vibrato_value = vibrato_value.Clone();
                        clone.carrier_table_value = carrier_table_value.Clone();
                        return clone;
                    }

                    public enum Yaml_Async_Carrier_Mode
                    {
                        Const, Moving, Vibrato, Table
                    }

                    public class Yaml_Async_Parameter_Carrier_Freq_Vibrato
                    {
                        public Yaml_Async_Parameter_Vibrato_Value highest { get; set; } = new();
                        public Yaml_Async_Parameter_Vibrato_Value lowest { get; set; } = new();


                        private Yaml_Async_Parameter_Vibrato_Value _interval = new();
                        public Yaml_Async_Parameter_Vibrato_Value interval { get { return _interval; } set { if (value != null) _interval = value; } }
                       
                        public bool continuous { get; set; } = true;

                        public override string ToString()
                        {
                            String final = "[\r\n";
                            final += "highest : " + get_Value(highest) + "\r\n";
                            final += "lowest : " + get_Value(lowest) + "\r\n";
                            final += "interval : " + get_Value(interval) + "\r\n";
                            final += "]";
                            return final;
                        }

                        public Yaml_Async_Parameter_Carrier_Freq_Vibrato Clone()
                        {
                            Yaml_Async_Parameter_Carrier_Freq_Vibrato clone = (Yaml_Async_Parameter_Carrier_Freq_Vibrato)MemberwiseClone();
                            clone.highest = highest.Clone();
                            clone.lowest = lowest.Clone();
                            clone.interval = interval.Clone();
                            return clone;
                        }

                        public class Yaml_Async_Parameter_Vibrato_Value
                        {
                            public Yaml_Async_Parameter_Vibrato_Mode mode { get; set; } = Yaml_Async_Parameter_Vibrato_Mode.Const;
                            public double const_value { get; set; } = -1;
                            public Yaml_Moving_Value moving_value { get; set; } = new Yaml_Moving_Value();
                            public override string ToString()
                            {
                                String final = "[\r\n";
                                final += "mode : " + mode.ToString() + "\r\n";
                                final += "const_value : " + String.Format("{0:f3}", const_value) + "\r\n";
                                final += "moving_value : " + get_Value(moving_value) + "\r\n";
                                final += "]";
                                return final;
                            }

                            public Yaml_Async_Parameter_Vibrato_Value Clone()
                            {
                                Yaml_Async_Parameter_Vibrato_Value clone = (Yaml_Async_Parameter_Vibrato_Value)MemberwiseClone();
                                clone.moving_value = this.moving_value.Clone();
                                return clone;
                            }

                            public enum Yaml_Async_Parameter_Vibrato_Mode
                            {
                                Const, Moving
                            }
                        }

                    }

                    public class Yaml_Async_Parameter_Carrier_Freq_Table
                    {
                        public List<Yaml_Async_Parameter_Carrier_Freq_Table_Single> carrier_freq_table { get; set; } = new List<Yaml_Async_Parameter_Carrier_Freq_Table_Single>();
                        public class Yaml_Async_Parameter_Carrier_Freq_Table_Single
                        {
                            public double from { get; set; } = -1;
                            public double carrier_freq { get; set; } = 1000;
                            public bool free_run_stuck_here { get; set; } = false;

                            public override string ToString()
                            {
                                String final = "[\r\n";
                                final += "from : " + String.Format("{0:f3}", from) + ",";
                                final += "carrier_freq : " + String.Format("{0:f3}", carrier_freq) + ",";
                                final += "free_run_stuck_here : " + get_Value(free_run_stuck_here) + ",";
                                final += "]";
                                return final;
                            }

                            public Yaml_Async_Parameter_Carrier_Freq_Table_Single Clone()
                            {
                                Yaml_Async_Parameter_Carrier_Freq_Table_Single clone = (Yaml_Async_Parameter_Carrier_Freq_Table_Single)MemberwiseClone();
                                return clone;
                            }
                        }

                        public override string ToString()
                        {
                            String final = "[\r\n";
                            for (int i = 0; i < carrier_freq_table.Count; i++)
                            {
                                final += carrier_freq_table[i].ToString() + "\r\n";
                            }
                            final += "]";
                            return final;
                        }

                        public Yaml_Async_Parameter_Carrier_Freq_Table Clone()
                        {
                            Yaml_Async_Parameter_Carrier_Freq_Table clone = new();
                            for(int i = 0; i < carrier_freq_table.Count; i++)
                            {
                                clone.carrier_freq_table.Add(carrier_freq_table[i].Clone());
                            }
                            return clone;
                        }
                    }
                }
                public class Yaml_Async_Parameter_Dipolar
                {
                    public Yaml_Async_Parameter_Dipolar_Mode value_mode { get; set; } = Yaml_Async_Parameter_Dipolar_Mode.Const;
                    public double const_value { get; set; } = -1;
                    public Yaml_Moving_Value moving_value { get; set; } = new Yaml_Moving_Value();
                    public override string ToString()
                    {
                        String final = "[\r\n";
                        final += "value_mode : " + value_mode.ToString() + "\r\n";
                        final += "const_value : " + String.Format("{0:f3}", const_value) + "\r\n";
                        final += "moving_value : " + get_Value(moving_value) + "\r\n";
                        final += "]";
                        return final;
                    }

                    public Yaml_Async_Parameter_Dipolar Clone()
                    {
                        Yaml_Async_Parameter_Dipolar clone = (Yaml_Async_Parameter_Dipolar)MemberwiseClone();
                        clone.moving_value = this.moving_value.Clone();
                        return clone;
                    }

                    public enum Yaml_Async_Parameter_Dipolar_Mode
                    {
                        Const, Moving
                    }


                }

            }

            public class Yaml_Control_Data_Amplitude_Control
            {
                public Yaml_Control_Data_Amplitude default_data { get; set; } = new Yaml_Control_Data_Amplitude();
                public Yaml_Control_Data_Amplitude_Free_Run free_run_data { get; set; } = new Yaml_Control_Data_Amplitude_Free_Run();

                public override string ToString()
                {
                    String re = "[ " +
                             "default_data : " + get_Value(default_data) + " , " +
                             "free_run_data : " + get_Value(free_run_data) +
                        "]";
                    return re;
                }

                public Yaml_Control_Data_Amplitude_Control Clone()
                {
                    Yaml_Control_Data_Amplitude_Control clone = (Yaml_Control_Data_Amplitude_Control)MemberwiseClone();

                    //Deep copy
                    clone.default_data = default_data.Clone();
                    clone.free_run_data = free_run_data.Clone();

                    return clone;
                }

                public class Yaml_Control_Data_Amplitude_Free_Run
                {
                    public Yaml_Control_Data_Amplitude mascon_on { get; set; } = new Yaml_Control_Data_Amplitude();
                    public Yaml_Control_Data_Amplitude mascon_off { get; set; } = new Yaml_Control_Data_Amplitude();

                    public override string ToString()
                    {
                        String re = "[ " +
                                 "mascon_on : " + get_Value(mascon_on) + " , " +
                                 "mascon_off : " + get_Value(mascon_off) +
                            "]";
                        return re;
                    }

                    public Yaml_Control_Data_Amplitude_Free_Run Clone()
                    {
                        Yaml_Control_Data_Amplitude_Free_Run clone = (Yaml_Control_Data_Amplitude_Free_Run)MemberwiseClone();

                        //Deep copy
                        clone.mascon_on = mascon_on.Clone();
                        clone.mascon_off = mascon_off.Clone();

                        return clone;
                    }

                }
                public class Yaml_Control_Data_Amplitude
                {
                    public Amplitude_Mode mode { get; set; } = Amplitude_Mode.Linear;
                    public Yaml_Control_Data_Amplitude_Single_Parameter parameter { get; set; } = new Yaml_Control_Data_Amplitude_Single_Parameter();

                    public override string ToString()
                    {
                        String re = "[ " +
                                 "Amplitude_Mode : " + get_Value(mode) + " , " +
                                 "parameter : " + get_Value(parameter) +
                            "]";
                        return re;
                    }

                    public Yaml_Control_Data_Amplitude Clone()
                    {
                        Yaml_Control_Data_Amplitude clone = (Yaml_Control_Data_Amplitude)MemberwiseClone();
                        clone.parameter = parameter.Clone();
                        return clone;
                    }


                    public class Yaml_Control_Data_Amplitude_Single_Parameter
                    {
                        public double start_freq { get; set; } = -1;
                        public double start_amp { get; set; } = -1;
                        public double end_freq { get; set; } = -1;
                        public double end_amp { get; set; } = -1;
                        public double curve_change_rate { get; set; } = 0;
                        public double cut_off_amp { get; set; } = -1;
                        public double max_amp { get; set; } = -1;
                        public bool disable_range_limit { get; set; } = false;
                        public double polynomial { get; set; } = 0;


                        public override string ToString()
                        {
                            String re = "[ " +
                                 "start_freq : " + String.Format("{0:f3}", start_freq) + " , " +
                                 "start_amp : " + String.Format("{0:f3}", start_amp) + " , " +
                                 "end_freq : " + String.Format("{0:f3}", end_freq) + " , " +
                                 "end_amp : " + String.Format("{0:f3}", end_amp) + " , " +
                                 "curve_change_rate : " + String.Format("{0:f3}", curve_change_rate) + " , " +
                                 "cut_off_amp : " + String.Format("{0:f3}", cut_off_amp) + " , " +
                                 "max_amp : " + String.Format("{0:f3}", max_amp) + " , " +
                                 "disable_range_limit : " + get_Value(disable_range_limit) + " , " +
                                 "polynomial : " + polynomial.ToString() +

                            "]";
                            return re;
                        }

                        public Yaml_Control_Data_Amplitude_Single_Parameter Clone()
                        {
                            Yaml_Control_Data_Amplitude_Single_Parameter clone = (Yaml_Control_Data_Amplitude_Single_Parameter)MemberwiseClone();
                            return clone;
                        }
                    }
                }
            }
        }
    }
    public static class Yaml_VVVF_Manage
    {
        public static Yaml_VVVF_Sound_Data current_data = new();

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

        public static bool load_Yaml(String path)
        {
            try
            {
                var input = new StreamReader(path, Encoding.UTF8);
                var deserializer = new Deserializer();
                Yaml_VVVF_Sound_Data deserializeObject = deserializer.Deserialize<Yaml_VVVF_Sound_Data>(input);
                Yaml_VVVF_Manage.current_data = deserializeObject;
                input.Close();
                return true;
            }
            catch (YamlException e)
            {
                throw e;
            }
        }

        public static Yaml_VVVF_Sound_Data DeepClone(Yaml_VVVF_Sound_Data src)
        {
            Yaml_VVVF_Sound_Data deserializeObject = new Deserializer().Deserialize<Yaml_VVVF_Sound_Data>(new Serializer().Serialize(src));
            return deserializeObject;
        }
    }
}
