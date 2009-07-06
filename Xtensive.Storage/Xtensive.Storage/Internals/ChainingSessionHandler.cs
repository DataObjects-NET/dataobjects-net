// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.06

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// The base class for <see cref="SessionHandler"/>s which support the chaining 
  /// with another handler.
  /// </summary>
  public abstract class ChainingSessionHandler : SessionHandler
  {
    /// <summary>
    /// The chained handler.
    /// </summary>
    public readonly SessionHandler ChainedHandler;

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      ChainedHandler.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      ChainedHandler.CommitTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      ChainedHandler.RollbackTransaction();
    }
    
    /// <inheritdoc/>
    public override void Dispose()
    {
      ChainedHandler.Dispose();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="chainedHandler">The handler to be chained.</param>
    public ChainingSessionHandler(SessionHandler chainedHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedHandler, "chainedHandler");
      ChainedHandler = chainedHandler;
    }
  }
}