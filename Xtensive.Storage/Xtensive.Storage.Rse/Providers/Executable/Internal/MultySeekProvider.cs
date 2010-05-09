// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.25

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Helpers;
using Xtensive.Indexing;
using Xtensive.Core.Linq;


namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public class MultySeekProvider: UnaryExecutableProvider<Compilable.FilterProvider>
  {
    private Func<IEnumerable<Tuple>> filterDataSource;

    #region Cached properties

    private const string CachedKeysName = "CachedKeys";

    private IEnumerable<Tuple> CachedKeys
    {
      get { return (IEnumerable<Tuple>)GetCachedValue<object>(EnumerationContext.Current, CachedKeysName); }
      set { SetCachedValue(EnumerationContext.Current, CachedKeysName, (object)value); }
    }

    #endregion

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedKeys = filterDataSource.Invoke();
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      foreach (var key in CachedKeys) {
        SeekResult<Tuple> seekResult = sourceEnumerable.Seek(key);
        if (seekResult.ResultType == SeekResultType.Exact)
          yield return seekResult.Result;
      }
    }


    public MultySeekProvider(Compilable.FilterProvider origin, 
      ExecutableProvider source, 
      Expression<Func<IEnumerable<Tuple>>> filterDataSource)
      : base(origin, source)
    {
      this.filterDataSource = filterDataSource.CachingCompile();
    }
  }
}