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
        public PointF[] FrontPoints { get; set; }
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

        private Pen _backPen;
        public Pen BackPen
        {
            get
            {
                if (_backPen == null) _backPen = new Pen(BackColor);
                return _backPen;
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
            FrontPoints = new PointF[2];
        }

        public void SetBaseValue(float wordHeight, float wordWidth,int minx)
        {
            WordHeight = wordHeight;
            WordWidth = wordWidth;
            _minx = minx;
        }

        public void SetMinX(int value) {
            _minx = value;
        }

        public void StepForward()
        {
            RecordFrontPosition();
            Points[0].X += WordWidth;
            Points[1].X += WordWidth;

            WordIndex++;
        }

        public void StepBack()
        {
            if (Points[0].X - WordWidth <= _minx) return;

            RecordFrontPosition();           
            Points[0].X -= WordWidth;
            Points[1].X -= WordWidth;

            WordIndex--;
        }

        public void Up()
        {
            if (LineIndex == 0) return;

            RecordFrontPosition();
            Points[0].Y -= WordHeight;
            Points[1].Y -= WordHeight;

            LineIndex--;
        }

        public void Down(int lines)
        {
            if (LineIndex >= lines - 1) return;

            RecordFrontPosition();
            Points[0].Y += WordHeight;
            Points[1].Y += WordHeight;

            LineIndex++;
        }

        public void NewLine()
        {
            LineIndex++;
            Points[0].X = _minx;
            Points[0].Y = WordHeight * LineIndex;

            Points[1].X = _minx;
            Points[1].Y = WordHeight * (LineIndex + 1);
        }

        private void RecordFrontPosition() {

            FrontPoints[0].X = Points[0].X;
            FrontPoints[0].Y = Points[0].Y;
            FrontPoints[1].X = Points[1].X;
            FrontPoints[1].Y = Points[1].Y;
        }
    }
}
