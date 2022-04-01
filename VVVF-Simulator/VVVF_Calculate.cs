using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.VVVF_Values;
using static VVVF_Simulator.My_Math;
using System;
using static VVVF_Simulator.VVVF_Calculate.Amplitude_Argument;
using System.Threading.Tasks;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.VVVF_Structs.Pulse_Mode;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Control_Data;

namespace VVVF_Simulator
{
	public class VVVF_Calculate
	{
		//
		// Basic Calculation
		//
		public static double Get_Saw(double x)
		{
			double val;
			double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
			if (0 <= fixed_x && fixed_x < M_PI_2)
				val = M_2_PI * fixed_x;
			else if (M_PI_2 <= fixed_x && fixed_x < 3.0 * M_PI_2)
				val = -M_2_PI * fixed_x + 2;
			else
				val = M_2_PI * fixed_x - 4;

			return -val;
		}

		public static double Get_Sine(double x)
		{
			return My_Math.sin(x);
		}

		public static double Get_Square(double x)
        {
			double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
			if (fixed_x / M_PI > 1) return -1;
			return 1;
		}

		public static double Get_Modified_Sine(double x, int level)
        {
			double sine = Get_Sine(x) * level;
			double value = Math.Round(sine) / level;
			return value;
		}

		public static double Get_Modified_Saw(double x, int level)
        {
			double saw = -Get_Saw(x) * level;
			double value = Math.Round(saw) / level;
			return value;
        }

		public static double Get_Modified_Sine_2(double x)
        {
			double sine = Get_Sine(x);
			int D = sine > 0 ? 1 : -1;
			if (Math.Abs(sine) > 0.5) sine = D;

			return sine;
        }

		public static double Get_Sine_Value_With_Harmonic(Pulse_Mode mode,double x,double amplitude)
        {
			double sin_value = 0;

			if (mode.Base_Wave.Equals(Base_Wave_Type.Saw))
				sin_value = -Get_Saw(x);
			else if (mode.Base_Wave.Equals(Base_Wave_Type.Sine))
				sin_value = Get_Sine(x);
			else if (mode.Base_Wave.Equals(Base_Wave_Type.Modified_Sine_1))
				sin_value = Get_Modified_Sine(x, 1);
			else if (mode.Base_Wave.Equals(Base_Wave_Type.Modified_Sine_2))
				sin_value = Get_Modified_Sine(x, 2);
			else if (mode.Base_Wave.Equals(Base_Wave_Type.Modified_Sine_3))
				sin_value = Get_Modified_Sine_2(x);


			for (int i = 0; i < mode.pulse_harmonics.Count; i++)
			{
				Pulse_Harmonic harmonic = mode.pulse_harmonics[i];
				double harmonic_value = 0, harmonic_x = harmonic.harmonic * (x + harmonic.initial_phase);
				if (harmonic.type == Pulse_Harmonic.Pulse_Harmonic_Type.Sine)
					harmonic_value = Get_Sine(harmonic_x);
				else if(harmonic.type == Pulse_Harmonic.Pulse_Harmonic_Type.Saw)
					harmonic_value = -Get_Saw(harmonic_x);
				else if (harmonic.type == Pulse_Harmonic.Pulse_Harmonic_Type.Square)
					harmonic_value = Get_Square(harmonic_x);

				sin_value += harmonic_value * harmonic.amplitude;
			}

			sin_value = sin_value > 1 ? 1 : sin_value < -1 ? -1 : sin_value;

			sin_value *= amplitude;
			return sin_value;
		}

		public static int get_pwm_value(double sin_value, double saw_value)
		{
			if (sin_value - saw_value > 0)
				return 1;
			else
				return 0;
		}

