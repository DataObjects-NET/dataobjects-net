// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.29

using System.Diagnostics;
using Xtensive.Collections;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Transaction transaction-level temporary data context.
  /// </summary>
  public class TransactionTemporaryData : NamedValueCollection
  {
    /// <summary>
    /// Gets the current <see cref="TransactionTemporaryData"/> instance.
    /// </summary>
    public static TransactionTemporaryData Current
    {
      [DebuggerStepThrough]
      get { return EnumerationContext.Current.TransactionTemporaryData; }
    }
  }
}