// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.03

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// An <see cref="IIndexResolver"/> implementation that forwards all requests
  /// to current <see cref="SessionHandler"/>.
  /// </summary>
  public class IndexResolver : IIndexResolver
  {
    private readonly HandlerAccessor handlers;

    /// <inheritdoc/>
    public IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      return handlers.SessionHandler.GetService<IIndexResolver>(true).GetIndex(indexInfo);
    }

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlers">The handlers.</param>
    public IndexResolver(HandlerAccessor handlers)
    {
      this.handlers = handlers;
    }
  }
}