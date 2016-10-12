<<<<<<< HEAD
﻿using System;
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
                float X = point.X;
                foreach (var item in text)
                {
                    float x = X + index * _wordWidth;
                    Chars.Add(new XChar(item, new PointF(x,point.Y)));
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
=======
﻿using System;
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
        public int Index { get; set; }
        public int Length { get { return Text.Length; } }

        public PointF Position { get; set; }
        public List<XChar> Chars { get; set; }
        public SizeF FontSize { get; set; }
        public bool NeedFlush { get; set; }

        public LineInfo() { }
        public LineInfo(string text,PointF point,SizeF fontSize) {

            this.Text = text;
            this.Position = point;
            this.FontSize = fontSize;
            this.NeedFlush = true;
            
            if (Chars == null) Chars = new List<XChar>();
            if (!string.IsNullOrEmpty(text))
            {
                int index = 0;
                float X = point.X;
                foreach (var item in text)
                {
                    float x = X + index * fontSize.Width;
                    Chars.Add(new XChar(item, new PointF(x,point.Y)));
                    index++;
                }
            }
        }

        public void AddChar(char xchar)
        {
            this.NeedFlush = true;
            this.Text += xchar.ToString();
            var cha = new XChar(xchar);
            if (Chars.Count == 0)
            {
                cha.Position = new PointF(this.Position.X, FontSize.Height * (Index + 1));
            }
            else
            {
                var lastChar = Chars[Chars.Count - 1];
                cha.Position = new PointF(lastChar.Position.X + FontSize.Width, lastChar.Position.Y);
            }
            Chars.Add(cha);
        }

        public void Move(bool moveLeft) {
            if (moveLeft)
            {
                MoveLeft();
            }
            else {
                MoveRight();
            }
        }

        private void MoveLeft()
        {
            this.Position = new PointF(this.Position.X - FontSize.Width, this.Position.Y);
            if (Chars != null)
            {
                this.NeedFlush = true;  
                foreach (var item in Chars)
                {
                    item.Position = new PointF(item.Position.X - FontSize.Width, item.Position.Y);
                }
            }
        }

        private void MoveRight()
        {
            this.Position = new PointF(this.Position.X + FontSize.Width, this.Position.Y);
            if (Chars != null)
            {
                this.NeedFlush = true;
                foreach (var item in Chars)
                {
                    item.Position = new PointF(item.Position.X + FontSize.Width, item.Position.Y);
                }
            }
        }
    }
}
>>>>>>> ea996090963851648529310e6a2bd4d8e5d63609
