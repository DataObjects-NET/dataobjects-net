// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.03

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Commands;
using Xtensive.Storage.Model;

namespace Xtensive.Orm.Providers.Indexing
{
  /// <summary>
  /// An <see cref="IIndexResolver"/> implementation that forwards all requests
  /// to current <see cref="SessionHandler"/>.
  /// </summary>
  public class IndexResolver : IIndexResolver
  {
    private readonly SessionHandler sessionHandler;

    /// <inheritdoc/>
    public IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      return sessionHandler.GetService<IIndexResolver>(true).GetIndex(indexInfo);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sessionHandler">The session handler.</param>
    public IndexResolver(SessionHandler sessionHandler)
    {
      this.sessionHandler = sessionHandler;
    }
  }
}