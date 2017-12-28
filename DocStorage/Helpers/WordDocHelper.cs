using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using GemBox.Document;

namespace DocStorage.Models
{
    public class WordDocHelper
    {
        public static Document Create(String UserName, String FileName)
        {

            string filePath = HttpContext.Current.Server.MapPath(string.Format("../Storage/{0}", DateTime.Now.ToString("yyyyMMddhhmmss")));

            CheckDirectory(filePath);

            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            DocumentModel document = new DocumentModel();

            Section section = new Section(document);
            document.Sections.Add(section);

            Paragraph paragraph = new Paragraph(document);
            section.Blocks.Add(paragraph);

            Run run = new Run(document, UserName + " " + FileName);
            paragraph.Inlines.Add(run);

            var fullPath = filePath + "\\" + FileName + ".docx";

            document.Save(fullPath);
        
            Document doc = new Document() {
                DocName = FileName + ".docx",
                Path = fullPath,
                Created = DateTime.Now
            };

            //Open(doc.Path);

            return doc;
        }

        public static void Open(String path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception)
            {         
                throw;
            }

        }

        public static void Delete(String path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                
                throw;
            }           
        }

        public static void CheckDirectory(String path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
    }
}