using ecom.TagHitList.Model;
using System.Collections.Generic;
using System.IO;

namespace ecom.TagHitList.Framework
{
    public class FileReaderWriter
    {

        public IList<TagRead> LoadFromFile(string file)
        {
            IList<TagRead> newList = new List<TagRead>();

            if (!File.Exists(file))
            {
                File.Create(file);
                return newList;
            }

            using (var reader = new StreamReader(file))
            {

                var firstLine = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    string serial = values[2];
                    string description = string.Empty;
                    if (values.Length > 3)
                        description = values[3];

                    TagRead newTag = new TagRead() { SerialNumber = serial, Description = description };

                    newList.Add(newTag);
                }


            }
            return newList;
        }

        public void ExportToFile(string file, IEnumerable<TagRead> tags)
        {
            int i = 0;
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine($"Count;Blank;UID;Description");
                foreach (var tag in tags)
                {
                    i++;
                    writer.WriteLine($"{i};tag;{tag.SerialNumber};{tag.Description}");
                }
            }
        }
    }
}
