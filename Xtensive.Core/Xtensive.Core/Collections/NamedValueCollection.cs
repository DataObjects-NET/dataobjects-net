// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="Dictionary{TKey,TValue}"/>-based 
  /// <see cref="INamedValueCollection"/> implementation.
  /// </summary>
  [DebuggerDisplay("Count = {Count}")]
  public class NamedValueCollection : INamedValueCollection
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    /// <inheritdoc/>
    public virtual object Get(string name)
    {
      object result;
      if (values.TryGetValue(name, out result))
        return result;
      return null;
    }

    /// <inheritdoc/>
    public virtual void Set(string name, object value)
    {
      values[name] = value;
      if (value==null)
        values.Remove(name);
    }
  }
}