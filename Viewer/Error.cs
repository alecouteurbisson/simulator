using System;
using System.Collections.Generic;
using System.Text;

namespace Simulator
{
  /// <summary>
  /// Simple exception class with message formatting
  /// </summary>
  class Error : Exception
  {
    public Error(string message, params object[] args)
      : base(string.Format(message, args))  { }
  }
}
