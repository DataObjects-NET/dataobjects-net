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
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Providers.InheritanceSupport;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public class IndexProviderCompiler : TypeCompiler<IndexProvider>
  {
    private readonly HandlerAccessor handlerAccessor;

    protected override ExecutableProvider Compile(IndexProvider provider)
    {
      IndexInfo indexInfo = provider.Index.Resolve(handlerAccessor.Domain.Model);
      ExecutableProvider result = CompileInternal(provider, indexInfo);
      return result;
    }

    private ExecutableProvider CompileInternal(IndexProvider provider, IndexInfo indexInfo)
    {
      var handler = (DomainHandler)handlerAccessor.DomainHandler;
      ExecutableProvider result;
      if (!indexInfo.IsVirtual)
        result = new Rse.Providers.Executable.IndexProvider(provider, provider.Index, handler.GetRealIndex);
      else {
        var firstUnderlyingIndex = indexInfo.UnderlyingIndexes.First();
        if ((indexInfo.Attributes & IndexAttributes.Filtered)!=0) {
          ExecutableProvider source = CompileInternal(new IndexProvider(firstUnderlyingIndex), firstUnderlyingIndex);
          int columnIndex;
          if (indexInfo.IsPrimary) {
            FieldInfo typeIdField = indexInfo.ReflectedType.Fields[handlerAccessor.NameBuilder.TypeIdFieldName];
            columnIndex = typeIdField.MappingInfo.Offset;
          }
          else
            columnIndex = indexInfo.Columns.Select((c, i) => c.Field.Name==handlerAccessor.NameBuilder.TypeIdFieldName ? i : 0).Sum();
          List<int> typeIdList = indexInfo.ReflectedType.GetDescendants(true).Select(info => info.TypeId).ToList();
          typeIdList.Add(indexInfo.ReflectedType.TypeId);
          result = new FilterInheritorsProvider(provider, source, columnIndex, handlerAccessor.Domain.Model.Types.Count, typeIdList.ToArray());
        }
        else if ((indexInfo.Attributes & IndexAttributes.Union)!=0) {
          ExecutableProvider[] sourceProviders = indexInfo.UnderlyingIndexes.Select(index => CompileInternal(new IndexProvider(index), index)).ToArray();
          result = new MergeInheritorsProvider(provider, sourceProviders);
        }
        else {
          var baseIndexes = new List<IndexInfo>(indexInfo.UnderlyingIndexes);
          ExecutableProvider rootProvider = CompileInternal(new IndexProvider(firstUnderlyingIndex), firstUnderlyingIndex);
          var inheritorsProviders = new ExecutableProvider[baseIndexes.Count - 1];
          for (int i = 1; i < baseIndexes.Count; i++)
            inheritorsProviders[i - 1] = CompileInternal(new IndexProvider(baseIndexes[i]), baseIndexes[i]);

          result = new JoinInheritorsProvider(provider, baseIndexes[0].IncludedColumns.Count, rootProvider, inheritorsProviders);
        }
      }
      return result;
    }


    // Constructors

    public IndexProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
      handlerAccessor = ((Compiler)provider).HandlerAccessor;
    }
  }
}
