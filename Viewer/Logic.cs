using System;

namespace Simulator
{


  /// <summary>
  /// The multi-valued logic type
  /// </summary>
  public struct Logic
  {
    /// <summary>
    /// The multi-valued logic states
    /// </summary>
    enum Mvl : byte
    {
      /// <summary>
      /// A weak low
      /// </summary>
      l,
      /// <summary>
      /// A weak high
      /// </summary>
      h,
      /// <summary>
      /// A forcing low
      /// </summary>
      L,
      /// <summary>
      /// A forcing high
      /// </summary>
      H,
      /// <summary>
      /// High impedance
      /// </summary>
      Z,
      /// <summary>
      /// Conflicting drivers
      /// </summary>
      X,
      /// <summary>
      /// Unknown
      /// </summary>
      U,
      /// <summary>
      /// Dummy state used as rogue value
      /// </summary>
      Dummy
    }

    /// <summary>
    /// Weak low state
    /// </summary>
    public static Logic l = new Logic(Mvl.l);
    /// <summary>
    /// Weak high state
    /// </summary>
    public static Logic h = new Logic(Mvl.h);
    /// <summary>
    /// Forcing low state
    /// </summary>
    public static Logic L = new Logic(Mvl.L);
    /// <summary>
    /// Forcing high state
    /// </summary>
    public static Logic H = new Logic(Mvl.H);
    /// <summary>
    /// High impedance state
    /// </summary>
    public static Logic Z = new Logic(Mvl.Z);
    /// <summary>
    /// Conflicting drive state
    /// </summary>
    public static Logic X = new Logic(Mvl.X);
    /// <summary>
    /// Unknown state
    /// </summary>
    public static Logic U = new Logic(Mvl.U);
    /// <summary>
    /// Dummy state used as rogue value
    /// (internal use only)
    /// </summary>
    public static Logic Dummy = new Logic(Mvl.Dummy);
    /// <summary>
    /// The present state
    /// </summary>
    Mvl state;

    /// <summary>
    /// Private constructor
    /// Use the static members to get states
    /// </summary>
    /// <param name="state"></param>
    private Logic(Mvl state)
    {
      this.state = state;
    }

    public static implicit operator Logic(char c)
    {
      switch(c)
      {
        case '0': return L;
        case '1': return H;
        case 'l': return l;
        case 'h': return h;
        case 'L': return L;
        case 'H': return H;
        case 'Z': return Z;
        case 'X': return X;
        case 'U': return U;
        default: throw new ApplicationException
          (string.Format("Invalid MVL character, {0}", c));
      }
    }

    /// <summary>
    /// Resolve wired outputs
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Logic Wire(Logic a, Logic b)
    {
      return new Logic(ResolutionTable.Wired[(byte)a.state, (byte)b.state]);
    }

    /// <summary>
    /// Logical and
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Logic operator &(Logic x, Logic y)
    {
      return new Logic(ResolutionTable.And[(byte)x.state, (byte)y.state]);
    }

    /// <summary>
    /// Logical or
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Logic operator |(Logic x, Logic y)
    {
      return new Logic(ResolutionTable.Or[(byte)x.state, (byte)y.state]);
    }

    /// <summary>
    /// Logical xor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Logic operator ^(Logic x, Logic y)
    {
      return new Logic(ResolutionTable.Xor[(byte)x.state, (byte)y.state]);
    }

    /// <summary>
    /// Logical not
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static Logic operator ~(Logic x)
    {
      return new Logic(ResolutionTable.Not[(byte)x.state]);
    }

    /// <summary>
    /// Buffer
    /// </summary>
    /// <remarks>
    /// There is no operator form of buffer because there is no appropriate
    /// operator.  Since the operators all produce buffered results there is
    /// no requirement for a buffer operator to use in expressions anyway.
    /// Use ~~x for x.Buffer if you must as this is at least fairly obvious
    /// (Better than using unary +)
    ///</remarks> 
    public Logic Buffer
    {
      get { return new Logic(ResolutionTable.Buffer[(byte)state]); }
    }

    /// <summary>
    /// Test for a valid low state
    /// </summary>
    public bool Lo { get { return (state == Mvl.l) || (state == Mvl.L); } }

    /// <summary>
    /// Test for a valid high state
    /// </summary>
    public bool Hi { get { return (state == Mvl.h) || (state == Mvl.H); } }

