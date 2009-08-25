// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.12

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <inheritdoc/>
  [Serializable]
  public abstract class RseCompiler : Compiler
  {
    private static readonly UrlInfo defaultClientLocation = new UrlInfo("rse://localhost/");
    private static readonly UrlInfo defaultServerLocation = new UrlInfo("rse://server/");

    /// <summary>
    /// Gets the default client location.
    /// </summary>
    public static UrlInfo DefaultClientLocation
    {
      get { return defaultClientLocation; }
    }

    /// <summary>
    /// Gets the default server location.
    /// </summary>
    public static UrlInfo DefaultServerLocation
    {
      get { return defaultServerLocation; }
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitTransfer(TransferProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.TakeProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSkip(SkipProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.SkipProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSelect(SelectProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.SelectProvider(
        provider,
        compiledSource,
        provider.ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSeek(SeekProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.SeekProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRaw(RawProvider provider)
    {
      return new Providers.Executable.RawProvider(provider);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRange(RangeProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.RangeProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRangeSet(RangeSetProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.RangeSetProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSort(SortProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.SortProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitJoin(JoinProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.JoinProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitPredicateJoin(PredicateJoinProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.PredicateJoinProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitFilter(FilterProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.FilterProvider(
       provider,
       compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitDistinct(DistinctProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.DistinctProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitCalculate(CalculateProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.CalculateProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAlias(AliasProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.AliasProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAggregate(AggregateProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      if (provider.GroupColumnIndexes.Length == 0)
        return new Providers.Executable.AggregateProvider(
          provider,
          compiledSource);

      var groupOrder = new bool[provider.GroupColumnIndexes.Length];
      var source = compiledSource.Origin;

      while (source != null) {
        if (!GroupIsOrdered(groupOrder) && (source.Type == ProviderType.Join || source.Type == ProviderType.Select))
          break;

        if (source.Type == ProviderType.Sort) {
          var order = ((SortProvider)source).Order;
          for (int i = 0; i < provider.GroupColumnIndexes.Length; i++)
            if (order.ContainsKey(provider.GroupColumnIndexes[i]))
              groupOrder[i] = true;
        }
        if (GroupIsOrdered(groupOrder))
          return new Providers.Executable.OrderedGroupProvider(
              provider,
              compiledSource);
        source = source.Sources.Length != 0
          ? (CompilableProvider)source.Sources[0]
          : null;
      }

      return new Providers.Executable.UnorderedGroupProvider(
      provider,
      compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitStore(StoreProvider provider)
    {
      ExecutableProvider ex = null;
      if (provider.Source != null) {
        var compiledSource = GetCompiled(provider.Source);
        ex = compiledSource;
      }
      return new Providers.Executable.StoreProvider(provider, ex);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitReindex(ReindexProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.ReindexProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitApply(ApplyProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.ApplyProvider(provider, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider)
    {
      if (provider.Header.Order.Count == 0)
        throw new InvalidOperationException(Strings.ExOrderingOfRecordsIsNotSpecifiedForRowNumberProvider);
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.RowNumberProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitExistence(ExistenceProvider provider)
    {
      var compiledSource = GetCompiled(provider.Source);
      return new Providers.Executable.ExistenceProvider(
        provider,
        compiledSource);
    }

    private static bool GroupIsOrdered(IEnumerable<bool> group)
    {
      foreach (var value in group)
        if (!value)
          return false;
      return true;
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIntersect(IntersectProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.IntersectProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitExcept(ExceptProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.ExceptProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitConcat(ConcatProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.ConcatProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitUnion(UnionProvider provider)
    {
      var left = GetCompiled(provider.Left);
      var right = GetCompiled(provider.Right);
      return new Providers.Executable.UnionProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitLock(LockProvider provider)
    {
      throw new NotImplementedException();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected RseCompiler(UrlInfo location, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(location, compiledSources)
    {
    }
  }
}