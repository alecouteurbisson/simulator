namespace Simulator
{
  partial class WaveViewer
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if(disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.labelCursor = new System.Windows.Forms.Label();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.labelCursor);
      this.splitContainer1.Panel1.Resize += new System.EventHandler(this.ResizeWaveButtons);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ControlDark;
      this.splitContainer1.Panel2.Resize += new System.EventHandler(this.ResizeWaves);
      this.splitContainer1.Panel2.MouseLeave += new System.EventHandler(this.MouseLeaveWaves);
      this.splitContainer1.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveWaves);
      this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintWaveforms);
      this.splitContainer1.Size = new System.Drawing.Size(234, 150);
      this.splitContainer1.SplitterDistance = 78;
      this.splitContainer1.TabIndex = 1;
      this.splitContainer1.Text = "splitContainer1";
      // 
      // labelCursor
      // 
      this.labelCursor.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelCursor.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelCursor.Location = new System.Drawing.Point(0, 0);
      this.labelCursor.Name = "labelCursor";
      this.labelCursor.Size = new System.Drawing.Size(74, 13);
      this.labelCursor.TabIndex = 0;
      this.labelCursor.Text = "No waves";
      this.labelCursor.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.labelCursor.Click += new System.EventHandler(this.ZoomOutFull);
      // 
      // WaveViewer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.splitContainer1);
      this.Name = "WaveViewer";
      this.Size = new System.Drawing.Size(234, 150);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Label labelCursor;
  }
}
