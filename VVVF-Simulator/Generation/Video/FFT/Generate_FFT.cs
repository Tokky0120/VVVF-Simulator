using NAudio.Dsp;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.Generation.Generate_Common.GenerationBasicParameter;
using static VVVF_Simulator.MainWindow;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;

namespace VVVF_Simulator.Generation.Video.FFT
{
    public class Generate_FFT
    {
        private static readonly int pow = 18;
        private static float[] FFT_NAudio(float[] sdata)
        {
            var fftsample = new Complex[sdata.Length];
            var res = new float[sdata.Length / 2];

            for (int i = 0; i < sdata.Length; i++)
            {
                fftsample[i].X = (float)(sdata[i] * FastFourierTransform.HammingWindow(i, sdata.Length));
                fftsample[i].Y = 0;
            }

            FastFourierTransform.FFT(true, pow, fftsample);

            for (int i = 0; i < sdata.Length / 2; i++)
            {
                res[i] = (float)Math.Sqrt(fftsample[i].X * fftsample[i].X + fftsample[i].Y * fftsample[i].Y);
            }
            return res;
        }

        public static float[] FFT_WaveForm(VVVF_Values control, Yaml_VVVF_Sound_Data sound_Data)
        {
            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_Data);

            control.set_Saw_Time(0);
            control.set_Sine_Time(0);

            int sample_count = (int)Math.Pow(2, pow);
            float[] samples = new float[sample_count];
            
            for(int i = 0; i < sample_count; i++)
            {
                Wave_Values value = VVVF_Calculate.calculate_values(control, calculated_Values, Math.PI / 6.0);
                int pwm = value.U - value.V;
                samples[i] = pwm;
                control.add_Saw_Time(1.0 / sample_count);
                control.add_Sine_Time(1.0 / sample_count);
            }

            return FFT_NAudio(samples);
        }

        public static Bitmap Get_FFT_Image(VVVF_Values control, Yaml_VVVF_Sound_Data sound_Data)
        {
            float[] points = FFT_WaveForm(control, sound_Data);

            Bitmap image = new Bitmap(1000, 1000);
            Graphics g = Graphics.FromImage(image);

            g.FillRectangle(new SolidBrush(Color.White),0,0, 1000, 1000);

            for (int i = 0; i < 2000 - 1; i++)
            {
                PointF start = new PointF((float)(i / 2.0), 1000 - points[i] * 6000);
                PointF end = new PointF((float)((i + 1) / 2.0), 1000 - points[i + 1] * 6000);
                g.DrawLine(new Pen(Color.Black, 2), start, end);
            }

            g.Dispose();

            return image;

        }

        public static void Generate_FFT_Video(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            Yaml_VVVF_Sound_Data vvvfData = generationBasicParameter.vvvfData;
            Yaml_Mascon_Data_Compiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            control.set_Allowed_Random_Freq_Move(false);

            int fps = 60;

            int image_width = 1000;
            int image_height = 1000;

            VideoWriter vr = new VideoWriter(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            Boolean START_WAIT = true;
            if (START_WAIT)
                Generate_Common.Add_Empty_Frames(image_width, image_height, 60, vr);

            // PROGRESS CHANGE
            progressData.Progress+=60;

            Boolean loop = true;
            while (loop)
            {

                control.set_Sine_Time(0);
                control.set_Saw_Time(0);

                Bitmap image = Get_FFT_Image(control, vvvfData);


                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                mat.Dispose();
                ms.Dispose();

                MemoryStream resized_ms = new MemoryStream();
                Bitmap resized = new Bitmap(image, image_width / 2, image_height / 2);
                resized.Save(resized_ms, ImageFormat.Png);
                byte[] resized_img = resized_ms.GetBuffer();
                Mat resized_mat = OpenCvSharp.Mat.FromImageData(resized_img);
                Cv2.ImShow("Wave Form", resized_mat);
                Cv2.WaitKey(1);
                resized_mat.Dispose();
                resized_ms.Dispose();

                image.Dispose();

                loop = Generate_Common.Check_For_Freq_Change(control, masconData, vvvfData.mascon_data, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
            }

            Boolean END_WAIT = true;
            if (END_WAIT)
                Generate_Common.Add_Empty_Frames(image_width, image_height, 60, vr);

            // PROGRESS CHANGE
            progressData.Progress += 60;

            vr.Release();
            vr.Dispose();
        }

        public static void Generate_FFT_Image(String fileName, Yaml_VVVF_Sound_Data sound_data, double d)
        {
            VVVF_Values control = new();

            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            control.set_Sine_Angle_Freq(d * My_Math.M_2PI);
            control.set_Control_Frequency(d);

            Bitmap image = Get_FFT_Image(control, sound_data);

            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = Mat.FromImageData(img);

            image.Save(fileName, ImageFormat.Png);


            Cv2.ImShow("FFT", mat);
            Cv2.WaitKey();
            image.Dispose();
        }
    }
}
