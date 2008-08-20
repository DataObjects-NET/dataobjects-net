// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Diagnostics;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// <see cref="Storage.Domain"/>-level handler.
  /// </summary>
  public abstract class DomainHandler : InitializableHandlerBase
  {
    /// <summary>
    /// Gets or sets the <see cref="Storage.Domain"/> this handler is bound to.
    /// </summary>
    public Domain Domain { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Rse.Compilation.CompilationContext"/>
    /// associated with the domain.
    /// </summary>
    public CompilationContext CompilationContext { get; private set; }

    /// <summary>
    /// Gets system session.
    /// </summary>
    protected Session SystemSession { get; private set; }

    /// <summary>
    /// Gets system session handler.
    /// </summary>
    protected SessionHandler SystemSessionHandler
    {
      get { return SystemSession.Handlers.SessionHandler; }
    }

    // Abstract methods

    /// <summary>
    /// Builds the <see cref="CompilationContext"/> value.
    /// Invoked from <see cref="Initialize"/>.
    /// </summary>
    protected abstract CompilationContext BuildCompilationContext();

    /// <summary>
    /// Builds the <see cref="Domain"/>.
    /// </summary>
    public abstract void Build();

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      CompilationContext = BuildCompilationContext();
      SystemSession = Domain.OpenSession(SessionType.DomainHandler).Session;
    }
  }
}