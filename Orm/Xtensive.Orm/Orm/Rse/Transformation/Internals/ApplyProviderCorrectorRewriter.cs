// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;


namespace Xtensive.Orm.Rse.Transformation
{
  internal sealed class ApplyProviderCorrectorRewriter : CompilableProviderVisitor
  {
    #region Nested types: CorrectorState

    public sealed class CorrectorState : IDisposable
    {
      private readonly CorrectorState previousState;
      private readonly ApplyProviderCorrectorRewriter owner;
      private bool isDisposed;

      public 
        Dictionary<ApplyParameter, List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>> 
        Predicates { get; set; }

      public Dictionary<ApplyParameter, bool> SelfConvertibleApplyProviders { get; set; }

      public Dictionary<ApplyParameter, List<Pair<CalculateProvider, ColumnCollection>>> 
        CalculateProviders { get; set; }

      public Dictionary<CalculateProvider, List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>>
        CalculateFilters { get; set; }

      public bool ExistsApplyProviderRequiringConversion {
        get { return Predicates.Count > 0 || CalculateProviders.Count > 0;}
      }


      // Constructors

      public CorrectorState(ApplyProviderCorrectorRewriter owner)
      {
        this.owner = owner;
        Predicates = 
          new Dictionary<ApplyParameter, List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>>();
        CalculateProviders = 
            new Dictionary<ApplyParameter, List<Pair<CalculateProvider, ColumnCollection>>>();
        CalculateFilters =
          new Dictionary<CalculateProvider, List<Pair<Expression<Func<Tuple, bool>>, ColumnCollection>>>();
        previousState = owner.State;
        if (previousState == null) {
          SelfConvertibleApplyProviders = new Dictionary<ApplyParameter, bool>();
        }
        else {
          SelfConvertibleApplyProviders = previousState.SelfConvertibleApplyProviders;
        }
        owner.State = this;
      }

      // IDisposable methods

      public void Dispose()
      {
        if (isDisposed)
          return;
        isDisposed = true;
        UpdateOwnerState();
        owner.State = previousState;
      }

      private void UpdateOwnerState()
      {
        if (previousState == null)
          return;
        foreach (var pair in CalculateProviders) {
          if (previousState.CalculateProviders.ContainsKey(pair.Key))
            previousState.CalculateProviders[pair.Key].AddRange(pair.Value);
          else
            previousState.CalculateProviders.Add(pair.Key, pair.Value);
        }
        foreach (var pair in CalculateFilters) {
          if (previousState.CalculateFilters.ContainsKey(pair.Key))
            previousState.CalculateFilters[pair.Key].AddRange(pair.Value);
          else
            previousState.CalculateFilters.Add(pair.Key, pair.Value);
        }
        foreach (var pair in Predicates) {
          if (previousState.Predicates.ContainsKey(pair.Key))
            ThrowInvalidOperationException();
          else
            previousState.Predicates.Add(pair.Key, pair.Value);
        }
      }
    }

    #endregion

    public CorrectorState State { get; private set;}

    private readonly bool throwOnCorrectionFault;

    private readonly ApplyFilterRewriter predicateRewriter = new ApplyFilterRewriter();
    private readonly CollectorHelper collectorHelper = new CollectorHelper();
    private readonly CalculateRelatedExpressionRewriter calculateExpressionRewriter =
      new CalculateRelatedExpressionRewriter();
    private readonly ApplyPredicateCollector predicateCollector;
    private readonly CalculateProviderCollector calculateProviderCollector;

    public CompilableProvider Rewrite(CompilableProvider rootProvider)
    {
      State = null;
      try {
        using (new CorrectorState(this))
          return VisitCompilable(rootProvider);
      }
      catch(InvalidOperationException) {
        if (throwOnCorrectionFault)
          throw;
        return rootProvider;
      }
    }

    public static void ThrowInvalidOperationException()
    {
      throw new InvalidOperationException(String.Format(Strings.ExCantConvertXToY,
        typeof (ApplyProvider).Name, typeof (PredicateJoinProvider).Name));
    }

