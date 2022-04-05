using System;
using System.Collections.Generic;
using static VVVF_Simulator.My_Math;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.VVVF_Structs.Pulse_Mode;

namespace VVVF_Simulator
{
    public class VVVF_Values
    {
        public VVVF_Values Clone()
        {
            VVVF_Values clone = (VVVF_Values)MemberwiseClone();

            //Deep copy
            clone.set_Video_Carrier_Freq_Data(clone.get_Video_Carrier_Freq_Data().Clone());
            clone.set_Video_Pulse_Mode(clone.get_Video_Pulse_Mode().Clone());

            return clone;
        }



        // variables for controlling parameters
        private bool brake = false;
        private bool free_run = false;
        private double wave_stat = 0;
        private bool mascon_off = false;
        private double free_freq_change = 0.0;

        private bool allow_sine_time_change = true;
        private bool allow_random_freq_move = true;

        public void reset_control_variables()
        {
            brake = false;
            free_run = false;
            wave_stat = 0;
            mascon_off = false;
            allow_sine_time_change = true;
            allow_random_freq_move = true;
            free_freq_change = 1.0;
        }

        public double get_Control_Frequency() { return wave_stat; }
        public void set_Control_Frequency(double b) { wave_stat = b; }
        public void add_Control_Frequency(double b) { wave_stat += b; }

        public bool is_Mascon_Off() { return mascon_off; }
        public void set_Mascon_Off(bool b) { mascon_off = b; }
        public void toggle_Mascon_Off() { mascon_off = !mascon_off; }

        public bool is_Free_Running() { return free_run; }
        public void set_Free_Running(bool b) { free_run = b; }
        public void toggle_Free_Running() { free_run = !free_run; }

        public bool is_Braking() { return brake; }
        public void set_Braking(bool b) { brake = b; }
        public void toggle_Braking() { brake = !brake; }

        public bool is_Allowed_Sine_Time_Change() { return allow_sine_time_change; }
        public void set_Allowed_Sine_Time_Change(bool b) { allow_sine_time_change = b; }

        public bool is_Allowed_Random_Freq_Move() { return allow_random_freq_move; }
        public void set_Allowed_Random_Freq_Move(bool b) { allow_random_freq_move = b; }

        public double get_Free_Freq_Change() { return free_freq_change; }
        public void set_Free_Freq_Change(double d) { free_freq_change = d; }


        //--- from vvvf wave calculate
        //sin value definitions
        private double sin_angle_freq = 0;
        private double sin_time = 0;
        //saw value definitions
        private double saw_angle_freq = 1050;
        private double saw_time = 0;
        private double pre_saw_random_freq = 0;
        private double random_freq_pre_time = 0;
        private double vibrato_freq_pre_time = 0;

        public void set_Sine_Angle_Freq(double b) { sin_angle_freq = b; }
        public double get_Sine_Angle_Freq() { return sin_angle_freq; }
        public void add_Sine_Angle_Freq(double b) { sin_angle_freq += b; }

        public double get_Sine_Freq() { return sin_angle_freq * M_1_2PI; }

        public void set_Sine_Time(double t) { sin_time = t; }
        public double get_Sine_Time() { return sin_time; }
        public void add_Sine_Time(double t) { sin_time += t; }
        public void multi_Sine_Time(double x) { sin_time *= x; }

        
        public void set_Saw_Angle_Freq(double f) { saw_angle_freq = f; }
        public double get_Saw_Angle_Freq() { return saw_angle_freq; }
        public void add_Saw_Angle_Freq(double f) { saw_angle_freq += f; }

        public void set_Saw_Time(double t) { saw_time = t; }
        public double get_Saw_Time() { return saw_time; }
        public void add_Saw_Time(double t) { saw_time += t; }
        public void multi_Saw_Time(double x) { saw_time *= x; }

        public void set_Pre_Saw_Random_Freq(double f) { pre_saw_random_freq = f; }
        public double get_Pre_Saw_Random_Freq() { return pre_saw_random_freq; }
        

        public void set_Random_Freq_Pre_Time(double i) { random_freq_pre_time = i; }
        public double get_Random_Freq_Pre_Time() { return random_freq_pre_time; }
        public void add_Random_Freq_Pre_Time(double x) { random_freq_pre_time += x; }

        public void set_Vibrato_Freq_Pre_Time(double i) { vibrato_freq_pre_time = i; }
        public double get_Vibrato_Freq_Pre_Time() { return vibrato_freq_pre_time; }
        public void add_Vibrato_Freq_Pre_Time(double x) { vibrato_freq_pre_time += x; }


        public void reset_all_variables()
        {
            sin_angle_freq = 0;
            sin_time = 0;

            saw_angle_freq = 1050;
            saw_time = 0;

            random_freq_pre_time = 0;
            random_freq_pre_time = 0;

            Generation_Current_Time = 0;
        }



        // Values for Video Generation.
        private Pulse_Mode v_pulse_mode { get; set; } = new();
        private double v_sine_amplitude { get; set; }
        private Carrier_Freq v_carrier_freq_data { get; set; } = new Carrier_Freq(0, 0, 0.0005);
        private double v_dipolar { get; set; }

