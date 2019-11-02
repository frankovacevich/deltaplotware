using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace Delta
{
    public class Archivo
    {
        public string DeltaPlotwareVersion = "1.5";
        public string FilePath = "";
        public string FileName = "";
        public int LastId = 1;

        public Opciones Options = new Opciones();
        public List<Serie> Series = new List<Serie>();

        
        public void load_file(String path)
        {
            XmlSerializer reader = new XmlSerializer(typeof(Archivo));
            StreamReader file = new StreamReader(path);

            try
            {
                Archivo loadedFile = (Archivo)reader.Deserialize(file);
                FilePath = loadedFile.FilePath;
                FileName = getFileName(FilePath);
                LastId = loadedFile.LastId;
                Options = loadedFile.Options;
                Series = loadedFile.Series;
            } catch(Exception ex)
            {
                MessageBox.Show("Sorry, there was a problem loading the file. It might be corrupted.\n\nError message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);   
            }

            file.Close();

        }

        public void clear_file()
        {
            Options = new Opciones();
            Series.Clear();
            FileName = "";
            FilePath = "";
            LastId = 1;
        }

        public void save_file(String path)
        {
            XmlSerializer writer = new XmlSerializer(typeof(Archivo));
            FileStream file = File.Create(path);
            writer.Serialize(file, this);
            file.Close();
        }

        public string getFileName(string path)
        {
            path = path.Replace("\\","/");
            int index1 = path.LastIndexOf("/") + 1;
            int index2 = path.LastIndexOf(".");
            return path.Substring(index1, index2 - index1);
        }

    }
}
