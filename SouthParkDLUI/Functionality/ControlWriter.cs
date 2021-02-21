using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace SouthParkDLUI.Functionality
{
    public class ControlWriter : TextWriter
    {
        private TextBlock textbox;
        public ControlWriter(TextBlock textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textbox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textbox.Text += value.ToString();
            }));
        }

        public override void Write(string value)
        {
            base.Write(value);
            textbox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textbox.Text += value + "\r\n";
            }));
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
