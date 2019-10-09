using System;
using System.Linq;
using System.Reflection;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Domain builder without reprocessing.
  /// </summary>
  public class DomainBuilder
  {
    /// <summary>
    /// Builds the domain with specified config.
    /// </summary>
    /// <param name="config">The config.</param>
    /// <returns>The domain.</returns>
    public virtual Domain Build(DomainConfiguration config)
    {
      try
      {
        var domain = Domain.Build(config);
        return domain;
      }
      catch (InvalidOperationException e)
      {
        var ex = e.InnerException as ReflectionTypeLoadException;
        if (ex != null)
          throw new InvalidOperationException(
              string.Join("\r\n", ex.LoaderExceptions.Select(a => a.Message)), ex);
        throw;
      }
    }
  }
}