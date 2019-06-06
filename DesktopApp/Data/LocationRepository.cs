using DesktopApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace DesktopApp.Data
{
    class LocationRepository
    {
        internal static void ReadFile(string fileName, out ObservableCollection<Location> locations)
        {
            // Create an instance of the XmlSerializer class and specify the type of object to deserialize.
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Location>));
            TextReader reader = new StreamReader(fileName);
            // Deserialize all the locations.
            locations = (ObservableCollection<Location>)serializer.Deserialize(reader);
            reader.Close();
        }

        internal static void SaveFile(string fileName, ObservableCollection<Location> locations)
        {
            // Create an instance of the XmlSerializer class and specify the type of object to serialize.
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Location>));
            TextWriter writer = new StreamWriter(fileName);
            // Serialize all the locations.
            serializer.Serialize(writer, locations);
            writer.Close();
        }
    }
}
