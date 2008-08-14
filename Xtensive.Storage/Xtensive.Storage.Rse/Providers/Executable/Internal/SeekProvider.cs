// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Helpers;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class SeekProvider : UnaryExecutableProvider<Compilable.SeekProvider>
  {
    #region Cached properties

    private const string CachedKeyName = "CachedKey";

    private Tuple CachedKey
    {
      get { return (Tuple)GetCachedValue<object>(EnumerationContext.Current, CachedKeyName); }
      set { SetCachedValue(EnumerationContext.Current, CachedKeyName, (object)value); }
    }

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedKey = Origin.Key.Invoke();
    }

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      SeekResult<Tuple> seekResult = sourceEnumerable.Seek(new Ray<IEntire<Tuple>>(Entire<Tuple>.Create(CachedKey)));
      if (seekResult.ResultType == SeekResultType.Exact)
        yield return seekResult.Result;
    }





    // Constructors

    public SeekProvider(Compilable.SeekProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}