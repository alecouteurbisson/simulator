using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  /// <summary>
  /// Simulator time
  /// A positive time interval with pico Second resolution
  /// and a maximum duration of ~512 Hours.
  /// Also used to denote absolute time relative to Time.Zero
  /// </summary>
  public struct Time : IComparable<Time>
  {
    /// <summary>
    /// SI exponents
    /// </summary>
    static string[] SI = {"p", "n", "u", "m", "", "k", "M" };

    /// <summary>
    /// Number of 1pS ticks in time interval
    /// </summary>
    ulong _ticks;

    /// <summary>
    /// Number of 1pS ticks in time interval
    /// </summary>
    public ulong Ticks { get { return _ticks; } }

    /// <summary>
    /// Minimum time value
    /// </summary>
    public static readonly Time MinValue = new Time(ulong.MinValue);

    /// <summary>
    /// Maximum time value (~512 hours)
    /// </summary>
    public static readonly Time MaxValue = new Time(ulong.MaxValue);

    /// <summary>
    /// Zero time value
    /// </summary>
    public static readonly Time Zero = new Time(0);

    /// <summary>
    /// Construct from a number of 1pS ticks
    /// </summary>
    /// <param name="ticks"></param>
    public Time(ulong ticks)
    {
      this._ticks = ticks;
    }

    /// <summary>
    /// Construct from a string specifying time in seconds
    /// SI multipliers from pico to Mega are available
    /// e.g. 12.425n, 13p, 1, 10.5M etc.
    /// </summary>
    /// <param name="tstr"></param>
    public Time(string tstr)
    {
      tstr = tstr.Trim();

      char si = tstr[tstr.Length - 1];

      if(char.IsDigit(si))
        si = 'S';
      else
        tstr = tstr.Remove(tstr.Length - 1);

      int mul = siToIndex(si);

      int dp = tstr.IndexOf('.');
      if(dp > 0)
      {
        tstr = tstr.Remove(dp, 1);
        mul -= tstr.Length - dp;
      }

      _ticks = ulong.Parse(tstr);

      while(mul > 0)
      {
        _ticks *= 10ul;
        mul--;
      }

      while(mul < 0)
      {
        _ticks /= 10;
        mul++;
      }
    }

    /// <summary>
    ///  Add two Times
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static Time operator +(Time t1, Time t2)
    {
      return new Time(t1._ticks + t2._ticks);
    }

    /// <summary>
    /// Calculate the Time difference (result must be positive)
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static Time operator -(Time t1, Time t2)
    {
      if(t2._ticks > t1.Ticks)
        throw new Error("Time difference result negative");
      return new Time(t1._ticks - t2._ticks);
    }

    /// <summary>
    /// Multiply a Time by a ulong
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Time operator *(Time t1, ulong x)
    {
      return new Time(t1._ticks * x);
    }

    /// <summary>
    /// Multiply a Time by a double
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Time operator *(Time t1, double x)
    {
      if(x < 0)
        throw new Error("Invalid time multiplyer, {0}", x);
      return new Time((ulong)(t1._ticks * x));
    }

    public static Time operator /(Time t1, ulong y)
    {
      return new Time(t1._ticks / y);
    }

    public static bool operator ==(Time t1, Time t2)
    {
      return t1._ticks == t2._ticks;
    }

    public static bool operator !=(Time t1, Time t2)
    {
      return t1._ticks != t2._ticks;
    }

    public static bool operator >(Time t1, Time t2)
    {
      return t1._ticks > t2._ticks;
    }

    public static bool operator <(Time t1, Time t2)
    {
      return t1._ticks < t2._ticks;
    }

    public static bool operator >=(Time t1, Time t2)
    {
      return t1._ticks >= t2._ticks;
    }

    public static bool operator <=(Time t1, Time t2)
    {
      return t1._ticks <= t2._ticks;
    }

    /// <summary>
    /// Compare times
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public int CompareTo(Time t)
    {
      return _ticks.CompareTo(t._ticks);
    }
 
    public static explicit operator ulong(Time t)
    {
      return t._ticks;
    }

    public override string ToString()
    {
      return ToString(10, 3, "S");
    }

    public string ToString(string format)
    {
      int width = 10;
      int places = 3;
      string unit = "S";

      string [] ss = format.Split('.');

      if(ss.Length > 0)
        width = int.Parse(ss[0]);

      if(ss.Length > 1)
        places = int.Parse(ss[1]);

      if(ss.Length > 2)
        unit = ss[2];

      return ToString(width, places, unit);
    }

    private string ToString(int width, int places, string unit)
    {
      var m = 0;                  // SI multiplyer index
      double ti = _ticks;         // Integer part of converted time
        
      while(ti >= 1000.0)
      {
        m++;
        ti /= 1000.0;
      }
      
      var result = ti.ToString("F" + places) + SI[m] + unit;
      return result.PadLeft(width, ' ');
    }

    private static int siToIndex(char si)
    {
      switch(si)
      {
        case 'p': return 0;
        case 'n': return 3;
        case 'u': return 6;
        case 'm': return 9;
        case 'S': return 12;
        case 'k': return 15;
        case 'M': return 18;

        default:
          throw new ApplicationException(string.Format("Invalid SI multiplier{0}", SI));
      }
    }

    public override bool Equals(object obj)
    {
      if((obj == null) || !(obj is Time))
        return false;

      if(((Time)obj)._ticks == _ticks)
        return true;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return _ticks.GetHashCode();
    }


  }
}
