using System;
using System.IO;
using System.Xml.Serialization;

namespace AudioVis
{
    public static class SettingsSerializationManager
    {
        public static void Serialize(string fName, object source)
        {
            XmlSerializer soapSerializer = new XmlSerializer(source.GetType());
            using (FileStream writer = new FileStream("default.xml", FileMode.Create))
            {
                try
                {
                    soapSerializer.Serialize(writer, source.GetType());
                }
                catch (Exception ex)
                {
                    writer.Close();
                    File.Delete("default.xml");
                }
            }
        }
        public static object DeSerialize(string fName, Type targetType)
        {
            try
            {
                using (FileStream reader = new FileStream(fName, FileMode.Open))
                {
                    XmlSerializer soapSerializer = new XmlSerializer(targetType);
                    return soapSerializer.Deserialize(reader);
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is System.Runtime.Serialization.SerializationException || ex is System.Xml.XmlException)
            {
                //suppress them and load default stuff
                return Activator.CreateInstance(targetType);
            }
        }
    }
}
