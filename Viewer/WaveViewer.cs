using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Simulator
{
  /// <summary>
  /// A waveform viewer
  /// </summary>
  public partial class WaveViewer : UserControl
  {
    /// <summary>
    /// Waveforms from the ReportGenerator
    /// </summary>
    Waveforms waves;

    // Miscellaneous layout  stuff
    private const int WaveHeight = 41;
    private const int WaveSpacing = 3;
    int waveWidth;

    private readonly Panel buttonPanel;
    private readonly Panel wavePanel;

    public Navigator WaveNavigator { get; private set; }

    /// <summary>
    /// Array of waveform graph regions
    /// </summary>
    private Rectangle[] waveRect;

    /// <summary>
    /// The mouse cursor
    /// </summary>
    private readonly LineCursor cursor;

    /// <summary>
    /// The waveform selection buttons
    /// </summary>
    private Button[] waveButtons;

    /// <summary>
    /// Constructor
    /// </summary>
    public WaveViewer()
    {
      InitializeComponent();
      buttonPanel = splitContainer1.Panel1;
      wavePanel = splitContainer1.Panel2;
      SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
      UpdateStyles();
      wavePanel.Cursor = Cursors.Cross;
      cursor = new LineCursor(wavePanel, Rectangle.Empty);
      WaveNavigator = new Navigator { Dock = DockStyle.Top, Height = 20 };
      wavePanel.Controls.Add(WaveNavigator);
      WaveNavigator.OnCursorTimeChanged += UpdateCursorTime;
    }

    /// <summary>
    /// Set the waveforms to be displayed
    /// </summary>
    /// <param name="waveforms"></param>
    public void LoadWaves(Waveforms waveforms)
    {
      waves = waveforms;

      WaveNavigator.Waves = waves;
      waveButtons = new Button[waves.Count];
      waveRect = new Rectangle[waves.Count];

      LayoutWaves();

      for(int i = 0; i < waves.Count; i++)
      {
        var b = new Button
        {
            Text = waves.Names[i],
            Width = buttonPanel.Width - 5,
            Height = WaveHeight,
            Left = 1,
            Top = waveRect[i].Top
        };
        b.Click += SelectWave;
        waveButtons[i] = b;
        buttonPanel.Controls.Add(b);
      }
      labelCursor.Text = "";
    }

    /// <summary>
    /// Layout the waveform display
    /// </summary>
    void LayoutWaves()
    {
      waveWidth = wavePanel.Width;
      WaveNavigator.Height = 20;
      var y = WaveNavigator.Height;

      for(var i = 0; i < waves.Count; i++)
      {
        if(i > 0)
          y += WaveSpacing;
        waveRect[i] = new Rectangle(0, y, waveWidth, WaveHeight);
        y += WaveHeight;
      }

      var cursorZone = new Rectangle(
          0,
          WaveNavigator.Height,
          waveWidth,
          y - WaveNavigator.Height
      );
      cursor.Zone = cursorZone;
    }

    /// <summary>
    /// Paint the waveform graphs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PaintWaveforms(object sender, PaintEventArgs e)
    {
      if(waves == null)
        return;

      var g = e.Graphics;
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      var labelBrush = new SolidBrush(Color.Black);
      var backBrush = new SolidBrush(Color.Beige);
      var weakPen = new Pen(Color.Blue, 2f);
      var strongPen = new Pen(Color.Black, 2f);
      var xPen = new Pen(Color.Red, 2f);
      var uPen = new Pen(Color.Green, 2f);
      var gridPen = new Pen(Color.MediumAquamarine, 2f);
      var gridPenM = new Pen(Color.LightGreen);

      var pen = uPen;

      for(var wave = 0; wave < waves.Count; wave++)
      {
          g.FillRectangle(backBrush, waveRect[wave]);


          int x = 0;
          int y = 0;
          int yo = 20 + waveRect[wave].Top;

          //int gx;
          //for(int i = 0; i <= 400; i += 10)
          //{
          //  gx = (int)((ulong)(i * 1000) * (ulong)waveWidth / (ulong)_displayed);
          //  Pen gpen = (i % 50) == 0 ? gridPen : gridPenM;
          //  g.DrawLine(gpen, gx, waveRect[wave].Top, gx, waveRect[wave].Bottom);
          //}


          Delta dinit = waves[wave][0];
          bool init = true;
          foreach(Delta d in waves[wave])
          {
              int ex = WaveNavigator.TimeToCoord(d.time);

              if(ex < 0)
              {
                  dinit = d;
                  continue;
              }

              if(ex > waveWidth)
                  break;

              if(init)
              {
                  switch(dinit.state)
                  {
                      case "l":
                          y = 18;
                          pen = weakPen;
                          break;

                      case "h":
                          y = -18;
                          pen = weakPen;
                          break;

                      case "L":
                          y = 18;
                          pen = strongPen;
                          break;

                      case "H":
                          y = -18;
                          pen = strongPen;
                          break;

                      case "Z":
                          y = 0;
                          pen = weakPen;
                          break;

                      case "X":
                          y = 0;
                          pen = xPen;
                          break;

                      case "U":
                          y = 0;
                          pen = uPen;
                          break;
                  }
                  init = false;
              }

              switch(d.state)
              {
                  case "l":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = weakPen;
                      g.DrawLine(pen, ex, y + yo, ex, 18 + yo);
                      y = 18;
                      break;

                  case "h":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = weakPen;
                      g.DrawLine(pen, ex, y + yo, ex, yo - 18);
                      y = -18;
                      break;

                  case "L":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = strongPen;
                      g.DrawLine(pen, ex, y + yo, ex, 18 + yo);
                      y = 18;
                      break;

                  case "H":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = strongPen;
                      g.DrawLine(pen, ex, y + yo, ex, yo - 18);
                      y = -18;
                      break;

                  case "Z":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = weakPen;
                      g.DrawLine(pen, ex, y + yo, ex, yo);
                      y = 0;
                      break;

                  case "X":
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      pen = xPen;
                      g.DrawLine(pen, ex, y + yo, ex, yo);
                      y = 0;
                      break;

                  case "U":
                      pen = uPen;
                      g.DrawLine(pen, x, y + yo, ex, y + yo);
                      g.DrawLine(pen, ex, y + yo, ex, yo);
                      y = 0;
                      break;
              }
              x = ex;
          }
          g.DrawLine(pen, x, y + yo, Width, y + yo);
      }
      cursor.Reset();
    }

    /// <summary>
    /// Respond to waveform button clicks
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SelectWave(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Update the mouse cursor time display
    /// Also updates the main waveform cursor if this change
    /// originated from navigator mouse tracking
    /// </summary>
    /// <remarks>
    /// HACK! Time.MaxValue means clear the display and cursor
    /// </remarks>
    /// <param name="t"></param>
    void UpdateCursorTime(Time t)
    {
        // HACK! Time.MaxValue means clear the display and cursor
        labelCursor.Text = t != Time.MaxValue ?
          WaveNavigator.CursorTime.ToString() :
          "";

        // Update waveform cursor if in range
        cursor.Update(WaveNavigator.MouseCursorX >= 0 ?
          new Point(WaveNavigator.MouseCursorX, 25) : 
          Point.Empty);
    }

      /// <summary>
    /// Handler for mouse movements in waves display area
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MouseMoveWaves(object sender, MouseEventArgs e)
    {
        if(WaveNavigator != null)
            WaveNavigator.MouseMoveWaves(e.X);

      cursor.Update(e.Location);
    }

    /// <summary>
    /// Handler for mouse leaving waves display area
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MouseLeaveWaves(object sender, EventArgs e)
    {
      cursor.Update(Point.Empty);
      labelCursor.Text = "";
      WaveNavigator.MouseCursorOff();
    }

    /// <summary>
    /// Handler for wave button area resize
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizeWaveButtons(object sender, EventArgs e)
    {
      if(waves != null)
      {
        for(var i = 0; i < waves.Count; i++)
          waveButtons[i].Width = buttonPanel.Width - 5;
      }
    }

    /// <summary>
    /// Handler for wave display area resize
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizeWaves(object sender, EventArgs e)
    {
      if(waves != null)
        LayoutWaves();
    }

    /// <summary>
    /// Reset zoom when user clicks on time display
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ZoomOutFull(object sender, EventArgs e)
    {
        WaveNavigator.ZoomReset();
    }
  }

  /// <summary>
  /// The line cursor for the waveform display
  /// </summary>
  class LineCursor
  {
    /// <summary>
    /// This zone defines the active area
    /// </summary>
    public Rectangle Zone
    {
      get { return zone; }
      set
      {
        Reset();
        zone = value;
      }
    }
    Rectangle zone;

    /// <summary>
    /// Mummy!
    /// </summary>
    readonly Control parent;

    /// <summary>
    /// True if the cursor is presently drawn
    /// </summary>
    bool drawn;
    int x;

    /// <summary>
    /// Construct from the parent control and active zone
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="zone"></param>
    public LineCursor(Control parent, Rectangle zone)
    {
      this.parent = parent;
      this.zone = zone;
      x = -1;
    }

    /// <summary>
    /// Call reset when the screen has been repainted
    /// or when cursor has been erased for any other reason.
    /// </summary>
    public void Reset()
    {
      drawn = false;
    }

    /// <summary>
    /// Update from mouse x coordinate
    /// </summary>
    /// <param name="mx"></param>
    public void Update(Point mx)
    {
      if(zone.Contains(mx))
      {
        if(drawn)
        {
          if(mx.X == x)
            return;
          Toggle();
        }

        x = mx.X;
        Toggle();
        drawn = true;
      }
      else
      {
        if(drawn)
          Toggle();
        drawn = false;
      }
    }

    /// <summary>
    /// Toggle the cursor line
    /// </summary>
    void Toggle()
    {
      ControlPaint.DrawReversibleLine(parent.PointToScreen(new Point(x, zone.Top)),
                                      parent.PointToScreen(new Point(x, zone.Bottom)),
                                      Color.Beige);
    }
  }

  /// <summary>
  /// Navigator cursor update callback
  /// </summary>
  /// <param name="t"></param>
  public delegate void MouseCursorMoved(Time t);

  /// <summary>
  /// Provides waveform navigation and zooming functions
  /// The navigator bar always represents the entire extent of the
  /// waveform records with the currently displayed section higlighted 
  /// in blue.  The mouse cursor is tracked in the waveform and navigator
  /// areas and the zoomed area can be selected by drag-selecting in the 
  /// navigator
  /// </summary>
  public class Navigator : Panel
  {
    /// <summary>
    /// The waveform records
    /// </summary>
    public Waveforms Waves
    {
      get { return waves; }
      set { waves = value; ZoomReset(); } 
    }
    Waveforms waves;

    /// <summary>
    /// Navigator start time
    /// </summary>
    public Time Start         { get { return waves.Start; } }

    /// <summary>
    /// Navigator end time
    /// </summary>
    public Time End           { get { return waves.End; } }

    /// <summary>
    /// Navigator time duration
    /// </summary>
    public Time Length        { get { return waves.Length; } }

    /// <summary>
    /// Waveform display start time
    /// </summary>
    public Time VisibleStart  { get; private set; }

    /// <summary>
    /// Waveform display end time
    /// </summary>
    public Time VisibleEnd    { get; private set; }
  
    /// <summary>
    /// Waveform display time duration
    /// </summary>
    public Time VisibleLength { get { return VisibleEnd - VisibleStart; } }

    /// <summary>
    /// The mouse cursor time
    /// </summary>
    public Time CursorTime 
    { get; private set; }

    /// <summary>
    /// The mouse cursor x coordinate in the waveform display
    /// (only used when tracking in the navigator, otherwise = -1)
    /// </summary>
    public int MouseCursorX { get; private set; }

    /// <summary>
    /// The navigaor cursor x coordinate
    /// </summary>
    int localCursorX;

    /// <summary>
    /// True when the cursor is displayed (xor drawing)
    /// </summary>
    bool cursorDrawn;

    /// <summary>
    /// Mouse cursor movement event
    /// </summary>
    public event MouseCursorMoved OnCursorTimeChanged;

    /// <summary>
    /// The zoomed area bar dimensions
    /// </summary>
    Rectangle bar;

    /// <summary>
    /// Constructor
    /// </summary>
    public Navigator()
    {
      MouseCursorX = -1;
    }

    /// <summary>
    /// Reset zoom to entire waveform
    /// </summary>
    public void ZoomReset()
    {
      VisibleStart = Start;
      VisibleEnd = End;
      bar = Bounds;
      Invalidate();
      Parent.Invalidate();
    }

    /// <summary>
    /// Repaint the zoomed area bar
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      Brush br = new SolidBrush(Color.PowderBlue);
      e.Graphics.FillRectangle(br, bar);
      cursorDrawn = false;
    }

    /// <summary>
    /// Track the mouse in the navigator and update zoom area
    /// if dragging
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);

      int x = e.X < 0 ? 0 : e.X;
      x = x > Width ? Width : x;

      if(Capture)
      {
        if(x > bar.Left)
        {
          bar.Width = x - bar.Left;
        }
        else
        {
          bar.Offset(x - bar.Left, 0);
          if(bar.Right > Right)
            bar.Width = Right - bar.Left;
        }

        Invalidate();
      }
      MouseCursorX = -1;
      var fraction = (double)x / Width;
      CursorTime = Start + Length * fraction;
      if(CursorTime > VisibleStart)
      {
          var loc = (double)(CursorTime - VisibleStart).Ticks / VisibleLength.Ticks;
        if(loc <= 1.0)
          MouseCursorX = (int)(Width * loc + 0.5);
      }

      if(!Capture)
        UpdateLocalCursor();

      if(OnCursorTimeChanged != null)
          OnCursorTimeChanged(CursorTime);
    }

    /// <summary>
    /// Capture mouse and start zoom area selection
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
      base.OnMouseDown(e);

      Capture = true;
      bar.Location = new Point(e.X, 0);
      //if(e.X > bar.Right)
      //{
      //  bar.Width = e.X - bar.Left;
      //}
      //else
      //{
      //  bar.Width += (bar.Left - e.X);
      //  bar.Offset(e.X - bar.Left, 0);
      //}
    }

    /// <summary>
    /// Release mouse capture and update zoomed area slection
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);

      Capture = false;
      double fraction = (double)bar.Left / (double)Width;
      VisibleStart = Start + Length * fraction;
      fraction = (double)bar.Right / (double)Width;
      VisibleEnd = Start + Length * fraction;
      Parent.Invalidate();
    }

    /// <summary>
    /// Clear cursors and time display by sending Time.MaxValue
    /// to event registrants as a rogue value
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseLeave(EventArgs e)
    {
      base.OnMouseLeave(e);
      MouseCursorOff();

      // Kill the wave window cursor too
      MouseCursorX = -1;

      if(OnCursorTimeChanged != null)
        OnCursorTimeChanged(Time.MaxValue);
    }

    /// <summary>
    /// Process mouse movements from the Waveform window
    /// </summary>
    /// <param name="mouseX"></param>
    public void MouseMoveWaves(int mouseX)
    {
      double fraction =  (double)mouseX / (double)Width;
      CursorTime = VisibleStart + VisibleLength * fraction;

      Debug.WriteLine(string.Format("x = {0}  time = {1}, fraction = {2}, Start = {3}, Length = {4}", mouseX, CursorTime, fraction, VisibleStart, VisibleLength));

      UpdateLocalCursor();

      if(OnCursorTimeChanged != null)
          OnCursorTimeChanged(CursorTime);
    }

    /// <summary>
    /// Locate and update the navigator mouse cursor
    /// </summary>
    void UpdateLocalCursor()
    {
        var fraction = (double)(CursorTime - Start).Ticks / Length.Ticks;
      // Waveform Width is always the same as ours

      int x = (int)(Width * fraction + 0.5);

      if(cursorDrawn)
      {
        if(x == localCursorX)
          return;
        ToggleCursorLine(localCursorX);
      }

      localCursorX = x;
      ToggleCursorLine(x);
      cursorDrawn = true;
    }

    /// <summary>
    /// Disable the navigator mouse cursor when the waveform cursor is disabled
    /// </summary>
    public void MouseCursorOff()
    {
      if(cursorDrawn)
        ToggleCursorLine(localCursorX);

      cursorDrawn = false;
    }

    /// <summary>
    /// Invert the navigator cursor line
    /// </summary>
    /// <param name="x"></param>
    void ToggleCursorLine(int x)
    {

      ControlPaint.DrawReversibleLine(PointToScreen(new Point(x, 0)),
                                      PointToScreen(new Point(x, Height)),
                                      BackColor);
    }

    /// <summary>
    /// Convert a time value to the integer X coordinate in the Waveform window
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public int TimeToCoord(Time t)
    {
      // Off left
      if(t < VisibleStart)
        return -1;

      var fraction = (double)(t - VisibleStart).Ticks / VisibleLength.Ticks;
      // Waveform Width is always the same as ours
      return (int)(Width * fraction);
    }

    /// <summary>
    /// Recalculate the zoomed area bar after a resize event
    /// </summary>
    /// <param name="e"></param>
    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      if(Waves != null)
      {
        var fraction = (double)(VisibleStart - Start).Ticks / Length.Ticks;
        bar.X = (int)(Width * fraction + 0.5);
        fraction = (double)(VisibleEnd - Start).Ticks / Length.Ticks;
        bar.Width = (int)(Width * fraction + 0.5) - bar.X;
        Invalidate();
      }
    }
  }
}
