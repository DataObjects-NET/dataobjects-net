// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public sealed class SkipProvider : UnaryExecutableProvider<Compilable.SkipProvider>
  {
    #region Cached properties

    private const string CachedCountName = "CachedCount";

    private int CachedCount {
      get { return (int) GetCachedValue<object>(EnumerationContext.Current, CachedCountName); }
      set { SetCachedValue(EnumerationContext.Current, CachedCountName, (object) value); }
    }

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedCount = Origin.Count.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Skip(CachedCount);
    }


    // Constructor

    public SkipProvider(Compilable.SkipProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}