// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract <see cref="CompilableProvider"/> visitor class.
  /// </summary>
  [Serializable]
  public abstract class ProviderVisitor
  {
    /// <summary>
    /// Visits the specified <paramref name="cp"/>.
    /// </summary>
    /// <param name="cp">The <see cref="CompilableProvider"/> to visit.</param>
    /// <returns>Visit result.</returns>
    protected virtual Provider Visit(CompilableProvider cp)
    {
      if (cp==null)
        return null;
      Provider result;
      ProviderType providerType = cp.Type;
      switch (providerType) {
        case ProviderType.Index:
          result = VisitIndex((IndexProvider) cp);
          break;
        case ProviderType.Reindex:
          result = VisitReindex((ReindexProvider) cp);
          break;
        case ProviderType.Store:
          result = VisitStore((StoredProvider) cp);
          break;
        case ProviderType.Aggregate:
          result = VisitAggregate((AggregateProvider) cp);
          break;
        case ProviderType.Alias:
          result = VisitAlias((AliasProvider) cp);
          break;
        case ProviderType.Calculate:
          result = VisitCalculate((CalculationProvider) cp);
          break;
        case ProviderType.Distinct:
          result = VisitDistinct((DistinctProvider) cp);
          break;
        case ProviderType.Filter:
          result = VisitFilter((FilterProvider) cp);
          break;
        case ProviderType.Join:
          result = VisitJoin((JoinProvider) cp);
          break;
        case ProviderType.Sort:
          result = VisitSort((SortProvider) cp);
          break;
        case ProviderType.Range:
          result = VisitRange((RangeProvider) cp);
          break;
        case ProviderType.Raw:
          result = VisitRaw((RawProvider) cp);
          break;
        case ProviderType.Seek:
          result = VisitSeek((SeekProvider) cp);
          break;
        case ProviderType.Select:
          result = VisitSelect((SelectProvider) cp);
          break;
        case ProviderType.Skip:
          result = VisitSkip((SkipProvider) cp);
          break;
        case ProviderType.Take:
          result = VisitTake((TakeProvider) cp);
          break;
        case ProviderType.ExecutionSite:
          result = VisitExecutionSite((ExecutionSiteProvider) cp);
          break;
        case ProviderType.RowNumber:
          result = VisitRowNumber((RowNumberProvider)cp);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return result;
    }

    /// <summary>
    /// Visits <see cref="ExecutionSiteProvider"/>.
    /// </summary>
    /// <param name="provider">Execution site provider.</param>
    protected abstract Provider VisitExecutionSite(ExecutionSiteProvider provider);

    /// <summary>
    /// Visits <see cref="RowNumberProvider"/>.
    /// </summary>
    /// <param name="provider">Row number provider.</param>
    protected abstract Provider VisitRowNumber(RowNumberProvider provider);

    /// <summary>
    /// Visits <see cref="TakeProvider"/>.
    /// </summary>
    /// <param name="provider">Take provider.</param>
    protected abstract Provider VisitTake(TakeProvider provider);

    /// <summary>
    /// Visits <see cref="SkipProvider"/>.
    /// </summary>
    /// <param name="provider">Skip provider.</param>
    protected abstract Provider VisitSkip(SkipProvider provider);

    /// <summary>
    /// Visits <see cref="SelectProvider"/>.
    /// </summary>
    /// <param name="provider">Select provider.</param>
    protected abstract Provider VisitSelect(SelectProvider provider);

    /// <summary>
    /// Visits <see cref="SeekProvider"/>.
    /// </summary>
    /// <param name="provider">Seek provider.</param>
    protected abstract Provider VisitSeek(SeekProvider provider);

    /// <summary>
    /// Visits <see cref="RawProvider"/>.
    /// </summary>
    /// <param name="provider">Raw provider.</param>
    protected abstract Provider VisitRaw(RawProvider provider);

    /// <summary>
    /// Visits <see cref="RangeProvider"/>.
    /// </summary>
    /// <param name="provider">Range provider.</param>
    protected abstract Provider VisitRange(RangeProvider provider);

    /// <summary>
    /// Visits <see cref="SortProvider"/>.
    /// </summary>
    /// <param name="provider">Sort provider.</param>
    protected abstract Provider VisitSort(SortProvider provider);

    /// <summary>
    /// Visits <see cref="JoinProvider"/>.
    /// </summary>
    /// <param name="provider">Join provider.</param>
    protected abstract Provider VisitJoin(JoinProvider provider);

    /// <summary>
    /// Visits <see cref="FilterProvider"/>.
    /// </summary>
    /// <param name="provider">Filter provider.</param>
    protected abstract Provider VisitFilter(FilterProvider provider);

    /// <summary>
    /// Visits <see cref="DistinctProvider"/>.
    /// </summary>
    /// <param name="provider">Distinct provider.</param>
    protected abstract Provider VisitDistinct(DistinctProvider provider);

    /// <summary>
    /// Visits <see cref="CalculationProvider"/>.
    /// </summary>
    /// <param name="provider">Calculation provider.</param>
    protected abstract Provider VisitCalculate(CalculationProvider provider);

    /// <summary>
    /// Visits <see cref="AliasProvider"/>.
    /// </summary>
    /// <param name="provider">Alias provider.</param>
    protected abstract Provider VisitAlias(AliasProvider provider);

    /// <summary>
    /// Visits <see cref="AggregateProvider"/>.
    /// </summary>
    /// <param name="provider">Aggregate provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitAggregate(AggregateProvider provider);

    /// <summary>
    /// Visits <see cref="StoredProvider"/>.
    /// </summary>
    /// <param name="provider">Store provider.</param>
    protected abstract Provider VisitStore(StoredProvider provider);

    /// <summary>
    /// Visits <see cref="IndexProvider"/>.
    /// </summary>
    /// <param name="provider">Index provider.</param>
    protected abstract Provider VisitIndex(IndexProvider provider);

    /// <summary>
    /// Visits the <see cref="ReindexProvider"/>.
    /// </summary>
    /// <param name="provider">Reindex provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitReindex(ReindexProvider provider);
  }
}