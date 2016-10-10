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
        private PointF _position;
        public PointF Position
        {
            get { return _position; }
            set
            {
                _position = value;
            }
        }
        public PointF[] Points { get; set; }
        public Color ForeColor { get; set; }
        public Color BackColor { get; set; }
        public Color CurrentColor { get; set; }
        public int WordIndex { get; set; }
        public int LineIndex { get; set; }

        public float WordWidth { get; set; }
        public float WordHeight { get; set; }
        private int _minx;

        private Pen _pen;
        public Pen Pen
        {
            get
            {
                if (_pen == null)
                {
                    _pen = new Pen(NextColor);
                }
                else
                {
                    _pen.Color = NextColor;
                }
                return _pen;
            }
        }

        private Color NextColor
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
            this.LineIndex = 0;

            Points = new PointF[2];
        }

        public void SetBaseValue(float wordHeight, float wordWidth,int minx)
        {
            WordHeight = wordHeight;
            WordWidth = wordWidth;
            _minx = minx;
        }

        public void StepForward()
        {
            //_position.X += WordWidth;
            Points[0].X += WordWidth;
            Points[1].X += WordWidth;

            WordIndex++;
        }

        public void StepBack()
        {
            if (Points[0].X - WordWidth <= _minx) return;

            //_position.X -= WordWidth;
            Points[0].X -= WordWidth;
            Points[1].X -= WordWidth;

            WordIndex--;
        }

        public void Up()
        {
            //_position.Y -= WordHeight;
            Points[0].Y -= WordHeight;
            Points[1].Y -= WordHeight;

            LineIndex--;
        }
        public void Down()
        {
            //_position.Y += WordHeight;
            Points[0].Y += WordHeight;
            Points[1].Y += WordHeight;

            
            LineIndex++;
        }
    }
}
