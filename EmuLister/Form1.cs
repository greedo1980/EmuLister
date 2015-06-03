using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EmuLister.Properties;

namespace EmuLister
{
    public partial class Form1 : Form
    {
        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //int timerCount = 0;

        //Timer timer = new Timer();

        string CurrentPublisher { get; set; }

        bool HasRoms
        {
            get {
                return Rom.GetRoms(1, 0).Count == 1;
            }
        }

      

        int LastPageIndex
        {
            get {
                return Rom.GetRoms(1000000, 0).Count / PageSize;                              
            }
        }

        List<LinkLabel> ListLinkLabels
        {
            get
            {

                List<LinkLabel> result = new List<LinkLabel>();

                foreach (Control c in ListPanel.Controls)
                {

                    if (c.GetType() == typeof(LinkLabel))
                        result.Add((LinkLabel)c);
                }

                return result;
            }

        }

        int currentPage = 0;

        int PageSize {
            get {
                return Settings.Default.PageSize;
            }
        }

        Size ScreenSize
        {
            get {
                return this.Size;                //return Screen.PrimaryScreen.WorkingArea.Size;
            }
        }

        Size ListPanelSize
        {
            get
            {
                return ListPanel.Size;                //return Screen.PrimaryScreen.WorkingArea.Size;
            }
        }

        Color ListHighlightColor
        {
            get {
                return Settings.Default.ListHighlightColor;
            }
        }

        Color ListColor
        {
            get
            {
                return Color.FromArgb(1, 255, 255, 255);
            }
        }

        float ListFontSize {
            get { 
                return (ListPanelSize.Height / PageSize) * 0.6f; 
            }
        }

        float ListSpacingSize
        {
            get
            {
                return ListFontSize * 0.5f;
            }
        }

        int ListLabelHeight
        {
            get
            {
                return (Convert.ToInt32(ListFontSize + (ListFontSize * 0.33f)));
            }
        }

        Font listFont {
            get { 
                

               Font font = new Font(EmuLister.Properties.Settings.Default.BaseFontName, ListFontSize);

               return font;
            }
        
        }

        public Form1()
        {
            InitializeComponent();
            
            if(Settings.Default.FullScreen)
                SetFullScreen();
            
            ApplyDefaultSettings();

            if (HasRoms)
            {
                CurrentPublisher = Rom.FirstPublisher();
                DrawPublisher(CurrentPublisher);
                DrawRomList(CurrentPublisher, 0);
               
            }
            else
                DrawNoRoms();
        }

        private void DrawPublisher(string publisher)
        {
            CurrentPublisher = publisher;

            PublisherPanel.Height = (int)((float)ScreenSize.Height * 0.12f);

            PublisherPanel.Controls.Clear();

            Label publisherLabel = new Label();

            publisherLabel.Font = listFont;
            publisherLabel.Font = new Font(listFont.FontFamily, ListFontSize * 2f);
            publisherLabel.Text = CurrentPublisher;
            publisherLabel.ForeColor = Settings.Default.PublisherColor;
            publisherLabel.TextAlign = ContentAlignment.MiddleCenter;
            publisherLabel.Height = PublisherPanel.Height;
            publisherLabel.Width = PublisherPanel.Width;

            PublisherPanel.Controls.Add(publisherLabel);
        }

        private void DrawNoRoms()
        {
            throw new NotImplementedException();
        }

        private void ApplyDefaultSettings()
        {
            BackColor = Settings.Default.BackgroundColor;
        }

        private List<Rom> GetRoms(string publisher, int requestedPageIndex)
        {
            List<Rom> result = Rom.GetRoms(publisher, Settings.Default.PageSize, requestedPageIndex);

            return result;
        }

        private void DrawRomList(string publisher, int requestedPageIndex)
        {
            List<Rom> roms = GetRoms(publisher, requestedPageIndex);

            if (roms.Count == 0 && requestedPageIndex > currentPage)
            {
                DrawRomList(publisher, 0);
                return;
            }
            else if (roms.Count == 0 && requestedPageIndex < currentPage)
            {
                DrawRomList(publisher, LastPageIndex);
                return;
            }
            else
                currentPage = requestedPageIndex;

            int y = 0;
            int count = 0;

            ListPanel.Height = (int)((float)ScreenSize.Height * 0.78f);
            ListPanel.Location = new Point(ListPanel.Location.X, PublisherPanel.Location.Y + PublisherPanel.Height);
            ListPanel.Controls.Clear();

            foreach (Rom rom in roms)
            {

                LinkLabel linkLabel = new LinkLabel();
                linkLabel.Text = ClipRomTitle(rom.Title);

                if (Settings.Default.UpperCaseList)
                    linkLabel.Text = linkLabel.Text.ToUpper() ;

                if (!string.IsNullOrEmpty(rom.Year))
                    linkLabel.Text += " (" + rom.Year + ")";

                linkLabel.Tag = rom.RomFile;
                linkLabel.Location = new Point(0, y);

                linkLabel.GotFocus += label_GotFocus;
                linkLabel.PreviewKeyDown += linkLabel_PreviewKeyDown;

                linkLabel.Font = listFont;

                linkLabel.Height = ListLabelHeight;
                linkLabel.Width = ScreenSize.Width;
                linkLabel.TextAlign = ContentAlignment.MiddleCenter;
                linkLabel.LinkBehavior = LinkBehavior.NeverUnderline;

                linkLabel.ActiveLinkColor = 
                    linkLabel.LinkColor = ListColor;
           
                ListPanel.Controls.Add(linkLabel);
                
                y += Convert.ToInt32(ListFontSize + ListSpacingSize);
                count++;
            }

            Debug.Print(string.Format("ListPanelSize W{0} W{1}", ListPanelSize.Width, ListPanelSize.Height));

        }

