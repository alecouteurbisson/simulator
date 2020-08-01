using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simulator
{
  public partial class Form1 : Form
  {
    WaveViewer viewer;
    static List<Time> times = new List<Time>();

    public Form1()
    {
      InitializeComponent();

      label1.Text = "";
      SuspendLayout();

      listBox1.Items.Clear();
      form = this;

      viewer = new WaveViewer();
      panel2.Controls.Add(viewer);
      viewer.BringToFront();
      viewer.Dock = DockStyle.Fill;
      panel2.Dock = DockStyle.Fill;
      ResumeLayout();

      // TestSpdif();
      // TestXorPh();
      // TestBus();
      // TestMono();
      // TestDFF();
      TestAdder();

      Sim.Reporter.Waves.Freeze();
      viewer.LoadWaves(Sim.Reporter.Waves);
      viewer.WaveNavigator.OnCursorTimeChanged += OnMouseCursor;
    }

    private void RunClick(object sender, EventArgs e)
    {
    //  Sim.Core.FullReset();
    //  Sim.Core.RunTo(new Time("400n"));
    }

    public static void ListAdd(Time time, string text, params object[] objs)
    {
      form.listBox1.Items.Add(string.Format(text, objs));
      times.Add(time);
    }

    public void OnMouseCursor(Time time)
    {
      if(time == Time.MaxValue)
      {
        listBox1.SelectedIndex = -1;
        return;
      }
        
      var index = 0;
      if(time > Time.Zero)
      {
        index = times.BinarySearch(time);

        if(index < 0)
          index = -2 - index;

        if(index >= form.listBox1.Items.Count)
          index = form.listBox1.Items.Count - 1;
      }
      listBox1.SelectedIndex = index;
    }

    static void TestSR_Latch()
    {
      // Declare the signals and define their reporting names
      var _s = new Signal("set", Logic.H);
      var _r = new Signal("rst", Logic.H);
      var  q = new Signal("q  ");
      var _q = new Signal("~q ");

      // Wire up some devices (an SR latch)
      new Nand2("U1", _s, _q, q);
      new Nand2("U2", _r, q, _q);

      // Report changes on specified signals
      _s.Display();
      _r.Display();
       q.Display();
      _q.Display();

      // Queue up some events to drive everything
      _s.SetAfter(Logic.L, new Time("50n"));    // set
      _s.SetAfter(Logic.H, new Time("100n"));
      _r.SetAfter(Logic.L, new Time("150n"));   // reset
      _s.SetAfter(Logic.L, new Time("200n"));
      _s.SetAfter(Logic.H, new Time("250n"));
      _r.SetAfter(Logic.H, new Time("300n"));


      // Apply the power!
      Sim.Core.RunTo(new Time("400n"));
    }

    static void TestDFF()
    {
      // Declare the signals and define their reporting names
      var clk  = new Signal("clk");
      var clk2 = new Signal("clk2");
      var rand = new Signal("rand");
      var d    = new Signal("d");
      var q    = new Signal("q ");
      var l    = new Signal("l");

      // Two clocks, a D flip-flop and a latch
      new Clock("U1", clk, new Time("8n"));
      new Clock("U2", clk2, new Time("11n"));
      new Rand("U3", d, new Time("11n"));
      new DFF("U4", clk, d, q, Signal.Open);
      new Latch("U5", clk2, clk, l, Signal.Open);
 
      // Report changes on specified signals
      clk.Display();
      clk2.Display();
      d.Display();
      d.Display();
      q.Display();
      l.Display();

      // Apply the power!
      Sim.Core.RunTo(new Time("400n"));
    }

    static void TestBus()
    {
      // Declare the signals and define their reporting names
      var b   = new Bus("data", 4);
      var _b  = new Bus("~data", 4);
      var clk = new Signal("clk");
      var rst = new Signal("rst", 'L');

      // Four D flip-flops and a clock
      new Clock("U1", clk, new Time("20n"));

      new DFF_R("U2", clk, rst, _b[0], b[0], _b[0]);
      new DFF_R("U3", _b[0], rst, _b[1], b[1], _b[1]);
      new DFF_R("U4", _b[1], rst, _b[2], b[2], _b[2]);
      new DFF_R("U5", _b[2], rst, _b[3], b[3], _b[3]);

      // Report changes on specified signals
       clk.Display();
       rst.Display();
       b[0].Display();
       b[1].Display();
       b[2].Display();
       b[3].Display();
      _b[0].Display();
      _b[1].Display();
      _b[2].Display();
      _b[3].Display();

      // Reset
      rst.SetAfter('H', new Time("25n"));
      rst.SetAfter('L', new Time("35n"));

      // Apply the power!
      Sim.Core.RunTo(new Time("800n"));
    }

    public static void TestClock()
    {
      var clk = new Signal("clk");
      new Clock("U1", clk, new Time("20n"));
      clk.Display();
      Sim.Core.RunTo(new Time("100n"));
    }

    public static void TestRand()
    {
      var clk = new Signal("clk");
      var rnd = new Signal("rnd");

      new Clock("U1", clk, new Time("20n"));
      new Rand("U2", rnd, new Time("20n"));

      clk.Display();
      rnd.Display();

      Sim.Core.RunTo(new Time("400n"));
    }

    public static void TestXor()
    {
      var clk = new Signal("clk");
      var dclk = new Signal("dclk");
      var edge = new Signal("edge");

      new Clock("U1", clk, new Time("20n"));
      new Buffer("U2", clk, dclk, new Time("4n"));
      new Xor2("U3", clk, dclk, edge);

      clk.Display();
      dclk.Display();
      edge.Display();

      Sim.Core.RunTo(new Time("400n"));
    }

    public static void TestXorPh()
    {
      var clk = new Signal("clk");
      var clk2 = new Signal("clk2");
      var phase = new Signal("phase");

      new Clock("U1", clk, new Time("20n"));
      new Clock("U2", clk2, new Time("22n"));
      new Xor2("U3", clk, clk2, phase);

      clk.Display();
      clk2.Display();
      phase.Display();

      Sim.Core.RunTo(new Time("400n"));
    }

    public static void TestSpdif()
    {
      var clk   = new Signal("clk");
      var rnd   = new Signal("rnd");
      var spdif = new Signal("spdif");
      var clk2  = new Signal("clk2");
      var hf    = new Signal("hf");
      var rst   = new Signal("rst", 'H');
      
      var edge  = new Signal("edge");
      var dly   = new Signal("dly");
      var mono  = new Signal("mono");

      new Clock("U1", clk, new Time("20n"));
      new Rand("U2", rnd, new Time("20n"));
      new And2("U3", clk, rnd, hf);
      new DFF_R("U4", clk, rst, Signal.Open, clk2, clk2);
      new Xor2("U5", clk2, hf, spdif);

      new Buffer("U6", spdif, dly, new Time("4n"));
      new Xor2("U7", spdif, dly, edge);
      new Mono_R("U8", edge, rst, mono, Signal.Open, new Time("15n"));

      clk.Display();
      rnd.Display();
      spdif.Display();
      edge.Display();
      mono.Display();
      rst.Display();

      rst.SetAfter('L', new Time("35n"));

      Sim.Core.RunTo(new Time("400n"));
    }

    public static void TestMono()
    {
      var clk = new Signal("clk");
      var   q = new Signal("q");
      var rnd = new Signal("rnd");
      var qrt = new Signal("q_retrig");

      new Clock("U1", clk, new Time("20n"));
      new Mono_R("U2", clk, Signal.PullDn, q, Signal.Open, new Time("30n"));
      new Rand("U3", rnd, new Time("10n"));
      new MonoRT_R("U4", rnd, Signal.PullDn, qrt, Signal.Open, new Time("30n"));


      clk.Display();
      q.Display();
      rnd.Display();
      qrt.Display();

      Sim.Core.RunTo(new Time("400n"));
    }

    public static void TestAdder()
    {
      var a = new Bus("a", 4);
      var b = new Bus("b", 4);
      var s = new Bus("s", 4);
      var c = new Bus("c", 4);

      HalfAdder("HA0", a[0], b[0], Signal.PullDn, s[0], c[0]);
      HalfAdder("HA1", a[1], b[1], c[0]         , s[1], c[1]);
      HalfAdder("HA2", a[2], b[2], c[1]         , s[2], c[2]);
      HalfAdder("HA3", a[3], b[3], c[2]         , s[3], c[3]);

        
      for(var i = 0; i < 4; i++)
        a[i].Display();
      for(var i = 0; i < 4; i++)
        b[i].Display();
      for(var i = 0; i < 4; i++)
        s[i].Display();
      for(var i = 0; i < 4; i++)
        c[i].Display();
      

        //a.Display();
        //b.Display();
        //s.Display();
        //c[3].Display();


      a.SetAfter("0000", new Time("100n"));
      b.SetAfter("0000", new Time("100n"));
      a.SetAfter("1111", new Time("300n"));
      b.SetAfter("1000", new Time("300n"));
      a.SetAfter("1110", new Time("500n"));
      b.SetAfter("1100", new Time("500n"));
      a.SetAfter("1110", new Time("700n"));
      b.SetAfter("0101", new Time("700n"));

      // Apply the power!
      Sim.Core.RunTo(new Time("900n"));
    }
    public static void HalfAdder(string name, Signal a, Signal b, Signal ci, Signal s, Signal co)
    {
        var hs = new Signal(name + ".hs");
        var hc = new Signal(name + ".hc");
        var cc = new Signal(name + ".cc");
        new Xor2(name + ".U1", a, b, hs);
        new Xor2(name + ".U2", hs, ci, s);
        new And2(name + ".U3", a, b, hc);
        new And2(name + ".U4", ci, hs, cc);
        new Or2(name + ".U5", hc, cc, co);
    }

    private void panel2_Resize(object sender, EventArgs e)
    {
      viewer.Size = panel2.Size;
    }
  }
}