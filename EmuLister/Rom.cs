using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace EmuLister
{
    public class Rom
    {
        public string Title { get; set; }
        public string RomFile { get; set; }
        public string ClippedTitle
        {
            get
            {

                string result = Title;

                if (Title.Length > 30)
                    result = Title.Substring(0, 50) + "...";

                return result;
            }
        }

        public Rom(string title, string romFile)
        {
            Title = title;
            RomFile = romFile;
        }

        public static List<Rom> GetRoms(int pageSize, int pageIndex)
        {

            List<Rom> result = AllRoms();

            return result.Skip((pageSize * pageIndex)).Take(pageSize).ToList();
        }

        private static List<Rom> AllRoms()
        {
            List<Rom> result = new List<Rom>();

            XmlDocument doc = new XmlDocument();
            doc.Load(Properties.Settings.Default.RomXmlPath);

            //Display all the book titles.
            XmlNodeList elemList = doc.GetElementsByTagName("rom");
            for (int i = 0; i < elemList.Count; i++)
            {
                XmlNode romNode = elemList[i];

                string title = romNode["title"].InnerText;
                string romFile = romNode["romFile"].InnerText;

                result.Add(new Rom(title, romFile));
            }


            if (Properties.Settings.Default.Alphabetize)
                return result.OrderBy(r => r.Title).ToList();
            else
                return result;
        }
    }
}
