using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;
//using System.Diagnostics;

namespace Turuga_Match_v1._0
{
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("70CC157D-A078-4f09-A7B6-A46CED2DC8E2")]

    public interface IManagedInterface2
    {

        void updateBox(String st1);
    }

    [ClassInterface(ClassInterfaceType.None)]
    [Guid("09C0B14E-E06F-4606-B5CF-4D47E0F48A4E")]
    public partial class MessageLogWindow : Form, IManagedInterface2
    {
        List<string> names = new List<string>();
        List<double> percentages = new List<double>();
        List<string> lbitems = new List<string>();
        //Process tinycad = new Process();
        //int selectedLBIndex;
        public MessageLogWindow()
        {
            InitializeComponent();
        }
        public MessageLogWindow(String st1)
        {
            InitializeComponent();
            richTextBox1.ResetText();
            updateBox(st1);
            
        }
        public MessageLogWindow(String st1, List<string> n, List<double> p)
        {
            InitializeComponent();
            richTextBox1.ResetText();
            updateBox(st1);
            names = n;
            percentages = p;
            updateListBoxFiltered();
 
        }
        public void updateListBoxFiltered()
        {
            List<double> temp = percentages;
            temp.Sort();
            lbitems.Clear();
            temp.Reverse();
            String tempString = null;
            for (int i = 0; i < temp.Count; i++)
            {
                int index = percentages.IndexOf(temp[i]);
                tempString = String.Concat(names[index], " (Match % = ", temp[i].ToString(), ")");
                if (!listBox1.Items.Contains(tempString))
                {
                    listBox1.Items.Add(tempString);
                    lbitems.Add(names[index]);
                }
            }
        }
        public void updateListBoxAll()
        {
            List<double> temp = percentages;
            lbitems.Clear();
            String tempString = null;
            for (int i = 0; i < temp.Count; i++)
            {      
                tempString = String.Concat(names[i], " (Architecture Match % = ", temp[i].ToString(), ")");
                if (!listBox1.Items.Contains(tempString))
                {
                    listBox1.Items.Add(tempString);
                    lbitems.Add(names[i]);
                }
            }
        }
        public void updateBox(String st1)
        {
            richTextBox1.Text = st1;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                listBox1.Items.Clear();
                updateListBoxFiltered();
                
                //checkBox1.Checked = true;
            }
            else
            {
                listBox1.Items.Clear();
                updateListBoxAll();
                //checkBox1.Checked = false;
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
            if (listBox1.SelectedItem != null)
            {
                //selectedLBIndex = listBox1.SelectedIndex;
                /*tinycad.StartInfo.FileName = @"C:\Turunga\TinyCad\src\Release\TinyCad.exe";
               

                tinycad.StartInfo.Arguments = foo[0];
                tinycad.Start();*/

                String tempStr = lbitems[listBox1.SelectedIndex];
                tempStr = tempStr.Replace(".net", ".dsn");
                //MessageBox.Show(tempStr);
                //System.Diagnostics.Process.Start(@"C:\Turunga\TinyCad\src\Release\TinyCad.exe C:\Turunga\TinyCad\test\AH-227.dsn");
                //System.Diagnostics.Process.Start(String.Concat(@"C:\Turunga\TinyCad\src\Release\TinyCad.exe ",tempStr));
                System.Diagnostics.Process tcad = new System.Diagnostics.Process();
                tcad.StartInfo.FileName = @"C:\Turunga\TinyCad\src\Release\TinyCad.exe";
                tcad.StartInfo.Arguments = tempStr;
                tcad.Start();
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
           
        }
    }
}
