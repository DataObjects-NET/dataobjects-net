// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.12

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <inheritdoc/>
  [Serializable]
  public abstract class RseCompiler : Compiler
  {
    private static readonly UrlInfo defaultLocation = new UrlInfo("rse://localhost/");

    /// <summary>
    /// Gets the default location.
    /// </summary>
    public static UrlInfo DefaultLocation
    {
      get { return defaultLocation; }
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitExecutionSite(ExecutionSiteProvider provider, ExecutableProvider[] sources)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitTake(TakeProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.TakeProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSkip(SkipProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.SkipProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSelect(SelectProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.SelectProvider(
        provider,
        compiledSource,
        provider.ColumnIndexes);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSeek(SeekProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.SeekProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRaw(RawProvider provider, ExecutableProvider[] sources)
    {
      return new Providers.Executable.RawProvider(provider);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRange(RangeProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.RangeProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitSort(SortProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.SortProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitJoin(JoinProvider provider, ExecutableProvider[] sources)
    {
      var left = sources[0];
      var right = sources[1];
      return new Providers.Executable.JoinProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitPredicateJoin(PredicateJoinProvider provider, ExecutableProvider[] sources)
    {
      var left = sources[0];
      var right = sources[1];
      return new Providers.Executable.PredicateJoinProvider(
        provider,
        left,
        right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitFilter(FilterProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.FilterProvider(
       provider,
       compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitDistinct(DistinctProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.DistinctProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitCalculate(CalculationProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.CalculationProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAlias(AliasProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.AliasProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitAggregate(AggregateProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      if (provider.GroupColumnIndexes.Length == 0)
        return new Providers.Executable.AggregateProvider(
          provider,
          compiledSource);

      var groupOrder = new bool[provider.GroupColumnIndexes.Length];
      var source = compiledSource.Origin;

      while (source != null)
      {
        if (!GroupIsOrdered(groupOrder) &&
          (typeof(JoinProvider) == source.GetType() || typeof(SelectProvider) == source.GetType()))
          break;

        if (typeof(SortProvider) == source.GetType())
        {
          for (int i = 0; i < provider.GroupColumnIndexes.Length; i++)
            if (((SortProvider)source).Order.ContainsKey(provider.GroupColumnIndexes[i]))
              groupOrder[i] = true;
        }
        if (GroupIsOrdered(groupOrder))
          return new Providers.Executable.OrderedGroupProvider(
              provider,
              compiledSource);
        source = (source.Sources.Length != 0) ? (CompilableProvider)source.Sources[0] : null;
      }

      return new Providers.Executable.UnorderedGroupProvider(
      provider,
      compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitStore(StoredProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      ExecutableProvider ex = null;
      if (provider.Source != null)
        ex = compiledSource;
      return new Providers.Executable.StoredProvider(provider, ex);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitReindex(ReindexProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.ReindexProvider(
        provider,
        compiledSource);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitApply(ApplyProvider provider, ExecutableProvider[] sources)
    {
      var left = sources[0];
      var right = sources[1];
      return new Providers.Executable.ApplyProvider(provider, left, right);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitRowNumber(RowNumberProvider provider, ExecutableProvider[] sources)
    {
      var compiledSource = sources[0];
      return new Providers.Executable.RowNumberProvider(
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


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected RseCompiler(UrlInfo location)
      : base(location)
    {}
  }
}