        private double v_sine_freq { get; set; }


        public void set_Video_Pulse_Mode(Pulse_Mode p) { v_pulse_mode = p; }
        public Pulse_Mode get_Video_Pulse_Mode() { return v_pulse_mode; }

        public void set_Video_Sine_Amplitude(double d) { v_sine_amplitude = d; }
        public double get_Video_Sine_Amplitude() { return v_sine_amplitude; }

        public void set_Video_Carrier_Freq_Data(Carrier_Freq c) { v_carrier_freq_data = c; }
        public Carrier_Freq get_Video_Carrier_Freq_Data() { return v_carrier_freq_data; }

        public void set_Video_Dipolar(double d) { v_dipolar = d; }
        public double get_Video_Dipolar() { return v_dipolar; }
        public void set_Video_Sine_Freq(double d) { v_sine_freq = d; }
        public double get_Video_Sine_Freq() { return v_sine_freq; }

        // Values for Check mascon
        private double Generation_Current_Time { get; set; } = 0;
        public void Set_Generation_Current_Time(double d) { Generation_Current_Time = d; }
        public double Get_Generation_Current_Time() { return Generation_Current_Time; }
        public void Add_Generation_Current_Time(double d) { Generation_Current_Time += d; }
    }

    public static class VVVF_Structs
    {
        public class Wave_Values
        {
            public int U = 0;
            public int V = 0;
            public int W = 0;

            public Wave_Values Clone()
            {
                return (Wave_Values)MemberwiseClone();
            }
        };

        public static Wave_Values get_Wave_Values_None()
        {
            Wave_Values wv = new Wave_Values();
            wv.U = 0;
            wv.V = 0;
            wv.W = 0;
            return wv;
        }
        public class Control_Values
        {
            public bool brake;
            public bool mascon_on;
            public bool free_run;
            public double wave_stat;
        }

        public class Carrier_Freq
        {
            public Carrier_Freq Clone()
            {
                return (Carrier_Freq)MemberwiseClone();
            }
            public Carrier_Freq(double base_freq_a, double range_b, double interval_c)
            {
                base_freq = base_freq_a;
                range = range_b;
                interval = interval_c;
            }

            public double base_freq;
            public double range;
            public double interval;
        }

        public class PWM_Calculate_Values
        {
            public Pulse_Mode pulse_mode = new();
            public Carrier_Freq carrier_freq = new Carrier_Freq(100, 0, 0.0005);

            public double dipolar;
            public int level;
            public bool none;

            public double amplitude;
            public double min_sine_freq;

            public PWM_Calculate_Values Clone()
            {
                var clone = (PWM_Calculate_Values)MemberwiseClone();
                clone.carrier_freq = carrier_freq.Clone();
                clone.pulse_mode = pulse_mode.Clone();

                return clone;
            }
        }

        //
        // Pulse Mode Struct
        //
        public class Pulse_Mode
        {
            public Pulse_Mode Clone()
            {
                var x = (Pulse_Mode)MemberwiseClone();
                List<Pulse_Harmonic> clone_pulse_harmonics = new();
                for (int i = 0; i < pulse_harmonics.Count; i++)
                {
                    clone_pulse_harmonics.Add(pulse_harmonics[i].Clone());
                }
                x.pulse_harmonics = clone_pulse_harmonics;
                return x;

            }

            public bool Shift { get; set; } = false;
            public bool Square { get; set; } = false;

            //
            // Pulse Mode Name
            //
            public Pulse_Mode_Names pulse_name { get; set; }
            public enum Pulse_Mode_Names
            {
                Async, P_Wide_3,

                P_1, P_2, P_3, P_4, P_5, P_6, P_7, P_8, P_9, P_10,
                P_11, P_12, P_13, P_14, P_15, P_16, P_17, P_18, P_19, P_20,
                P_21, P_22, P_23, P_24, P_25, P_26, P_27, P_28, P_29, P_30,
                P_31, P_32, P_33, P_34, P_35, P_36, P_37, P_38, P_39, P_40,
                P_41, P_42, P_43, P_44, P_45, P_46, P_47, P_48, P_49, P_50,
                P_51, P_52, P_53, P_54, P_55, P_56, P_57, P_58, P_59, P_60,
                P_61,

                // Current harmonic minimum Pulse width modulation
                CHMP_3, CHMP_Wide_3, CHMP_5, CHMP_Wide_5, CHMP_7, CHMP_Wide_7,
                CHMP_9, CHMP_Wide_9, CHMP_11, CHMP_Wide_11, CHMP_13,
                CHMP_Wide_13, CHMP_15, CHMP_Wide_15,

                // Selective harmonic elimination Pulse width modulation
                SHEP_3, SHEP_5, SHEP_7, SHEP_9, SHEP_11, SHEP_13, SHEP_15
            };

            //
            // Compare Base Wave
            //
            public Base_Wave_Type Base_Wave { get; set; } = Base_Wave_Type.Sine;
            public enum Base_Wave_Type
            {
                Sine, Saw, Modified_Sine_1, Modified_Sine_2, Modified_Saw_1
            }

