using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace OperationViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void WriteLine(string val)
        {
            textBox1.Text += val + Environment.NewLine;
        }

        public void HandleDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            textBox1.Text = "";
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                List<LINE.Datatypes.Operation> g = (List<LINE.Datatypes.Operation>)DeOp.Deserialize("recv_fetchOperations", File.ReadAllBytes(file));
                foreach (LINE.Datatypes.Operation o in g)
                {
                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(o))
                    {
                        string name = descriptor.Name;

                        object value = descriptor.GetValue(o);
                        WriteLine(name + " = " + value);
                        if (name.Contains("Message"))
                        {
                            foreach (PropertyDescriptor r in TypeDescriptor.GetProperties(o.Message))
                            {
                                string d = r.Name;
                                object v = r.GetValue(o.Message);
                                WriteLine(">>" + d + "=" + v);
                            }
                        }
                    }
                    textBox1.Text += ("--------------------") + Environment.NewLine;
                }
            }
        }
    }
}
