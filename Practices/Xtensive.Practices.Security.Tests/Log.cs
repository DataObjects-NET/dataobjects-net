using System.Reflection;
using Xtensive.Diagnostics;

namespace Xtensive.Practices.Security.Tests
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  internal sealed class Log : LogTemplate<Log>
  {
    // Copy-paste this code!
    /// <summary>
    /// Gets the name of this log.
    /// </summary>
    public static readonly string Name;

    static Log()
    {
      string className = MethodBase.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}