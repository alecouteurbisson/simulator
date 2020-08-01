//#define Debug

using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  //TODO Allow specification of time unit (default 1pS)
  //TODO Input/display times properly, E.g. 10.53nS
  //TODO Add glitch/setup/width checking and reporting
  //TODO Lots more device models
  //TODO Device macros
  //TODO Add model parameter structure and a library facility for them
  //TODO Add waveform output to some standard format
  //TODO Waveform display/editing

  /// <summary>
  /// Simulator core control
  /// </summary>
  public class Sim
  {
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static readonly Sim Core;

    /// <summary>
    /// The master clock
    /// </summary>
    public static Time Now { get; private set; }

    /// <summary>
    /// Simulator event queue
    /// </summary>
    PriorityQueue queue;

    /// <summary>
    /// False during simulator reset
    /// </summary>
    public static bool Initialised { get; private set; }

    /// <summary>
    /// The report generator
    /// </summary>
    public static ReportGenerator Reporter
    { get; private set; }

    /// <summary>
    /// Cycle counter
    /// </summary>
    int cycle;

    /// <summary>
    /// Static constructor
    /// Creates the singleton instance
    /// </summary>
    static Sim()
    {
      Core = new Sim();
    }

    private Sim()
    {
      FullReset();
    }

    public void FullReset()
    {
      queue = new PriorityQueue(100);
      Initialised = false;
      Reporter = new ReportGenerator();
    }

    /// <summary>
    /// Reset the simulator
    /// </summary>
    public void Reset()
    {
      // Save any initial events
      var qsave = queue;
      queue = new PriorityQueue(30);

      Now = new Time(0);
      Signal.Initialise();
      EventLoop(100, Time.MaxValue);

      Device.Initialise();
      if(EventLoop(100, Time.MaxValue))
        Form1.ListAdd(Time.Zero, "******Reset incomplete******");

      Now = new Time(0);
      Initialised = true;

      // Restore the original events
      queue = qsave;
    }

    /// <summary>
    /// Run until no more changes
    /// </summary>
    public void Run()
    {
      RunTo(Time.MaxValue);
    }

    /// <summary>
    /// Run up to a specified time
    /// </summary>
    /// <param name="time"></param>
    public void RunTo(Time time)
    {
      if(!Initialised)
        Reset();

      if(time < Now)
        return;

      Signal.PartialResetAll();

      // Display the initial states
      Signal.ReportInitial();
      Bus.ReportInitial();

      // Stir briskly...
      EventLoop(1000, time);
    }

    /// <summary>
    /// The main simulator event loop
    /// This can be subject to a limit on simulation time or main simulation 
    /// cycles (not delta cycles) or both.
    /// </summary>
    /// <param name="iterLimit"></param>
    /// <param name="timeLimit"></param>
    /// <returns></returns>
    private bool EventLoop(int iterLimit, Time timeLimit)
    {
      // Reset delta cycle counter
      cycle = 0;
      while(queue.Count > 0)
      {
        // Check for the end of a time point
        if(queue.Peek().At != Now)
        {
          if(--iterLimit <= 0)
            return false;
          CycleEnd();
        }

        // Grab the next event
        Event e = queue.Dequeue();
        Now = e.At;

        // You're a bit young aren't you...
        if(e.At < Now)
          throw new ApplicationException("Event sequence error");

        if(Now >= timeLimit)
          return true;

#if Debug
        Console.WriteLine("<< {0}", e);
#endif
        // 
        e.Fire();

        if(queue.Count == 0)
          // Empty! See if we can wring a few more cycles out it
          CycleEnd();
      }
      return true;
    }

    /// <summary>
    /// Called when simulator time advances at the end of the delta cycles
    /// </summary>
    /// <param name="t"></param>
    private void CycleEnd()
    {
#if Debug
      Form1.ListAdd("CycleEnd---------------------------");
#endif
      Signal.CycleEnd();
      Bus.CycleEnd();
      cycle = 0;
    }

    /// <summary>
    /// Queue an event to be actioned later
    /// </summary>
    /// <param name="e"></param>
    public void QueueEvent(Event e)
    {
      e.Cycle = cycle++;
#if Debug
      Form1.ListAdd(">> {0}", e);
#endif
      queue.Enqueue(e);
    }
  }
}
