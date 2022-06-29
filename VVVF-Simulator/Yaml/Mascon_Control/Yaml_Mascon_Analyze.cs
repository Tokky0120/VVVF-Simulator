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

            public List<Yaml_Mascon_Data_Point> points = new();

            public Yaml_Mascon_Data Clone()
            {
                Yaml_Mascon_Data ymd = (Yaml_Mascon_Data)MemberwiseClone();

                List<Yaml_Mascon_Data_Point> clone_points = new();
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
                    Yaml_Mascon_Data_Point point = points[i];
                    totalDuration += point.duration > 0 ? point.duration : 0;
                }

                return totalDuration / sampleTime;
            }

            public Yaml_Mascon_Data_Compiled GetCompiled()
            {
                return new Yaml_Mascon_Data_Compiled(this);
            }

            public class Yaml_Mascon_Data_Point {

                public int order { get; set; } = 0;
                public double rate { get; set; } = 0; //Hz / s
                public double duration { get; set; } = 0;// S
                public bool brake { get; set; } = false;
                public bool mascon_on { get; set; } = true;


                public Yaml_Mascon_Data_Point Clone()
                {
                    return (Yaml_Mascon_Data_Point)MemberwiseClone();
                }
            }

        }

        public class Yaml_Mascon_Data_Compiled
        {
            public List<Yaml_Mascon_Data_Compiled_Point> Points { get; set; } = new List<Yaml_Mascon_Data_Compiled_Point>();

            public Yaml_Mascon_Data_Compiled(Yaml_Mascon_Data ymd)
            {
                Yaml_Mascon_Data _ymd = ymd.Clone();
                _ymd.points.Sort((a, b) => a.order - b.order);

                double currentTime = 0;
                double currentFrequency = 0;
                for(int i = 0; i < _ymd.points.Count; i++)
                {
                    Yaml_Mascon_Data_Point yaml_Mascon_Data_Point = _ymd.points[i];
                    if(yaml_Mascon_Data_Point.duration == -1)
                    {
                        currentFrequency = yaml_Mascon_Data_Point.rate;
                        continue;
                    }

                    double deltaTime = yaml_Mascon_Data_Point.duration;
                    double deltaFrequency = deltaTime * yaml_Mascon_Data_Point.rate * (yaml_Mascon_Data_Point.brake ? -1 : 1);
                    Yaml_Mascon_Data_Compiled_Point yaml_Mascon_Data_Compiled_Point = new()
                    {
                        StartTime = currentTime,
                        EndTime = currentTime + deltaTime,
                        StartFrequency = currentFrequency,
                        EndFrequency = currentFrequency + deltaFrequency,
                        IsMasconOn = yaml_Mascon_Data_Point.mascon_on
                    };
                    Points.Add(yaml_Mascon_Data_Compiled_Point);

                    currentTime += deltaTime;
                    currentFrequency += deltaFrequency;
                }
            }

            public double GetEstimatedSteps(double sampleTime)
            {
                double totalTime = this.Points.Last().EndTime;
                return totalTime / sampleTime;
            }

            public class Yaml_Mascon_Data_Compiled_Point
            {
                public double StartTime { get; set; } = 0;
                public double EndTime { get; set; } = 0;
                public double StartFrequency { get; set; } = 0;
                public double EndFrequency { get; set; } = 0;
                public Boolean IsMasconOn { get; set; } = true;

                public Boolean IsAccel()
                {
                    return EndFrequency - StartFrequency > 0;
                }

                public Yaml_Mascon_Data_Compiled_Point Clone()
                {
                    return (Yaml_Mascon_Data_Compiled_Point)MemberwiseClone();
                }
            }
        }

        public class Yaml_Mascon_Manage
        {

            public static Yaml_Mascon_Data DefaultData = new()
            {
                points = new()
                {
                    new Yaml_Mascon_Data_Point()
                    {
                        rate = 5,
                        duration = 20,
                        brake = false,
                        mascon_on = true,
                        order = 0,
                    },
                    new Yaml_Mascon_Data_Point()
                    {
                        rate = 0,
                        duration = 4,
                        brake = false,
                        mascon_on = false,
                        order = 1,
                    },
                    new Yaml_Mascon_Data_Point()
                    {
                        rate = 5,
                        duration = 20,
                        brake = true,
                        mascon_on = true,
                        order = 2,
                    },
                }
            };
            public static Yaml_Mascon_Data CurrentData = DefaultData.Clone();

            public static Yaml_Mascon_Data Sort()
            {
                CurrentData.points.Sort((a, b) => Math.Sign(a.order - b.order));
                return CurrentData;
            }
            public static bool save_Yaml(String path)
            {
                try
                {
                    using TextWriter writer = File.CreateText(path);
                    var serializer = new Serializer();
                    serializer.Serialize(writer, CurrentData);
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
                    CurrentData = deserializeObject;
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