		//
		// Pulse Calculation
		//
		public static int Get_P_Wide_3(double time, double angle_frequency, double initial_phase, double voltage, bool saw_oppose)
		{
			double sin = Get_Sine(time * angle_frequency + initial_phase);
			double saw = Get_Saw(time * angle_frequency + initial_phase);
			if (saw_oppose)
				saw = -saw;
			double pwm = ((sin - saw > 0) ? 1 : -1) * voltage;
			double nega_saw = (saw > 0) ? saw - 1 : saw + 1;
			int gate = get_pwm_value(pwm, nega_saw) * 2;
			return gate;
		}
		public static int Get_P_1_3Level(double x, double voltage)
        {
			double sine = Get_Sine(x);
			int D = sine > 0 ? 1 : -1;
			double voltage_fix = D * (1 - voltage);

			int gate = (D * (sine - voltage_fix) > 0) ? D : 0;
			gate += 1;
			return gate;
        }
		public static int Get_P_with_Saw(double x, double carrier_initial_phase, double voltage, double carrier_mul, bool saw_oppose)
		{
			double carrier_saw = -Get_Saw(carrier_mul * x + carrier_initial_phase);
			double saw = -Get_Saw(x);
			if (saw_oppose)
				saw = -saw;
			double pwm = (saw > 0) ? voltage : -voltage;
			int gate = get_pwm_value(pwm, carrier_saw) * 2;
			return gate;
		}
		public static int Get_P_with_SwitchingAngle(
			double alpha1,
			double alpha2,
			double alpha3,
			double alpha4,
			double alpha5,
			double alpha6,
			double alpha7,
			int flag,
			double time, double sin_angle_frequency, double initial_phase)
		{
			double theta = (initial_phase + time * sin_angle_frequency) - (double)((int)((initial_phase + time * sin_angle_frequency) * M_1_2PI) * M_2PI);

			int PWM_OUT = (((((theta <= alpha2) && (theta >= alpha1)) || ((theta <= alpha4) && (theta >= alpha3)) || ((theta <= alpha6) && (theta >= alpha5)) || ((theta <= M_PI - alpha1) && (theta >= M_PI - alpha2)) || ((theta <= M_PI - alpha3) && (theta >= M_PI - alpha4)) || ((theta <= M_PI - alpha5) && (theta >= M_PI - alpha6))) && ((theta <= M_PI) && (theta >= 0))) || (((theta <= M_PI - alpha7) && (theta >= alpha7)) && ((theta <= M_PI) && (theta >= 0)))) || ((!(((theta <= alpha2 + M_PI) && (theta >= alpha1 + M_PI)) || ((theta <= alpha4 + M_PI) && (theta >= alpha3 + M_PI)) || ((theta <= alpha6 + M_PI) && (theta >= alpha5 + M_PI)) || ((theta <= M_2PI - alpha1) && (theta >= M_2PI - alpha2)) || ((theta <= M_2PI - alpha3) && (theta >= M_2PI - alpha4)) || ((theta <= M_2PI - alpha5) && (theta >= M_2PI - alpha6))) && ((theta <= M_2PI) && (theta >= M_PI))) && !((theta <= M_2PI - alpha7) && (theta >= M_PI + alpha7)) && (theta <= M_2PI) && (theta >= M_PI)) ? 1 : -1;

			int gate = flag == 'A' ? -PWM_OUT + 1 : PWM_OUT + 1;
			return gate;

		}

		//
		// Amplitude Calculation
		//
		public enum Amplitude_Mode
        {
			Linear, Wide_3_Pulse, Inv_Proportional , Exponential,
			Linear_Polynomial,Sine
		}
		public class Amplitude_Argument
        {
			public class General_Amplitude_Argument
			{
				public double min_freq = 0;
				public double min_amp = 0;
				public double max_freq = 0;
				public double max_amp = 0;
				public bool disable_range_limit = true;

				public double current = 0;

				public General_Amplitude_Argument(double Minimum_Freq, double Minimum_Amplitude, double Maximum_Freq, double Maximum_Amplitude, double Current, bool Disable_Range_Limit)
				{
					min_freq = Minimum_Freq;
					max_freq = Maximum_Freq;

					min_amp = Minimum_Amplitude;
					max_amp = Maximum_Amplitude;

					disable_range_limit = Disable_Range_Limit;

					current = Current;
				}

			}

