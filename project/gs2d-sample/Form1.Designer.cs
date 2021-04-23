namespace gs2d_sample
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.motorTrackBar1 = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.comComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.motorTrackBar2 = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.motorTrackBar3 = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.motorTrackBar4 = new System.Windows.Forms.TrackBar();
            this.openButton = new System.Windows.Forms.Button();
            this.reloadButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.motorNumericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.motorNumericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.motorNumericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.motorNumericUpDown4 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown4)).BeginInit();
            this.SuspendLayout();
            // 
            // motorTrackBar1
            // 
            this.motorTrackBar1.LargeChange = 10;
            this.motorTrackBar1.Location = new System.Drawing.Point(53, 86);
            this.motorTrackBar1.Maximum = 150;
            this.motorTrackBar1.Minimum = -150;
            this.motorTrackBar1.Name = "motorTrackBar1";
            this.motorTrackBar1.Size = new System.Drawing.Size(244, 45);
            this.motorTrackBar1.SmallChange = 5;
            this.motorTrackBar1.TabIndex = 0;
            this.motorTrackBar1.TickFrequency = 10;
            this.motorTrackBar1.Scroll += new System.EventHandler(this.motorTrackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Motor1";
            // 
            // comComboBox
            // 
            this.comComboBox.FormattingEnabled = true;
            this.comComboBox.Location = new System.Drawing.Point(12, 12);
            this.comComboBox.Name = "comComboBox";
            this.comComboBox.Size = new System.Drawing.Size(123, 20);
            this.comComboBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Motor2";
            // 
            // motorTrackBar2
            // 
            this.motorTrackBar2.LargeChange = 10;
            this.motorTrackBar2.Location = new System.Drawing.Point(53, 137);
            this.motorTrackBar2.Maximum = 150;
            this.motorTrackBar2.Minimum = -150;
            this.motorTrackBar2.Name = "motorTrackBar2";
            this.motorTrackBar2.Size = new System.Drawing.Size(244, 45);
            this.motorTrackBar2.SmallChange = 5;
            this.motorTrackBar2.TabIndex = 0;
            this.motorTrackBar2.TickFrequency = 10;
            this.motorTrackBar2.Scroll += new System.EventHandler(this.motorTrackBar2_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "Motor3";
            // 
            // motorTrackBar3
            // 
            this.motorTrackBar3.LargeChange = 10;
            this.motorTrackBar3.Location = new System.Drawing.Point(53, 188);
            this.motorTrackBar3.Maximum = 150;
            this.motorTrackBar3.Minimum = -150;
            this.motorTrackBar3.Name = "motorTrackBar3";
            this.motorTrackBar3.Size = new System.Drawing.Size(239, 45);
            this.motorTrackBar3.SmallChange = 5;
            this.motorTrackBar3.TabIndex = 0;
            this.motorTrackBar3.TickFrequency = 10;
            this.motorTrackBar3.Scroll += new System.EventHandler(this.motorTrackBar3_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 248);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Motor4";
            // 
            // motorTrackBar4
            // 
            this.motorTrackBar4.LargeChange = 10;
            this.motorTrackBar4.Location = new System.Drawing.Point(53, 239);
            this.motorTrackBar4.Maximum = 150;
            this.motorTrackBar4.Minimum = -150;
            this.motorTrackBar4.Name = "motorTrackBar4";
            this.motorTrackBar4.Size = new System.Drawing.Size(239, 45);
            this.motorTrackBar4.SmallChange = 5;
            this.motorTrackBar4.TabIndex = 0;
            this.motorTrackBar4.TickFrequency = 10;
            this.motorTrackBar4.Scroll += new System.EventHandler(this.motorTrackBar4_Scroll);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(141, 10);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 9;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // reloadButton
            // 
            this.reloadButton.Location = new System.Drawing.Point(222, 10);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Size = new System.Drawing.Size(75, 23);
            this.reloadButton.TabIndex = 10;
            this.reloadButton.Text = "Reload";
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.reloadButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 326);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 11;
            this.button3.Text = "Sin Wave";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // motorNumericUpDown1
            // 
            this.motorNumericUpDown1.Location = new System.Drawing.Point(303, 93);
            this.motorNumericUpDown1.Name = "motorNumericUpDown1";
            this.motorNumericUpDown1.Size = new System.Drawing.Size(69, 19);
            this.motorNumericUpDown1.TabIndex = 12;
            // 
            // motorNumericUpDown2
            // 
            this.motorNumericUpDown2.Location = new System.Drawing.Point(303, 144);
            this.motorNumericUpDown2.Name = "motorNumericUpDown2";
            this.motorNumericUpDown2.Size = new System.Drawing.Size(69, 19);
            this.motorNumericUpDown2.TabIndex = 13;
            // 
            // motorNumericUpDown3
            // 
            this.motorNumericUpDown3.Location = new System.Drawing.Point(303, 195);
            this.motorNumericUpDown3.Name = "motorNumericUpDown3";
            this.motorNumericUpDown3.Size = new System.Drawing.Size(69, 19);
            this.motorNumericUpDown3.TabIndex = 14;
            // 
            // motorNumericUpDown4
            // 
            this.motorNumericUpDown4.Location = new System.Drawing.Point(303, 246);
            this.motorNumericUpDown4.Name = "motorNumericUpDown4";
            this.motorNumericUpDown4.Size = new System.Drawing.Size(69, 19);
            this.motorNumericUpDown4.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(151, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "Position";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(301, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 12);
            this.label6.TabIndex = 17;
            this.label6.Text = "temperature";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.motorNumericUpDown4);
            this.Controls.Add(this.motorNumericUpDown3);
            this.Controls.Add(this.motorNumericUpDown2);
            this.Controls.Add(this.motorNumericUpDown1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.reloadButton);
            this.Controls.Add(this.openButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.motorTrackBar4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.motorTrackBar3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.motorTrackBar2);
            this.Controls.Add(this.comComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.motorTrackBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "gs2d sample";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorTrackBar4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.motorNumericUpDown4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar motorTrackBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar motorTrackBar2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar motorTrackBar3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar motorTrackBar4;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown motorNumericUpDown1;
        private System.Windows.Forms.NumericUpDown motorNumericUpDown2;
        private System.Windows.Forms.NumericUpDown motorNumericUpDown3;
        private System.Windows.Forms.NumericUpDown motorNumericUpDown4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}

