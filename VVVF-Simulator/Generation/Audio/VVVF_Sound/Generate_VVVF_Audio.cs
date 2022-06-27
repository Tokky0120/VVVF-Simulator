using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.Mascon_Control;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.MainWindow;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;

namespace VVVF_Simulator.Generation.Audio.VVVF_Sound
{
    public class Generate_VVVF_Audio
    {
        // -------- VVVF SOUND -------------
        public static byte Get_VVVF_Sound(VVVF_Values control, Yaml_VVVF_Sound_Data sound_data)
        {
            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
            Wave_Values value = VVVF_Calculate.calculate_values(control, calculated_Values, 0);

            double pwm_value = value.U - value.V;
            byte sound_byte = 0x80;
            if (pwm_value == 2) sound_byte += 0x40;
            else if (pwm_value == 1) sound_byte += 0x20;
            else if (pwm_value == -1) sound_byte -= 0x20;
            else if (pwm_value == -2) sound_byte -= 0x40;

            return sound_byte;
        }


        // Export Audio
        public static void Export_VVVF_Sound(ProgressData progressData ,String output_path, Boolean resize, int sample_freq, Yaml_VVVF_Sound_Data sound_data)
        {
            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data_Compiled ymdc = Yaml_Mascon_Manage.CurrentData.GetCompiled();

            int sound_block_count = 0;

            BinaryWriter writer = new BinaryWriter(new FileStream(resize ? temp : output_path, FileMode.Create));

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

            byte[] temp_bytes = new byte[19200];
            int temp_bytes_count = 0;

            //TASK DATA PREPARE
            progressData.Total = ymdc.GetEstimatedSteps(1.0/sample_freq);

            while (true)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                temp_bytes[temp_bytes_count] = Get_VVVF_Sound(control, sound_data);
                temp_bytes_count++;
                if (temp_bytes_count == 19200)
                {
                    writer.Write(temp_bytes);
                    temp_bytes_count = 0;
                }

                sound_block_count++;
                progressData.Progress = sound_block_count;

                bool flag_continue = Check_For_Freq_Change(control, ymdc, sound_data.mascon_data, 1.0 / sample_freq);
                bool flag_cancel = progressData.Cancel;

                if (flag_cancel || !flag_continue) break;

            }

            if (temp_bytes_count > 0)
                writer.Write(temp_bytes);

            writer.Seek(4, SeekOrigin.Begin);
            writer.Write(sound_block_count + 36);

            writer.Seek(40, SeekOrigin.Begin);
            writer.Write(sound_block_count);

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
