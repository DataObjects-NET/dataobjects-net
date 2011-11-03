// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Helpers;
using Xtensive.Indexing;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class SeekProvider : UnaryExecutableProvider<Compilable.SeekProvider>
  {
    private const string ToString_SeekParameters = "{0}, Value: {1}";

    #region Cached properties

    private const string CachedKeyName = "CachedKey";

    private Tuple CachedKey
    {
      get { return (Tuple)GetCachedValue<object>(EnumerationContext.Current, CachedKeyName); }
      set { SetCachedValue(EnumerationContext.Current, CachedKeyName, (object)value); }
    }

    #endregion

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedKey = Origin.CompiledKey.Invoke();
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      SeekResult<Tuple> seekResult = sourceEnumerable.Seek(CachedKey);
      if (seekResult.ResultType == SeekResultType.Exact)
        yield return seekResult.Result;
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      Tuple key = null;
      try {
        key  = Origin.CompiledKey.Invoke();
      }
      catch {}
      return string.Format(ToString_SeekParameters,
        base.ParametersToString(),
        key!=null ? key.ToString() : Strings.NotAvailable);
    }


    // Constructors

    public SeekProvider(Compilable.SeekProvider origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}