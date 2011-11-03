// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.25

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Helpers;
using Xtensive.Indexing;
using Xtensive.Linq;


namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Multi-seek operation executable provider.
  /// </summary>
  [Serializable]
  public class MultiSeekProvider: UnaryExecutableProvider<Compilable.FilterProvider>
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
    public override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      CachedKeys = filterDataSource.Invoke();
    }

    /// <inheritdoc/>
    public override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sourceEnumerable = Source.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      foreach (var key in CachedKeys) {
        SeekResult<Tuple> seekResult = sourceEnumerable.Seek(key);
        if (seekResult.ResultType == SeekResultType.Exact)
          yield return seekResult.Result;
      }
    }


    public MultiSeekProvider(Compilable.FilterProvider origin, 
      ExecutableProvider source, 
      Expression<Func<IEnumerable<Tuple>>> filterDataSource)
      : base(origin, source)
    {
      this.filterDataSource = filterDataSource.CachingCompile();
    }
  }
}