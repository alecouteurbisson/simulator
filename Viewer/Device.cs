using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  ///<remarks>
  /// Device Model design
  /// 
  /// Combinatorial models are very easy.  Just look at Gates.cs and derive a new
  /// model from the Gate gate class. 
  /// 
  /// Models can also be constructed by aggregating existing models as sub-circuits.
  ///  
  /// When a model registers as being SensitiveTo() a signal or bus it simply means
  /// that the models Process() method will be called when that signal/bus changes.
  /// 
  /// Insensitive inputs must be clocked (there must be a source of events for every
  /// input)
  /// 
  /// Never read back from an output, you will not always read what you write.
  /// Synchronous models must implement SetOutputs() which loads the output
  /// signals from a local copy of the outputs state (Signal qi in the register
  /// models).
  /// 
  /// Never implement a model with a zero propgation delay.  Just 1pS of delay will
  /// enable correct operation.
  /// 
  /// Always use the supplied methods, Signal.Rising(), Signal.Falling() and
  /// Signal.Changing() to test for clocking edges.
  /// 
  /// 
  /// 
  /// </remarks>

  /// <summary>
  /// The base class of all devices
  /// </summary>
  public abstract class Device
  {
    static Dictionary<string, Device> all;
    //public Device[] All { get { return all.ToArray(); } }

    string _name;
    public string Name { get { return _name; } }

    /// <summary>
    /// A simple delay model
    /// In the absence of detailed propogation delay parameters
    /// a simple model is used that assumes a fixed gate delay.
    /// The delay is set for all models here
    /// </summary>
    protected Time _delay = new Time("1n");
    public Time Delay
    {
      get { return _delay; }
      set { _delay = value; }
    }

    static Device()
    {
      all = new Dictionary<string, Device>();
    }

    //protected Device() : this("") { }

    /// <summary>
    /// Constructor
    /// Maintain Device.all and ensure that the new device has a distinct name
    /// </summary>
    /// <param name="name"></param>
    protected Device(string name)
    {
      _name = name;

      all.Add(_name, this);
    }

    /// <summary>
    /// Common initialisation routine so that derived classes can
    /// re-use initialisation code even though the constructors
    /// are different
    /// </summary>
    protected virtual void InitDevice()
    {
    }

    /// <summary>
    /// Register sensitivity to a signal
    /// </summary>
    /// <param name="s"></param>
    protected void SensitiveTo(Signal s)
    {
      s.OnChange += new SignalChanged(Process);
    }

    /// <summary>
    /// Set the output(s) of a synchronous device
    /// </summary>
    protected virtual void SetOutputs()
    {
    }

    /// <summary>
    /// Generate output states for current input states
    /// s will be null on initialisation; otherwise it is the signal that has changed.
    /// Only combinatorial logic should respond to the initialisation. However, you
    /// will never get an edge during initialisation so this is normally automatic.
    /// </summary>
    /// <param name="s"></param>
    protected abstract void Process(Signal s);

    /// <summary>
    /// Initialise synchronous outputs 
    /// </summary>
    public static void Initialise()
    {
      foreach(Device d in all.Values)
        d.SetOutputs();
    }

    /// <summary>
    /// The preferred method for creating names for internal signals, busses and devices
    /// A signal 'clk' in device 'U7' is rendered as 'U7:clk'
    /// </summary>
    /// <param name="netname"></param>
    /// <returns></returns>
    protected string Internal(string subname)
    {
      return string.Format("{0}:{1}", Name, subname);
    }
  }
}
