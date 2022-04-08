using System;
using System.IO;
using static VVVF_Simulator.Generation.Audio.Generate_Audio_Core;
using static VVVF_Simulator.Generation.Generate_Common;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.NAudio_Filter;
using NAudio.Wave;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;

namespace VVVF_Simulator.Generation.Audio
{
    public class Generate_Sound
    {
        public enum Sound_Export_Extension
        {
            WAV,MP3
        }

        private static void Export_Wav_VVVF_Sound(String output_path, Yaml_VVVF_Sound_Data sound_data)
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

            byte[] temp_bytes = new byte[19200];
            int temp_bytes_count = 0;

            while (loop)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                temp_bytes[temp_bytes_count] = Get_VVVF_Sound(control, sound_data);
                temp_bytes_count++;
                if(temp_bytes_count == 19200)
                {
                    writer.Write(temp_bytes);
                    temp_bytes_count = 0;
                }

                sound_block_count++;

                loop = Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / sample_freq);

            }

            if (temp_bytes_count > 0)
                writer.Write(temp_bytes);

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
        public static void Export_VVVF_Sound(String output_path, Yaml_VVVF_Sound_Data sound_data, Sound_Export_Extension type)
        {
            if (type == Sound_Export_Extension.WAV)
                Export_Wav_VVVF_Sound(output_path, sound_data);
            else if(type == Sound_Export_Extension.MP3)
            {
                string temp = Path.GetTempFileName();
                Export_Wav_VVVF_Sound(temp, sound_data);

                using (var reader = new WaveFileReader(temp))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, output_path);
                }

                File.Delete(temp);
            }

        }

        

        private static void Export_Wav_Train_Sound(String output_path, Yaml_VVVF_Sound_Data sound_data)
        {

            DateTime dt = DateTime.Now;
            String gen_time = dt.ToString("yyyy-MM-dd_HH-mm-ss");
            string temp = Path.GetDirectoryName(output_path) + "\\" + "temp-" + gen_time + ".wav";

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            int sample_freq = 192000;
            
            BufferedWaveProvider wave_provider = new BufferedWaveProvider(new WaveFormat(sample_freq, 8, 1));
            wave_provider.BufferLength = 20000;

            Equalizer equalizer = new Equalizer(wave_provider.ToSampleProvider(), Get_Filter(192000));
            IWaveProvider equal_wave_provider = equalizer.ToWaveProvider();
            WaveFileWriter writer = new WaveFileWriter(temp, equal_wave_provider.WaveFormat);


            bool loop = true;


            while (loop)
            {
                control.add_Sine_Time(1.00 / sample_freq);
                control.add_Saw_Time(1.00 / sample_freq);

                byte sound_byte = Get_Train_Sound(control, sound_data);

                wave_provider.AddSamples(new byte[] { sound_byte }, 0, 1);

                if(wave_provider.BufferedBytes == wave_provider.BufferLength)
                {
                    byte[] buffer = new byte[wave_provider.BufferedBytes];
                    int bytesRead = equal_wave_provider.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, bytesRead);
                }

                loop = Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / sample_freq);

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

        public static void Export_Train_Sound(String output_path, Yaml_VVVF_Sound_Data sound_data, Sound_Export_Extension type)
        {
            if (type == Sound_Export_Extension.WAV)
                Export_Wav_Train_Sound(output_path, sound_data);
            else if (type == Sound_Export_Extension.MP3)
            {
                string temp = Path.GetTempFileName();
                Export_Wav_Train_Sound(temp, sound_data);

                using (var reader = new WaveFileReader(temp))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, output_path);
                }

                File.Delete(temp);
            }

        }
    }

}
