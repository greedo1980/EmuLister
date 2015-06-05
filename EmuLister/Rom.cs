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
        public string Year { get; set; }
        string _publisher;

        public string Publisher
        {
            get
            {
                if (string.IsNullOrEmpty(_publisher))
                    return "Unknown";
                else
                    return _publisher;
            }
            set
            {
                _publisher = value;
            }
        }       

        public Rom(string title, string romFile, string year, string publisher)
        {
            Title = title;
            RomFile = romFile;
            Year = year;
            _publisher = publisher;
        }

        public static List<Rom> GetRoms(int pageSize, int pageIndex)
        {
            List<Rom> result = AllRoms();

            return result.Skip((pageSize * pageIndex)).Take(pageSize).ToList();
        }

        public static List<Rom> GetRoms(string publisher, int pageSize, int pageIndex)
        {
            List<Rom> result = AllRoms();

            return result.Where(r => r.Publisher == publisher).Skip((pageSize * pageIndex)).Take(pageSize).ToList();
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
                string year = null;
                if (romNode["year"] != null)
                    year = romNode["year"].InnerText;

                string publisher = null;
                if (romNode["publisher"] != null)
                    publisher = romNode["publisher"].InnerText;

                result.Add(new Rom(title, romFile, year, publisher));
            }


            if (Properties.Settings.Default.Alphabetize)
                return result.OrderBy(r => r.Title).ToList();
            else
                return result;
        }


        public static List<string> AllPublishers
        {
            get {
                return AllRoms().OrderBy(r => r.Publisher).Select(p => p.Publisher).Distinct().ToList();
            }
        }

        public static string FirstPublisher()
        {
            List<Rom> roms = AllRoms();

            return roms.OrderBy(r => r.Publisher).First().Publisher;
        }

        public static string LastPublisher()
        {
            List<Rom> roms = AllRoms();

            return roms.OrderBy(r => r.Publisher).Last().Publisher;
        }

        public static int TotalPagesForPublisher(string publisher, int pageSize)
        {
            int publisherRomCount = AllRoms().Where(p => p.Publisher == publisher).Count();

            return (publisherRomCount / pageSize) +1;
        }

        public static string NextPublisher(string publisher)
        {
            if (publisher == LastPublisher())
                return FirstPublisher();

            List<Rom> roms = AllRoms();

            List<string> publishers = roms.OrderBy(r => r.Publisher).Select(r => r.Publisher).Distinct().ToList();

            return publishers[publishers.IndexOf(publisher) + 1];
        }

        public static string PreviousPublisher(string publisher)
        {
            if (publisher == FirstPublisher())
                return LastPublisher();

            List<Rom> roms = AllRoms();

            List<string> publishers = roms.OrderBy(r => r.Publisher).Select(r => r.Publisher).Distinct().ToList();

            return publishers[publishers.IndexOf(publisher) - 1];
        }
    }

}
