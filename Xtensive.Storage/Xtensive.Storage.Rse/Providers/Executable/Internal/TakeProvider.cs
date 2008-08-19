// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class TakeProvider : UnaryExecutableProvider<Compilable.TakeProvider>
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
      CachedCount = Origin.CompiledCount.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Take(CachedCount);
    }


    // Constructor

    public TakeProvider(Compilable.TakeProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}