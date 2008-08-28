// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Core.Collections
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
      values[name] = value;
      if (value==null)
        values.Remove(name);
    }
  }
}