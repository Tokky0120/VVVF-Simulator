using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static VVVF_Simulator.VVVF_Calculate;

using static VVVF_Simulator.Generation.Generate_Common;
using System.Drawing.Drawing2D;

using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.VVVF_Structs.Pulse_Mode;

namespace VVVF_Simulator.Generation.Video.Control_Info
{
    public class Generate_Control_Original
    {
        private static String[] get_Pulse_Name(VVVF_Values control)
        {
            Pulse_Mode_Names mode = control.get_Video_Pulse_Mode().pulse_name;
            //Not in sync
            if (mode == Pulse_Mode_Names.Async )
            {
                string[] names = new string[3];
                int count = 0;

                Carrier_Freq carrier_freq_data = control.get_Video_Carrier_Freq_Data();

                names[count] = String.Format("Async - " + carrier_freq_data.base_freq.ToString("F2")).PadLeft(6);
                count++;

                if (carrier_freq_data.range != 0)
                {
                    names[count] = String.Format("Random ± " + carrier_freq_data.range.ToString("F2")).PadLeft(6);
                    count++;
                }

                if (control.get_Video_Dipolar() != -1)
                {
                    names[count] = String.Format("Dipolar : " + control.get_Video_Dipolar().ToString("F0")).PadLeft(6);
                    count++;
                }
                return names;

            }

            //Abs
            if (mode == Pulse_Mode_Names.P_Wide_3)
                return new string[] { "Wide 3 Pulse" };

            if (mode.ToString().StartsWith("CHM"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = ((contain_wide) ? "Wide " : "") + mode_name_type[1] + " Pulse";

                return new string[] { final_mode_name, "Current Harmonic Minimum" };
            }
            if (mode.ToString().StartsWith("SHE"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "Wide " : "" + mode_name_type[1] + " Pulse";

                return new string[] { final_mode_name, "Selective Harmonic Elimination" };
            }
            else
            {
                String[] mode_name_type = mode.ToString().Split("_");
                String mode_name = "";
                if (mode_name_type[0] == "SQ") mode_name = "Square ";

                mode_name += mode_name_type[1] + " Pulse";

                if (control.get_Video_Dipolar() == -1) return new string[] { mode_name };
                else return new string[] { mode_name, "Dipolar : " + control.get_Video_Dipolar().ToString("F1") };
            }
        }

        private static void generate_opening(int image_width, int image_height, VideoWriter vr)
        {
            //opening
            for (int i = 0; i < 128; i++)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);

                LinearGradientBrush gb = new(new System.Drawing.Point(0, 0), new System.Drawing.Point(image_width, image_height), Color.FromArgb(0xFF, 0xFF, 0xFF), Color.FromArgb(0xFD, 0xE0, 0xE0));
                g.FillRectangle(gb, 0, 0, image_width, image_height);

                FontFamily simulator_title = new("Fugaz One");
                Font simulator_title_fnt = new(
                    simulator_title,
                    40,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);
                Font simulator_title_fnt_sub = new(
                    simulator_title,
                    20,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

                FontFamily title_fontFamily = new("Fugaz One");
                Font title_fnt = new(
                    title_fontFamily,
                    40,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

                Brush title_brush = Brushes.Black;

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 0, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 68 - 0);
                g.DrawString("Pulse Mode", title_fnt, title_brush, (int)((i < 40) ? -1000 : (double)((i > 80) ? 17 : 17 * (i - 40) / 40.0)), 8);
                g.FillRectangle(Brushes.Blue, 0, 68, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 226, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 291 - 226);
                g.DrawString("Sine Freq[Hz]", title_fnt, title_brush, (int)((i < 40 + 10) ? -1000 : (double)((i > 80 + 10) ? 17 : 17 * (i - (40 + 10)) / 40.0)), 231);
                g.FillRectangle(Brushes.Blue, 0, 291, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 447, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 513 - 447);
                g.DrawString("Sine Amplitude[%]", title_fnt, title_brush, (int)((i < 40 + 20) ? -1000 : (i > 80 + 20) ? 17 : 17 * (i - (40 + 20)) / 40.0), 452);
                g.FillRectangle(Brushes.Blue, 0, 513, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 669, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 735 - 669);
                g.DrawString("Freerun", title_fnt, title_brush, (int)((i < 40 + 30) ? -1000 : (i > 80 + 30) ? 17 : 17 * (i - (40 + 30)) / 40.0), 674);
                g.FillRectangle(Brushes.LightGray, 0, 735, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 847, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 913 - 847);
                g.DrawString("Brake", title_fnt, title_brush, (int)((i < 40 + 40) ? -1000 : (i > 80 + 40) ? 17 : 17 * (i - (40 + 40)) / 40.0), 852);
                g.FillRectangle(Brushes.LightGray, 0, 913, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb((int)(0xB0 * ((i > 96) ? (128 - i) / 36.0 : 1)), 0x00, 0x00, 0x00)), 0, 0, image_width, image_height);
                int transparency = (int)(0xFF * ((i > 96) ? (128 - i) / 36.0 : 1));
                g.DrawString("C# VVVF Simulator", simulator_title_fnt, new SolidBrush(Color.FromArgb(transparency, 0xFF, 0xFF, 0xFF)), 50, 420);
                g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(transparency, 0xA0, 0xA0, 0xFF))), 0, 464, (int)((i > 20) ? image_width : image_width * i / 20.0), 464);
                g.DrawString("presented by JOTAN", simulator_title_fnt_sub, new SolidBrush(Color.FromArgb(transparency, 0xE0, 0xE0, 0xFF)), 135, 460);

                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Cv2.ImShow("Wave Status View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);
            }


        }

        public static Bitmap Get_Control_Original_Image(VVVF_Values control , bool final_show)
        {
            int image_width = 500;
            int image_height = 1080;

            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);

            Color gradation_color;
            if (control.is_Free_Running())
            {
                gradation_color = Color.FromArgb(0xE0, 0xFD, 0xE0);
            }
            else if (!control.is_Braking())
            {
                gradation_color = Color.FromArgb(0xE0, 0xE0, 0xFD);
            }
            else
            {
                gradation_color = Color.FromArgb(0xFD, 0xE0, 0xE0);
            }


            LinearGradientBrush gb = new(
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image_width, image_height),
                Color.FromArgb(0xFF, 0xFF, 0xFF),
                gradation_color
            );

            g.FillRectangle(gb, 0, 0, image_width, image_height);

            FontFamily title_fontFamily = new("Fugaz One");
            Font title_fnt = new(
               title_fontFamily,
               40,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            FontFamily val_fontFamily = new("Arial Rounded MT Bold");
            Font val_fnt = new(
               val_fontFamily,
               50,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            FontFamily val_mini_fontFamily = new("Arial Rounded MT Bold");
            Font val_mini_fnt = new(
               val_mini_fontFamily,
               25,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            Brush title_brush = Brushes.Black;
            Brush letter_brush = Brushes.Black;

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 0, image_width, 68 - 0);
            g.DrawString("Pulse Mode", title_fnt, title_brush, 17, 8);
            g.FillRectangle(Brushes.Blue, 0, 68, image_width, 8);
            if (!final_show)
            {
                String[] pulse_name = get_Pulse_Name(control);

                g.DrawString(pulse_name[0], val_fnt, letter_brush, 17, 100);

                if (pulse_name.Length > 1)
                {
                    if (pulse_name.Length == 2)
                    {
                        g.DrawString(pulse_name[1], val_mini_fnt, letter_brush, 17, 170);
                    }
                    else if (pulse_name.Length == 3)
                    {
                        g.DrawString(pulse_name[1], val_mini_fnt, letter_brush, 17, 160);
                        g.DrawString(pulse_name[2], val_mini_fnt, letter_brush, 17, 180);
                    }
                }

            }


            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 226, image_width, 291 - 226);
            g.DrawString("Sine Freq[Hz]", title_fnt, title_brush, 17, 231);
            g.FillRectangle(Brushes.Blue, 0, 291, image_width, 8);
            double sine_freq = control.get_Video_Sine_Freq();
            if (!final_show)
                g.DrawString(String.Format("{0:f2}", sine_freq).PadLeft(6), val_fnt, letter_brush, 17, 323);

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 447, image_width, 513 - 447);
            g.DrawString("Sine Amplitude[%]", title_fnt, title_brush, 17, 452);
            g.FillRectangle(Brushes.Blue, 0, 513, image_width, 8);
            if (!final_show)
                g.DrawString(String.Format("{0:f2}", control.get_Video_Sine_Amplitude() * 100).PadLeft(6), val_fnt, letter_brush, 17, 548);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 669, image_width, 735 - 669);
            g.DrawString("Freerun", title_fnt, title_brush, 17, 674);
            g.FillRectangle(Brushes.LightGray, 0, 735, image_width, 8);
            if (!final_show)
                g.DrawString(control.is_Mascon_Off().ToString(), val_fnt, letter_brush, 17, 750);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 847, image_width, 913 - 847);
            g.DrawString("Brake", title_fnt, title_brush, 17, 852);
            g.FillRectangle(Brushes.LightGray, 0, 913, image_width, 8);
            if (!final_show)
                g.DrawString(control.is_Braking().ToString(), val_fnt, letter_brush, 17, 930);

            g.Dispose();

            return image;
        }

        public static void Generate_Control_Original_Video(String output_path, Yaml_VVVF_Sound_Data sound_data)
        {
            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            int fps = 60;

            int image_width = 500;
            int image_height = 1080;
            VideoWriter vr = new(output_path, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));

            if (!vr.IsOpened())
            {
                return;
            }

            generate_opening(image_width, image_height, vr);


            bool loop = true, video_finished, final_show = false, first_show = true;
            int freeze_count = 0;

            while (loop)
            {
                Control_Values cv = new Control_Values
                {
                    brake = control.is_Braking(),
                    mascon_on = !control.is_Mascon_Off(),
                    free_run = control.is_Free_Running(),
                    wave_stat = control.get_Control_Frequency()
                };
                PWM_Calculate_Values calculated_Values = Yaml_VVVF_Wave.calculate_Yaml(control, cv, sound_data);
                Wave_Values value = calculate_values(control, calculated_Values, 0);

                control.set_Sine_Time(0);
                control.set_Saw_Time(0);
                Bitmap image = Get_Control_Original_Image(control, final_show);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                image.Dispose();
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Cv2.ImShow("Wave Status View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

                if (first_show)
                {
                    freeze_count++;
                    if (freeze_count > 60)
                    {
                        freeze_count = 0;
                        first_show = false;
                    }
                    continue;
                }

                video_finished = !Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / fps);
                if (video_finished)
                {
                    final_show = true;
                    freeze_count++;
                }
                if (freeze_count > 60) loop = false;
            }
            vr.Release();
            vr.Dispose();
        }
    }
}
