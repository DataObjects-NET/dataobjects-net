// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.12

using System;
using System.Collections.Generic;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  [Serializable]
  public abstract class RseCompiler : ProviderVisitor
  {
    protected override Provider VisitExecutionSite(ExecutionSiteProvider provider)
    {
      throw new System.NotImplementedException();
    }

    protected override Provider VisitTake(TakeProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.TakeProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitSkip(SkipProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.SkipProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitSelect(SelectProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.SelectProvider(
        provider,
        compiledSource,
        provider.ColumnIndexes);
    }

    protected override Provider VisitSeek(SeekProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.SeekProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitRaw(RawProvider provider)
    {
      return new Providers.Executable.RawProvider(provider);
    }

    protected override Provider VisitRange(RangeProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.RangeProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitSort(SortProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.SortProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitJoin(JoinProvider provider)
    {
      var left = (ExecutableProvider)Visit((CompilableProvider)provider.Sources[0]);
      var right = (ExecutableProvider)Visit((CompilableProvider)provider.Sources[1]);
      return new Providers.Executable.JoinProvider(
        provider,
        left,
        right);
    }

    protected override Provider VisitFilter(FilterProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.FilterProvider(
       provider,
       compiledSource);
    }

    protected override Provider VisitDistinct(DistinctProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.DistinctProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitCalculate(CalculationProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.CalculationProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitAlias(AliasProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.AliasProvider(
        provider,
        compiledSource);
    }

    protected override Provider VisitAggregate(AggregateProvider provider)
    {
      var compiledSource = (ExecutableProvider) Visit(provider.Source);
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

    protected override Provider VisitStore(StoredProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit((CompilableProvider)provider.Source);
      ExecutableProvider ex = null;
      if (provider.Source != null)
        ex = compiledSource;
      return new Providers.Executable.StoredProvider(provider, ex);
    }

    protected override Provider VisitReindex(ReindexProvider provider)
    {
      var compiledSource = (ExecutableProvider)Visit(provider.Source);
      return new Providers.Executable.ReindexProvider(
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
  }
}