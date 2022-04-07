using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.My_Math;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;


namespace VVVF_Simulator.Generation.Motor
{
    public class Generate_Motor_Core
    {
        public class Motor_Model
        {
            readonly public static double M_2PI = 6.283185307179586476925286766559;
            readonly public static double M_PI = 3.1415926535897932384626433832795;
            readonly public static double M_PI_2 = 1.5707963267948966192313216916398;
            readonly public static double M_2_PI = 0.63661977236758134307553505349006;
            readonly public static double M_1_PI = 0.31830988618379067153776752674503;
            readonly public static double M_1_2PI = 0.15915494309189533576888376337251;
            readonly public static double M_PI_180 = 0.01745329251994329576923690768489;
            readonly public static double M_PI_4 = 0.78539816339744830961566084581988;
            public readonly double R_s = 1.898; /*stator resistance(ohm)*/
            public readonly double R_r = 1.45;  /*Rotor resistance(ohm)*/
            public readonly double L_s = 0.196; /*Stator inductance(H)*/
            public readonly double L_r = 0.196; /*Rotor inductance(H)*/
            public readonly double L_m = 0.187; /*Mutual inductance(H)*/
            public readonly double NP = 2;/* Polar logarithm*/
            public readonly double DAMPING = 500.0;/* damping */
            public readonly double INERTIA = 0.05; /*Rotational inertia mass(kg.m^2)*/
            public readonly double STATICF = 0.005879; /*Static friction(N.m.s)*/
            public class Async_Param
            {
                // Inherent Parameters
                public double R_s;
                public double R_r;
                public double L_s;
                public double L_r;
                public double L_m;
                public double NP;
                public double RateCurent;

                public double[] Iabc = new double[3];
                public double[] Idq0 = new double[3];
                public double[] Uabc = new double[3];
                public double[] Udq0 = new double[3];
                public double Ubus;
                public double wsl;
                public double w_e;
                public double w_r;
                public double w_machine;
                // Response mechanical parameters
                public double sita_r;
                public double sita_r_sum;
                public double w_mr;
                public double sitamr;
                public double sita_machie;
                public double TL;
                public double Te;
                public double r_Flux;
                public double ZUNI;
                public double INERTIA;
                public double STATICF;
            }

            public static Async_Param Get_Asyn_ParamInit()
            {
                Async_Param param = new()
                {
                    // Inherent Parameters
                    R_s = 1.898,
                    R_r = 1.45,
                    L_s = 0.196,
                    L_r = 0.196,
                    L_m = 0.187,
                    NP = 2,
                    RateCurent = 16.5,
                    Iabc = new double[3] { 0, 0, 0 },
                    Idq0 = new double[3] { 0, 0, 0 },
                    Uabc = new double[3] { 0, 0, 0 },
                    Udq0 = new double[3] { 0, 0, 0 },
                    Ubus = 380.0,

                    w_r = 0,
                    w_machine = 0,
                    //Response mechanical parameters
                    sita_r = 0,
                    sita_r_sum = 0,
                    sita_machie = 0,
                    sitamr = 0,
                    TL = 1,
                    Te = 0,
                };

                return param;
            }


