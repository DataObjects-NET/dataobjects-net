// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using Xtensive.Orm.Rse.Providers.Compilable;
using Xtensive.Orm.Rse.Compilation;


namespace Xtensive.Orm.Rse.Providers
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
        case ProviderType.Store:
          result = VisitStore((StoreProvider) cp);
          break;
        case ProviderType.Aggregate:
          result = VisitAggregate((AggregateProvider) cp);
          break;
        case ProviderType.Alias:
          result = VisitAlias((AliasProvider) cp);
          break;
        case ProviderType.Calculate:
          result = VisitCalculate((CalculateProvider) cp);
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
        case ProviderType.Paging:
          result = VisitPaging((PagingProvider)cp);
          break;
        case ProviderType.RowNumber:
          result = VisitRowNumber((RowNumberProvider)cp);
          break;
        case ProviderType.Apply:
          result = VisitApply((ApplyProvider)cp);
          break;
        case ProviderType.Existence:
          result = VisitExistence((ExistenceProvider)cp);
          break;
        case ProviderType.PredicateJoin:
          result = VisitPredicateJoin((PredicateJoinProvider)cp);
          break;
        case ProviderType.Intersect:
          result = VisitIntersect((IntersectProvider)cp);
          break;
        case ProviderType.Except:
          result = VisitExcept((ExceptProvider)cp);
          break;
        case ProviderType.Concat:
          result = VisitConcat((ConcatProvider)cp);
          break;
        case ProviderType.Union:
          result = VisitUnion((UnionProvider)cp);
          break;
        case ProviderType.Lock:
          result = VisitLock((LockProvider) cp);
          break;
        case ProviderType.Include:
          result = VisitInclude((IncludeProvider) cp);
          break;
        case ProviderType.FreeText:
          result = VisitFreeText((FreeTextProvider) cp);
          break;
        case ProviderType.Void:
          throw new NotSupportedException(Strings.ExProcessingOfVoidProviderIsNotSupported);
        default:
          throw new ArgumentOutOfRangeException();
      }
      return result;
    }

    /// <summary>
    /// Visits <see cref="PredicateJoinProvider"/>.
    /// </summary>
    /// <param name="provider">Predicate join provider.</param>
    protected abstract Provider VisitPredicateJoin(PredicateJoinProvider provider);

    /// <summary>
    /// Visits <see cref="ExistenceProvider"/>.
    /// </summary>
    /// <param name="provider">Existence provider.</param>
    protected abstract Provider VisitExistence(ExistenceProvider provider);

    /// <summary>
    /// Visits <see cref="ApplyProvider"/>.
    /// </summary>
    /// <param name="provider">Apply provider.</param>
    protected abstract Provider VisitApply(ApplyProvider provider);

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
    /// Visits <see cref="PagingProvider"/>.
    /// </summary>
    /// <param name="provider">Paging provider.</param>
    protected abstract Provider VisitPaging(PagingProvider provider);

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
    /// Visits <see cref="CalculateProvider"/>.
    /// </summary>
    /// <param name="provider">Calculate provider.</param>
    protected abstract Provider VisitCalculate(CalculateProvider provider);

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
    /// Visits <see cref="StoreProvider"/>.
    /// </summary>
    /// <param name="provider">Store provider.</param>
    protected abstract Provider VisitStore(StoreProvider provider);

    /// <summary>
    /// Visits <see cref="IndexProvider"/>.
    /// </summary>
    /// <param name="provider">Index provider.</param>
    protected abstract Provider VisitIndex(IndexProvider provider);

    /// <summary>
    /// Visits the <see cref="IntersectProvider"/>.
    /// </summary>
    /// <param name="provider">Intersect provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitIntersect(IntersectProvider provider);

    /// <summary>
    /// Visits the <see cref="ExceptProvider"/>.
    /// </summary>
    /// <param name="provider">Except provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitExcept(ExceptProvider provider);

    /// <summary>
    /// Visits the <see cref="ConcatProvider"/>.
    /// </summary>
    /// <param name="provider">Concat provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitConcat(ConcatProvider provider);

    /// <summary>
    /// Visits the <see cref="UnionProvider"/>.
    /// </summary>
    /// <param name="provider">Union provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitUnion(UnionProvider provider);

    /// <summary>
    /// Visits the <see cref="LockProvider"/>.
    /// </summary>
    /// <param name="provider">Lock provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitLock(LockProvider provider);

    /// <summary>
    /// Visits the <see cref="IncludeProvider"/>.
    /// </summary>
    /// <param name="provider">Include provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitInclude(IncludeProvider provider);

    /// <summary>
    /// Visits the <see cref="FreeTextProvider"/>.
    /// </summary>
    /// <param name="provider">FreeText provider.</param>
    /// <returns></returns>
    protected abstract Provider VisitFreeText(FreeTextProvider provider);
  }
}