// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.InheritanceSupport;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public class IndexProviderCompiler : TypeCompiler<IndexProvider>
  {
    private readonly HandlerAccessor handlerAccessor;

    protected override Provider Compile(IndexProvider provider)
    {
      Provider result;
      IndexInfo indexInfo = provider.Index;
      var handler = (DomainHandler) handlerAccessor.DomainHandler;
      if (!indexInfo.IsVirtual)
        result = new Rse.Providers.Executable.IndexProvider(provider, indexInfo, handler.GetRealIndex);
      else {
        if ((indexInfo.Attributes & IndexAttributes.Filtered)!=0) {
          Provider source = Compile(new IndexProvider(indexInfo.BaseIndexes.First()));
          int columnIndex;
          if (indexInfo.IsPrimary) {
            FieldInfo typeIDField = indexInfo.ReflectedType.Fields[handlerAccessor.Domain.NameProvider.TypeId];
            columnIndex = typeIDField.MappingInfo.Offset;
          }
          else
            columnIndex = indexInfo.Columns.Select((c, i) => c.Field.Name==handlerAccessor.Domain.NameProvider.TypeId ? i : 0).Sum();
          List<int> typeIDList = indexInfo.ReflectedType.GetDescendants(true).Select(info => info.TypeId).ToList();
          typeIDList.Add(indexInfo.ReflectedType.TypeId);
          result = new FilterInheritorsProvider(provider, source, columnIndex, handlerAccessor.Domain.Model.Types.Count, typeIDList.ToArray());
        }
        else if ((indexInfo.Attributes & IndexAttributes.Union)!=0) {
          Provider[] sourceProviders = indexInfo.BaseIndexes.Select(index => Compile(new IndexProvider(index))).ToArray();
          result = new MergeInheritorsProvider(provider, sourceProviders);
        }
        else {
          var baseIndexes = new List<IndexInfo>(indexInfo.BaseIndexes);
          Provider rootProvider = Compile(new IndexProvider(indexInfo.BaseIndexes.First()));
          var inheritorsProviders = new Provider[baseIndexes.Count - 1];
          for (int i = 1; i < baseIndexes.Count; i++)
            inheritorsProviders[i - 1] = Compile(new IndexProvider(baseIndexes[i]));

          result = new JoinInheritorsProvider(provider, baseIndexes[0].IncludedColumns.Count, rootProvider, inheritorsProviders);
        }
      }
      return result;
    }
    

    // Constructors

    public IndexProviderCompiler(Compiler provider)
      : base(provider)
    {
      handlerAccessor = ((CompilerResolver)provider).HandlerAccessor;
    }
  }
}