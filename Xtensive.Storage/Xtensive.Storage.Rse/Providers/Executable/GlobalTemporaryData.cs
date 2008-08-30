// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Diagnostics;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Global temporary data context.
  /// </summary>
  public class GlobalTemporaryData : NamedValueCollection
  {
    /// <summary>
    /// Gets the current <see cref="GlobalTemporaryData"/> instance.
    /// </summary>
    public static GlobalTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return EnumerationContext.Current.GlobalTemporaryData; }
    }
  }
}