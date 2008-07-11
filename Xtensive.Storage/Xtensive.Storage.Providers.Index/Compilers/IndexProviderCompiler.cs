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
using Xtensive.Storage.Rse.Providers.Implementation;
using Xtensive.Storage.Rse.Providers.InheritanceSupport;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public class IndexProviderCompiler : ProviderCompiler<Rse.Providers.Declaration.IndexProvider>
  {
    protected override Provider Compile(Rse.Providers.Declaration.IndexProvider provider)
    {
      Provider result;
      var indexInfo = provider.Index;
      var handler = (DomainHandler)SessionScope.Current.Session.Domain.Handler;
      if (!indexInfo.IsVirtual)
        result = new IndexProvider(provider.Header, indexInfo, handler.GetRealIndex);
      else {
        if ((indexInfo.Attributes & IndexAttributes.Filtered) !=0 ) {
          var source = Compile(new Rse.Providers.Declaration.IndexProvider(indexInfo.BaseIndexes.First()));
          int columnIndex;
          if (indexInfo.IsPrimary) {
            FieldInfo typeIDField = indexInfo.ReflectedType.Fields[handler.Domain.NameProvider.TypeId];
            columnIndex = typeIDField.MappingInfo.Offset;
          }
          else
            columnIndex = indexInfo.Columns.Select((c, i) => c.Field.Name==handler.Domain.NameProvider.TypeId ? i : 0).Sum();
          List<int> typeIDList = indexInfo.ReflectedType.GetDescendants(true).Select(info => info.TypeId).ToList();
          typeIDList.Add(indexInfo.ReflectedType.TypeId);
          result = new FilterInheritorsProvider(source, columnIndex, handler.Domain.Model.Types.Count, typeIDList.ToArray());
        }
        else if ((indexInfo.Attributes & IndexAttributes.Union)!=0) {
          var sourceProviders = indexInfo.BaseIndexes.Select(index => Compile(new Rse.Providers.Declaration.IndexProvider(index))).ToArray();
          var header = new RecordHeader(indexInfo);
          result = new MergeInheritorsProvider(header, sourceProviders);
        }
        else {
          var baseIndexes = new List<IndexInfo>(indexInfo.BaseIndexes);
          var rootProvider = Compile(new Rse.Providers.Declaration.IndexProvider(indexInfo.BaseIndexes.First()));
          var header = new RecordHeader(indexInfo);
          var inheritorsProviders = new Provider[baseIndexes.Count - 1];
          for (int i = 1; i < baseIndexes.Count; i++)
            inheritorsProviders[i - 1] = Compile(new Rse.Providers.Declaration.IndexProvider(baseIndexes[i]));

          result = new JoinInheritorsProvider(header, baseIndexes[0].IncludedColumns.Count, rootProvider, inheritorsProviders);
        }
      }
      return result;
    }
  }
}