    /// <summary>
    /// Test for a driven state
    /// </summary>
    public bool Driven { get { return state != Mvl.Z; } }

    /// <summary>
    /// Test for an invalid state
    /// </summary>
    public bool Invalid { get { return (state == Mvl.X) || (state == Mvl.U); } }


    /// <summary>
    /// Test states for equality
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool operator ==(Logic x, Logic y)
    {
      return x.state == y.state;
    }

    /// <summary>
    /// Test states for inequality
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool operator !=(Logic x, Logic y)
    {
      return x.state != y.state;
    }

    /// <summary>
    /// Convert to the state letter
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string[] names = { "l", "h", "L", "H", "Z", "X", "U" };
      return names[(byte)state];
    }

    public override bool Equals(object obj)
    {
      if((obj == null) || !(obj is Logic))
        return false;

      if(((Logic)obj).state == state)
        return true;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return state.GetHashCode();
    }

    /// <summary>
    /// The resolution tables for Mvl states
    /// </summary>
    class ResolutionTable
    {
      /// <summary>
      /// Resolution table for wired-or
      /// </summary>
      public static Mvl[,] Wired =
      //     l      h      L      H      Z      X      U
      {{ Mvl.l, Mvl.U, Mvl.L, Mvl.H, Mvl.l, Mvl.X, Mvl.U },  // l
       { Mvl.U, Mvl.h, Mvl.L, Mvl.H, Mvl.h, Mvl.X, Mvl.U },  // h
       { Mvl.L, Mvl.L, Mvl.L, Mvl.X, Mvl.L, Mvl.X, Mvl.U },  // L
       { Mvl.H, Mvl.H, Mvl.X, Mvl.H, Mvl.H, Mvl.X, Mvl.U },  // H
       { Mvl.l, Mvl.H, Mvl.L, Mvl.H, Mvl.Z, Mvl.X, Mvl.U },  // Z
       { Mvl.X, Mvl.X, Mvl.X, Mvl.X, Mvl.X, Mvl.X, Mvl.X },  // X
       { Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.X, Mvl.U }}; // U

      /// <summary>
      /// Resolution table for and
      /// </summary>
      public static Mvl[,] And =
      //     l      h      L      H      Z      X      U
      {{ Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L },  // l
       { Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // h
       { Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L, Mvl.L },  // L
       { Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // H
       { Mvl.L, Mvl.U, Mvl.L, Mvl.U, Mvl.U, Mvl.U, Mvl.U },  // Z
       { Mvl.L, Mvl.U, Mvl.L, Mvl.U, Mvl.U, Mvl.U, Mvl.U },  // X
       { Mvl.L, Mvl.U, Mvl.L, Mvl.U, Mvl.U, Mvl.U, Mvl.U }}; // U

      /// <summary>
      /// Resolution table for or
      /// </summary>
      public static Mvl[,] Or =
      //     l      h      L      H      Z      X      U
      {{ Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // l
       { Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H },  // h
       { Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.L, Mvl.L, Mvl.L },  // L
       { Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H, Mvl.H },  // H
       { Mvl.U, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // Z
       { Mvl.U, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // X
       { Mvl.U, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U }}; // U

      /// <summary>
      /// Resolution table for xor
      /// </summary>
      public static Mvl[,] Xor =
      //     l      h      L      H      Z      X      U
      {{ Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // l
       { Mvl.H, Mvl.L, Mvl.H, Mvl.L, Mvl.U, Mvl.U, Mvl.U },  // h
       { Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U },  // L
       { Mvl.H, Mvl.L, Mvl.H, Mvl.L, Mvl.U, Mvl.U, Mvl.U },  // H
       { Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U },  // Z
       { Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U },  // X
       { Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U, Mvl.U }}; // U

      /// <summary>
      /// Resolution table for buffer
      /// </summary>
      public static Mvl[] Buffer =
      //    l      h      L      H      Z      X      U
      { Mvl.L, Mvl.H, Mvl.L, Mvl.H, Mvl.U, Mvl.U, Mvl.U };

      /// <summary>
      /// Resolution table for not
      /// </summary>
      public static Mvl[] Not =
      //    l      h      L      H      Z      X      U
      { Mvl.H, Mvl.L, Mvl.H, Mvl.L, Mvl.U, Mvl.U, Mvl.U };
      }
  }
}
