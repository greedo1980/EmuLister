using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using EmuLister.Properties;

namespace EmuLister
{
    public partial class Form1 : Form
    {
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
                return ListPanelSize.Height / (PageSize + 10); 
            }
        }

        float ListSpacingSize
        {
            get
            {
                return ListFontSize / 1.5f;
            }
        }

        int ListLabelHeight
        {
            get
            {
                return (Convert.ToInt32(ListFontSize + (ListFontSize * 0.75)));
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
                DrawRomList(0);
            else
                DrawNoRoms();
        }

        private void DrawNoRoms()
        {
            throw new NotImplementedException();
        }

        private void ApplyDefaultSettings()
        {
            BackColor = Settings.Default.BackgroundColor;

        }

        private List<Rom> GetRoms(int requestedPageIndex)
        {

            List<Rom> result = Rom.GetRoms(Settings.Default.PageSize, requestedPageIndex);

            return result;
        }

        private void DrawRomList(int requestedPageIndex)
        {
            List<Rom> roms = GetRoms(requestedPageIndex);

            if (roms.Count == 0 && requestedPageIndex > currentPage)
            {
                DrawRomList(0);
                return;
            }
            else if (roms.Count == 0 && requestedPageIndex < currentPage)
            {
                DrawRomList(LastPageIndex);
                return;
            }
            else
                currentPage = requestedPageIndex;

            int y = 0;
            int count = 0;

            ListPanel.Controls.Clear();

            foreach (Rom rom in roms)
            {

                LinkLabel linkLabel = new LinkLabel();
                linkLabel.Text = rom.ClippedTitle;

                if (Settings.Default.UpperCaseList)
                    linkLabel.Text = linkLabel.Text.ToUpper();

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

        void linkLabel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            string key = e.KeyCode.ToString();
            
            LinkLabel currentLinkLabel = ((LinkLabel)sender);
            
            //next page
            if (key == Settings.Default.ControllerRight)
            {
                DrawRomList(currentPage + 1);
                SetFocusListItem(0);
            }
            //previous page
            if (key == Settings.Default.ControllerLeft)
            {
                if(currentPage == 0)
                    DrawRomList(LastPageIndex);
                else
                    DrawRomList(currentPage - 1);

                SetFocusListItem(0);
            }

            //end of list go to next page
            if (key == Settings.Default.ControllerDown && ListLinkLabels.IndexOf(currentLinkLabel) == ListLinkLabels.Count - 1)
            {
                DrawRomList(currentPage + 1);
                SetFocusListItem(0);
            }

            //start of list go to previous
            if (key == Settings.Default.ControllerUp && ListLinkLabels.IndexOf(currentLinkLabel) == 0)
            {
                if (currentPage == 0)
                    DrawRomList(LastPageIndex);
                else
                    DrawRomList(currentPage - 1);

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
            Process emulator = new Process();
            emulator.StartInfo.FileName = Settings.Default.EmulatorPath;

            emulator.StartInfo.Arguments = romFile;

            emulator.Start();

            

            // iHandle = FindWindow(null, "Calculator");
            //if (iHandle != IntPtr.Zero)
            //{
            //    SetForegroundWindow(iHandle);
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
            DrawRomList(currentPage);

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
