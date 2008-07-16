// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Rse.Providers.InheritanceSupport;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public class IndexProviderCompiler : TypeCompiler<Xtensive.Storage.Rse.Providers.IndexProvider>
  {
    private HandlerAccessor handlerAccessor;

    protected override Provider Compile(Xtensive.Storage.Rse.Providers.IndexProvider provider)
    {
      Provider result;
      var indexInfo = provider.Index;
      var handler = (DomainHandler)handlerAccessor.DomainHandler;
      if (!indexInfo.IsVirtual)
        result = new IndexProvider(provider.Header, indexInfo, handler.GetRealIndex);
      else {
        if ((indexInfo.Attributes & IndexAttributes.Filtered) !=0 ) {
          var source = Compile(new Xtensive.Storage.Rse.Providers.IndexProvider(indexInfo.BaseIndexes.First()));
          int columnIndex;
          if (indexInfo.IsPrimary) {
            FieldInfo typeIDField = indexInfo.ReflectedType.Fields[handlerAccessor.NameProvider.TypeId];
            columnIndex = typeIDField.MappingInfo.Offset;
          }
          else
            columnIndex = indexInfo.Columns.Select((c, i) => c.Field.Name==handlerAccessor.NameProvider.TypeId ? i : 0).Sum();
          List<int> typeIDList = indexInfo.ReflectedType.GetDescendants(true).Select(info => info.TypeId).ToList();
          typeIDList.Add(indexInfo.ReflectedType.TypeId);
          result = new FilterInheritorsProvider(source, columnIndex, handlerAccessor.Model.Types.Count, typeIDList.ToArray());
        }
        else if ((indexInfo.Attributes & IndexAttributes.Union)!=0) {
          var sourceProviders = indexInfo.BaseIndexes.Select(index => Compile(new Xtensive.Storage.Rse.Providers.IndexProvider(index))).ToArray();
          var header = new RecordHeader(indexInfo);
          result = new MergeInheritorsProvider(header, sourceProviders);
        }
        else {
          var baseIndexes = new List<IndexInfo>(indexInfo.BaseIndexes);
          var rootProvider = Compile(new Xtensive.Storage.Rse.Providers.IndexProvider(indexInfo.BaseIndexes.First()));
          var header = new RecordHeader(indexInfo);
          var inheritorsProviders = new Provider[baseIndexes.Count - 1];
          for (int i = 1; i < baseIndexes.Count; i++)
            inheritorsProviders[i - 1] = Compile(new Xtensive.Storage.Rse.Providers.IndexProvider(baseIndexes[i]));

          result = new JoinInheritorsProvider(header, baseIndexes[0].IncludedColumns.Count, rootProvider, inheritorsProviders);
        }
      }
      return result;
    }


    // Constructors

    public IndexProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
      handlerAccessor = ((CompilerResolver)provider).HandlerAccessor;
    }
  }
}