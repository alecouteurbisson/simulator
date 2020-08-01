using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  /// <summary>
  /// Miscellaneous synchronous devices.
  /// 
  /// Clock               - A clock generator.
  /// Rand                - A random bit stream generator
  /// MonoRT_R and Mono_R - Retriggerable and non-retriggerable monostables with asynch reset
  /// </summary>
  /// 
  /// <summary>
  /// A clock generator with individual control of the initial state
  /// and the on and off times.
  /// </summary> 
  public class Clock : Device
  {
    protected Signal clk;
    protected Signal clkgen;
    protected Time on;
    protected Time off;
    protected Time first;

    public Clock(string name, Signal clk, Time on, Time off, Time first, Logic init)
      : base(name)
    {
      if((init == Logic.U) || (init == Logic.X) || (init == Logic.Z))
        throw new ApplicationException
          (string.Format("Invalid initial clock state, {0}, in {1}", init, Name));

      if((off == Time.Zero) || (on == Time.Zero))
        throw new ApplicationException
          (string.Format("Clock period must be non-zero in {0}", Name));

      this.clk = clk;
      this.on = on;
      this.off = off;
      this.first = first;

      // Create an internal signal for the clock
      clkgen = new Signal(Internal("clkgen"), init.Buffer);

      // Initialise the output signal
      // clk.Init(clkgen.State);

      // Create an infinite loop of events
      SensitiveTo(clkgen);
      clkgen.SetAfter(~clkgen.State, Time.Zero);
    }

    public Clock(string name, Signal clk, Time on, Time off)
      : this(name, clk, on, off, off, Logic.L) { }

    public Clock(string name, Signal clk, Time period)
      : this(name, clk, period / 2, period / 2, period / 2, Logic.L) { }

    /// <summary>
    /// At each event post the next
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      if(clkgen.Changing)
      {
        if(clkgen.Lo)
          clkgen.SetAfter(Logic.H, on);
        else
          clkgen.SetAfter(Logic.L, off);

        SetOutputs();
      }
    }

    protected override void SetOutputs()
    {
      clk.SetAfter(clkgen.State, first);
    }
  }

  /// <summary>
  /// A random data generator with individual control of the initial state
  /// and the on and off times.
  /// </summary> 
  public class Rand : Clock
  {
    Random r;
    Logic randbit;

    public Rand(string name, Signal data, Time period)
      : base(name, data, period, period, period, Logic.L)
    {
      r = new Random();
      randbit = Logic.L;
    }

    /// <summary>
    /// At each event post the next
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      if(clkgen.Changing)
      {
        if(clkgen.Lo)
          clkgen.SetAfter(Logic.H, on);
        else
          clkgen.SetAfter(Logic.L, off);

        // Pick a bit
        randbit = (r.Next() & 4096) == 0 ? Logic.H : Logic.L;
        SetOutputs();
      }
    }
    protected override void SetOutputs()
    {
      clk.SetAfter(randbit, first);
    }
  }

  /// <summary>
  /// A non retriggerable monostable with an asynchronous clear
  /// </summary> 
  public class Mono_R : Device
  {
    protected Signal clk;
    protected Signal rst;
    protected Signal q;
    protected Signal _q;
    protected Signal dlygen;
    protected Time period;

    protected Logic qi;

    public Mono_R(string name, Signal clk, Signal rst, Signal q, Signal _q, Time period)
      : base(name)
    {
      this.clk = clk;
      this.rst = rst;
      this.period = period;
      this.q = q;
      this._q = _q;

      if(period < _delay)
        throw new ApplicationException
          (string.Format("Monostable period must be greater than propogation delay in {0}", Name));

      qi = Logic.L;

      // Create an internal signal for the delay
      dlygen = new Signal(Internal("dlygen"), Logic.L);

      // Initialise the output signal
      // clk.Init(clkgen.State);

      SensitiveTo(dlygen);
      SensitiveTo(clk);
      SensitiveTo(rst);
    }

    /// <summary>
    /// Monostable using a signal delay
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      // Asynchronous reset
      if(rst.Hi)
      {
        if(qi != Logic.L)
          qi = Logic.L;
        SetOutputs();
        return;
      }

      // Time-out
      if(dlygen.Falling)
      {
        qi = Logic.L;
        SetOutputs();
        return;
      }

      // Trigger
      if((qi != Logic.H) && clk.Rising)
      {
        dlygen.Set(Logic.H);
        dlygen.SetAfter(Logic.L, period);

        qi = Logic.H;
        SetOutputs();
      }
    }

    protected override void SetOutputs()
    {
      q.SetAfter(qi, _delay);
      _q.SetAfter(~qi, _delay);
    }
  }

  /// <summary>
  /// A retriggerable monostable with an asynchronous reset
  /// </summary> 
  public class MonoRT_R : Mono_R
  {
    Time expires;

    public MonoRT_R(string name, Signal clk, Signal rst, Signal q, Signal _q, Time period)
      : base(name, clk, rst, q, _q, period)
    {
      expires = new Time("0p");
    }

    /// <summary>
    /// Monostable using a signal delay
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      // Asynchronous reset
      if(rst.Hi)
      {
        if(qi != Logic.L)
          qi = Logic.L;
        SetOutputs();
        return;
      }

      // Time-out
      if(dlygen.Falling)
      {
        // Are we there yet...?
        if(expires <= Sim.Now)
        {
          // Yup!
          qi = Logic.L;
          SetOutputs();
        }
        else
        {
          // Nope, queue up another delay for the remainder
          dlygen.SetAfter(Logic.L, expires - Sim.Now);
        }
      }

      // Trigger
      if(clk.Rising)
      {
        dlygen.Set(Logic.H);
        dlygen.SetAfter(Logic.L, period);

        // This expiry time will extend with each retrigger
        expires = Sim.Now + period;

        if(qi != Logic.H)
        {
          qi = Logic.H;
          SetOutputs();
        }
      }
    }
  }
}
