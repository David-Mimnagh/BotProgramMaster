using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotProgram.Models;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace BotProgram
{
    public class XmlWriter
    {
        public void WriteLevelToFile(Level l, int level_number)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(l.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, l);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save("../../Resources/level_" + level_number.ToString() + ".xml");
                stream.Close();
            }
        }
    }
}
