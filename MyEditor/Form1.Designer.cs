namespace MyEditor
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.myControl1 = new MyEditor.MyControl();
            this.SuspendLayout();
            // 
            // myControl1
            // 
            this.myControl1.AutoScroll = true;
            this.myControl1.BackColor = System.Drawing.Color.White;
            this.myControl1.Location = new System.Drawing.Point(1, 1);
            this.myControl1.Name = "myControl1";
            this.myControl1.Size = new System.Drawing.Size(407, 538);
            this.myControl1.TabIndex = 0;
            this.myControl1.Tag = "";
            this.myControl1.Text = null;
            this.myControl1.Load += new System.EventHandler(this.myControl1_Load);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(409, 538);
            this.Controls.Add(this.myControl1);
            this.Name = "Form1";
            this.Text = "CustomTextBox Enabled";
            this.ResumeLayout(false);

        }

        #endregion

        private MyControl myControl1;
    }
}

