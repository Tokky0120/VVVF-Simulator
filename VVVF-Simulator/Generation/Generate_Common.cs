using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VVVF_Simulator.Yaml.Mascon_Control;
using VVVF_Simulator.Yaml.VVVF_Sound;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze;
using Yaml_Mascon_Data = VVVF_Simulator.Yaml.VVVF_Sound.Yaml_VVVF_Sound_Data.Yaml_Mascon_Data;

namespace VVVF_Simulator.Generation
{
    public class Generate_Common
    {
        /// <summary>
        /// この関数は、音声生成や、動画生成時の、マスコンの制御状態等を記述する関数です。
        /// この関数を呼ぶたびに、更新されます。
        /// 
        /// This is a function which will control a acceleration or brake when generating audio or video.
        /// It will be updated everytime this function colled.
        /// </summary>
        /// <returns></returns>
        public static bool Check_For_Freq_Change(VVVF_Values control,Yaml_Mascon_Data_Compiled ymdc, Yaml_Mascon_Data ymd, double add_time)
        {
            return Yaml_Mascon_Control.Check_For_Freq_Change(control, ymdc, ymd, add_time);
        }


        public static void Add_Empty_Frames(int image_width, int image_height,int frames, VideoWriter vr)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
            g.Dispose();
            image.Dispose();
        }

        public static void Add_Image_Frames(Bitmap image, int frames, VideoWriter vr)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
        }

        

        public class GenerationBasicParameter
        {
            public Yaml_Mascon_Data_Compiled masconData { get; set; }
            public Yaml_VVVF_Sound_Data vvvfData { get; set; }
            public ProgressData progressData { get; set; }

            public GenerationBasicParameter(Yaml_Mascon_Data_Compiled yaml_Mascon_Data_Compiled, Yaml_VVVF_Sound_Data yaml_VVVF_Sound_Data, ProgressData progressData)
            {
                this.masconData = yaml_Mascon_Data_Compiled;
                this.vvvfData = yaml_VVVF_Sound_Data;
                this.progressData = progressData;
            }
            public class ProgressData
            {
                public double Progress = 1;
                public double Total = 1;

                public double RelativeProgress
                {
                    get
                    {
                        return Progress / Total * 100;
                    }
                }

                public bool Cancel = false;
            }

        }



    }
}