            double i_m1 = 0;
            void Asyn_ModuleMt(Async_Param p_Asyn_Parm)
            {
                //The following motor models are from
                //《Research on some key technologies of high performance frequency converter for asynchronous motor》Wang siran, Zhejiang University
                double u_sm = p_Asyn_Parm.Udq0[0];
                double u_st = p_Asyn_Parm.Udq0[1];
                double T_e = p_Asyn_Parm.Te;
                double T_L = p_Asyn_Parm.TL;
                double R_s = p_Asyn_Parm.R_s;
                double R_r = p_Asyn_Parm.R_r;
                double L_m = p_Asyn_Parm.L_m;
                double L_r = p_Asyn_Parm.L_r;
                double L_s = p_Asyn_Parm.L_s;
                double i_d = p_Asyn_Parm.Idq0[0];
                double i_q = p_Asyn_Parm.Idq0[1];
                double w_r = p_Asyn_Parm.w_r;
                double FLUX = p_Asyn_Parm.r_Flux;
                double NP = p_Asyn_Parm.NP;
                double wsl = p_Asyn_Parm.wsl;
                double rsita = p_Asyn_Parm.sita_r;
                double wmr = p_Asyn_Parm.w_mr;
                double sitamr = p_Asyn_Parm.sitamr;
                //Rotor electrical constant
                double T_r = L_r / R_r;
                double temp2 = L_m * L_m;
                double temp1 = 0;
                double eta = 1 - temp2 / (L_s * L_r);
                double temp = 2 * T_r * SIM_TIME;
                temp1 = eta * L_s;
                temp = eta * L_s * L_r * T_r;
                i_d = ((-R_s / (temp1) - (temp2) / (temp)) * i_d + (L_m / (temp)) * FLUX + u_sm / (temp1)) / SIM_TIME + i_d; ///励磁电流方程
                i_q = ((-R_s / (temp1) - (temp2) / (temp)) * i_q - FLUX * (L_m * w_r / (temp1 * L_r)) + u_st / (temp1)) / SIM_TIME + i_q;  ///转矩电流方程
                                                                                                                                           ///转子磁链方程
                                                                                                                                           ///FLUX = i_mL_m/(T_rs +1);///一阶惯性环节 双线性变换推导出
                FLUX = (L_m / (temp + 1)) * (i_d + i_m1) - (1 - temp) * FLUX / (temp + 1); /*Rotor flux linkage is the first - order inertia of excitation current*/
                i_m1 = i_d;
                T_e = NP * i_q * FLUX * L_m / L_r; /*Moment equation*/
                if (FLUX != 0)
                    wsl = L_m * i_q / (T_r * FLUX); /*The slip equation may be divided by 0 here*/
                if ((Math.Abs((int)(T_e - T_L)) < STATICF) && (w_r == 0)) /*Simulating static friction*/
                    w_r = 0;
                else /*Simulated running equation of motion*/
                    w_r = (NP * ((T_e - T_L - (DAMPING * w_r / NP)) / INERTIA)) / SIM_TIME + w_r;

                rsita += w_r / SIM_TIME;
                wmr = (wsl + w_r) / SIM_TIME;
                /*Input rotor position*/
                sitamr += wmr;
                if (sitamr > M_2PI)
                {
                    sitamr = sitamr - M_2PI;
                }
                else if (sitamr < 0)
                {
                    sitamr = sitamr + M_2PI;
                }
                /*Rotor position obtained by integration*/
                if (rsita > M_2PI)
                {
                    rsita = rsita - M_2PI;
                }
                else if (rsita < 0)
                {
                    rsita = rsita + M_2PI;
                }
                p_Asyn_Parm.w_mr = wmr;
                p_Asyn_Parm.sitamr = sitamr;
                p_Asyn_Parm.sita_r = rsita;
                p_Asyn_Parm.Idq0[0] = i_d;
                p_Asyn_Parm.Idq0[1] = i_q;
                p_Asyn_Parm.w_r = w_r;
                p_Asyn_Parm.r_Flux = FLUX;
                p_Asyn_Parm.NP = NP;
                p_Asyn_Parm.wsl = wsl;
                p_Asyn_Parm.Te = T_e;
                p_Asyn_Parm.TL = T_L;
            }

