// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using System.Collections.Generic;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class AggregateProviderCompiler : TypeCompiler<AggregateProvider>
  {
    protected override ExecutableProvider Compile(AggregateProvider provider, params ExecutableProvider[] compiledSources)
    {
      if (provider.GroupColumnIndexes.Length == 0)
        return new Providers.Executable.AggregateProvider(
          provider,
          compiledSources[0]);
      
      var groupOrder = new bool[provider.GroupColumnIndexes.Length];
      var source = compiledSources[0].Origin;
      
      while (source != null) {
        if (!GroupIsOrdered(groupOrder) && 
          (typeof(JoinProvider) == source.GetType() || typeof(SelectProvider) == source.GetType()))
            break;

        if (typeof (SortProvider) == source.GetType()) {
          for (int i = 0; i < provider.GroupColumnIndexes.Length; i++)
            if (((SortProvider) source).Order.ContainsKey(provider.GroupColumnIndexes[i]))
              groupOrder[i] = true;
        }
        if (GroupIsOrdered(groupOrder))
          return new Providers.Executable.OrderedGroupProvider(
              provider,
              compiledSources[0]);
        source = (source.Sources.Length != 0) ? (CompilableProvider)source.Sources[0] : null;
      }

          return new Providers.Executable.UnorderedGroupProvider(
          provider,
          compiledSources[0]);
  }

    private static bool GroupIsOrdered(IEnumerable<bool> group)
    {
      foreach (var value in group) 
        if (!value)
          return false;
      return true;
    }
    

    // Constructor

    public AggregateProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}