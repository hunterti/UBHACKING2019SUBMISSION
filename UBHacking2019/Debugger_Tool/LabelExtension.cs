using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Debugger_Tool
{
    class Label_Extension : Label
    {
        public List<string> inputs, outputs;
        public Dictionary<Label, Label> current_mappings;
        public Label next_label;
        public Dictionary<Label, object> value_mappings;
        public MethodInfo methodInfo;
        public List<Label> childLabels;
        bool MovingLabel = false;
        int offsetX = 0;
        int offsetY = 0;
        int label_widths = 100;
        int label_height = 20;
        int init_x;
        int init_y;
        public bool movable = true;
        public Label_Extension(string text, List<string> ins, List<string> outs, int width, int height, int x, int y, Color background, Color foreground, MethodInfo info)
        {
            inputs = ins;
            outputs = outs;
            BackColor = background;
            Width = width;
            Height = height;
            Left = x;
            Top = y;
            init_x = x;
            init_y = y;
            ForeColor = foreground;
            Text = text;
            Font = new Font(FontFamily.GenericSansSerif, 14);
            TextAlign = ContentAlignment.MiddleCenter;
            methodInfo = info;
            current_mappings = new Dictionary<Label, Label>();
            value_mappings = new Dictionary<Label, object>();
            MouseDown += Temp_MouseDown;
            MouseUp += Temp_MouseUp;
            MouseMove += Temp_MouseMove;
            GenerateChildLabels();
        }
        private void Temp_MouseMove(object sender, MouseEventArgs e)
        {
            if (MovingLabel && movable)
            {
                Label_Extension label = (Label_Extension)sender;
                label.Left = e.X + label.Left - offsetX;
                label.Top = e.Y + label.Top - offsetY;
                foreach (Label olabel in label.childLabels)
                {
                    olabel.Left = e.X + olabel.Left - offsetX;
                    olabel.Top = e.Y + olabel.Top - offsetY;
                }
            }
        }

        private void Temp_MouseUp(object sender, MouseEventArgs e)
        {
            MovingLabel = false;
        }

        private void Temp_MouseDown(object sender, MouseEventArgs e)
        {
            MovingLabel = true;
            offsetX = e.X;
            offsetY = e.Y;
        }
        public void GenerateChildLabels() 
        {
            int relativeXIn = init_x - label_widths;
            int relativeYIn = 10;
            childLabels = new List<Label>();
            childLabels.Add(generateLabel("Input Thread", label_widths, label_height, relativeXIn, relativeYIn, Color.DarkRed, Color.Black));
            foreach (string ins in inputs)
            {
                relativeXIn = relativeXIn;
                relativeYIn = relativeYIn + 25;
                childLabels.Add(generateLabel(ins, label_widths, label_height, relativeXIn, relativeYIn, Color.Orange, Color.Black));
            }
            relativeYIn = 10;
            relativeXIn = init_x + Width;
            childLabels.Add(generateLabel("Output Thread", label_widths + 20, label_height, relativeXIn, relativeYIn, Color.DarkRed, Color.Black));
            foreach (string outs in outputs)
            {
                relativeXIn = relativeXIn;
                relativeYIn = relativeYIn + 25;
                childLabels.Add(generateLabel(outs, label_widths + 20, label_height, relativeXIn, relativeYIn, Color.Orange, Color.Black));
            }
        }
        public Label generateLabel(string text, int width, int height, int x, int y, Color background, Color foreground)
        {
            Label retval = new Label();
            int left = x;
            int top = y;
            retval.Width = width;
            retval.Height = height;
            retval.Text = text;
            retval.Location = new Point(left, top);
            retval.BackColor = background;
            retval.ForeColor = foreground;
            return retval;
        }
    }
}
