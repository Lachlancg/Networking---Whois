using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace locationserver
{
    public partial class Form1 : Form
    {
        public Form1(ref string filename, ref string savefile)
        {
            InitializeComponent();
            //If arguements are passed from the command line they are added to the text boxes here
            if (filename != null)
            {
                logfileText.Text = filename;
            }
            if (savefile != null)
            {
                filenameText.Text = savefile;
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //Before the save and load objects are created the error handling checks the boxes are not empty
            
            if (!string.IsNullOrWhiteSpace(logfileText.Text))
            {
                Logging log = new Logging(logfileText.Text);
            }
            if (!string.IsNullOrWhiteSpace(filenameText.Text))
            {
                saving save = new saving(filenameText.Text);
            }
            else
            {
                MessageBox.Show("Ensure text box is not empty");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //When the start button is pressed the window is closed and the main server method is ran.
            this.Close();

            logic.runServer();
        }
       
    }
}
