﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using MyEditor.Model;
using System.Diagnostics;

namespace MyEditor
{
    /// <summary>
    /// 1. 行号显示
    /// 2. 语法高亮
    /// 3. 语法提示
    /// 4. 关键字提示
    /// </summary>
    public partial class MyControl : UserControl
    {
        private Color _dividerColor = ColorTranslator.FromHtml("#999999");
        private int _lines = 1;
        private int _startLine = 0;
        private readonly int _lineHeight = 2;
        private int _fontSize = 14;
        private int _dividerX = 0;
        private bool _drawCaret = false;
        private Caret _caret;
        private Rectangle textAreaRect;
        private Timer _timer;
        private Point[] _textAreaPoints;
        private bool _firstPaint = true;
        private int _rowHeight = 0; // fontheight + lineheight
        private float _fontWidth = 0;
        private int _fontHeigth = 0;
        private Font _font = null;
        private Pen _dividerPen = null;
        private StringFormat _stringFormat;

        private SolidBrush _textBrush;
        private SolidBrush _lineNumberBrush;
        private Graphics _caretGraphics;
        private VScrollBar _vscrollBar;
        private HScrollBar _hscrollBar;

        public int Lines
        {
            get { return _lines; }
            set
            {
                int lines = value;
                var temp = _lines;
                _lines = lines;

                if (lines.ToString().Length != temp.ToString().Length)
                {
                    MoveTextArea(lines > _lines);
                }
                if (this.Parent != null && (lines+1) * _rowHeight > this.Parent.Height - SystemInformation.CaptionHeight) {
                    _startLine++;
                    MoveText(_rowHeight);
                }
                if (this.Parent != null && (lines + 2) * _rowHeight > this.Parent.Height - SystemInformation.CaptionHeight) {
 
                    if (_vscrollBar == null)
                    {
                        _vscrollBar = new VScrollBar();
                        _vscrollBar.Width = 20;
                        _vscrollBar.Height = this.Parent.Height - SystemInformation.CaptionHeight;
                        _vscrollBar.Left = this.Parent.Width - _vscrollBar.Width;
                        _vscrollBar.Top = 0;

                        _vscrollBar.Maximum = 100;
                        _vscrollBar.LargeChange = 100;
                        _vscrollBar.Value = 100;
                        _vscrollBar.Dock = DockStyle.Right;
                        _vscrollBar.ValueChanged += _vscrollBar_ValueChanged;
                        _vscrollBar.Visible = true;
                        this.Controls.Add(_vscrollBar);
                    }
                    else {
                        _vscrollBar.Maximum = 100;
                        _vscrollBar.LargeChange = (int)(((this.Parent.Height - SystemInformation.CaptionHeight) / (lines * _rowHeight*1.0)) * 100);
                        _vscrollBar.Value = 100;
                    }
                }
            }
        }

        void _vscrollBar_ValueChanged(object sender, EventArgs e)
        {
            var rate = _vscrollBar.Value/100.0;
            int index = (int)rate * (this.Parent.Height - SystemInformation.CaptionHeight);

            MoveText(index);
            Invalidate();
        }

        public float MaxLineNumberLength
        {
            get
            {
                return _lines.ToString().Length * _fontWidth;
            }
        }

        public int DividerX { get { return Constants.LEFTINDENT + (int)MaxLineNumberLength + 5; } }

        private Point[] TextAreaPoints
        {
            get
            {
                if (_textAreaPoints == null)
                    _textAreaPoints = new Point[]{ textAreaRect.Location,
                            new Point(textAreaRect.Left, textAreaRect.Top + textAreaRect.Height),
                            new Point(textAreaRect.Left + textAreaRect.Width, textAreaRect.Top + textAreaRect.Height),
                            new Point(textAreaRect.Left + textAreaRect.Width, textAreaRect.Top)};

                return _textAreaPoints;
            }
        }

        private List<LineInfo> _lineInfos = new List<LineInfo>();

