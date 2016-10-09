using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEditor.Model
{
    public class LineInfo
    {
        public string Text { get; set; }

        public int Length { get { return Text.Length; } }

        public Point Position { get; set; }

        public LineInfo() { }
        public LineInfo(string text,Point point) {
            this.Text = text;
            this.Position = point;
        }
    }
}