        private string ClipRomTitle(string title)
        {
            string result = title;

            int clipLength = ListPanelSize.Width /10;

            if (result.Length > clipLength)
                result = result.Substring(0, clipLength -3) + "...";

            return result;
        }

        void linkLabel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            string key = e.KeyCode.ToString();
            
            LinkLabel currentLinkLabel = ((LinkLabel)sender);
            
            //next page
            if (key == Settings.Default.ControllerRight)
            {
                //DrawRomList(currentPage + 1);
                string nextPublisher = Rom.NextPublisher(CurrentPublisher);

                DrawPublisher(nextPublisher);
                DrawRomList(nextPublisher, currentPage + 1);
                
                SetFocusListItem(0);
            }
            //previous page
            if (key == Settings.Default.ControllerLeft)
            {
                string previousPublisher = Rom.PreviousPublisher(CurrentPublisher);

                DrawPublisher(previousPublisher);

                if (currentPage == 0)
                    DrawRomList(previousPublisher, LastPageIndex);
                else
                    DrawRomList(previousPublisher, currentPage - 1);

                SetFocusListItem(0);
            }

            //end of list go to next page
            if (key == Settings.Default.ControllerDown && ListLinkLabels.IndexOf(currentLinkLabel) == ListLinkLabels.Count - 1)
            {
                DrawRomList(CurrentPublisher, currentPage + 1);
                SetFocusListItem(0);
            }

            //start of list go to previous
            if (key == Settings.Default.ControllerUp && ListLinkLabels.IndexOf(currentLinkLabel) == 0)
            {
                if (currentPage == 0)
                    DrawRomList(CurrentPublisher, LastPageIndex);
                else
                    DrawRomList(CurrentPublisher, currentPage - 1);

                SetFocusListItem(ListLinkLabels.Count - 1);
            }

            //load rom
            if (key == Settings.Default.ControllerYes)
            {
                string romFile = currentLinkLabel.Tag.ToString();
                LoadRom(romFile);
            }

            //quit
            if (key == Settings.Default.ControllerQuit)
                Close();
        }

        private void LoadRom(string romFile)
        {
            //timer = new Timer();
            //timer.Interval = 1000;
            //timer.Tick += timer_Tick;
            //timerCount = 0;

            //Process process = new Process();
            //process.StartInfo.FileName = @"C:\Windows\Notepad.exe";
            //process.Start();

          



            Process process = new Process();

            process.StartInfo.FileName = Settings.Default.EmulatorPath;

            process.StartInfo.Arguments = romFile;

            process.StartInfo.WorkingDirectory = Settings.Default.WorkingPath;

            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            process.Start();
            
            // Need to wait for process to start

            //timer.Start();
            //IntPtr p = process.MainWindowHandle;
            //ShowWindow(p, 1);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            
                //SendKeys.SendWait("OK");
   
            
            //timerCount++;
            //if(timerCount > 5)
            //timer.Stop();

        }

        void label_GotFocus(object sender, EventArgs e)
        {
            LinkLabel activeLinkLabel = (LinkLabel)sender;

            activeLinkLabel.ActiveLinkColor =
                    activeLinkLabel.LinkColor = ListHighlightColor;

            foreach (Control c in ListPanel.Controls)
            {
                if (c != activeLinkLabel && c.GetType() == typeof(LinkLabel))
                {
                    LinkLabel inactiveLinkLabel = (LinkLabel)c;

                    inactiveLinkLabel.ActiveLinkColor =
                      inactiveLinkLabel.LinkColor = ListColor;
                }
            }

        }

        private void SetFullScreen()
        {
           
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {    
            DrawPublisher(CurrentPublisher);
        
            DrawRomList(CurrentPublisher, currentPage);

        }

        void SetFocusListItem(int index)
        {
            int counter = 0;

            foreach (LinkLabel linkLabel in ListLinkLabels)
            {
                    if (counter == index)
                   
                        linkLabel.Focus();
               
                    else
                        linkLabel.ActiveLinkColor =
                            linkLabel.LinkColor = ListColor;

                    counter++;
            } 
        }
    }
}
