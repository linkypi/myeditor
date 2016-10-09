using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void myControl1_Load(object sender, EventArgs e)
        {
            //this.Leave += this.myControl1.Leave;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.myControl1.Dispose();
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            this.myControl1.MyControl_Leave(sender,e);
        }


      
    }
}
