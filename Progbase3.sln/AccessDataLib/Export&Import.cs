using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
namespace AccessDataLib
{
    public static class Export_Import
    {
        private static XmlSerializer sr = new XmlSerializer(typeof(List<Post>));
        private static StreamReader reader;
        public static void Export(List<Post> posts, string filename)
        {
            StreamWriter output = new StreamWriter(filename);
            XmlWriterSettings set = new XmlWriterSettings();
            set.Indent = true;
            set.NewLineHandling = NewLineHandling.Entitize;
            XmlWriter writer = XmlWriter.Create(output, set);
            sr.Serialize(writer, posts);
        }
        public static List<Post> Import(string filename)
        {
            if(File.Exists(filename))
            {
                reader = new StreamReader(filename);
                List<Post> posts = (List<Post>)sr.Deserialize(reader);
                reader.Close();
                return posts;
            }
            return null;
        }
    }
}