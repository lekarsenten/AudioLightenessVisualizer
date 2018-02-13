using System;
using System.IO;
using System.Xml.Serialization;

namespace AudioVis
{
    public static class SettingsSerializationManager
    {
        public static void Serialize(string fName, object source)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());
            using (FileStream writer = new FileStream(fName, FileMode.Create))
            {
                try
                {
                    xmlSerializer.Serialize(writer, source);
                }
                catch (Exception ex)
                {
                    writer.Close();
                    File.Delete(fName);
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
            catch (Exception ex) when (ex is FileNotFoundException || ex is System.Runtime.Serialization.SerializationException || ex is System.Xml.XmlException || ex is InvalidOperationException)
            {
                //suppress them and load default stuff
                return Activator.CreateInstance(targetType);
            }
        }
    }
}
