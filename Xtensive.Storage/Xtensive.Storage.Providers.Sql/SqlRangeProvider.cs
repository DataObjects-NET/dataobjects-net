// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.01

using System;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlRangeProvider : SqlProvider
  {
    private const string CachedRange = "CachedRange";

    public Range<Entire<Tuple>> CurrentRange
    {
      get
      {
        var cachedValue = GetCachedValue<object>(EnumerationContext.Current, CachedRange);
        if (cachedValue == null) lock (this) if (GetCachedValue<object>(EnumerationContext.Current, CachedRange) == null) {
          var rangeProvider = (RangeProvider)Origin;
          var range = rangeProvider.CompiledRange.Invoke();
          if (range.IsEqualityRange(AdvancedComparer<Tuple>.Default)) {
            SetCachedValue(EnumerationContext.Current, CachedRange, (object)range);
            return range;
          }
          if(!range.IsEmpty)
            throw new NotSupportedException(Strings.ExOnlyEqualityRangesAreSupported);
        }
        return (Range<Entire<Tuple>>)cachedValue;
      }
    }


    // Constructor

    public SqlRangeProvider(CompilableProvider origin, SqlFetchRequest request, HandlerAccessor handlers, params ExecutableProvider[] sources)
      : base(origin, request, handlers, sources)
    {
    }
  }
}