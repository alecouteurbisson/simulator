using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  /// <summary>
  /// The base class for all combinational logic
  /// </summary>
  public abstract class Gate : Device
  {
    protected Signal[] y;
    protected Signal q;

    /// <summary>
    /// Constructor has inputs last to make it generic.
    /// Delays must be strictly positive so zero delay actually
    /// uses the default.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="output"></param>
    /// <param name="delay"></param>
    /// <param name="inputs"></param>
    public Gate(string name, Signal output, Time delay, params Signal[] inputs)
      : base(name)
    {
      y = inputs;
      q = output;

      if(delay > Time.Zero)
        _delay = delay;

      foreach(Signal s in inputs)
        SensitiveTo(s);
    }
  }

  /// <summary>
  /// Buffer gate
  /// </summary>
  public class Buffer : Gate
  {
    /// <summary>
    /// Constructor with delay override
    /// </summary>
    /// <param name="a"></param>
    /// <param name="q"></param>
    /// <param name="delay"></param>
    public Buffer(string name, Signal a, Signal q, Time delay) : base(name, q, delay, a) { }

    /// <summary>
    /// Constructor with default delay
    /// </summary>
    /// <param name="a"></param>
    /// <param name="q"></param>
    public Buffer(string name, Signal a, Signal q) : this(name, a, q, Time.Zero) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      q.SetAfter(y[0].State, _delay);
    }
  }

  /// <summary>
  /// Inverter gate
  /// </summary>
  public class Inverter : Gate
  {
    public Inverter(string name, Signal a, Signal q, Time delay) : base(name, q, delay, a) { }

    public Inverter(string name, Signal a, Signal q) : this(name, a, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(~y[0].State, _delay);
    }
  }

  /// <summary>
  /// Two input and gate
  /// </summary>
  public class And2 : Gate
  {
    public And2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public And2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(y[0].State & y[1].State, _delay);
    }
  }

  /// <summary>
  /// Two input or gate
  /// </summary>
  public class Or2 : Gate
  {
    public Or2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public Or2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(y[0].State | y[1].State, _delay);
    }
  }

  /// <summary>
  /// Two input or gate
  /// </summary>
  public class Xor2 : Gate
  {
    public Xor2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public Xor2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(y[0].State ^ y[1].State, _delay);
    }
  }

  /// <summary>
  /// Two input nand gate
  /// </summary>
  public class Nand2 : Gate
  {
    public Nand2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public Nand2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(~(y[0].State & y[1].State), _delay);
    }
  }

  /// <summary>
  /// Two input nor gate
  /// </summary>
  public class Nor2 : Gate
  {
    public Nor2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public Nor2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(~(y[0].State | y[1].State), _delay);
    }
  }

  /// <summary>
  /// Two input xnor gate
  /// </summary>
  public class Xnor2 : Gate
  {
    public Xnor2(string name, Signal a, Signal b, Signal q, Time delay) : base(name, q, delay, a, b) { }

    public Xnor2(string name, Signal a, Signal b, Signal q) : this(name, a, b, q, Time.Zero) { }

    protected override void Process(Signal changed)
    {
      q.SetAfter(~(y[0].State ^ y[1].State), _delay);
    }
  }

  /// <summary>
  /// Tri-state buffer gate
  /// </summary>
  public class TSBuffer : Gate
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="a"></param>
    /// <param name="en"></param>
    /// <param name="q"></param>
    /// <param name="delay"></param>
    public TSBuffer(string name, Signal a, Signal en, Signal q, Time delay) : base(name, q, delay, a, en) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="a"></param>
    /// <param name="en"></param>
    /// <param name="q"></param>
    public TSBuffer(string name, Signal a, Signal en, Signal q) : this(name, a, en, q, Time.Zero) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      if(y[1].Hi)
        q.SetAfter(y[0].State, _delay);
      else
        q.SetAfter(Logic.Z, _delay);
    }
  }


  /// <summary>
  /// Tri-state inverter gate
  /// </summary>
  public class TSInverter : Gate
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="a"></param>
    /// <param name="en"></param>
    /// <param name="q"></param>
    /// <param name="delay"></param>
    public TSInverter(string name, Signal a, Signal en, Signal q, Time delay) : base(name, q, delay, a, en) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="a"></param>
    /// <param name="en"></param>
    /// <param name="q"></param>
    public TSInverter(string name, Signal a, Signal en, Signal q) : this(name, a, en, q, Time.Zero) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changed"></param>
    protected override void Process(Signal changed)
    {
      if(y[1].Hi)
        q.SetAfter(~y[0].State, _delay);
      else
        q.SetAfter(Logic.Z, _delay);
    }
  }
}