    public static void ThrowInvalidOperationException(string description)
    {
      var sb = new StringBuilder();
      sb.Append(String.Format(Strings.ExCantConvertXToY,
        typeof (ApplyProvider).Name, typeof (PredicateJoinProvider).Name));
      sb.Append(" ");
      sb.Append(description);
      throw new InvalidOperationException(sb.ToString());
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      var isSelfConvertibleApply = provider.SequenceType != ApplySequenceType.All;
      State.SelfConvertibleApplyProviders.Add(provider.ApplyParameter, isSelfConvertibleApply);
      VisitBinaryProvider(provider, out left, out right);
      State.SelfConvertibleApplyProviders.Remove(provider.ApplyParameter);

      if (isSelfConvertibleApply) {
        return ProcesSelfConvertibleApply(provider, left, right);
      }

      CompilableProvider convertedApply = !State.Predicates.ContainsKey(provider.ApplyParameter) 
        ? new PredicateJoinProvider(left, right,(tLeft, tRight) => true, provider.ApplyType)
        : ConvertGenericApply(provider, left, right);
      return InsertCalculateProviders(provider, convertedApply);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if (calculateProviderCollector.TryAddFilter(provider))
        return source;

      var newProvider = source!=provider.Source 
        ? new FilterProvider(source, provider.Predicate) : provider;

      if (predicateCollector.TryAdd(newProvider))
        return source;
      return newProvider;
    }

    protected override Provider VisitAlias(AliasProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = source!=provider.Source ? new AliasProvider(source, provider.Alias) : provider;
      calculateProviderCollector.AliasColumns(provider);
      predicateCollector.AliasColumns(provider);
      return newProvider;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      predicateCollector.ValidateSelectedColumnIndexes(provider);
      calculateProviderCollector.ValidateSelectedColumnIndexes(provider);
      if (source != provider.Source)
        newProvider = new SelectProvider(source, provider.ColumnIndexes);
      return newProvider;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);

      if (provider.JoinType == JoinType.LeftOuter)
        EnsureAbsenceOfApplyProviderRequiringConversion();

      if (left != provider.Left || right != provider.Right)
        return new JoinProvider(left, right, provider.JoinType, provider.EqualIndexes);
      return provider;
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);

      if (provider.JoinType == JoinType.LeftOuter)
        EnsureAbsenceOfApplyProviderRequiringConversion();