			public class Inv_Proportional_Amplitude_Argument
			{
				public double min_freq = 0;
				public double min_amp = 0;
				public double max_freq = 0;
				public double max_amp = 0;
				public bool disable_range_limit = true;

				public double change_const = 0.43;

				public double current = 0;

				public Inv_Proportional_Amplitude_Argument(double Minimum_Freq, double Minimum_Amplitude, double Maximum_Freq, double Maximum_Amplitude, double Current, double Change_Const, bool Disable_Range_Limit)
				{
					min_freq = Minimum_Freq;
					max_freq = Maximum_Freq;

					min_amp = Minimum_Amplitude;
					max_amp = Maximum_Amplitude;

					disable_range_limit = Disable_Range_Limit;

					change_const = Change_Const;

					current = Current;
				}

			}

			public class Linear_Polynomial_Amplitude_Argument
			{
				public double polynomial = 2;

				public double max_freq = 0;
				public double max_amp = 0;
				public bool disable_range_limit = true;

				public double current = 0;

				public Linear_Polynomial_Amplitude_Argument(double Maximum_Freq, double Maximum_Amplitude, double Polynomial, double Current, bool Disable_Range_Limit)
				{
					max_freq = Maximum_Freq;

					max_amp = Maximum_Amplitude;

					polynomial = Polynomial;

					disable_range_limit = Disable_Range_Limit;

					current = Current;
				}

			}

			public class Exponential_Amplitude_Argument
			{

				public double max_freq = 0;
				public double max_amp = 0;
				public bool disable_range_limit = true;

				public double current = 0;

				public Exponential_Amplitude_Argument(double Maximum_Freq, double Maximum_Amplitude, double Current, bool Disable_Range_Limit)
				{
					max_freq = Maximum_Freq;

					max_amp = Maximum_Amplitude;

					disable_range_limit = Disable_Range_Limit;

					current = Current;
				}

			}

			public class Sine_Amplitude_Argument
			{

				public double max_freq = 0;
				public double max_amp = 0;
				public bool disable_range_limit = true;

				public double current = 0;

				public Sine_Amplitude_Argument(double Maximum_Freq, double Maximum_Amplitude, double Current, bool Disable_Range_Limit)
				{
					max_freq = Maximum_Freq;

					max_amp = Maximum_Amplitude;

					disable_range_limit = Disable_Range_Limit;

					current = Current;
				}

			}
		}

		public static double get_Amplitude(Amplitude_Mode mode , Object arg_o)
        {
			double val = 0;
			if (mode == Amplitude_Mode.Linear)
            {
				General_Amplitude_Argument arg = (General_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current < arg.min_freq) arg.current = arg.min_freq;
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}
				val = (arg.max_amp - arg.min_amp) / (arg.max_freq - arg.min_freq) * (arg.current - arg.min_freq) + arg.min_amp;
			}
				
			else if(mode == Amplitude_Mode.Wide_3_Pulse)
            {
				General_Amplitude_Argument arg = (General_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current < arg.min_freq) arg.current = arg.min_freq;
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}
				val = (0.2 * ((arg.current - arg.min_freq) * ((arg.max_amp - arg.min_amp) / (arg.max_freq - arg.min_freq)) + arg.min_amp)) + 0.8;
			}
				

			else if(mode == Amplitude_Mode.Inv_Proportional)
            {
				Inv_Proportional_Amplitude_Argument arg = (Inv_Proportional_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current < arg.min_freq) arg.current = arg.min_freq;
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}

				double x = get_Amplitude(Amplitude_Mode.Linear, new General_Amplitude_Argument(arg.min_freq, 1 / arg.min_amp, arg.max_freq, 1 / arg.max_amp, arg.current, arg.disable_range_limit));

