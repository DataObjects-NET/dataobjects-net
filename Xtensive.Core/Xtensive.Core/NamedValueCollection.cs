using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Core
{
  /// <inheritdoc/>
  public class NamedValueCollection : INamedValueCollection
  {
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    /// <inheritdoc/>
    public object Get(string name)
    {
      object result;
      if (values.TryGetValue(name, out result))
        return result;
      return null;
       
    }

    /// <inheritdoc/>
    public void Set(string name, object value)
    {
      object result;
      if (!values.TryGetValue(name, out result))
        values.Add(name, value);
    }
  }
}