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
using static VVVF_Simulator.Generation.Audio.Train_Sound.Generate_Train_Audio_Filter.NAudio_Filter;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.Generation.Generate_Common.GenerationBasicParameter;
using static VVVF_Simulator.Generation.Motor.Generate_Motor_Core;
using static VVVF_Simulator.MainWindow;
using static VVVF_Simulator.My_Math;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze;
using static VVVF_Simulator.Yaml.TrainAudio_Setting.Yaml_TrainSound_Analyze.Yaml_TrainSound_Data;
using static VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Mascon_Data;

namespace VVVF_Simulator.Generation.Audio.Train_Sound
{
    public class Generate_Train_Audio
    {
        // -------- TRAIN SOUND --------------
        public static byte Get_Train_Sound(VVVF_Values control, Yaml_VVVF_Sound_Data sound_data, Motor_Data motor, Yaml_TrainSound_Data train_Harmonic_Data)
        {

            double pwm_sound_val;
            Control_Values cv = new()
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
            pwm_sound_val = pwm_sound_val * 2 / 3.0;

            double sound_val = 0, total_sound_count = 0;

            // MOTOR HARMONICS
            for (int harmonic = 0; harmonic < train_Harmonic_Data.Sine_Harmonics.Count; harmonic++)
            {
                Harmonic_Data harmonic_data = train_Harmonic_Data.Sine_Harmonics[harmonic];

                if (harmonic_data.range.start > control.get_Sine_Freq()) continue;
                if (harmonic_data.range.end >= 0 && harmonic_data.range.end < control.get_Sine_Freq()) continue;

                var amplitude_data = harmonic_data.amplitude;

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = amplitude_data.start_val + (amplitude_data.end_val - amplitude_data.start_val) / (amplitude_data.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > amplitude_data.max_val) amplitude = amplitude_data.max_val;
                if (amplitude < amplitude_data.min_val) amplitude = amplitude_data.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;
                sine_val *= amplitude * amplitude_disappear;

                sound_val += Math.Round(sine_val);
                total_sound_count++;
            }

            // 
            // Gear Sound
            //
            List<Harmonic_Data> Gear_Harmonics = train_Harmonic_Data.Gear_Harmonics;
            double Gear_Sound = 0;
            for (int harmonic = 0; harmonic < Gear_Harmonics.Count; harmonic++)
            {
                Harmonic_Data harmonic_data = Gear_Harmonics[harmonic];
                var amplitude_data = harmonic_data.amplitude;

                if (harmonic_data.range.start > control.get_Sine_Freq()) continue;
                if (harmonic_data.range.end >= 0 && harmonic_data.range.end < control.get_Sine_Freq()) continue;

                double harmonic_freq = harmonic_data.harmonic * control.get_Sine_Freq();

                if (harmonic_data.disappear != -1 && harmonic_freq > harmonic_data.disappear) continue;
                double sine_val = sin(control.get_Sine_Time() * control.get_Sine_Angle_Freq() * harmonic_data.harmonic);

                double amplitude = amplitude_data.start_val + (amplitude_data.end_val - amplitude_data.start_val) / (amplitude_data.end - harmonic_data.amplitude.start) * (control.get_Sine_Freq() - harmonic_data.amplitude.start);
                if (amplitude > amplitude_data.max_val) amplitude = amplitude_data.max_val;
                if (amplitude < amplitude_data.min_val) amplitude = amplitude_data.min_val;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.disappear) ?
                    ((harmonic_data.disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * (harmonic_data.disappear == -1 ? 1 : amplitude_disappear);
                Gear_Sound += sine_val;
                total_sound_count++;
            }
            // Gear sound amplitude change
            Yaml_Mascon_Data_On_Off ymdoo;
            if (cv.brake) ymdoo = sound_data.mascon_data.braking;
            else ymdoo = sound_data.mascon_data.accelerating;
            double freq_to_go, gear_amp_rate;
            if (cv.mascon_on) freq_to_go = ymdoo.on.control_freq_go_to;
            else freq_to_go = ymdoo.off.control_freq_go_to;
            gear_amp_rate = control.get_Control_Frequency() / (control.get_Sine_Freq() > freq_to_go ? freq_to_go : control.get_Sine_Freq());
            sound_val += Math.Round(Gear_Sound * (gear_amp_rate > 1 ? 1 : gear_amp_rate));

            int pre_sound_byte;

            if(total_sound_count == 0)
                pre_sound_byte = (int)Math.Round(pwm_sound_val * 2 + 0xFF / 2);
            else
                pre_sound_byte = (int)Math.Round(sound_val / total_sound_count / 2.0 + pwm_sound_val * 2 + 0xFF / 2);

            byte sound_byte = (byte)(pre_sound_byte);
            return sound_byte;

        }


        public static void Export_Train_Sound(GenerationBasicParameter generationBasicParameter, String output_path, Boolean resize, Yaml_TrainSound_Data train_Sound_Data)
        {
            Yaml_VVVF_Sound_Data vvvfData = generationBasicParameter.vvvfData;
            Yaml_Mascon_Data_Compiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            int sample_freq = 200000;

            BufferedWaveProvider wave_provider = new(new WaveFormat(sample_freq, 8, 1));
            wave_provider.BufferLength = 20000;

            Equalizer equalizer = new(wave_provider.ToSampleProvider(), train_Sound_Data.Get_NFilters());
            IWaveProvider equal_wave_provider = equalizer.ToWaveProvider();
            WaveFileWriter writer = new(resize ? temp : output_path, equal_wave_provider.WaveFormat);

            Motor_Data motor = new()
            {
                motor_Specification = train_Sound_Data.Motor_Specification.Clone(),
                SIM_SAMPLE_FREQ = sample_freq,
            };
            motor.motor_Param.TL = 0.0;

            progressData.Total = masconData.GetEstimatedSteps(1.0 / sample_freq);

            while (true)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                byte sound_byte = Get_Train_Sound(control, vvvfData, motor , train_Sound_Data);

                wave_provider.AddSamples(new byte[] { sound_byte }, 0, 1);

                if (wave_provider.BufferedBytes == wave_provider.BufferLength)
                {
                    byte[] buffer = new byte[wave_provider.BufferedBytes];
                    int bytesRead = equal_wave_provider.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, bytesRead);
                }

                progressData.Progress++;

                bool flag_continue = Check_For_Freq_Change(control, masconData, vvvfData.mascon_data, 1.0 / sample_freq);
                bool flag_cancel = progressData.Cancel;
                if (!flag_continue || flag_cancel) break;
            }

            //last 
            if (wave_provider.BufferedBytes > 0)
            {
                byte[] buffer = new byte[wave_provider.BufferedBytes];
                int bytesRead = equal_wave_provider.Read(buffer, 0, buffer.Length);
                writer.Write(buffer, 0, bytesRead);
            }

            writer.Close();


            if (!resize) return;
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
