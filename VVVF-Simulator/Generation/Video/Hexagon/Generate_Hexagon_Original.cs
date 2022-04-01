using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Values;
using static VVVF_Simulator.Generation.Generate_Common;
using static VVVF_Simulator.My_Math;
using VVVF_Simulator.Yaml.VVVF_Sound;
using System.Collections.Generic;
using Point = System.Drawing.Point;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;

namespace VVVF_Simulator.Generation.Video.Hexagon
{
    public class Generate_Hexagon_Original
    {


        public static Bitmap Get_Hexagon_Original_Image(
            VVVF_Values control,
            Yaml_VVVF_Sound_Data sound_data,
            int image_width,
            int image_height, 
            int hex_div_seed,
            int line_width,
            bool draw_zero_vector_circle,
            bool precise
        )
        {

            if(control.get_Control_Frequency() == 0)
            {
                Bitmap empty_image = new(image_width, image_height);
                Graphics empty_g = Graphics.FromImage(empty_image);
                empty_g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                empty_g.Dispose();
                return empty_image;
            }

            Control_Values cv = new Control_Values
            {
                brake = control.is_Braking(),
                mascon_on = !control.is_Mascon_Off(),
                free_run = control.is_Free_Running(),
                wave_stat = control.get_Control_Frequency()
            };
            PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);

            double hex_div_amp = (control.get_Sine_Freq() > 0 && control.get_Sine_Freq() < 1) ? 1.0 / control.get_Sine_Freq() : 1;
            int fix_hex_div_seed = precise ? (int)Math.Round(hex_div_seed * hex_div_amp) : hex_div_seed;
            int hex_div = fix_hex_div_seed * 6;

            Bitmap hexagon_image = new(image_width, image_height);
            Graphics hexagon_g = Graphics.FromImage(hexagon_image);

            Boolean drawn_circle = false;
            Bitmap zero_circle_image = new(image_width, image_height);
            Graphics zero_circle_g = Graphics.FromImage(zero_circle_image);

            double[] hexagon_coordinate = new double[] { 100, 500 };
            double[] x_min_max = new double[2] { 10000, 0 };

            List<int> points_x = new() { 100 };
            List<int> points_y = new() { 500 };
            Wave_Values pre_wave_Values = new Wave_Values();

            for (int i = 0; i < hex_div; i++)
            {

                control.add_Sine_Time(1.0 / hex_div * ((control.get_Sine_Freq() == 0) ? 0 : 1 / control.get_Sine_Freq()));
                control.add_Saw_Time(1.0 / hex_div * ((control.get_Sine_Freq() == 0) ? 0 : 1 / control.get_Sine_Freq()));

                Wave_Values value = calculate_values(control, calculated_Values, 0);

                double move_x = -0.5 * value.W - 0.5 * value.V + value.U;
                double move_y = -0.866025403784438646763 * value.W + 0.866025403784438646763 * value.V;

                double int_move_x = 200 * move_x / fix_hex_div_seed;
                double int_move_y = 200 * move_y / fix_hex_div_seed;

                if(!pre_wave_Values.Equals(value))
                {
                    points_x.Add((int)Math.Round(hexagon_coordinate[0] + int_move_x));
                    points_y.Add((int)Math.Round(hexagon_coordinate[1] + int_move_y));
                    pre_wave_Values = value.Clone();
                }

                if (move_x == 0 && move_y == 0 && draw_zero_vector_circle)
                {
                    if (!drawn_circle)
                    {
                        drawn_circle = true;
                        double radius = 5 * ((control.get_Control_Frequency() > 40) ? 1 : (control.get_Control_Frequency() / 40.0));
                        zero_circle_g.FillEllipse(new SolidBrush(Color.White),
                            (int)Math.Round(hexagon_coordinate[0] - radius),
                            (int)Math.Round(hexagon_coordinate[1] - radius),
                            (int)Math.Round(radius * 2),
                            (int)Math.Round(radius * 2)
                        );
                        zero_circle_g.DrawEllipse(new Pen(Color.Black),
                            (int)Math.Round(hexagon_coordinate[0] - radius),
                            (int)Math.Round(hexagon_coordinate[1] - radius),
                            (int)Math.Round(radius * 2),
                            (int)Math.Round(radius * 2)
                        );
                    }

                }
                else
                    drawn_circle = false;

                hexagon_coordinate[0] = hexagon_coordinate[0] + int_move_x;
                hexagon_coordinate[1] = hexagon_coordinate[1] + int_move_y;

                if (x_min_max[0] > hexagon_coordinate[0]) x_min_max[0] = hexagon_coordinate[0];
                if (x_min_max[1] < hexagon_coordinate[0]) x_min_max[1] = hexagon_coordinate[0];

            }

            if(control.get_Control_Frequency() != 0)
            {
                for (int i = 0; i < points_x.Count - 1; i++)
                {
                    Point start = new Point(points_x[i], points_y[i]);
                    Point end = new Point(points_x[i + 1], points_y[i + 1]);
                    hexagon_g.DrawLine(new Pen(Color.Black, line_width), start, end);
                }
            }

            Bitmap final_image = new(image_width, image_height);
            Graphics final_g = Graphics.FromImage(final_image);
            final_g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);

            double moved_x = (image_width - x_min_max[1] - x_min_max[0]) / 2.0;
            final_g.DrawImage(hexagon_image, (int)Math.Round(moved_x), 0);
            final_g.DrawImage(zero_circle_image, (int)Math.Round(moved_x), 0);

            final_g.Dispose();
            hexagon_g.Dispose();
            final_g.Dispose();
            zero_circle_g.Dispose();

            hexagon_image.Dispose();
            zero_circle_image.Dispose();

            return final_image;
        }

        public static void Generate_Hexagon_Original_Video(String fileName, Yaml_VVVF_Sound_Data sound_data, bool circle)
        {
            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            Boolean draw_zero_vector_circle = circle;

            int fps = 60;


            int image_width = 1000;
            int image_height = 1000;

            int hex_div_seed = 10000;

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
                MemoryStream ms = new();
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

                Bitmap final_image = Get_Hexagon_Original_Image(control, sound_data, image_width, image_height, hex_div_seed, 2, draw_zero_vector_circle, true);


                MemoryStream ms = new MemoryStream();
                final_image.Save(ms, ImageFormat.Png);
                final_image.Dispose();
                byte[] img = ms.GetBuffer();
                Mat mat = Mat.FromImageData(img);

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"> Path for png file </param>
        /// <param name="sound_data">SOUND DATA</param>
        /// <param name="circle">Setting whether see zero vector circle or not</param>
        /// <param name="d">Frequency you want to see</param>
        public static void Generate_Hexagon_Original_Image(String fileName, Yaml_VVVF_Sound_Data sound_data, Boolean circle, double d)
        {
            VVVF_Values control = new();

            control.reset_control_variables();
            control.reset_all_variables();
            control.set_Allowed_Random_Freq_Move(false);

            Boolean draw_zero_vector_circle = circle;

            control.set_Sine_Angle_Freq(d * M_2PI);
            control.set_Control_Frequency(d);

            int image_width = 1000;
            int image_height = 1000;

            int hex_div_seed = 10000;
            Bitmap final_image = Get_Hexagon_Original_Image(control, sound_data, image_width, image_height, hex_div_seed, 2, draw_zero_vector_circle, true);

            MemoryStream ms = new MemoryStream();
            final_image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = Mat.FromImageData(img);

            final_image.Save(fileName, ImageFormat.Png);


            Cv2.ImShow("Hexagon", mat);
            Cv2.WaitKey();
            final_image.Dispose();
        }
    }
}
