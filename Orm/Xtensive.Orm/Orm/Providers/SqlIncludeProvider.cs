// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Default implementation for SQL <see cref="IncludeProvider"/>.
  /// </summary>
  public sealed class SqlIncludeProvider : SqlTemporaryDataProvider
  {
    internal static readonly Parameter<List<Tuple>> rowFilterParameter = new Parameter<List<Tuple>>("RowFilterData");

    private readonly Func<ParameterContext, IEnumerable<Tuple>> filterDataSource;

    private new IncludeProvider Origin { get { return (IncludeProvider) base.Origin; } }

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var parameterContext = ((EnumerationContext) context).ParameterContext;
      switch (Origin.Algorithm) {
      case IncludeAlgorithm.Auto:
        var filterData = filterDataSource.Invoke(parameterContext).ToList();
        if (filterData.Count > WellKnown.MaxNumberOfConditions) {
          LockAndStore(context, filterData);
        }
        else {
          parameterContext.SetValue(rowFilterParameter, filterData);
        }

        break;
      case IncludeAlgorithm.ComplexCondition:
        // nothing
        break;
      case IncludeAlgorithm.TemporaryTable:
        LockAndStore(context, filterDataSource.Invoke(parameterContext));
        break;
      default:
        throw new ArgumentOutOfRangeException("Origin.Algorithm");
      }
    }

    /// <inheritdoc/>
    protected internal override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      ClearAndUnlock(context);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="handlers">The handlers.</param>
    /// <param name="request">The request.</param>
    /// <param name="tableDescriptor">The table descriptor.</param>
    /// <param name="filterDataSource">The filter data source.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="source">The source.</param>
    public SqlIncludeProvider(
      HandlerAccessor handlers, QueryRequest request,
      TemporaryTableDescriptor tableDescriptor, Func<ParameterContext, IEnumerable<Tuple>> filterDataSource,
      IncludeProvider origin, ExecutableProvider source)
      : base(handlers, request, tableDescriptor, origin, new []{source})
    {
      this.filterDataSource = filterDataSource;
      Initialize();
    }
  }
}