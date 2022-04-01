using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.VVVF_Structs;
using static VVVF_Simulator.VVVF_Calculate;
using System.Diagnostics;

namespace VVVF_Simulator.Generation.Video.Control_Info
{
    public class Generate_Control_Common
    {

        public class Pre_Voltage_Data
        {
            public bool enable;
            public double value;
            public Pre_Voltage_Data(bool b, double k)
            {
                enable = b;
                value = k;
            }
        }
    
        /// <summary>
        /// Do clone about control!
        /// </summary>
        /// <param name="sound_data"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public static double Get_Voltage_Rate(Yaml_VVVF_Sound_Data sound_data, VVVF_Values control, bool precise)
        {
            double hex_div_seed_amp = (control.get_Sine_Freq() > 0 && control.get_Sine_Freq() < 1) ? 1 / control.get_Sine_Freq() : 1;
            double hex_div_seed = 20000 * (precise ? hex_div_seed_amp : 1);
            int hex_div = (int)Math.Round(6 * hex_div_seed);

            double[] hexagon_coordinate = new double[] { 0, 0 };

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

            if (cv.wave_stat == 0) return 0;

            double val = 0;

            double max_x = 0;
            try
            {
                for (int i = 0; i < hex_div; i++)
                {
                    control.add_Sine_Time(1.0 / hex_div * ((control.get_Sine_Freq() == 0) ? 0 : 1 / control.get_Sine_Freq()));
                    control.add_Saw_Time(1.0 / hex_div * ((control.get_Sine_Freq() == 0) ? 0 : 1 / control.get_Sine_Freq()));

                    Wave_Values value = calculate_values(control, calculated_Values, 0);

                    double move_x = -0.5 * value.W - 0.5 * value.V + value.U;
                    double move_y = -0.866025403784438646763 * value.W + 0.866025403784438646763 * value.V;
                    double int_move_x = 200 * move_x / hex_div_seed;
                    double int_move_y = 200 * move_y / hex_div_seed;

                    hexagon_coordinate[0] = hexagon_coordinate[0] + int_move_x;
                    hexagon_coordinate[1] = hexagon_coordinate[1] + int_move_y;

                    if (i < hex_div / 2)
                    {
                        if (max_x < hexagon_coordinate[0]) max_x = hexagon_coordinate[0];
                    }
                    else
                    {
                        double len = Math.Sqrt(Math.Pow(hexagon_coordinate[0] - max_x / 2.0, 2) + Math.Pow(hexagon_coordinate[1], 2));
                        val += len / hex_div;
                    }


                }
            }
            catch
            {
                return -1;
            }
            double rate = val / 182.4 * 100.0;
            return Math.Round(rate,2);
        }


        public static void filled_corner_curved_rectangle(Graphics g, Brush br, Point start, Point end, int round_radius)
        {
            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.FillRectangle(br, start.X + round_radius, start.Y, width - 2 * round_radius, height);
            g.FillRectangle(br, start.X, start.Y + round_radius, round_radius, height - 2 * round_radius);
            g.FillRectangle(br, end.X - round_radius, start.Y + round_radius, round_radius, height - 2 * round_radius);

            g.FillEllipse(br, start.X, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);

        }

        public static Point center_text_with_filled_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Brush br,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {
            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            filled_corner_curved_rectangle(g, br, start, end, round_radius);

            Point str_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));

            g.DrawString(str, fnt, str_br, str_pos);

            return str_pos;




        }

        public static void line_corner_curved_rectangle(Graphics g, Pen pen, Point start, Point end, int round_radius)
        {
            int width = (int)(end.X - start.X);
            int height = (int)(end.Y - start.Y);

            g.DrawLine(pen, start.X + round_radius, start.Y, end.X - round_radius + 1, start.Y);
            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static void title_str_with_line_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.DrawLine(pen, start.X + round_radius, start.Y, start.X + width / 2 - strSize.Width / 2 - 10, start.Y);
            g.DrawLine(pen, end.X - round_radius + 1, start.Y, start.X + width / 2 + strSize.Width / 2 + 10, start.Y);

            g.DrawString(str, fnt, str_br, start.X + width / 2 - strSize.Width / 2 + str_compen.X, start.Y - fnt.Height / 2 + str_compen.Y);

            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static Point center_text_with_line_corner_curved_rectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);
            int width = end.X - start.X;
            int height = end.Y - start.Y;
            line_corner_curved_rectangle(g, pen, start, end, round_radius);

            Point string_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));
            g.DrawString(str, fnt, str_br, string_pos);

            return string_pos;


        }

    }
}
