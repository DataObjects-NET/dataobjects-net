// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.InheritanceSupport;

namespace Xtensive.Storage.Providers.Index
{
  /// <inheritdoc/>
  [Serializable]
  public class IndexCompiler : RseCompiler
  {
    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> object providing access to available storage handlers.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return provider;
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(Rse.Providers.Compilable.IndexProvider provider, ExecutableProvider[] sources)
    {
      IndexInfo indexInfo = provider.Index.Resolve(Handlers.Domain.Model);
      ExecutableProvider result = CompileInternal(provider, indexInfo);
      return result;
    }

    private ExecutableProvider CompileInternal(Rse.Providers.Compilable.IndexProvider provider, IndexInfo indexInfo)
    {
      var handler = (DomainHandler)Handlers.DomainHandler;
      ExecutableProvider result;
      if (!indexInfo.IsVirtual)
        result = new IndexProvider(provider, provider.Index, handler.GetRealIndex);
      else
      {
        var firstUnderlyingIndex = indexInfo.UnderlyingIndexes.First();
        if ((indexInfo.Attributes & IndexAttributes.Filtered) != 0)
        {
          ExecutableProvider source = CompileInternal(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
          int columnIndex;
          if (indexInfo.IsPrimary)
          {
            FieldInfo typeIdField = indexInfo.ReflectedType.Fields[Handlers.Domain.NameBuilder.TypeIdFieldName];
            columnIndex = typeIdField.MappingInfo.Offset;
          }
          else
            columnIndex = indexInfo.Columns.Select((c, i) => c.Field.Name == Handlers.Domain.NameBuilder.TypeIdFieldName ? i : 0).Sum();
          List<int> typeIdList = indexInfo.ReflectedType.GetDescendants(true).Select(info => info.TypeId).ToList();
          typeIdList.Add(indexInfo.ReflectedType.TypeId);
          result = new FilterInheritorsProvider(provider, source, columnIndex, typeIdList.ToArray());
        }
        else if ((indexInfo.Attributes & IndexAttributes.Union) != 0)
        {
          ExecutableProvider[] sourceProviders = indexInfo.UnderlyingIndexes.Select(index => CompileInternal(Rse.Providers.Compilable.IndexProvider.Get(index), index)).ToArray();
          if (sourceProviders.Length == 1)
            result = sourceProviders[0];
          else
            result = new MergeInheritorsProvider(provider, sourceProviders);
        }
        else
        {
          var baseIndexes = new List<IndexInfo>(indexInfo.UnderlyingIndexes);
          ExecutableProvider rootProvider = CompileInternal(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
          var inheritorsProviders = new ExecutableProvider[baseIndexes.Count - 1];
          for (int i = 1; i < baseIndexes.Count; i++)
            inheritorsProviders[i - 1] = CompileInternal(Rse.Providers.Compilable.IndexProvider.Get(baseIndexes[i]), baseIndexes[i]);

          result = new JoinInheritorsProvider(provider, baseIndexes[0].IncludedColumns.Count, rootProvider, inheritorsProviders);
        }
      }
      return result;
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexCompiler(HandlerAccessor handlers)
      : base(handlers.Domain.Configuration.ConnectionInfo)
    {
      Handlers = handlers;
    }
  }
}