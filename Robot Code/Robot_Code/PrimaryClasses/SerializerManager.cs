using System.Collections.Generic;
using System.IO;
using System.Text;
using Motor_Control;
using Newtonsoft.Json;

namespace PrimaryClasses
{
    public static class SerializerManager
    {
        public static List<string> SaveFiles = new List<string> { "/home/pi/Desktop/MotionPaths/path1.json", "/home/pi/Desktop/MotionPaths/path2.json", "/home/pi/Desktop/MotionPaths/path3.json" };
        private static readonly JsonSerializer m_Serializer = new JsonSerializer();
        
        public static void CheckForDirectories()
        {
            if (!Directory.Exists("/home/pi/Desktop/MotionPaths/"))
            {
                Directory.CreateDirectory("/home/pi/Desktop/MotionPaths/");
            }
        }
        public static void SaveMotionPath(Dictionary<int, PathNode> motionPath, int fileToWrite)
        {
            CheckForDirectories();
            using (StreamWriter file = File.CreateText(SaveFiles[fileToWrite]))
            {
                m_Serializer.Serialize(file, motionPath);
            }
        }

        public static Dictionary<int, PathNode> GetMotionPath(int fileToLoad)
        {
            if (File.Exists(SaveFiles[fileToLoad]))
            {
                using (StreamReader file = File.OpenText(SaveFiles[fileToLoad]))
                {
                    return (Dictionary<int, PathNode>)m_Serializer.Deserialize(file, typeof(Dictionary<int, PathNode>));
                }
            }

            return  new Dictionary<int, PathNode>();
        } 

    }
}