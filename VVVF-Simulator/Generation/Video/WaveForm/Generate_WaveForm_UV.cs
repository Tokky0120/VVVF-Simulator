using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;

namespace VVVF_Simulator.Generation.Video.WaveForm
{
    public class Generate_WaveForm_UV
    {

        /// <summary>
        /// Do clone before call this!
        /// </summary>
        /// <param name="control"></param>
        /// <param name="values"></param>
        /// <param name="image_width"></param>
        /// <param name="image_height"></param>
        /// <param name="wave_height"></param>
        /// <param name="calculate_div"></param>
        /// <returns></returns>
        public static Bitmap Get_WaveForm_Image(
            VVVF_Values control,
            PWM_Calculate_Values values,
            int image_width, 
            int image_height, 
            int wave_height,
            int calculate_div,
            int line_width,
            int spacing
        )
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);

            List<int> points_x = new();
            List<int> points_y = new();

            points_x.Add(0);
            points_y.Add(0);

            int pre_pwm = 0;
            for (int i = 0; i < (image_width - spacing * 2) * calculate_div; i++)
            {
                Wave_Values value = calculate_values(control, values, Math.PI / 6.0);
                int pwm = value.U - value.V;
                if (pre_pwm != pwm)
                {
                    points_x.Add(i);
                    points_y.Add(pre_pwm);

                    points_x.Add(i);
                    points_y.Add(pwm);
                    pre_pwm = pwm;
                }

                control.add_Saw_Time(2 / (60.0 * calculate_div * (image_width - spacing * 2)));
                control.add_Sine_Time(2 / (60.0 * calculate_div * (image_width - spacing * 2)));
            }

            points_x.Add((image_width - spacing * 2) * calculate_div);
            points_y.Add(pre_pwm);

            for (int i = 0; i < points_x.Count - 1; i++)
            {
                int x_1 = points_x[i];
                int x_2 = points_x[i + 1];
                int y_1 = points_y[i];
                int y_2 = points_y[i + 1];

                int curr_val = (int)(-y_1 * wave_height + image_height / 2.0);
                int next_val = (int)(-y_2 * wave_height + image_height / 2.0);
                g.DrawLine(new Pen(Color.Black, line_width), (int)(x_1 / (double)calculate_div) + spacing, curr_val, (int)(x_2 / (double)calculate_div) + spacing, next_val);
            }

            g.Dispose();
            return image;
        }
        public static void Generate_UV_1(String fileName, Yaml_VVVF_Sound_Data sound_data)
        {
            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            control.set_Allowed_Random_Freq_Move(false);

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.current_data.Clone();

            int fps = 60;

            int image_width = 2880;
            int image_height = 540;

            int wave_height = 100;
            int calculate_div = 30;

            VideoWriter vr = new VideoWriter(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            Boolean START_WAIT = true;
            if (START_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {


                    vr.Write(mat);


                }
                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {

                control.set_Sine_Time(0);
                control.set_Saw_Time(0);

                Control_Values cv = new Control_Values
                {
                    brake = control.is_Braking(),
                    mascon_on = !control.is_Mascon_Off(),
                    free_run = control.is_Free_Running(),
                    wave_stat = control.get_Control_Frequency()
                };
                PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);

                Bitmap image = Get_WaveForm_Image(control, calculated_Values, image_width, image_height, wave_height, calculate_div, 2, 100);


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

                loop = Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / fps);

            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {
                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();
        }

        public static void Generate_UV_2(String fileName, Yaml_VVVF_Sound_Data sound_data)
        {
            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.current_data.Clone();

            int fps = 60;

            int image_width = 2000;
            int image_height = 500;

            int wave_height = 100;
            int calculate_div = 10;

            VideoWriter vr = new VideoWriter(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            Boolean START_WAIT = true;
            if (START_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                for (int i = 0; i < 60; i++)
                {
                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {

                control.set_Sine_Time(0);
                control.set_Saw_Time(0);

                Control_Values cv = new Control_Values
                {
                    brake = control.is_Braking(),
                    mascon_on = !control.is_Mascon_Off(),
                    free_run = control.is_Free_Running(),
                    wave_stat = control.get_Control_Frequency()
                };
                PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);

                Bitmap image = Get_WaveForm_Image(control, calculated_Values, image_width, image_height, wave_height, calculate_div, 1, 0);

                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

                image.Dispose();
                loop = Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / fps);

            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {
                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();
        }
    }
}