            //
            // Compare Wave Harmonics
            //
            private List<Pulse_Harmonic> _pulse_harmonics = new();
            public List<Pulse_Harmonic> pulse_harmonics
            {
                set
                {
                    if (value != null) _pulse_harmonics = value;
                }
                get
                {
                    return _pulse_harmonics;
                }
            }
            public class Pulse_Harmonic
            {
                public double harmonic { get; set; } = 3;
                public double amplitude { get; set; } = 0.2;
                public double initial_phase { get; set; } = 0;
                public Pulse_Harmonic_Type type { get; set; } = Pulse_Harmonic_Type.Sine;

                public enum Pulse_Harmonic_Type
                {
                    Sine, Saw, Square
                }

                public Pulse_Harmonic Clone()
                {
                    return (Pulse_Harmonic)MemberwiseClone();
                }
            }

            //
            // Alternative Modes
            //
            public Pulse_Alternative_Mode Alt_Mode { get; set; } = Pulse_Alternative_Mode.Default;
            public enum Pulse_Alternative_Mode
            {
                Default, Alt1
            }
        }

        

        private static int Get_Name_Num(Pulse_Mode_Names mode)
        {
            int[] pulse_list = new int[]
            {
                0, 0,

                1,2,3,4,5,6,7,8,9,10,
                11,12,13,14,15,16,17,18,19,20,
                21,22,23,24,25,26,27,28,29,30,
                31,32,33,34,35,36,37,38,39,40,
                41,42,43,44,45,46,47,48,49,50,
                51,52,53,54,55,56,57,58,59,60,
                61,

                // Current harmonic minimum Pulse width modulation
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,

                // Selective harmonic elimination Pulse width modulation
                0, 0, 0, 0, 0, 0, 0
            };
            return pulse_list[(int)mode];
        }

        public static bool Is_Harmonic_BaseWaveChange_Available(Pulse_Mode mode, int level)
        {
            if (level == 3) return true;

            if (Is_Square_Available(mode, level) && mode.Square) return false;

            bool[] pulse_list = new bool[]
            {
                true, false,

                false,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,

                // Current harmonic minimum Pulse width modulation
                false,false,false,false,false,false,
                false,false,false,false,false,false,
                false,false,false,false,false,

                // Selective harmonic elimination Pulse width modulation
                false,false,false,false,false,false,false,
            };
            return pulse_list[(int)mode.pulse_name];
        }

        public static bool Is_Shifted_Available(Pulse_Mode mode, int level)
        {
            if (level == 3) return true;

            int id = (int)mode.pulse_name;
            bool stat_1 = (id > (int)Pulse_Mode_Names.P_1 && id <= (int)Pulse_Mode_Names.P_61);

            return stat_1;
        }

        public static bool Is_Square_Available(Pulse_Mode mode, int level)
        {
            if (level == 3) return false;

            int id = (int)mode.pulse_name;
            bool stat_1 = (id > (int)Pulse_Mode_Names.P_1 && id <= (int)Pulse_Mode_Names.P_61);

            return stat_1;
        }

        public static int Get_Pulse_Num(Pulse_Mode mode, int level)
        {
            int pulses = Get_Name_Num(mode.pulse_name);
            if (level == 3) return pulses;

            if (mode.Square)
            {
                if (pulses % 2 == 0) pulses = (int)(pulses * 1.5);
                else pulses = (int)((pulses - 1) * 1.5);
            }

            return pulses;
        }
        public static double Get_Pulse_Initial(Pulse_Mode mode, int level)
        {
            if (level == 3) return 0;

            if (mode.Square)
            {
                if (Get_Name_Num(mode.pulse_name) % 2 == 0) return M_PI_2;
                else return 0;
            }

            return 0;
        }

        public static List<Pulse_Alternative_Mode> Get_Avail_Alt_Modes(Pulse_Mode pulse_Mode, int level)
        {
            if(level == 3) // level 3
            {
                if (pulse_Mode.pulse_name == Pulse_Mode_Names.P_1)
                    return new List<Pulse_Alternative_Mode>() { Pulse_Alternative_Mode.Default, Pulse_Alternative_Mode.Alt1 };
            }
            else // level 2
            {
                if (pulse_Mode.pulse_name == Pulse_Mode_Names.CHMP_11)
                    return new List<Pulse_Alternative_Mode>() { Pulse_Alternative_Mode.Default, Pulse_Alternative_Mode.Alt1 };
                if (pulse_Mode.pulse_name == Pulse_Mode_Names.CHMP_13)
                    return new List<Pulse_Alternative_Mode>() { Pulse_Alternative_Mode.Default, Pulse_Alternative_Mode.Alt1 };
                if (pulse_Mode.pulse_name == Pulse_Mode_Names.CHMP_15)
                    return new List<Pulse_Alternative_Mode>() { Pulse_Alternative_Mode.Default, Pulse_Alternative_Mode.Alt1 };
            }

            return new List<Pulse_Alternative_Mode>() { Pulse_Alternative_Mode.Default };
        }
    }
}