      if (left != provider.Left || right != provider.Right)
        return new PredicateJoinProvider(left, right, provider.Predicate, provider.JoinType);
      return provider;
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if (left != provider.Left || right != provider.Right)
        return new IntersectProvider(left, right);
      return provider;
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if (left != provider.Left || right != provider.Right)
        return new ExceptProvider(left, right);
      return provider;
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if (left != provider.Left || right != provider.Right)
        return new ConcatProvider(left, right);
      return provider;
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if (left != provider.Left || right != provider.Right)
        return new UnionProvider(left, right);
      return provider;
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      if (source != provider.Source)
        newProvider = RecreateAggregate(provider, source);
      predicateCollector.ValidateAggregatedColumns(newProvider);
      return newProvider;
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      if (source != provider.Source)
        newProvider = RecreateCalculate(provider, source);
      return calculateProviderCollector.TryAdd(newProvider) ? source : newProvider;
    }

    protected override Provider VisitTake(TakeProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      EnsureAbsenceOfApplyProviderRequiringConversion();
      if (source != provider.Source)
        return new TakeProvider(source, provider.Count);
      return provider;
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      EnsureAbsenceOfApplyProviderRequiringConversion();
      if (source != provider.Source)
        return new SkipProvider(source, provider.Count);
      return provider;
    }

    #region Private \ internal methods

    private void VisitBinaryProvider(BinaryProvider provider, out CompilableProvider left,
      out CompilableProvider right)
    {
      using (new CorrectorState(this))
        left = VisitCompilable(provider.Left);

      using (new CorrectorState(this))
        right = VisitCompilable(provider.Right);
    }
    
    private Provider ProcesSelfConvertibleApply(ApplyProvider provider, CompilableProvider left,
      CompilableProvider right)
    {
      if (State.Predicates.ContainsKey(provider.ApplyParameter))
        State.Predicates.Remove(provider.ApplyParameter);
      if (left != provider.Left || right != provider.Right)
        return new ApplyProvider(provider.ApplyParameter, left, right, provider.IsInlined, provider.SequenceType, provider.ApplyType);
      return provider;
    }

    private PredicateJoinProvider ConvertGenericApply(ApplyProvider provider, CompilableProvider left,
      CompilableProvider right)
    {
      var oldPredicate = State.Predicates[provider.ApplyParameter];
      Expression<Func<Tuple, Tuple, bool>> concatenatedPredicate = null;
      foreach (var predicateAndColumns in oldPredicate) {
        var newPredicate = predicateRewriter.Rewrite(predicateAndColumns.First, predicateAndColumns.Second,
          right.Header.Columns);
        if (concatenatedPredicate == null)
          concatenatedPredicate = newPredicate;
        else
          concatenatedPredicate = collectorHelper
            .CreatePredicatesConjunction(newPredicate, concatenatedPredicate);
      }
      State.Predicates.Remove(provider.ApplyParameter);
      return new PredicateJoinProvider(left, right, concatenatedPredicate, provider.ApplyType);
    }

    private Provider InsertCalculateProviders(ApplyProvider provider, CompilableProvider convertedApply)
    {
      if (!State.CalculateProviders.ContainsKey(provider.ApplyParameter))
        return convertedApply;
      var result = convertedApply;
      foreach (var providerPair in State.CalculateProviders[provider.ApplyParameter]) {
        result = RewriteCalculateColumnExpressions(providerPair, result);
        result = InsertCalculateFilter(result, providerPair.First);
      }
      State.CalculateProviders.Remove(provider.ApplyParameter);
      return result;
    }

    private CompilableProvider InsertCalculateFilter(CompilableProvider source,
      CalculateProvider calculateProvider)
    {
      var result = source;
      if (State.CalculateFilters.ContainsKey(calculateProvider)) {
        Expression<Func<Tuple, bool>> concatenatedPredicate = null;
        foreach (var filterPair in State.CalculateFilters[calculateProvider]) {
          var currentPredicate = (Expression<Func<Tuple, bool>>) calculateExpressionRewriter
            .Rewrite(filterPair.First, filterPair.First.Parameters[0],
              filterPair.Second, result.Header.Columns);
          if (concatenatedPredicate == null)
            concatenatedPredicate = currentPredicate;
          else
            concatenatedPredicate = collectorHelper
              .CreatePredicatesConjunction(currentPredicate, concatenatedPredicate);
        }
        result = new FilterProvider(result, concatenatedPredicate);
        State.CalculateFilters.Remove(calculateProvider);
      }
      return result;
    }

    private void EnsureAbsenceOfApplyProviderRequiringConversion()
    {
      if (State.ExistsApplyProviderRequiringConversion)
        ThrowInvalidOperationException();
    }

    private static AggregateProvider RecreateAggregate(AggregateProvider provider,
      CompilableProvider source)
    {
      var acd = provider.AggregateColumns.Select(
        ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType));
      return new AggregateProvider(source, provider.GroupColumnIndexes, acd.ToArray());
    }

    private static CalculateProvider RecreateCalculate(CalculateProvider provider,
      CompilableProvider source)
    {
      var ccd = provider.CalculatedColumns.Select(
        column => new CalculatedColumnDescriptor(column.Name, column.Type, column.Expression));
      return new CalculateProvider(source, ccd.ToArray());
    }

    private CalculateProvider RewriteCalculateColumnExpressions(
      Pair<CalculateProvider, ColumnCollection> providerPair, CompilableProvider source)
    {
      var ccd = providerPair.First.CalculatedColumns.Select(
        column => {
          var newColumnExpression = (Expression<Func<Tuple, object>>) calculateExpressionRewriter
            .Rewrite(column.Expression, column.Expression.Parameters[0], providerPair.Second,
              source.Header.Columns);
          var currentName = providerPair.Second.Single(c => c.Index==column.Index).Name;
          return new CalculatedColumnDescriptor(currentName, column.Type, newColumnExpression);
        });
      return new CalculateProvider(source, ccd.ToArray());
    }

    #endregion


    // Constructors

    public ApplyProviderCorrectorRewriter(bool throwOnCorrectionFault)
    {
      this.throwOnCorrectionFault = throwOnCorrectionFault;
      predicateCollector = new ApplyPredicateCollector(this);
      calculateProviderCollector = new CalculateProviderCollector(this);
    }
  }
}