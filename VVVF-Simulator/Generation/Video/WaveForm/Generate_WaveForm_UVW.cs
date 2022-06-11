using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static VVVF_Simulator.VVVF_Calculate;
using static VVVF_Simulator.VVVF_Values;
using static VVVF_Simulator.Generation.Generate_Common;
using VVVF_Simulator.Yaml.VVVF_Sound;
using System.Collections.Generic;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using static VVVF_Simulator.MainWindow;

namespace VVVF_Simulator.Generation.Video.WaveForm
{
    public class Generate_WaveForm_UVW
    {
        public static void generate_wave_UVW(ProgressData progressData, String fileName, Yaml_VVVF_Sound_Data sound_data)
        {
            VVVF_Values control = new();
            control.reset_control_variables();
            control.reset_all_variables();

            Yaml_Mascon_Data ymd = Yaml_Mascon_Manage.Sort().Clone();

            int fps = 60;

            int image_width = 1500;
            int image_height = 1000;

            int calculate_div = 10;

            VideoWriter vr = new VideoWriter(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = ymd.GetEstimatedSteps(1.0 / fps) + 120;

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

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);
                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }

                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {

                control.set_Saw_Time(0);
                control.set_Sine_Time(0);

                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);




                for (int i = 0; i < image_width * calculate_div; i++)
                {
                    int[] points_U = new int[2];
                    int[] points_V = new int[2];
                    int[] points_W = new int[2];

                    for (int j = 0; j < 2; j++)
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

                        points_U[j] = value.U;
                        points_V[j] = value.V;
                        points_W[j] = value.W;

                        if (j == 0)
                        {
                            control.add_Saw_Time(Math.PI / (120000.0 * calculate_div));
                            control.add_Sine_Time(Math.PI / (120000.0 * calculate_div));
                        }
                    }

                    //U
                    g.DrawLine(new Pen(Color.Black),
                        (int)Math.Round(i / (double)calculate_div),
                        points_U[0] * -100 + 300,
                        (int)Math.Round(((points_U[0] != points_U[1]) ? i : i + 1) / (double)calculate_div),
                        points_U[1] * -100 + 300
                    );

                    //V
                    g.DrawLine(new Pen(Color.Black),
                        (int)Math.Round(i / (double)calculate_div),
                        points_V[0] * -100 + 600,
                        (int)Math.Round(((points_V[0] != points_V[1]) ? i : i + 1) / (double)calculate_div),
                        points_V[1] * -100 + 600
                    );

                    //W
                    g.DrawLine(new Pen(Color.Black),
                        (int)Math.Round(i / (double)calculate_div),
                        points_W[0] * -100 + 900,
                        (int)Math.Round(((points_W[0] != points_W[1]) ? i : i + 1) / (double)calculate_div),
                        points_W[1] * -100 + 900
                    );

                }


                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);

                vr.Write(mat);

                g.Dispose();
                image.Dispose();

                loop = Check_For_Freq_Change(control, ymd, sound_data.mascon_data, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
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

                Cv2.ImShow("Wave Form View", mat);
                Cv2.WaitKey(1);
                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

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
