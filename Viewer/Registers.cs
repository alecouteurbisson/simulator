using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  ///<remarks>
  /// Latches and D-type registers with synch/asynch set and reset
  /// All have complementary outputs
  /// </remarks>
  
  /// <summary>
  /// A positive edge triggered transparent latch
  /// </summary>
  public class Latch : Device
  {
    protected Signal clk;
    protected Signal d;
    protected Signal q;
    protected Signal _q;

    protected Logic qi;

    public Latch(string name, Signal clk, Signal d, Signal q, Signal _q)
      : base(name)
    {
      this.clk = clk;
      this.d = d;
      this.q = q;
      this._q = _q;
      if(clk.Lo)
        qi = d.State.Buffer;
      else
        qi = Logic.U;

      SensitiveTo(clk);
      SensitiveTo(d);
    }

    /// <summary>
    /// Transfer internal state in qi to outputs
    /// </summary>
    protected override void SetOutputs()
    {
      q.SetAfter(qi, _delay);
      _q.SetAfter(~qi, _delay);
    }

    /// <summary>
    /// The behavioural model
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      // Data transmitted at clk lo and held on a rising edge
      if(clk.Rising || clk.Lo)
      {
        qi = d.State.Buffer;
        SetOutputs();
      }
    }
  }

  /// <summary>
  /// Basic D-type register with positive edge triggering
  /// and complementary outputs
  /// </summary>
  public class DFF : Latch
  {
    protected Time setup = new Time("1n");

    public DFF(string name, Signal clk, Signal d, Signal q, Signal _q)
      : base(name, clk, d, q, _q)
    {
      this.clk = clk;
      this.d = d;
      this.q = q;
      this._q = _q;
      qi = Logic.U;

      SensitiveTo(clk);
    }

    protected override void Process(Signal changed)
    {
      if(clk.Rising)
      {
        qi = d.State.Buffer;
        SetOutputs();

        if(d.Stable < setup)
          Console.WriteLine("Setup violation at d input of {0} ", Name);
      }
    }
  }

  /// <summary>
  /// Basic D-type register with positive edge triggering,
  /// complementary outputs and an asynchronous reset
  /// </summary>
  public class DFF_R : DFF
  {
    protected Signal rst;

    public DFF_R(string name, Signal clk, Signal rst, Signal d, Signal q, Signal _q)
      : base(name, clk, d, q, _q)
    {
      this.rst = rst;

      if(rst.Hi)
        qi = Logic.L;

      SensitiveTo(rst);
    }

    protected override void Process(Signal changed)
    {
      if(rst.Hi)
      {
        if(qi != Logic.L)
        {
          qi = Logic.L;
          SetOutputs();
        }
      }
      else
      {
        base.Process(changed);
      }
    }
  }

  /// <summary>
  /// Basic D-type register with positive edge triggering,
  /// complementary outputs and a synchronous reset
  /// </summary>
  public class DFF_r : DFF_R
  {
    public DFF_r(string name, Signal clk, Signal rst, Signal d, Signal q, Signal _q)
      : base(name, clk, rst, d, q, _q) { }

    protected override void Process(Signal changed)
    {
      if(clk.Rising)
        base.Process(changed);
    }
  }

  /// <summary>
  /// Basic D-type register with positive edge triggering,
  /// complementary outputs and an asynchronous set
  /// </summary>
  public class DFF_S : DFF
  {
    protected Signal set;

    public DFF_S(string name, Signal clk, Signal set, Signal d, Signal q, Signal _q)
      : base(name, clk, d, q, _q)
    {
      this.set = set;

      SensitiveTo(set);
    }

    protected override void Process(Signal changed)
    {
      if(set.Hi && (qi != Logic.H))
      {
        qi = Logic.H;
        SetOutputs();
      }
      else
      {
        base.Process(changed);
      }
    }
  }

  /// <summary>
  /// Basic D-type register with positive edge triggering,
  /// complementary outputs and a synchronous set
  /// </summary>
  public class DFF_s : DFF_S
  {
    public DFF_s(string name, Signal clk, Signal set, Signal d, Signal q, Signal _q)
      : base(name, clk, set, d, q, _q) { }

    protected override void Process(Signal changed)
    {
      if(clk.Rising)
        base.Process(changed);
    }
  }
}
