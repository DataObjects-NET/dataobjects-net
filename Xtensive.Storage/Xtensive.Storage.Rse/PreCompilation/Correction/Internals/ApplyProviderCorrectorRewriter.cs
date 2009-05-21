// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class ApplyProviderCorrectorRewriter : CompilableProviderVisitor
  {
    #region Nested types: CorrectorState

    private sealed class CorrectorState : IDisposable
    {
      private readonly CorrectorState previousState;
      private readonly ApplyProviderCorrectorRewriter owner;
      private bool isDisposed;

      public 
        Dictionary<ApplyParameter, Pair<Expression<Func<Tuple, bool>>, ColumnCollection>?> 
        Predicates { get; set; }


      // Constructors

      public CorrectorState(ApplyProviderCorrectorRewriter owner)
      {
        this.owner = owner;
        Predicates = 
          new Dictionary<ApplyParameter, Pair<Expression<Func<Tuple, bool>>, ColumnCollection>?>();
        previousState = owner.State;
        owner.State = this;
      }

      // IDisposable methods

      public void Dispose()
      {
        if(isDisposed)
          return;
        isDisposed = true;
        Dispose(true);
      }

      private void Dispose(bool disposing)
      {
        UpdateOwnerState();
        owner.State = previousState;
        if(disposing)
          GC.SuppressFinalize(this);
      }

      private void UpdateOwnerState()
      {
        if(previousState == null)
          return;
        foreach (var pair in Predicates) {
          if(previousState.Predicates.ContainsKey(pair.Key)) {
            if (previousState.Predicates[pair.Key]!=null)
              ThrowInvalidOperationException();
            previousState.Predicates[pair.Key] = pair.Value;
          }
          else
            previousState.Predicates.Add(pair.Key, pair.Value);
        }
      }

      // Finalizer

      ~CorrectorState()
      {
        Dispose(false);
      }
    }

    #endregion

    private CorrectorState State { get; set;}
    private readonly Dictionary<ApplyParameter, bool> selfConvertibleApplyProviders =
      new Dictionary<ApplyParameter, bool>();
    private readonly Dictionary<ApplyParameter, List<CalculateProvider>> calculateProviders = 
      new Dictionary<ApplyParameter, List<CalculateProvider>>();
    private readonly Dictionary<CalculateProvider, Expression<Func<Tuple, bool>>> calculateFilters = 
      new Dictionary<CalculateProvider, Expression<Func<Tuple, bool>>>();

    private readonly bool throwOnCorrectionFault;

    private readonly ApplyFilterRewriter predicateRewriter = new ApplyFilterRewriter();
    private readonly ApplyParameterSearcher parameterSearcher = new ApplyParameterSearcher();
    private readonly ParameterRewriter parameterRewriter = new ParameterRewriter();
    private readonly TupleAccessGatherer tupleGatherer = new TupleAccessGatherer((p, i) => {});
    private readonly ApplyCalculateRewriter applyCalculateRewriter = new ApplyCalculateRewriter();

    public CompilableProvider Rewrite(CompilableProvider rootProvider)
    {
      selfConvertibleApplyProviders.Clear();
      calculateProviders.Clear();
      calculateFilters.Clear();
      try {
        using (new CorrectorState(this))
          return VisitCompilable(rootProvider);
      }
      catch(InvalidOperationException) {
        if(throwOnCorrectionFault)
          throw;
        return rootProvider;
      }
    }

    protected override Provider VisitApply(ApplyProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      var isSelfConvertibleApply = provider.IsApplyExistence || provider.IsApplyAggregate;
      selfConvertibleApplyProviders.Add(provider.ApplyParameter, isSelfConvertibleApply);
      VisitBinaryProvider(provider, out left, out right);
      selfConvertibleApplyProviders.Remove(provider.ApplyParameter);
      if(isSelfConvertibleApply) {
        return ProcesSelfConvertibleApply(provider, left, right);
      }
      CompilableProvider result = !State.Predicates.ContainsKey(provider.ApplyParameter) 
        ? new PredicateJoinProvider(left, right,(tLeft, tRight) => true, provider.ApplyType)
        : ConvertGenericApply(provider, left, right);
      if(!calculateProviders.ContainsKey(provider.ApplyParameter))
        return result;
      foreach (var calculateProvider in calculateProviders[provider.ApplyParameter]) {
        result = RecreateCalculate(calculateProvider, result, left.Header.Length);
        if(calculateFilters.ContainsKey(calculateProvider)) {
          var indexOffset = left.Header.Length;
          var mapping = new List<int>(ArrayUtils<int>.Create(result.Header.Length));
          foreach (var column in right.Header.Columns) {
            mapping[column.Index + indexOffset] = column.Index;
          }
          mapping[mapping.Count - 1] = calculateProvider.Header.Length - 1;
          var accessRewriter = new TupleAccessRewriter(mapping);
          result = new FilterProvider(result,
            (Expression<Func<Tuple, bool>>) accessRewriter.Rewrite(calculateFilters[calculateProvider]));
        }
      }
      calculateProviders.Remove(provider.ApplyParameter);
      return result;
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = source!=provider.Source 
        ? new FilterProvider(source, provider.Predicate) : provider;
      var tupleAccesses = tupleGatherer.Gather(provider.Predicate);
      if(tupleAccesses.Count == 0)
        return newProvider;
      var isDependingOnCalculate = TryConcatenateWithCalculateFilter(provider, tupleAccesses);
      var applyParameter = parameterSearcher.Find(newProvider.Predicate);
      if(applyParameter != null) {
        if(isDependingOnCalculate)
          ThrowInvalidOperationException();
        if (!selfConvertibleApplyProviders[applyParameter]) {
          SaveApplyPredicate(newProvider, applyParameter);
          return source;
        }
      }
      if(isDependingOnCalculate)
        return source;
      return newProvider;
    }

    protected override Provider VisitAlias(AliasProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      if(State.Predicates.Count > 0) {
        AliasPredicateColumns(provider);
      }
      return source != provider.Source ? new AliasProvider(source, provider.Alias) : provider;
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      if(source != provider.Source)
        newProvider = new SelectProvider(source, provider.ColumnIndexes);
      if(State.Predicates.Count > 0) {
        UpdateSelectedColumnIndexes(newProvider);
      }
      return newProvider;
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);

      if(provider.JoinType == JoinType.LeftOuter)
        foreach (var key in State.Predicates.Keys)
          State.Predicates[key] = null;

      if(left != provider.Left || right != provider.Right)
        return new JoinProvider(left, right, provider.JoinType, provider.JoinAlgorithm,
          provider.EqualIndexes);
      return provider;
    }

    protected override Provider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if(left != provider.Left || right != provider.Right)
        return new PredicateJoinProvider(left, right, provider.Predicate, provider.JoinType);
      return provider;
    }

    protected override Provider VisitIntersect(IntersectProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if(left != provider.Left || right != provider.Right)
        return new IntersectProvider(left, right);
      return provider;
    }

    protected override Provider VisitExcept(ExceptProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if(left != provider.Left || right != provider.Right)
        return new ExceptProvider(left, right);
      return provider;
    }

    protected override Provider VisitConcat(ConcatProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if(left != provider.Left || right != provider.Right)
        return new ConcatProvider(left, right);
      return provider;
    }

    protected override Provider VisitUnion(UnionProvider provider)
    {
      CompilableProvider left;
      CompilableProvider right;
      VisitBinaryProvider(provider, out left, out right);
      if(left != provider.Left || right != provider.Right)
        return new UnionProvider(left, right);
      return provider;
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      if(source != provider.Source) {
        var acd = provider.AggregateColumns.Select(
          ac => new AggregateColumnDescriptor(ac.Name, ac.SourceIndex, ac.AggregateType));
        newProvider = new AggregateProvider(source, provider.GroupColumnIndexes, acd.ToArray());
      }
      CheckGroupedColumnIndexes(newProvider);
      return newProvider;
    }

    protected override Provider VisitCalculate(CalculateProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      var newProvider = provider;
      if(source != provider.Source)
        newProvider = RecreateCalculate(provider, source);
      List<ApplyParameter> applyParameters = FindApplyParameters(newProvider);
      if(applyParameters.Count == 0)
        return newProvider;
      return VisitCalculateContainingApplyParameter(source, newProvider, applyParameters);
    }

    protected override Provider VisitTake(TakeProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      ValidateTakeSkip();
      if(source != provider.Source)
        return new TakeProvider(source, provider.Count);
      return provider;
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      var source = VisitCompilable(provider.Source);
      ValidateTakeSkip();
      if(source != provider.Source)
        return new SkipProvider(source, provider.Count);
      return provider;
    }

    #region Private \ internal methods

    private Pair<Expression<Func<Tuple, bool>>, ColumnCollection> ConcatenateWithExistingPredicate(
      FilterProvider provider, Pair<Expression<Func<Tuple, bool>>, ColumnCollection>? existingPredicatePair)
    {
      var newPredicate = CreatePredicatesConjunction(provider.Predicate, existingPredicatePair.Value.First);
      var currentColumns = existingPredicatePair.Value.Second;
      foreach (var column in provider.Header.Columns)
        if (!currentColumns.Any(c => c.Name==column.Name))
          currentColumns.Add(column);
      return new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(newPredicate, currentColumns);
    }

    private Expression<Func<Tuple, bool>> CreatePredicatesConjunction(
      Expression<Func<Tuple, bool>> newPredicate, Expression<Func<Tuple, bool>> oldPredicate)
    {
      var oldParameter = oldPredicate.Parameters[0];
      var newParameter = newPredicate.Parameters[0];
      var result = (Expression<Func<Tuple, bool>>) parameterRewriter
        .Rewrite(oldPredicate, oldParameter, newParameter);
      return (Expression<Func<Tuple, bool>>) Expression.Lambda(Expression
        .AndAlso(result.Body, newPredicate.Body),newParameter);
    }

    private void ValidateColumns(ApplyProvider provider)
    {
      if(State.Predicates[provider.ApplyParameter] == null 
        || State.Predicates[provider.ApplyParameter].Value.Second.Count == 0)
          ThrowInvalidOperationException();
    }

    private static void ThrowInvalidOperationException()
    {
      throw new InvalidOperationException(String.Format(Strings.ExCantConvertXToY,
        typeof (ApplyProvider).Name, typeof (PredicateJoinProvider).Name));
    }

    private void AliasPredicateColumns(AliasProvider provider)
    {
      var newPredicates =
        new Dictionary<ApplyParameter, Pair<Expression<Func<Tuple, bool>>, ColumnCollection>?>(
          State.Predicates.Count);
      foreach (var predicatePair in State.Predicates) {
        var filterAndColumns = predicatePair.Value;
        if (filterAndColumns!=null) {
          filterAndColumns = new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(
            filterAndColumns.Value.First, filterAndColumns.Value.Second.Alias(provider.Alias));
        }
        newPredicates.Add(predicatePair.Key, filterAndColumns);
      }
      State.Predicates = newPredicates;
    }

    private void SaveApplyPredicate(FilterProvider provider, ApplyParameter applyParameter)
    {
      bool isParameterAlreadyExists = State.Predicates.ContainsKey(applyParameter);
      Pair<Expression<Func<Tuple, bool>>, ColumnCollection>? oldPair = null;
      if(!isParameterAlreadyExists)
        State.Predicates.Add(applyParameter, null);
      else {
        oldPair = State.Predicates[applyParameter];
        if(oldPair == null)
          return;
      }
      Pair<Expression<Func<Tuple, bool>>, ColumnCollection> newPair;
      if(isParameterAlreadyExists)
        newPair = ConcatenateWithExistingPredicate(provider, oldPair);
      else
        newPair = new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(provider.Predicate,
          provider.Header.Columns);
      State.Predicates[applyParameter] = newPair;
    }

    private void VisitBinaryProvider(BinaryProvider provider, out CompilableProvider left,
      out CompilableProvider right)
    {
      using(new CorrectorState(this))
        left = VisitCompilable(provider.Left);

      using(new CorrectorState(this))
        right = VisitCompilable(provider.Right);
    }

    private void UpdateSelectedColumnIndexes(SelectProvider provider)
    {
      var newParameterAndColumns = CalculateNewColumnIndexes(provider);
      AssignNewColumnIndexes(newParameterAndColumns);
    }

    private void AssignNewColumnIndexes(Dictionary<ApplyParameter, List<Column>> newParameterAndColumns)
    {
      foreach (var pair in newParameterAndColumns) {
        if(pair.Value.Count > 0)
          State.Predicates[pair.Key] =
            new Pair<Expression<Func<Tuple, bool>>, ColumnCollection>(
              State.Predicates[pair.Key].Value.First, new ColumnCollection(pair.Value));
        else
          State.Predicates[pair.Key] = null;
      }
    }

    private Dictionary<ApplyParameter, List<Column>> CalculateNewColumnIndexes(SelectProvider provider)
    {
      var result = new Dictionary<ApplyParameter, List<Column>>();
      foreach (var pair in State.Predicates) {
        var predicataAndColumns = pair.Value;
        if (predicataAndColumns!=null) {
          var newColumns = new List<Column>();
          result.Add(pair.Key, newColumns);
          foreach (var column in predicataAndColumns.Value.Second) {
            if (!provider.Header.Columns.Contains(column)) {
              newColumns.Clear();
              break;
            }
            var columnName = column.Name;
            var newIndex = provider.Header.Columns[columnName].Index;
            newColumns.Add(column.Clone(newIndex));
          }
        }
      }
      return result;
    }

    private Provider ProcesSelfConvertibleApply(ApplyProvider provider, CompilableProvider left,
      CompilableProvider right)
    {
      if(State.Predicates.ContainsKey(provider.ApplyParameter))
        State.Predicates.Remove(provider.ApplyParameter);
      if(left != provider.Left || right != provider.Right)
        return new ApplyProvider(provider.ApplyParameter, left, right, provider.ApplyType);
      return provider;
    }

    private PredicateJoinProvider ConvertGenericApply(ApplyProvider provider, CompilableProvider left,
      CompilableProvider right)
    {
      ValidateColumns(provider);
      var oldPredicate = State.Predicates[provider.ApplyParameter];
      var newPredicate = predicateRewriter.Rewrite(oldPredicate.Value.First, oldPredicate.Value.Second,
        right.Header.Columns);
      State.Predicates.Remove(provider.ApplyParameter);
      return new PredicateJoinProvider(left, right, newPredicate, provider.ApplyType);
    }

    private void CheckGroupedColumnIndexes(AggregateProvider provider)
    {
      if(State.Predicates.Count == 0)
        return;
      var predicateKeyToBeRemoved = new List<ApplyParameter>();
      foreach (var predicatePair in State.Predicates) {
        if (predicatePair.Value==null)
          continue;
        foreach (var column in predicatePair.Value.Value.Second)
          if(!provider.GroupColumnIndexes.Contains(column.Index)) {
            predicateKeyToBeRemoved.Add(predicatePair.Key);
            break;
          }
      }
      foreach (var predicate in predicateKeyToBeRemoved)
        State.Predicates[predicate] = null;
    }

    private void ValidateTakeSkip()
    {
      if(State.Predicates.Count > 0 || State.Predicates.Any(p => p.Value != null))
        ThrowInvalidOperationException();
    }

    private Provider VisitCalculateContainingApplyParameter(CompilableProvider source,
      CalculateProvider newProvider, List<ApplyParameter> applyParameters)
    {
      if(applyParameters.Count > 1)
        ThrowInvalidOperationException();
      var applyParameter = applyParameters[0];
      if(!State.Predicates.ContainsKey(applyParameter))
        State.Predicates.Add(applyParameter, null);
      if(selfConvertibleApplyProviders[applyParameter])
        return newProvider;
      if(calculateProviders.ContainsKey(applyParameter))
        calculateProviders[applyParameter].Add(newProvider);
      else
        calculateProviders.Add(applyParameter, new List<CalculateProvider> {newProvider});
      return source;
    }

    private List<ApplyParameter> FindApplyParameters(CalculateProvider newProvider)
    {
      return (from column in newProvider.CalculatedColumns
      let parameter = parameterSearcher.Find(column.Expression)
      where parameter!=null
      select parameter).Distinct().ToList();
    }

    private bool TryConcatenateWithCalculateFilter(FilterProvider provider, List<int> tupleAccesses)
    {
      var isDependingOnCalculate = false;
      foreach (var key in State.Predicates.Keys) {
        if (!calculateProviders.ContainsKey(key))
          continue;
        foreach (var calculateProvider in calculateProviders[key]) {
          if (tupleAccesses.Any(
            i => calculateProvider.Header.Columns.Contains(provider.Header.Columns[i]))) {
            isDependingOnCalculate = true;
            if (calculateFilters.ContainsKey(calculateProvider)) {
              var newCalculateFilter =
                CreatePredicatesConjunction(calculateFilters[calculateProvider], provider.Predicate);
              calculateFilters[calculateProvider] = newCalculateFilter;
            }
            else
              calculateFilters.Add(calculateProvider, provider.Predicate);
          }
        }
      }
      return isDependingOnCalculate;
    }

    private static CalculateProvider RecreateCalculate(CalculateProvider provider,
      CompilableProvider source)
    {
      var ccd = provider.CalculatedColumns.Select(
        column => new CalculatedColumnDescriptor(column.Name, column.Type, column.Expression));
      return new CalculateProvider(source, ccd.ToArray());
    }

    private CalculateProvider RecreateCalculate(CalculateProvider provider, CompilableProvider source,
      int columnIndexOffset)
    {
      var ccd = provider.CalculatedColumns.Select(
        column => {
          var newColumnExpression = (Expression<Func<Tuple, object>>) applyCalculateRewriter
            .Rewrite(column.Expression, 0, columnIndexOffset);
          return new CalculatedColumnDescriptor(column.Name, column.Type, newColumnExpression);
        });
      return new CalculateProvider(source, ccd.ToArray());
    }

    #endregion


    // Constructors

    public ApplyProviderCorrectorRewriter(bool throwOnCorrectionFault)
    {
      this.throwOnCorrectionFault = throwOnCorrectionFault;
    }
  }
}