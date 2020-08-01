using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace Simulator
{
  [DebuggerDisplay("Bus {_name}: {Value}")]
  public class Bus : IReportable
  {
    static Dictionary<string, Bus> all;

    public string Name 
    { get; private set; }

    public int Size 
    { get; private set; }

    private readonly Signal[] signals;

    public bool Reporting 
    { get; private set; }

    Time changedAt;
    bool changed;

    /// <summary>
    /// The reporters waveform index
    /// </summary>
    int _waveIndex;

    /// <summary>
    /// The reporters waveform index
    /// </summary>
    public int WaveIndex
    {
      get { return _waveIndex; }
      set { _waveIndex = value; }
    }

    static Bus()
    {
      all = new Dictionary<string, Bus>();
    }

    public Bus(string name, int size, string init = null)
    {
      Name = name;
      Size = size;

      if(init == null)
        init = new string('U', size);

      if(init.Length != size)
        throw new ApplicationException
          (string.Format("Invalid initialiser, {0}, for {1}[0..{2}]", init, name, size));

      
      signals = new Signal[size];
      for(var i = 0; i < size; i++)
        signals[i] = new Signal(string.Format("{0}[{1}]", name, i), init[i]);

      Reporting = false;
      all.Add(name, this);
      changed = false;
    }

    public Signal this[int i]
    { get { return signals[i]; } }

    public void SetAfter(string value, Time delay)
    {
      if(value.Length != Size)
        throw new ApplicationException
          (string.Format("Invalid initialiser, {0}, for {1}[0..{2}]", Value, Name, Size));

      for(var i = 0; i < Size; i++)
        signals[i].SetAfter(value[i], delay);
    }

    public void Display()
    {
      Display(true);
    }

    public void Display(bool on)
    {
      if(Reporting != on)
      {
        Reporting = on;

        foreach(var s in signals)
        {
          if(Reporting)
            s.OnChange += BusChanged;
          else
            s.OnChange -= BusChanged;
             
        }      
      }
    }

    private void BusChanged(Signal s)
    {
      changedAt = Sim.Now;
      changed = true;
    }

    public string Value
    {
      get
      {
        var states = new char[signals.Length];

        for(var i = 0; i < signals.Length; i++)
          states[i] = signals[i].State.ToString()[0];

        return new string(states);
      }
    }

    /// <summary>
    /// Report the initial state of all reporting nodes
    /// </summary>
    public static void ReportInitial()
    {
      foreach(var b in all.Values.Where(b => b.Reporting))
          Sim.Reporter.ReportInitial(b);
    }

    public static void CycleEnd()
    {
      if(Sim.Initialised)
      {
        foreach(var b in all.Values.Where(b => b.changed))
        {
            Sim.Reporter.Report(b.changedAt, b);
            b.changed = false;
        }
      }
    }
  }
}
