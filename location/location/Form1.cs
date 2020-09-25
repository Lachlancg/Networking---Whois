using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace location
{
    public partial class Form1 : Form
    {
        public Form1(string host, int port, int timeoutPeriod, string protocol, string name, string location)
        {
            InitializeComponent();
            //If values have been entered into the command line then they are retrieved here and added to the relevant boxes
            if (name != "")
            {
                nameText.Text = name;
            }
            if (location != "")
            {
                locationText.Text = location;
            }
            if (host != null)
            {
                hostText.Text = host;
            }
            if (protocol != "")
            {
                protocolComboBox.SelectedItem = protocol;
            }
            portText.Text = port.ToString();
            timeoutText.Text = timeoutPeriod.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //When start button is pressed the values selected in the UI are added to variables
            //before the values are added to variables the system checks that values have been selected properly

            try
            {
                string protocol = "";
                int port = 0;
                int timeout = 0;
                string hostName = "";
                string name = "";
                string slocationText = "";
                bool searchLocation = true;
                int count = 0;
                if (!string.IsNullOrWhiteSpace(comboBoxType.Text))
                {
                    if (comboBoxType.Text == "Search location")
                    {
                        searchLocation = true;
                    }
                    else
                    {
                        searchLocation = false;
                    }
                }
                else
                {
                    MessageBox.Show("Ensure type of request has been selected");
                }
                if (protocolComboBox.Text == "whois")
                {
                    protocol = "";
                    count++;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(protocolComboBox.Text))
                    {
                        protocol = protocolComboBox.Text;
                        count++;
                    }
                    else
                    {
                        MessageBox.Show("Ensure a protocol has been selected");
                    }
                }
                
                if (portText.Text == "")
                {
                    port = 43;
                    count++;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(portText.Text))
                    {
                        port = int.Parse(portText.Text);
                        count++;
                    }
                    else
                    {
                        MessageBox.Show("Ensure you have entered a port number");
                    }
                }
                if (timeoutText.Text == "")
                {
                    timeout = 1000;
                    count++;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(timeoutText.Text))
                    {
                        timeout = int.Parse(timeoutText.Text);
                        count++;
                    }
                    else
                    {
                        MessageBox.Show("Ensure you have entered a time out");
                    }
                }
                if (!string.IsNullOrWhiteSpace(hostText.Text))
                {
                    hostName = hostText.Text;
                    count++;
                }
                else
                {
                    MessageBox.Show("Ensure you have entered a host address");
                }
                if (!string.IsNullOrWhiteSpace(nameText.Text))
                {
                    name = nameText.Text;
                    count++;
                }
                else
                {
                    MessageBox.Show("Ensure you have entered a name");
                }
                if (!string.IsNullOrWhiteSpace(locationText.Text))
                {
                    slocationText = locationText.Text;
                    count++;
                }
                else
                {
                    if (searchLocation == false)
                    {
                        MessageBox.Show("Ensure you have entered a location");
                    }
                }
                if (searchLocation == true)
                {
                    //Count checks how many values have been successfully filled before it sends the request to the server
                    //Then the selecected values are passed to the main program
                    if (count == 5)
                    {
                        logic.startClient(hostName, port, timeout, protocol, name, slocationText);
                    }
                    if (count == 6)
                    {
                        logic.startClient(hostName, port, timeout, protocol, name, slocationText = "");
                    }
                }
                if (searchLocation == false)
                {
                    if (count == 6)
                    {
                        logic.startClient(hostName, port, timeout, protocol, name, slocationText);
                    }
                }

            }
            catch (Exception)
            {


            }
            
        }
    }
}
