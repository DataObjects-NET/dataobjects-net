// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation context that manages <see cref="IOperation"/> registration.
  /// </summary>
  public sealed class OperationContext : IDisposable
  {
    private static readonly OperationContext defaultContext = new OperationContext();
    private readonly Session session;
    private readonly OperationContext parentOperationContext;
    private List<IOperation> operations;
    internal bool completed;

    internal List<IOperation> Operations
    {
      get
      {
        if (operations == null)
          operations = new List<IOperation>();
        return operations;
      }
    }

    /// <summary>
    /// Gets the default operation context.
    /// </summary>
    public static OperationContext Default
    {
      get { return defaultContext; }
    }

    /// <summary>
    /// Gets a flag indicating disabling of nested <see cref="OperationContext"/>.
    /// </summary>
    internal bool DisableNested { get; private set; }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
      session.CurrentOperationContext = parentOperationContext;
      if (operations != null)
        foreach (var operation in operations)
          session.NotifyOperationRegister(operation);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="disableNested">Disable nested operation contexts.</param>
    public OperationContext(Session session, bool disableNested)
    {
      this.session = session;
      parentOperationContext = session.CurrentOperationContext;
      session.CurrentOperationContext = this;
      DisableNested = disableNested;
    }

    private OperationContext()
    {}
  }
}