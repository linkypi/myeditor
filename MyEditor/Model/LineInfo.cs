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

        public PointF Position { get; set; }
        public List<XChar> Chars { get; set; }
        private float _wordWidth;

        public LineInfo() { }
        public LineInfo(string text,PointF point,float wordWidth) {
            this.Text = text;
            this.Position = point;
            this._wordWidth = wordWidth;

            if (Chars == null) Chars = new List<XChar>();
            if (!string.IsNullOrEmpty(text))
            {
                int index = 0;
                foreach (var item in text)
                {
                    point.X += index * _wordWidth;
                    Chars.Add(new XChar(item, point));
                    index++;
                }
            }
            //else
            //{
            //    Chars.Add(new XChar("",point));
            //}
        }
    }
}