        public MyControl()
        {
            InitializeComponent();
        }

        private void MyControl_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw |  
              ControlStyles.OptimizedDoubleBuffer |  
              ControlStyles.AllPaintingInWmPaint, true);  
            this.UpdateStyles();

            _startLine = 1;
            _textBrush = new SolidBrush(Color.Black);
            var fontColor = ColorTranslator.FromHtml("#009999");
            _lineNumberBrush = new SolidBrush(fontColor);

            _caretGraphics = CreateGraphics();
            _caret = new Caret(this.BackColor);
            _font = new Font("宋体", _fontSize, GraphicsUnit.Pixel);
            _dividerPen = new Pen(_dividerColor);

            _stringFormat = StringFormat.GenericTypographic;
            _stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            _timer = new Timer();
            _timer.Interval = 300;
            _timer.Tick += timer_Tick;
            _timer.Start();
        }

 
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.PageUnit = GraphicsUnit.Pixel;
            Graphics graphics = pe.Graphics;

            //BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            //BufferedGraphics buffer = currentContext.Allocate(pe.Graphics, pe.ClipRectangle);
            //Graphics graphics = buffer.Graphics;
            graphics.PageUnit = GraphicsUnit.Pixel;
            graphics.Clear(this.BackColor);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

            if (_firstPaint)
            {
                _firstPaint = false;

                SizeF size = graphics.MeasureString("0", _font, 2, _stringFormat);
                _fontWidth = (int)size.Width;
                _fontHeigth = _font.Height;
                _rowHeight = _font.Height + _lineHeight;
            }
            
            //float maxLineNumberLength = _lines.ToString().Length * _fontWidth;
            //绘制左边分隔线
            //var dividerX = Constants.LEFTINDENT + (int)maxLineNumberLength + 5;
            //graphics.DrawLine(_dividerPen,dividerX, 0, dividerX, 81);
            graphics.DrawLine(_dividerPen, DividerX, 0, DividerX, this.Height);

            #region 绘制区块折叠符号

            //DrawBlockMarker(graphics, (int)maxLineNumberLength + 1, 81, 200);

            #endregion

            #region draw text area

            if (textAreaRect.Height == 0)// || DividerX != _dividerX
            {
                _dividerX = DividerX;
                textAreaRect = new Rectangle(new Point(_dividerX + Constants.TEXTAREAMARGIN, -1), new Size(this.Width - (int)_dividerX, this.Height));

                var point = new Point(textAreaRect.Left + Constants.TEXTAREAPADDING, 0);
                var info = new LineInfo(string.Empty, point, new SizeF(_fontWidth, _rowHeight));
                info.Index = _lineInfos.Count == 0 ? 0 : _lineInfos.Count - 1;
                _lineInfos.Add(info);

                _caret.SetBaseValue(_rowHeight, _fontWidth, textAreaRect.Left+ Constants.TEXTAREAPADDING + 2);
                ReCalcCaret();
            }
            
            graphics.DrawRectangle(new Pen(Color.SteelBlue), textAreaRect);

            #endregion
            
            //draw text
            if (_lineInfos != null)
            {
                //foreach (var item in _lineInfos)
                //{
                //    //if (!item.NeedFlush) continue;

                //    //item.NeedFlush = false;
                //    graphics.DrawString(item.Text, _font, _textBrush, item.Position);
                //}

                for (int index = _startLine-1; index < _lineInfos.Count; index++)
                {
                    var item = _lineInfos[index];
                     graphics.DrawString(item.Text, _font, _textBrush, item.Position);
                }
            }
            //draw line number
            DrawLineNumbers(_startLine, graphics, _font, _stringFormat);
            //DrawLineNumbers(10, 15, 5, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(100, 105, 10, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(1000, 1005, 15, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(10000, 10005, 20, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(100000, 100005, 25, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(1000000, 1000005, 30, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(10000000, 10000005, 35, pe, font, maxLineNumberLength sf);

            //buffer.Render(pe.Graphics);  //呈现图像至关联的Graphics  
            //buffer.Dispose();
            //graphics.Dispose();  

        }

        void timer_Tick(object sender, EventArgs e)
        {
            //_caretGraphics.FillRectangle( new SolidBrush(Color.FromArgb(0,this.BackColor)), textAreaRect);
            _caretGraphics.DrawLine(_caret.Pen, _caret.Points[0], _caret.Points[1]);
            //_caretGraphics.DrawLine(_caret.BackPen, _caret.FrontPoints[0], _caret.FrontPoints[1]);
        }

        private bool IsInRegion(Point input, Point[] points)
        {
            GraphicsPath myGraphicsPath = new GraphicsPath();
            Region myRegion = new Region();
            myGraphicsPath.Reset();
            myGraphicsPath.AddPolygon(points);
            myRegion.MakeEmpty();
            myRegion.Union(myGraphicsPath);
            //返回判断点是否在多边形里
            return myRegion.IsVisible(input);
        }

        /// <summary>
        /// move text area and the text
        /// </summary>
        /// <param name="moveLeft"></param>
        private void MoveTextArea(bool moveLeft)
        {
            textAreaRect = new Rectangle(new Point(DividerX + 5, -1), new Size(this.Width - (int)DividerX, this.Height));

            //move caret
            _caret.SetMinX(textAreaRect.Left + Constants.TEXTAREAPADDING + 2);
            if (moveLeft) { _caret.StepBack(); }
            else { _caret.StepForward(); }
            //move text
            foreach (var item in _lineInfos)
            {
                item.MoveLeftRight(moveLeft);
            }
        }

        private void MoveText(int y) {
            foreach (var item in _lineInfos)
            {
                item.UpDown(y);
            }
        }

        #region Draw
        
        private void DrawBlockMarker(Graphics graphics,int x, int y,int y2)
        {
            x += Constants.LEFTINDENT;
            //draw start block 
            int half = Constants.BLOCKMARKERSIDE / 2;
            graphics.DrawRectangle(new Pen(_dividerColor), x, y, Constants.BLOCKMARKERSIDE, Constants.BLOCKMARKERSIDE);
            graphics.DrawLine(new Pen(_dividerColor), x + 2, y + half, x + 2 + half, y + half);

            //draw end block
            graphics.DrawLine(new Pen(_dividerColor), x + half, y2, x + Constants.BLOCKMARKERSIDE, y2);
        }

        private void DrawLineNumbers(int startline, Graphics graphics, Font font, StringFormat sf)
        {
            for (int i = startline; i < Lines + 1; i++)
            {
                int y = (i - startline) * _rowHeight;
                graphics.DrawString(i.ToString(), font, _lineNumberBrush, new RectangleF(Constants.LEFTINDENT, y, MaxLineNumberLength, font.Height), sf);
            }
        }

        private void ReCalcCaret()
        {
            int line = (int)Math.Ceiling((double)_caret.Position.Y / _rowHeight);
            if (line > _lines || line == 0) line = _lines;

            var lineInfo = _lineInfos.Count >= line && _lineInfos.Count != 0 ? _lineInfos[line - 1] : null;

            int wordIndex = (int)Math.Ceiling((_caret.Position.X) * 1.0 / _fontWidth);
            wordIndex = wordIndex < 0 ? 0 : wordIndex;
            //wordIndex = (lineInfo != null && wordIndex > lineInfo.Text.Length) ? lineInfo.Text.Length : wordIndex;
            //int x = lineInfo != null ? Convert.ToInt32(lineInfo.Position.X + wordIndex * _fontWidth) : (textAreaRect.Left + Constants.TEXTAREAPADDING);
            float x = 0, y1 = 0, y2;
            if (lineInfo == null || lineInfo.Chars.Count == 0)
            {
                x = textAreaRect.Left + Constants.TEXTAREAPADDING;
                y2 = _fontHeigth;
            }
            else
            {
                var chars = lineInfo.Chars;
                wordIndex = (wordIndex > chars.Count ? chars.Count -1 : wordIndex);
                x = chars[wordIndex].Position.X;
                y1 = chars[wordIndex].Position.Y;
                y2 = chars[wordIndex].Position.Y + _fontHeigth;
            }
            //float x = lineInfo != null ?  lineInfo.Chars[wordIndex].Position.X: (textAreaRect.Left + Constants.TEXTAREAPADDING);

            _caret.WordIndex = wordIndex;
            _caret.Points[0] = new PointF(x + 2, y1);// (line - 1) * _fontHeigth);
            _caret.Points[1] = new PointF(x + 2, y2);// line * _fontHeigth);
        }

        #endregion

        #region Events
        
        private void MyControl_Click(object sender, EventArgs e)
        {
            var point = MousePosition;
            _caret.Position = new Point(point.X - this.Parent.Left - this.Left - DividerX - (Constants.TEXTAREAPADDING + Constants.TEXTAREAMARGIN)*2,// 2*(int)_fontWidth, 
                            point.Y - this.Top - this.Parent.Top - SystemInformation.CaptionHeight);
            Trace.WriteLine(string.Format("caret x:{0}, y:{1}.",_caret.Position.X,_caret.Position.Y));
            ReCalcCaret();
            Invalidate();
        }

        private void MyControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            string _char = e.KeyChar.ToString();
            switch (_char)
            {
                case "\r":

                    Lines++;
                    _caret.NewLine();
                    var point = new Point(textAreaRect.Left + Constants.TEXTAREAPADDING, _rowHeight * (Lines - 1));
                    var newInfo = new LineInfo(string.Empty, point, new SizeF(_fontWidth, _fontHeigth + _lineHeight));
                    newInfo.Index = _lineInfos.Count - 1;
                    _lineInfos.Add(newInfo);

                    break;
                case "\b":
                    _caret.StepBack();
                    break;
                default:

                    _caret.StepForward();
                    var editinfo = _lineInfos[_caret.LineIndex];
                    editinfo.AddChar(e.KeyChar);
                    break;
            }

            Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                    _caret.StepForward();
                    break;
                case Keys.Left:
                    _caret.StepBack();
                    break;
                case Keys.Up:
                    _caret.Up();
                    break;
                case Keys.Down:
                    _caret.Down(Lines);
                    break;
                default:

                    break;
            }
            //若要调用KeyDown,这里一定要返回false才行,否则只响应重写方法里的按键.
            //这里调用一下父类方向,相当于调用普通的KeyDown事件.//所以按空格会弹出两个对话框
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void MyControl_Leave(object sender, EventArgs e)
        {
            _timer.Stop();
        }

        public void Dispose() {
            _lineNumberBrush.Dispose();
            _textBrush.Dispose();
            _timer.Dispose();
        }

        private void MyControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsInRegion(e.Location, TextAreaPoints))
            {
                this.Cursor = Cursors.IBeam;
            }
            else {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void MyControl_MouseLeave(object sender, EventArgs e)
        {
            //_timer.Stop();
        }

        private void MyControl_MouseHover(object sender, EventArgs e)
        {
            _timer.Start();
        }

        #endregion

        #region Key map

        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);
        private static char KeyCodeToChar(Keys k)
        {
            int nonVirtualKey = MapVirtualKey((uint)k, 2);
            char mappedChar = Convert.ToChar(nonVirtualKey);
            return mappedChar;
        }

        public static string KeyCodeToStr(Keys k)
        {
            char mappedChar = KeyCodeToChar(k);
            string str = mappedChar.ToString();
            if (Char.IsWhiteSpace(mappedChar) || string.IsNullOrEmpty(str) || mappedChar == '\r' || mappedChar == '\n' || mappedChar == KeyCodeToChar(Keys.F1))
            {
                return k.ToString();
            }
            else
            {
                return str + "";
            }
        }

        #endregion
    }
}
