using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Simulator
{
  /// <summary>
  /// Signal change event delegate
  /// </summary>
  /// <param name="s"></param>
  public delegate void SignalChanged(Signal s);

  public interface IReportable
  {
    string Name { get; }
    string Value { get; }
    void Display(bool on);
    void Display();
    int WaveIndex { get; set; }
  }

  /// <summary>
  /// A node in a circuit
  /// </summary>
  [DebuggerDisplay("Signal {_name}: {Value}")]
  public class Signal : IReportable
  {
    static Dictionary<string, Signal> all;

    public static readonly Signal Open;
    public static readonly Signal PullUp;
    public static readonly Signal PullDn;

    static Signal()
    {
      all = new Dictionary<string, Signal>();
      Open = new Signal("Open", Logic.Z);
      PullUp = new Signal("PullUp", Logic.h);
      PullDn = new Signal("PullDn", Logic.l);
    }

    /// <summary>
    /// The display name used to label results
    /// </summary>
    string name;

    /// <summary>
    /// The current signal state
    /// </summary>
    Logic state;

    /// <summary>
    /// The (tentative) next signal state
    /// </summary>
    Logic next;

    /// <summary>
    /// The previous signal state
    /// </summary>
    Logic last;

    /// <summary>
    /// When this signal next changes
    /// </summary>
    Time nextChangeAt;

    /// <summary>
    /// When this signal became stable
    /// </summary>
    Time changedAt;

    /// <summary>
    /// True if this node is being reported
    /// </summary>
    bool reporting;

    /// <summary>
    /// True if this node is being reported
    /// </summary>
    public bool Reporting { get { return reporting; } }

    /// <summary>
    /// The reporters waveform index
    /// </summary>
    public int WaveIndex
    { get; set; }

    /// <summary>
    /// Event hook for signal propogation and reporting
    /// </summary>
    public event SignalChanged OnChange;

    bool doReport;

    /// <summary>
    /// Construct and initialise a signal
    /// Driven signals will be re-initialised during the simulator reset.
    /// </summary>
    /// <param name="name">The display name of this signal</param>
    public Signal(string name, Logic init)
    {
      this.name = name;
      state = init;
      last = init;
      next = Logic.Dummy;
      changedAt = Time.Zero;
      reporting = false;
      doReport = false;
      all.Add(this.name, this);
    }

    /// <summary>
    /// Construct a signal initialised to Logic.U
    /// Driven signals will be re-initialised during the simulator reset.
    /// </summary>
    /// <param name="name"></param>
    public Signal(string name) : this(name, Logic.U) { }

    /// <summary>
    /// Initialise a signal after construction
    /// </summary>
    /// <param name="init"></param>
    public void Init(Logic init)
    {
      if(Sim.Initialised)
        throw new ApplicationException
          (string.Format("Trying to initialise a signal, {0}, after Reset", name));

      state = init;
      last = init;
      next = Logic.Dummy;
    }

    /// <summary>
    /// Signal display name used to label results
    /// </summary>
    public string Name
    {
      get { return name; }
    }

    /// <summary>
    /// Present state
    /// </summary>
    public Logic State
    { get { return state; } }

    /// <summary>
    /// Post a delayed change of state
    /// </summary>
    /// <param name="state">New state</param>
    /// <param name="delay">Time until new state asserted</param>
    public void SetAfter(Logic state, Time delay)
    {
      Sim.Core.QueueEvent(new Event(delay, this, state));
    }

    /// <summary>
    /// The previous state
    /// </summary>
    public Logic Last
    {
      get { return last; }
    }

    /// <summary>
    /// How long this state has been stable
    /// </summary>
    public Time Stable
    {
      get { return Sim.Now - changedAt; }
    }

    /// <summary>
    /// True if this signal is a valid high
    /// </summary>
    /// <returns></returns>
    public bool Hi
    {
      get { return state.Hi; }
    }

    /// <summary>
    /// True if this signal is a valid low
    /// </summary>
    /// <returns></returns>
    public bool Lo
    {
      get { return state.Lo; }
    }

    /// <summary>
    /// True if this signal is changing
    /// </summary>
    /// <returns></returns>
    public bool Changing
    {
      // Never propogate edges during reset
      get { return Sim.Initialised && changedAt == Sim.Now; }
    }

    /// <summary>
    /// True if this signal is rising
    /// </summary>
    /// <returns></returns>
    public bool Rising
    {
      get { return Changing && state.Hi && last.Lo; }
    }

    /// <summary>
    /// True if this signal is falling
    /// </summary>
    /// <returns></returns>
    public bool Falling
    {
      get { return Changing && state.Lo && last.Hi; }
    }

    /// <summary>
    /// True if this signal is valid
    /// i.e. neither Logic.X nor Logic.U
    /// </summary>
    /// <returns></returns>
    public bool Valid
    {
      get { return (state != Logic.X) && (state != Logic.U); }
    }

    /// <summary>
    /// Control reporting for this signal
    /// </summary>
    public void Display(bool on)
    {
      reporting = on;
    }

    /// <summary>
    /// Enable reporting for this signal
    /// </summary>
    public void Display()
    {
      Display(true);
    }

    /// <summary>
    /// Store the latest event on this signal
    /// </summary>
    /// <param name="s"></param>
    public void Set(Logic s)
    {
      if(next != s)
      {
        next = s;
        nextChangeAt = Sim.Now;
      }
   }

    /// <summary>
    /// Change the signal state at the end of a simulation cycle
    /// and propagate changes to the registered devices
    /// </summary>
    private void Update()
    {
      if(next != Logic.Dummy && next != state)
      {
        last = state;
        state = next;
        
        changedAt = nextChangeAt;
        doReport = reporting;
        if(OnChange != null)
          OnChange(this);
      }
      next = Logic.Dummy;
    }

    /// <summary>
    /// Create an event for each valid signal prior to the initial condition run
    /// </summary>
    public static void Initialise()
    {
      foreach(var s in all.Values)
        if(s.Valid && (s.OnChange != null))
          s.OnChange(s);
    }

    public static void CycleEnd()
    {
      foreach(var s in all.Values)
        s.Update();

      if(Sim.Initialised)
        foreach(var s in all.Values)
          s.Report();
    }

    /// <summary>
    /// Perform a partial reset of the signal before running
    /// </summary>
    public void PartialReset()
    {
      last = state;
      next = Logic.Dummy;
      changedAt = Time.Zero;
      nextChangeAt = Time.Zero;
      doReport = false;
    }

    public void Report()
    {
      if(doReport)
        Sim.Reporter.Report(changedAt, this);

      doReport = false;
    }

    /// <summary>
    /// Report the initial state of all reporting nodes
    /// </summary>
    public static void ReportInitial()
    {
      foreach(var s in all.Values)
        if(s.reporting)
          Sim.Reporter.ReportInitial(s);
    }
    /// <summary>
    /// Partial reset of all nodes
    /// </summary>
    public static void PartialResetAll()
    {
      foreach(Signal s in all.Values)
          s.PartialReset();
    }

    public string Value
    {
      get { return state.ToString(); }
    }
  }
}

