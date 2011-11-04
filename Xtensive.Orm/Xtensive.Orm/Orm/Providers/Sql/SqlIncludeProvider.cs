// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Default implementation for SQL <see cref="IncludeProvider"/>.
  /// </summary>
  public class SqlIncludeProvider : SqlTemporaryDataProvider
  {
    public const string RowFilterDataName = "RowFilterData";
    
    private readonly Func<IEnumerable<Tuple>> filterDataSource;

    private new IncludeProvider Origin { get { return (IncludeProvider) base.Origin; } }

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      switch (Origin.Algorithm) {
      case IncludeAlgorithm.Auto:
        var filterData = filterDataSource.Invoke().ToList();
        if (filterData.Count > WellKnown.MaxNumberOfConditions)
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

    /// <inheritdoc/>
    public override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      ClearAndUnlock(context);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlers">The handlers.</param>
    /// <param name="request">The request.</param>
    /// <param name="tableDescriptor">The table descriptor.</param>
    /// <param name="filterDataSource">The filter data source.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="source">The source.</param>
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