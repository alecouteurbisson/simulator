using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Simulator
{
  /// <summary>
  /// A Part is a (potentially) fully specified device model
  /// equivalent to a physical chip.
  /// 
  /// The numerous timing/validation parameters required for a decent
  /// physical model are impractical for a normal constructor.
  /// A Part holds a device model and a set of parameters that are plugged
  /// into it automatically.  Parts can saved and loaded so that libraries
  /// of devices can be created.
  /// 
  /// It is also intended that Parts might contain state machine descriptions
  /// read by a generic FSM device that could be instantiated multiple times
  /// in a single device.  This would allow big behavioural models to be created
  /// easily and to run efficiently.
  /// </summary>
  public class Part
  {
    Parameters _parameters;
    Device _device;

    public Part(Device device, Parameters parameters)
    {
      _device = device;
      _parameters = parameters;
    }

    /// <summary>
    /// Bind the model parameters to the values supplied
    /// </summary>
    private void Bind()
    {
      Type devType = _device.GetType();
      foreach(PropertyInfo prop in devType.GetProperties())
      {
        object[] attr = prop.GetCustomAttributes(typeof(ParameterAttribute), true);
        if(attr.Length == 1)
        {
          string name = (attr[0] as ParameterAttribute).Name;
          string val = _parameters[name];
          try
          {
            if((val != null) && (val != string.Empty))
            {
              switch(prop.PropertyType.Name)
              {
                case "Simulator.Time":
                  prop.SetValue(_device, new Time(val), null);
                  break;

                case "Simulator.Logic":
                  prop.SetValue(_device, (Logic)val[0], null);
                  break;

                case "System.String":
                  prop.SetValue(_device, val, null);
                  break;

                case "System.Int32":
                  prop.SetValue(_device, Int32.Parse(val), null);
                  break;

                default:
                  Console.WriteLine("Bad type for parameter {0} in {1}", name, _device.Name);
                  break;
              }
            }
          }
          catch(Exception)
          {
            Console.WriteLine("Bad parameter value, {0}, for {1} in {2}", val, name, _device.Name);
          }
        }
      }
    }
  }

  /// <summary>
  /// A collection of parameters
  /// A parameter is always stored as a string and can be
  /// A Time.
  /// A logic state given as a single character.
  /// A string (obviously!)
  /// An integer.
  /// Incorrectly typed parameters are ignored with a warning.
  /// </summary>
  public class Parameters
  {
    private Dictionary<string, string> settings;

    public Parameters()
    {
      settings = new Dictionary<string,string>();
    }

    public void Add(string parameter, string val)
    {
      settings.Add(parameter, val);
    }

    public string this[string index]
    {
      get { return settings[index]; }
      set { settings[index] = value; }
    }
  }

  public class ParameterAttribute : Attribute
  {
    string _name;
    public string Name { get { return _name; } }

    public ParameterAttribute(string name)
    {
      _name = name;
    }
  }
}
