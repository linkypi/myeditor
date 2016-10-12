<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEditor.Model
{
    public class XChar
    {
        public char Value { get; set; }
        public PointF Position { get; set; }

        public XChar() { }
        public XChar(char value,PointF position) {
            this.Value = value;
            this.Position = position;
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
    public class XChar
    {
        public char Value { get; set; }
        public PointF Position { get; set; }

        public XChar() { }
        public XChar(char value,PointF position) {
            this.Value = value;
            this.Position = position;
        }

        public XChar(char value) {
            this.Value = value;
        }
    }
}
>>>>>>> ea996090963851648529310e6a2bd4d8e5d63609
