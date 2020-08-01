using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Simulator
{
  // TODO Implement a full analysis of output states
  // Collect all reported activity in each Cycle
  // At the cycle end, analyse and report 
  public class ReportGenerator
  {
    /// <summary>
    /// The waveform records
    /// </summary>
    Waveforms waves;

    /// <summary>
    /// The waveform records
    /// </summary>
    public Waveforms Waves { get { return waves; } }

    /// <summary>
    /// Constructor
    /// </summary>
    public ReportGenerator()
    {
      waves = new Waveforms();
    }

    /// <summary>
    /// Report initial signal value
    /// </summary>
    /// <param name="s"></param>
    public void ReportInitial(IReportable r)
    {
      Form1.ListAdd(Time.Zero, "Initial   : {0,4} = {1}", r.Name, r.Value);
      waves.Add(r);
      waves.AddDelta(Time.MinValue, r);
    }

    /// <summary>
    /// Report signal state change during run
    /// (Signal changed callback)
    /// </summary>
    /// <param name="s"></param>
    public void Report(Time t, IReportable r)
    {
      Form1.ListAdd(t, "{0}: {1,4} = {2}", t, r.Name, r.Value);
      waves.AddDelta(t, r);
    }
  }

  /// <summary>
  /// A single element of a waveform record
  /// Records the absolute Time and the new state
  /// </summary>
  [DebuggerDisplay("Delta<{time}, {state}>")]
  public class Delta
  {
    /// <summary>
    /// Waveform absolute time
    /// </summary>
    public Time time;
    /// <summary>
    /// Waveform state
    /// </summary>
    public string state;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="time"></param>
    /// <param name="state"></param>
    public Delta(Time time, string state)
    {
      this.time = time;
      this.state = state;
    }
  }

  /// <summary>
  /// A collection of waveform records
  /// </summary>
  public class Waveforms
  {
    /// <summary>
    /// Signal names
    /// </summary>
    private List<string> _names;

    /// <summary>
    /// Signal names
    /// </summary>
    public IList<string> Names { get { return _names; } }

    /// <summary>
    /// Collection of waveform records
    /// </summary>
    private IList<IList<Delta>> _waves;

    /// <summary>
    /// When frozen the wavform records are optimised and made readonly
    /// </summary>
    public bool Frozen { get { return _frozen; } }

    /// <summary>
    /// Wavform records are optimised and made readonly
    /// </summary>
    bool _frozen;

    /// <summary>
    /// Absolute time of record start
    /// </summary>
    public Time Start { get { return _start; } }

    /// <summary>
    /// Absolute time of record start
    /// </summary>
    Time _start;

    /// <summary>
    /// Absolute time of record end
    /// </summary>
    public Time End { get { return _end; } }

    /// <summary>
    /// Absolute time of record end
    /// </summary>
    Time _end;

    /// <summary>
    /// Record duration
    /// </summary>
    public Time Length { get { return _end - _start; } }

    /// <summary>
    /// Constructor
    /// </summary>
    public Waveforms()
    {
      _names = new List<string>();
      _waves = new List<IList<Delta>>();
    }

    /// <summary>
    /// Add a new reporting source
    /// </summary>
    /// <param name="r"></param>
    public void Add(IReportable r)
    {
      if(_frozen)
        Console.WriteLine("Can't add to frozen Waveforms");
      _names.Add(r.Name);
      r.WaveIndex = _names.Count - 1;
      _waves.Add(new List<Delta>());
    }

    /// <summary>
    /// Record a Delta from a reporting source
    /// </summary>
    /// <param name="t"></param>
    /// <param name="r"></param>
    public void AddDelta(Time t, IReportable r)
    {
      if(_frozen)
        Console.WriteLine("Can't add Deltas to frozen Waveforms");

      if(t > _end)
        _end = t;

      if(t < _start)
        _start = t;

      _waves[r.WaveIndex].Add(new Delta(t, r.Value)); 
    }

    /// <summary>
    /// Ensure that the waveform data is in straight arrays and
    /// make data read only
    /// </summary>
    public void Freeze()
    {
      if(_frozen)
        return;

      Delta[][] waves = new Delta[_waves.Count][];
      for(int i = 0; i < _waves.Count; i++)
      {
        waves[i] = new Delta[_waves[i].Count];
        _waves[i].CopyTo(waves[i], 0);
        _waves[i] = null;
      }
      _waves = waves;
      _frozen = true;
    }

    /// <summary>
    /// Access a waveform by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IList<Delta> this[int index]
    {
      get { return _waves[index]; }
    }

    /// <summary>
    /// Access a waveform by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IList<Delta> this[string name]
    {
      get
      {
        int index = _names.IndexOf(name);
        if(index < 0)
          return null;

        return _waves[index];
      }
    }

    /// <summary>
    /// Returns the number of waveforms recorded
    /// </summary>
    public int Count
    {
      get { return _waves.Count; }
    }
  }
}
