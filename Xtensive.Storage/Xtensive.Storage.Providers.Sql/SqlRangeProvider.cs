// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.01

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlRangeProvider : SqlProvider
  {
    private readonly Range<IEntire<Tuple>> original;
    private const string CachedRange = "CachedRange";

    public Range<IEntire<Tuple>> CurrentRange
    {
      get
      {
        var cachedValue = GetCachedValue<object>(EnumerationContext.Current, CachedRange);
        if (cachedValue == null) lock (this) if (GetCachedValue<object>(EnumerationContext.Current, CachedRange) == null) {
          var rangeProvider = (RangeProvider)Origin;
          var range = rangeProvider.CompiledRange.Invoke();
          if (original.IsSimilar(range)) {
            SetCachedValue(EnumerationContext.Current, CachedRange, (object) range);
            cachedValue = range;
          }
        }
        return (Range<IEntire<Tuple>>)cachedValue;
      }
    }


    // Constructor

    public SqlRangeProvider(CompilableProvider origin, SqlFetchRequest request, HandlerAccessor handlers, Range<IEntire<Tuple>> original, params ExecutableProvider[] sources)
      : base(origin, request, handlers, sources)
    {
      this.original = original;
    }
  }
}