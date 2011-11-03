// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.24

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Storage.Rse.Resources;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class PagingProvider : UnaryExecutableProvider<Compilable.PagingProvider>
  {
    #region Cached properties

    private const string CachedSkipName = "CachedSkip";
    private const string CachedTakeName = "CachedTake";

    private int CachedSkip
    {
      get { return (int)GetCachedValue<object>(EnumerationContext.Current, CachedSkipName); }
      set { SetCachedValue(EnumerationContext.Current, CachedSkipName, (object)value); }
    }

    private int CachedTake
    {
      get { return (int)GetCachedValue<object>(EnumerationContext.Current, CachedTakeName); }
      set { SetCachedValue(EnumerationContext.Current, CachedTakeName, (object)value); }
    }

    #endregion

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedSkip = Origin.Skip.Invoke();
      CachedTake = Origin.Take.Invoke();
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Skip(CachedSkip).Take(CachedTake);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      int? skip = null;
      int? take = null;
      try {
        skip = Origin.Skip.Invoke();
        take = Origin.Take.Invoke();
      }
      catch { }
      return string.Format("{0} : [{1};{2}]",
        base.ParametersToString(),
        skip.HasValue ? skip.ToString() : Strings.NotAvailable,
        take.HasValue ? take.ToString() : Strings.NotAvailable);
    }


    // Constructors

    public PagingProvider(Compilable.PagingProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}