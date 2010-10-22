// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Vakhtina Elena
// Created:    2009.02.13

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Indexing.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Executable.VirtualIndex;

namespace Xtensive.Storage.Providers.Indexing
{
  /// <inheritdoc/>
  [Serializable]
  public class IndexCompiler : RseCompiler
  {
    private readonly IIndexResolver indexResolver;

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
    protected override ExecutableProvider VisitIndex(Rse.Providers.Compilable.IndexProvider provider)
    {
      var indexInfo = provider.Index.Resolve(Handlers.Domain.Model);
      var result = BuildIndexProvider(provider, indexInfo);
      return result;
    }

    private ExecutableProvider BuildIndexProvider(Rse.Providers.Compilable.IndexProvider provider, IndexInfo index)
    {
      if (index.IsVirtual)
      {
        if ((index.Attributes & IndexAttributes.Union) > 0)
          return BuildUnionProvider(provider, index);
        if ((index.Attributes & IndexAttributes.Join) > 0)
          return BuildJoinProvider(provider, index);
        if ((index.Attributes & IndexAttributes.Filtered) > 0)
          return BuildFilterProvider(provider, index);
        if ((index.Attributes & IndexAttributes.View) > 0)
          return BuildViewProvider(provider, index);
        if ((index.Attributes & IndexAttributes.Typed) > 0)
          return BuildTypedProvider(provider, index);
        throw new NotSupportedException(String.Format(Strings.ExUnsupportedIndex, index.Name, index.Attributes));
      }
      return BuildIndexProviderInternal(provider);
    }

    private ExecutableProvider BuildUnionProvider(CompilableProvider provider, IndexInfo index)
    {
      var sourceProviders = index.UnderlyingIndexes.Select(i => BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(i), i)).ToArray();
      return sourceProviders.Length == 1 
        ? sourceProviders[0] 
        : new UnionIndexProvider(provider, sourceProviders);
    }

    private ExecutableProvider BuildJoinProvider(Rse.Providers.Compilable.IndexProvider provider, IndexInfo index)
    {
      var firstUnderlyingIndex = index.UnderlyingIndexes.First();
      var baseIndexes = new List<IndexInfo>(index.UnderlyingIndexes);
      var rootProvider = BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
      var inheritorsProviders = new ExecutableProvider[baseIndexes.Count - 1];
      for (int i = 1; i < baseIndexes.Count; i++)
        inheritorsProviders[i - 1] = BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(baseIndexes[i]), baseIndexes[i]);
      return new JoinIndexProvider(provider, baseIndexes[0].KeyColumns.Count, index.ValueColumnsMap, rootProvider, inheritorsProviders);
    }

    private ExecutableProvider BuildFilterProvider(Rse.Providers.Compilable.IndexProvider provider, IndexInfo index)
    {
      var firstUnderlyingIndex = index.UnderlyingIndexes.First();
      var source = BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
      int columnIndex;
      if (index.IsPrimary) {
        FieldInfo typeIdField = index.ReflectedType.Fields[WellKnown.TypeIdFieldName];
        columnIndex = typeIdField.MappingInfo.Offset;
      }
      else
        columnIndex = index.Columns.Select((c, i) => c.Field.Name == WellKnown.TypeIdFieldName ? i : 0).Sum();
      return new FilterIndexProvider(provider, source, columnIndex, index.FilterByTypes.Select(t => t.TypeId).ToList());
    }

    private ExecutableProvider BuildViewProvider(Rse.Providers.Compilable.IndexProvider provider, IndexInfo index)
    {
      var firstUnderlyingIndex = index.UnderlyingIndexes.First();
      var source = BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
      var columnMap = index.SelectColumns.ToArray();
      return new ViewIndexProvider(provider, source, columnMap);
    }

    private ExecutableProvider BuildTypedProvider(Rse.Providers.Compilable.IndexProvider provider, IndexInfo index)
    {
      var firstUnderlyingIndex = index.UnderlyingIndexes.First();
      var source = BuildIndexProvider(Rse.Providers.Compilable.IndexProvider.Get(firstUnderlyingIndex), firstUnderlyingIndex);
      var typeIdColumnIndex = index.Columns
        .Select((c, i) => new {c, i})
        .Single(p => p.c.Field.IsTypeId).i;
      return new TypedIndexProvider(provider, source, typeIdColumnIndex, index.ReflectedType.TypeId);
    }

    private ExecutableProvider BuildIndexProviderInternal(Rse.Providers.Compilable.IndexProvider provider)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      return new IndexProvider(provider, domainHandler.GetStorageIndexInfo(provider.Index), indexResolver);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexCompiler(HandlerAccessor handlers, IIndexResolver indexResolver)
      : base(handlers.DomainHandler.StorageLocation)
    {
      Handlers = handlers;
      this.indexResolver = indexResolver;
    }
  }
}