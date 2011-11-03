// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class SkipProvider : UnaryExecutableProvider<Compilable.SkipProvider>
  {
    private const string ToString_SkipParameters = "{0}, Value: {1}";

    #region Cached properties

    private const string CachedCountName = "CachedCount";

    private int CachedCount {
      get { return (int) GetCachedValue<object>(EnumerationContext.Current, CachedCountName); }
      set { SetCachedValue(EnumerationContext.Current, CachedCountName, (object) value); }
    }

    #endregion

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedCount = Origin.Count.Invoke();
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Skip(CachedCount);
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      long? count = null;
      try {
        count = Origin.Count.Invoke();
      }
      catch {}
      return string.Format(ToString_SkipParameters,
        base.ParametersToString(),
        count.HasValue ? count.ToString() : Strings.NotAvailable);
    }


    // Constructors

    public SkipProvider(Compilable.SkipProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}