// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Rse.Providers;
using System.Threading.Tasks;
using System.Threading;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Default implementation for SQL <see cref="IncludeProvider"/>.
  /// </summary>
  public sealed class SqlIncludeProvider : SqlTemporaryDataProvider
  {
    private class RowFilterParameter : Parameter<List<Tuple>>, IEquatable<RowFilterParameter>
    {
      private readonly TemporaryTableDescriptor temporaryTableDescriptor;

      public bool Equals(RowFilterParameter other) =>
        other is not null
        && (ReferenceEquals(this, other) || ReferenceEquals(temporaryTableDescriptor, other.temporaryTableDescriptor));

      public override bool Equals(object obj) =>
        ReferenceEquals(this, obj)
          || obj is RowFilterParameter other && Equals(other);

      public override int GetHashCode() => temporaryTableDescriptor != null ? temporaryTableDescriptor.GetHashCode() : 0;

      public RowFilterParameter(TemporaryTableDescriptor temporaryTableDescriptor) : base("RowFilterData")
      {
        this.temporaryTableDescriptor = temporaryTableDescriptor;
      }
    }

    private readonly Func<ParameterContext, IEnumerable<Tuple>> filterDataSource;

    private new IncludeProvider Origin => (IncludeProvider) base.Origin;

    internal static Parameter<List<Tuple>> CreateFilterParameter(TemporaryTableDescriptor temporaryTableDescriptor) =>
      new RowFilterParameter(temporaryTableDescriptor);

    /// <inheritdoc/>
    protected internal override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var parameterContext = ((EnumerationContext) context).ParameterContext;
      switch (Origin.Algorithm) {
        case IncludeAlgorithm.Auto:
          var filterData = filterDataSource.Invoke(parameterContext).ToList();
          if (filterData.Count > DomainHandler.Domain.Configuration.MaxNumberOfConditions) {
            LockAndStore(context, filterData);
          }
          else {
            parameterContext.SetValue(CreateFilterParameter(tableDescriptor), filterData);
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

    protected internal override async Task OnBeforeEnumerateAsync(Rse.Providers.EnumerationContext context, CancellationToken token)
    {
      await base.OnBeforeEnumerateAsync(context, token).ConfigureAwaitFalse();
      var parameterContext = ((EnumerationContext) context).ParameterContext;
      switch (Origin.Algorithm) {
        case IncludeAlgorithm.Auto:
          var filterData = filterDataSource.Invoke(parameterContext).ToList();
          if (filterData.Count > DomainHandler.Domain.Configuration.MaxNumberOfConditions)
            await LockAndStoreAsync(context, filterData, token).ConfigureAwaitFalse();
          else
            parameterContext.SetValue(CreateFilterParameter(tableDescriptor), filterData);
          break;
        case IncludeAlgorithm.ComplexCondition:
          // nothing
          break;
        case IncludeAlgorithm.TemporaryTable:
          await LockAndStoreAsync(context, filterDataSource.Invoke(parameterContext), token).ConfigureAwaitFalse();
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