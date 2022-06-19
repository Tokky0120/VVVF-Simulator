using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static VVVF_Simulator.Yaml.Mascon_Control.Yaml_Mascon_Analyze.Yaml_Mascon_Data;

namespace VVVF_Simulator.Yaml.Mascon_Control
{
    public class Yaml_Mascon_Analyze
    {
        public class Yaml_Mascon_Data
        {

            public List<Yaml_Mascon_Point_Data> points = new List<Yaml_Mascon_Point_Data>();

            public Yaml_Mascon_Data Clone()
            {
                Yaml_Mascon_Data ymd = (Yaml_Mascon_Data)MemberwiseClone();

                List<Yaml_Mascon_Point_Data> clone_points = new List<Yaml_Mascon_Point_Data>();
                for(int i = 0; i < points.Count; i++)
                {
                    clone_points.Add(points[i].Clone());
                }

                ymd.points = clone_points;
                return ymd;

            }

            public double GetEstimatedSteps(double sampleTime)
            {
                double totalDuration = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    Yaml_Mascon_Point_Data point = points[i];
                    totalDuration += point.duration > 0 ? point.duration : 0;
                }

                return totalDuration / sampleTime;
            }

            public class Yaml_Mascon_Point_Data {

                public int order { get; set; } = 0;
                public double rate { get; set; } = 0; //Hz / s
                public double duration { get; set; } = 0;// S
                public bool brake { get; set; } = false;
                public bool mascon_on { get; set; } = true;


                public Yaml_Mascon_Point_Data Clone()
                {
                    return (Yaml_Mascon_Point_Data)MemberwiseClone();
                }
            }

        }

        public class Yaml_Mascon_Manage
        {

            public static Yaml_Mascon_Data default_data = new()
            {
                points = new()
                {
                    new Yaml_Mascon_Point_Data()
                    {
                        rate = 5,
                        duration = 20,
                        brake = false,
                        mascon_on = true,
                        order = 0,
                    },
                    new Yaml_Mascon_Point_Data()
                    {
                        rate = 0,
                        duration = 4,
                        brake = false,
                        mascon_on = false,
                        order = 1,
                    },
                    new Yaml_Mascon_Point_Data()
                    {
                        rate = 5,
                        duration = 20,
                        brake = true,
                        mascon_on = true,
                        order = 2,
                    },
                }
            };
            public static Yaml_Mascon_Data current_data = default_data.Clone();

            public static Yaml_Mascon_Data Sort()
            {
                current_data.points.Sort((a, b) => Math.Sign(a.order - b.order));
                return current_data;
            }
            public static bool save_Yaml(String path)
            {
                try
                {
                    using TextWriter writer = File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, current_data);
                    writer.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static bool load_Yaml(String path)
            {
                try
                {
                    var input = new StreamReader(path, Encoding.UTF8);
                    var deserializer = new Deserializer();
                    Yaml_Mascon_Data deserializeObject = deserializer.Deserialize<Yaml_Mascon_Data>(input);
                    current_data = deserializeObject;
                    input.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static Yaml_Mascon_Data DeepClone(Yaml_Mascon_Data src)
            {
                Yaml_Mascon_Data deserializeObject = new Deserializer().Deserialize<Yaml_Mascon_Data>(new Serializer().Serialize(src));
                return deserializeObject;
            }

        }
        
    }
}
