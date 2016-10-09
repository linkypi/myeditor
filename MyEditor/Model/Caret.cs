using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEditor.Model
{
    public class Caret
    {
        public Point Position { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public Color CurrentColor { get; set; }

        public Color NextColor
        {
            get
            {
                if (this.CurrentColor == ForeColor)
                {
                    CurrentColor = BackColor;
                    return BackColor;
                }
                CurrentColor = ForeColor;
                return ForeColor;
            }
        }

        public Caret() { }
        public Caret(Color backColor)
        {
            this.ForeColor = Color.Black;
            this.BackColor = backColor;
            this.CurrentColor = Color.Black;
        }
    }
}