				double c = -arg.change_const;
				double k = arg.max_amp;
				double l = arg.min_amp;
				double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
				double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));

				//val = 1 / (6.25*x - 2.5) + 0.4;
				val = 1 / (a * x + b) + c;
			}
			else if(mode == Amplitude_Mode.Exponential)
            {
				Exponential_Amplitude_Argument arg = (Exponential_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}

				double t = 1 / arg.max_freq *  Math.Log(arg.max_amp + 1);

				val = Math.Pow(Math.E, t * arg.current) - 1;
			}
			else if(mode == Amplitude_Mode.Linear_Polynomial)
            {
				Linear_Polynomial_Amplitude_Argument arg = (Linear_Polynomial_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}

				val = Math.Pow(arg.current, arg.polynomial) / Math.Pow(arg.max_freq, arg.polynomial) * arg.max_amp;

			}
			else if (mode == Amplitude_Mode.Sine)
			{
				Sine_Amplitude_Argument arg = (Sine_Amplitude_Argument)arg_o;

				if (!arg.disable_range_limit)
				{
					if (arg.current > arg.max_freq) arg.current = arg.max_freq;
				}

				double x = (Math.PI * arg.current) / (2.0 * arg.max_freq);

				val = My_Math.sin(x) * arg.max_amp;
			}


			return val;
		}

		//
		// Carrier Freq Calculation
		//
		private static double get_Random_freq(Carrier_Freq data, VVVF_Values control)
		{
			if (data.range == 0) return data.base_freq;

            if (control.is_Allowed_Random_Freq_Move())
            {
				double random_freq;
				if (control.get_Random_Freq_Pre_Time() == 0 || control.get_Pre_Saw_Random_Freq() == 0)
				{
					int random_v = My_Math.my_random();
					double diff_freq = My_Math.mod_d(random_v, data.range);
					if ((random_v & 0x01) == 1)
						diff_freq = -diff_freq;
					double silent_random_freq = data.base_freq + diff_freq;
					random_freq = silent_random_freq;
					control.set_Pre_Saw_Random_Freq(silent_random_freq);
					control.set_Random_Freq_Pre_Time(control.Get_Generation_Current_Time());
				}
				else
				{
					random_freq = control.get_Pre_Saw_Random_Freq();
				}

				if (control.get_Random_Freq_Pre_Time() + data.interval < control.Get_Generation_Current_Time())
					control.set_Random_Freq_Pre_Time(0);

				return random_freq;
            }
            else
            {
				return data.base_freq;
            }
		}
		public static double get_Vibrato_Freq(double lowest, double highest, double interval_time, bool continuous , VVVF_Values control)
		{

			if (!control.is_Allowed_Random_Freq_Move())
				return (highest + lowest) / 2.0;

			double random_freq;
			double current_t = control.Get_Generation_Current_Time();
			double solve_t = control.get_Vibrato_Freq_Pre_Time();

			if (continuous)
			{
				if (interval_time / 2.0 > current_t - solve_t)
					random_freq = lowest + (highest - lowest) / (interval_time / 2.0) * (current_t - solve_t);
				else
					random_freq = highest + (lowest - highest) / (interval_time / 2.0) * (current_t - solve_t - interval_time / 2.0);
			}
            else
            {
				if (interval_time / 2.0 > current_t - solve_t)
					random_freq = highest;
				else
					random_freq = lowest;
			}

			if (current_t - solve_t > interval_time)
				control.set_Vibrato_Freq_Pre_Time(current_t);
			return random_freq;
        }

		//
		// VVVF Calculation
		//
		public static double check_for_mascon_off(Control_Values cv, VVVF_Values control, double max_voltage_freq)
        {
			if (cv.free_run && !cv.mascon_on && cv.wave_stat > max_voltage_freq)
			{
				control.set_Control_Frequency(max_voltage_freq);
				return max_voltage_freq;
				
			}
			else if (cv.free_run && cv.mascon_on && cv.wave_stat > max_voltage_freq)
			{
				double rolling_freq = control.get_Sine_Freq();
				control.set_Control_Frequency(rolling_freq);
				return rolling_freq;
			}
			return -1;
		}

		public static Wave_Values calculate_values(VVVF_Values control,PWM_Calculate_Values value, double add_initial)
        {

			if (control.get_Sine_Freq() < value.min_sine_freq && control.get_Control_Frequency() > 0) control.set_Video_Sine_Freq(value.min_sine_freq);
			else control.set_Video_Sine_Freq(control.get_Sine_Freq());

			if (value.none) return new Wave_Values() { U = 0, V = 0, W = 0 };

			control.set_Video_Pulse_Mode(value.pulse_mode);
			control.set_Video_Sine_Amplitude(value.amplitude);
			if(value.carrier_freq != null) control.set_Video_Carrier_Freq_Data(value.carrier_freq.Clone());
			control.set_Video_Dipolar(value.dipolar);

			int U=0, V = 0, W = 0;
			for(int i = 0; i < 3; i++)
            {
				
				int val;
				double initial = M_2PI / 3.0 * i + add_initial;
				if (value.level == 2) val = calculate_two_level(control, value, initial);
				else val = calculate_three_level(control, value, initial);
				if (i == 0) U = val;
				else if (i == 1) V = val;
				else W = val;

			}

			return new Wave_Values() { U = U, V = V, W = W };
        }

        public static int calculate_three_level(VVVF_Values control, PWM_Calculate_Values calculate_values, double initial_phase)
		{
			double sine_angle_freq = control.get_Sine_Angle_Freq();
			double sine_time = control.get_Sine_Time();
			double min_sine_angle_freq = calculate_values.min_sine_freq * M_2PI;
			Pulse_Mode pulse_mode = calculate_values.pulse_mode;
			Carrier_Freq freq_data = calculate_values.carrier_freq;
			double dipolar = calculate_values.dipolar;

			if (sine_angle_freq < min_sine_angle_freq && control.get_Control_Frequency() > 0)
            {
				control.set_Allowed_Sine_Time_Change(false);
				sine_angle_freq = min_sine_angle_freq;
            }
            else
				control.set_Allowed_Sine_Time_Change(true);

			if (pulse_mode.pulse_name == Pulse_Mode_Names.Async)
            {

				double desire_saw_angle_freq = (freq_data.range == 0) ? freq_data.base_freq * M_2PI : get_Random_freq(freq_data, control) * M_2PI;

				double saw_time = control.get_Saw_Time();
				double saw_angle_freq = control.get_Saw_Angle_Freq();

				if (desire_saw_angle_freq == 0)
					saw_time = 0;
				else
					saw_time = saw_angle_freq / desire_saw_angle_freq * saw_time;
				saw_angle_freq = desire_saw_angle_freq;

				control.set_Saw_Angle_Freq(saw_angle_freq);
				control.set_Saw_Time(saw_time);

				double sine_x = sine_time * sine_angle_freq + initial_phase;
				double sin_value = Get_Sine_Value_With_Harmonic(pulse_mode.Clone(), sine_x, calculate_values.amplitude);

				double saw_value = Get_Saw(control.get_Saw_Time() * control.get_Saw_Angle_Freq());
				if (pulse_mode.Shift)
					saw_value = -saw_value;

				double changed_saw = ((dipolar != -1) ? dipolar : 0.5) * saw_value;
				int pwm_value = get_pwm_value(sin_value, changed_saw + 0.5) + get_pwm_value(sin_value, changed_saw - 0.5);

				return pwm_value;

				

			}
			else
			{
				double sine_x = sine_time * sine_angle_freq + initial_phase;

				if (pulse_mode.pulse_name == Pulse_Mode_Names.P_1)
                {
					if(pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Alt1)
						return Get_P_1_3Level(sine_x, calculate_values.amplitude);
				}
					

				int pulses = Get_Pulse_Num(pulse_mode,3);
				double saw_value = Get_Saw(pulses * (sine_angle_freq * sine_time + initial_phase));
				if (pulse_mode.Shift)
					saw_value = -saw_value;
				
				double sin_value = Get_Sine_Value_With_Harmonic(pulse_mode.Clone(), sine_x, calculate_values.amplitude);

				double changed_saw = ((dipolar != -1) ? dipolar : 0.5) * saw_value;
				int pwm_value = get_pwm_value(sin_value, changed_saw + 0.5) + get_pwm_value(sin_value, changed_saw - 0.5);

				control.set_Saw_Angle_Freq(sine_angle_freq * pulses);
				control.set_Saw_Time(sine_time);

				return pwm_value;
			}

			
		}

		public static int calculate_two_level (VVVF_Values control , PWM_Calculate_Values calculate_values, double initial_phase)
		{
			double sin_angle_freq = control.get_Sine_Angle_Freq();
			double sin_time = control.get_Sine_Time();
			double min_sine_angle_freq = calculate_values.min_sine_freq * M_2PI;
			if (sin_angle_freq < min_sine_angle_freq && control.get_Control_Frequency() > 0)
			{
				control.set_Allowed_Sine_Time_Change(false);
				sin_angle_freq = min_sine_angle_freq;
			}
			else
				control.set_Allowed_Sine_Time_Change(true);

			double saw_time = control.get_Saw_Time();
			double saw_angle_freq = control.get_Saw_Angle_Freq();

			double amplitude = calculate_values.amplitude;
			Pulse_Mode pulse_mode = calculate_values.pulse_mode;
			Pulse_Mode_Names pulse_name = pulse_mode.pulse_name;
			Carrier_Freq carrier_freq_data = calculate_values.carrier_freq;

			if (calculate_values.none) 
				return 0;
			//if mode is wide 3
			if (pulse_name == Pulse_Mode_Names.P_Wide_3)
				return Get_P_Wide_3(sin_time, sin_angle_freq, initial_phase, amplitude, false);

			//if async
			if (pulse_name == Pulse_Mode_Names.Async)
			{
				double desire_saw_angle_freq = (carrier_freq_data.range == 0) ? carrier_freq_data.base_freq * M_2PI : get_Random_freq(carrier_freq_data, control) * M_2PI;

				if (desire_saw_angle_freq == 0)
					saw_time = 0;
				else 
					saw_time = saw_angle_freq / desire_saw_angle_freq * saw_time;
				saw_angle_freq = desire_saw_angle_freq;

				double sine_x = sin_time * sin_angle_freq + initial_phase;
				double sin_value = Get_Sine_Value_With_Harmonic(pulse_mode.Clone(), sine_x, amplitude);


				double saw_value = Get_Saw(saw_time * saw_angle_freq);
				int pwm_value = get_pwm_value(sin_value, saw_value) * 2;

				control.set_Saw_Angle_Freq(saw_angle_freq);
				control.set_Saw_Time(saw_time);

				return pwm_value;

			}

			if (pulse_name == Pulse_Mode_Names.CHMP_15)
            {
				if(pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Default)
                {
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
						My_Switchingangles._7Alpha[(int)(1000 * amplitude) + 1, 6] * M_PI_180,
						My_Switchingangles._7Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
				else if(pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Alt1)
                {
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
						My_Switchingangles._7Alpha_Old[(int)(1000 * amplitude) + 1, 6] * M_PI_180,
						My_Switchingangles._7OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
            }

			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_15)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 5] * M_PI_180,
				   My_Switchingangles._7WideAlpha[(int)(1000 * amplitude) - 999, 6] * M_PI_180,
				   'B', sin_time, sin_angle_freq, initial_phase);

			if (pulse_name == Pulse_Mode_Names.CHMP_13)
            {
				if (pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Default)
				{
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						My_Switchingangles._6Alpha[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
						M_PI_2,
						My_Switchingangles._6Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
				else if (pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Alt1)
				{
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						My_Switchingangles._6Alpha_Old[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
						M_PI_2,
						My_Switchingangles._6OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
			}
				
			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_13)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
				   My_Switchingangles._6WideAlpha[(int)(1000 * amplitude) - 999, 5] * M_PI_180,
				   M_PI_2,
				   'A', sin_time, sin_angle_freq, initial_phase);

			if (pulse_name == Pulse_Mode_Names.CHMP_11)
            {
				if(pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Default)
                {
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._5Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._5Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._5Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._5Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._5Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						M_PI_2,
						M_PI_2,
						My_Switchingangles._5Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
				else if(pulse_mode.Alt_Mode == Pulse_Alternative_Mode.Alt1)
                {
					return Get_P_with_SwitchingAngle(
						My_Switchingangles._5Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
						My_Switchingangles._5Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
						My_Switchingangles._5Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
						My_Switchingangles._5Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
						My_Switchingangles._5Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
						M_PI_2,
						M_PI_2,
						My_Switchingangles._5OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
					);
				}
            }
				

			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_11)
				return Get_P_with_SwitchingAngle(
					My_Switchingangles._5WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
					My_Switchingangles._5WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
					My_Switchingangles._5WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
					My_Switchingangles._5WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
					My_Switchingangles._5WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
					M_PI_2,
					M_PI_2,
					'B', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_9)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._4Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._4Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   My_Switchingangles._4Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
				   My_Switchingangles._4Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   My_Switchingangles._4Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_9)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._4WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
				   My_Switchingangles._4WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
				   My_Switchingangles._4WideAlpha[(int)(1000 * amplitude) - 799, 2] * M_PI_180,
				   My_Switchingangles._4WideAlpha[(int)(1000 * amplitude) - 799, 3] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'A', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_7)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._3Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._3Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   My_Switchingangles._3Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   My_Switchingangles._3Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_7)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._3WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
				   My_Switchingangles._3WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
				   My_Switchingangles._3WideAlpha[(int)(1000 * amplitude) - 799, 2] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'B', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_5)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._2Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._2Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   My_Switchingangles._2Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_5)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._2WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
				   My_Switchingangles._2WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'A', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.CHMP_Wide_3)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._WideAlpha[(int)(500 * amplitude) + 1] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'B', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.SHEP_3)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._1Alpha_SHE[(int)(1000 * amplitude) + 1] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'B', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.SHEP_5)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._2Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._2Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'A', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.SHEP_7)
				return Get_P_with_SwitchingAngle(
				   My_Switchingangles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   My_Switchingangles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   M_PI_2,
				   'B', sin_time, sin_angle_freq, initial_phase);
			if (pulse_name == Pulse_Mode_Names.SHEP_11)
				return Get_P_with_SwitchingAngle(
				  My_Switchingangles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
				   My_Switchingangles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
				   My_Switchingangles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
				   My_Switchingangles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
				   My_Switchingangles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
				   M_PI_2,
				   M_PI_2,
				   'A', sin_time, sin_angle_freq, initial_phase);


			if (
				pulse_name == Pulse_Mode_Names.CHMP_3 ||
				pulse_mode.Square && Is_Square_Available(pulse_mode, 2)
			)
			{
				bool is_shift = pulse_mode.Shift;
				int pulse_num = Get_Pulse_Num(pulse_mode,2);
				double pulse_initial_phase = Get_Pulse_Initial(pulse_mode,2);
				return Get_P_with_Saw(sin_angle_freq * sin_time + initial_phase, pulse_initial_phase, amplitude, pulse_num, is_shift);
			}


			//sync mode but no the above.
			{
				int pulse_num = Get_Pulse_Num(pulse_mode,2);
				double x = sin_angle_freq * sin_time + initial_phase;
				double saw_value = -Get_Sine(pulse_num * x);//Get_Saw(pulse_num * x);
				double sin_value = Get_Sine_Value_With_Harmonic(pulse_mode.Clone(), x, amplitude);

				if (pulse_mode.Shift)
					saw_value = -saw_value;

				int pwm_value = get_pwm_value(sin_value, saw_value) * 2;

				control.set_Saw_Angle_Freq(sin_angle_freq * pulse_num);
				control.set_Saw_Time(sin_time);
				//Console.WriteLine(pwm_value);
				return pwm_value;
			}

			


		}
	}
}