            public void Asyn_Moduleabc(Async_Param p)
            {
                p.Udq0[0] = Math.Cos(p.sitamr) * p.Uabc[0] + Math.Cos(p.sitamr - M_2PI / 3) * p.Uabc[1] + Math.Cos(p.sitamr + M_2PI / 3) * p.Uabc[2];
                p.Udq0[1] = -Math.Sin(p.sitamr) * p.Uabc[0] + -Math.Sin(p.sitamr - M_2PI / 3) * p.Uabc[1] + -Math.Sin(p.sitamr + M_2PI / 3) * p.Uabc[2];

                Asyn_ModuleMt(p);

                double al = p.Idq0[0] * Math.Cos(p.sitamr) - p.Idq0[1] * Math.Sin(p.sitamr);
                double be = p.Idq0[1] * Math.Cos(p.sitamr) + p.Idq0[0] * Math.Sin(p.sitamr);

                p.Iabc[0] = Math.Sqrt(3 / 2) * (al * 1 + be * 0);
                p.Iabc[1] = Math.Sqrt(3 / 2) * (al * -1 / 2.0 + be * Math.Sqrt(3) / 2);
                p.Iabc[2] = Math.Sqrt(3 / 2) * (al * -1 / 2.0 + be * -Math.Sqrt(3) / 2);
            }
            public void AynMotorControler(Async_Param p_Asyn_Parm, Wave_Values Volteage)
            {
                p_Asyn_Parm.Uabc[0] = (double)(220 * Volteage.U / 2.0);//Phase to ground 220 Phase to phase 380 
                p_Asyn_Parm.Uabc[1] = (double)(220 * Volteage.V / 2.0);//Phase to ground 220 Phase to phase 380 
                p_Asyn_Parm.Uabc[2] = (double)(220 * Volteage.W / 2.0);//Phase to ground 220 Phase to phase 380 
            }

            private double SIM_TIME = 1;

            public void Set_SampleFreq(double x)
            {
                SIM_TIME = x;
            }
        }

        public static void Export_Wav(String output_path, Yaml_VVVF_Sound_Data sound_data)
        {
            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            int sample_freq = 192000;
            int sound_block_count = 0;

            BinaryWriter writer = new BinaryWriter(new FileStream(temp, FileMode.Create));

            //WAV FORMAT DATA
            writer.Write(0x46464952); // RIFF
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); //CHUNK SIZE
            writer.Write(0x45564157); //WAVE
            writer.Write(0x20746D66); //fmt 
            writer.Write(16);
            writer.Write(new byte[] { 0x01, 0x00 }); // LINEAR PCM
            writer.Write(new byte[] { 0x01, 0x00 }); // MONORAL
            writer.Write(sample_freq); // SAMPLING FREQ
            writer.Write(sample_freq); // BYTES IN 1SEC
            writer.Write(new byte[] { 0x01, 0x00 }); // Block Size = 1
            writer.Write(new byte[] { 0x08, 0x00 }); // 1 Sample bits
            writer.Write(0x61746164);
            writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }); //WAVE SIZE

            bool loop = true;

            Motor_Model motor = new Motor_Model();
            motor.Set_SampleFreq(sample_freq);

            var p_Asyn_Parm = Motor_Model.Get_Asyn_ParamInit();
            p_Asyn_Parm.TL = 0.0;

            byte[] temp_bytes = new byte[19200];
            int temp_bytes_count = 0;

            while (loop)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                Control_Values cv = new Control_Values
                {
                    brake = control.is_Braking(),
                    mascon_on = !control.is_Mascon_Off(),
                    free_run = control.is_Free_Running(),
                    wave_stat = control.get_Control_Frequency()
                };
                PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
                Wave_Values value = VVVF_Calculate.calculate_values(control, calculated_Values, 0);

                p_Asyn_Parm.sitamr = control.get_Sine_Angle_Freq();
                motor.AynMotorControler(p_Asyn_Parm, value);
                motor.Asyn_Moduleabc(p_Asyn_Parm);


                temp_bytes[temp_bytes_count] = (byte)(p_Asyn_Parm.Te * 0.5 + 0xFF / 2);
                temp_bytes_count++;
                if (temp_bytes_count == 19200)
                {
                    writer.Write(temp_bytes);
                    temp_bytes_count = 0;
                }

                sound_block_count++;

                loop = Generate_Common.Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / sample_freq);

            }



            writer.Seek(4, SeekOrigin.Begin);
            writer.Write(sound_block_count + 36);

            writer.Seek(40, SeekOrigin.Begin);
            writer.Write(sound_block_count);

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
