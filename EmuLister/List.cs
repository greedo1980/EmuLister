using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EmuLister.Properties;
namespace EmuLister
{
    public partial class List : Form
    {


        public List()
        {
            InitializeComponent();
            SetFullScreen();
        }

        private void SetFullScreen()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void List_Resize(object sender, EventArgs e)
        {
           
        }
    }
}
