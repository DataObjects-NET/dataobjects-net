// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlIncludeProvider : SqlTemporaryDataProvider
  {
    public const string RowFilterDataName = "RowFilterData";
    
    private const int MaxNumberOfConditions = 20;

    private readonly Func<IEnumerable<Tuple>> filterDataSource;

    private new IncludeProvider Origin { get { return (IncludeProvider) base.Origin; } }

    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      switch (Origin.Algorithm) {
      case IncludeAlgorithm.Auto:
        var filterData = filterDataSource.Invoke().ToList();
        if (filterData.Count > MaxNumberOfConditions)
          LockAndStore(context, filterData);
        else
          context.SetValue(filterDataSource, RowFilterDataName, filterData);
        break;
      case IncludeAlgorithm.ComplexCondition:
        // nothing
        break;
      case IncludeAlgorithm.TemporaryTable:
        LockAndStore(context, filterDataSource.Invoke());
        break;
      default:
        throw new ArgumentOutOfRangeException("Origin.Algorithm");
      }
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      ClearAndUnlock(context);
      base.OnAfterEnumerate(context);
    }

    // Constructors

    public SqlIncludeProvider(
      HandlerAccessor handlers, QueryRequest request,
      TemporaryTableDescriptor tableDescriptor, Func<IEnumerable<Tuple>> filterDataSource,
      IncludeProvider origin, ExecutableProvider source)
      : base(handlers, request, tableDescriptor, origin, new []{source})
    {
      this.filterDataSource = filterDataSource;
    }
  }
}