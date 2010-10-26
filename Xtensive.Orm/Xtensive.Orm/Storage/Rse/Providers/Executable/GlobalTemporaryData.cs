// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Global temporary data context.
  /// </summary>
  public class GlobalTemporaryData : NamedValueCollection
  {
    public object _lock = new object();

    /// <summary>
    /// Gets the current <see cref="GlobalTemporaryData"/> instance.
    /// </summary>
    public static GlobalTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return EnumerationContext.Current.GlobalTemporaryData; }
    }

    /// <inheritdoc/>
    public override object Get(string name)
    {
      lock (_lock) {
        return base.Get(name);
      }
    }

    /// <inheritdoc/>
    public override void Set(string name, object value)
    {
      lock (_lock) {
        base.Set(name, value);
      }
    }
  }
}