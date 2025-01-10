// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;


namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class DecimalAggregateColumnRewriter : CompilableProviderVisitor
  {
    private readonly List<CalculateProvider> calculateProviders = new();
    private readonly DomainModel domainModel;
    private readonly CompilableProvider rootProvider;

    public CompilableProvider Rewrite()
    {
      return VisitCompilable(rootProvider);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      OnRecursionEntrance(provider);
      var source = VisitCompilable(provider.Source);
      var resultParameters = OnRecursionExit(provider);
      var shouldUseNewProvider = source != provider.Source && resultParameters == null;

      var aggregateColumns = provider.AggregateColumns;
      var headerColumns = source.Header.Columns;
      var newDescriptors = new AggregateColumnDescriptor[aggregateColumns.Length];

      for (int i = 0, count = aggregateColumns.Length; i < count; i++) {
        var column = aggregateColumns[i];
        if (column.Type == WellKnownTypes.Decimal) {
          var originDescriptor = column.Descriptor;
          var aggregatedColumn = headerColumns[originDescriptor.SourceIndex];

          var hints = TryGuessDecimalPrecisionAndSclale(aggregatedColumn, source);
          if (hints.HasValue) {
            newDescriptors[i] = new AggregateColumnDescriptor(originDescriptor.Name, originDescriptor.SourceIndex, originDescriptor.AggregateType, hints.Value);
            shouldUseNewProvider = true;
            continue;
          }
        }
        newDescriptors[i] = column.Descriptor;
      }

      if (!shouldUseNewProvider) {
        return provider;
      }

      return source.Aggregate(provider.GroupColumnIndexes, newDescriptors);
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var visitedProvider = base.VisitCalculate(provider);
      calculateProviders.Add((CalculateProvider) visitedProvider);
      return visitedProvider;
    }


    private (int, int)? TryGuessDecimalPrecisionAndSclale(
      Column aggregatedColumn, CompilableProvider originDataSource)
    {
      var headerColumns = originDataSource.Header.Columns;

      if (aggregatedColumn is MappedColumn mColumn) {
        var resolvedColumn = mColumn.ColumnInfoRef.Resolve(domainModel);
        if (resolvedColumn.Precision.HasValue && resolvedColumn.Scale.HasValue)
          return (resolvedColumn.Precision.Value, resolvedColumn.Scale.Value);
      }
      else if (aggregatedColumn is CalculatedColumn cColumn) {
        if (headerColumns.Count == 1) {
          // If current source contains only calculated column which is aggregate,
          // that means it uses indexes of its source in the calculated column
          var ownerProvider = calculateProviders.FirstOrDefault(cp => cp.CalculatedColumns.Contains(cColumn));
          if (ownerProvider == null)
            return null;
          headerColumns = ownerProvider.Header.Columns;
        }
        var expression = cColumn.Expression;
        var usedColumns = new TupleAccessGatherer().Gather(expression);

        var maxFloorDigits = -1;
        var maxScaleDigits = -1;
        foreach (var cIndex in usedColumns.Distinct()) {
          var usedColumn = headerColumns[cIndex];
          if (usedColumn is MappedColumn mmColumn) {
            var resolvedColumn = mmColumn.ColumnInfoRef.Resolve(domainModel);

            (int? p, int? s) @params = Type.GetTypeCode(resolvedColumn.ValueType) switch {
              TypeCode.Decimal => (resolvedColumn.Precision, resolvedColumn.Scale),
              TypeCode.Int32 or TypeCode.UInt32 => (19, 8),
              TypeCode.Int64 or TypeCode.UInt64 => (28, 8),
              TypeCode.Byte or TypeCode.SByte => (8, 5),
              TypeCode.Int16 or TypeCode.UInt16 => (10, 5),
              _ => (null, null),
            };

            if (@params.p.HasValue && @params.s.HasValue) {
              if (maxScaleDigits < @params.s.Value)
                maxScaleDigits = @params.s.Value;
              var floorDigits = @params.p.Value - @params.s.Value;
              if (maxFloorDigits < floorDigits)
                maxFloorDigits = floorDigits;
            }
          }
        }

        if (maxFloorDigits == -1 || maxScaleDigits == -1)
          return null;
        if (maxFloorDigits + maxScaleDigits <= 28)
          return (maxFloorDigits + maxScaleDigits, maxScaleDigits);
      }

      return null;
    }

    public DecimalAggregateColumnRewriter(DomainModel model, CompilableProvider rootProvider)
    {
      domainModel = model;
      this.rootProvider = rootProvider;
    }
  }
}