using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  /// <summary>
  /// Simulation event callback that updates a signal node 
  /// </summary>
  /// <param name="state"></param>
  public delegate void UpdateSignal(Logic state);

  /// <summary>
  /// Simulation event structure
  /// </summary>
  public struct Event : IComparable<Event>
  {
    /// <summary>
    /// When to fire
    /// </summary>
    Time _at;

    /// <summary>
    /// When to fire
    /// </summary>
    public Time At { get { return _at; } }

    /// <summary>
    /// Event target
    /// </summary>
    Signal _signal;
    public Signal Signal { get { return _signal; } }

    /// <summary>
    /// The new signal state
    /// </summary>
    Logic _state;
    public Logic State { get { return _state; } }

    /// <summary>
    /// Cycle counter to maintain chronology within a time point
    /// </summary>
    int _cycle;
    public int Cycle
    {
      get { return _cycle; }
      set { _cycle = value; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="delay">Event delay time</param>
    /// <param name="update">Signal update callback</param>
    /// <param name="state">New signal state</param>
    public Event(Time delay, Signal signal, Logic state)
    {
      _at = Sim.Now + delay;
      this._signal = signal;
      this._state = state;
      _cycle = 0;
    }

    /// <summary>
    /// Fire the event by setting the targets next state 
    /// </summary>
    public void Fire()
    {
      _signal.Set(this._state);
    }

    /// <summary>
    /// Compare events chronologically
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public int CompareTo(Event e)
    {
      int dif =_at.CompareTo(e._at);
      if(dif == 0)
        return _cycle.CompareTo(e._cycle);
      else return dif;
    }

    public override string ToString()
    {
      return string.Format("{0}:{1} {2}<-{3}", _at, _cycle, _signal.Name, _state);
    }
  }
}
