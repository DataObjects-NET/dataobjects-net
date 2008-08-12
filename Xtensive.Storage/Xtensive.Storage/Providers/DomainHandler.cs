// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections.Generic;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers
{
  public abstract class DomainHandler : HandlerBase
  {
    private CompilationContext compiler;
    private Dictionary<IndexInfo, IndexProvider> providerCache = new Dictionary<IndexInfo, IndexProvider>();

    public Domain Domain { get; internal set; }

    public abstract void Build();

    internal IndexProvider GetIndexProvider(IndexInfo index)
    {
      IndexProvider result;
      if (!providerCache.TryGetValue(index, out result)) lock (providerCache) if (!providerCache.TryGetValue(index, out result)) {
        result = new IndexProvider(index);
        providerCache.Add(index, result);
      }
      return result;
    }

    public CompilationContext Compiler
    {
      get
      {
        if (compiler == null) lock(this) if (compiler == null)
          compiler = GetCompilationContext();
        return compiler;
      }
    }

    protected abstract CompilationContext GetCompilationContext();

  }
}