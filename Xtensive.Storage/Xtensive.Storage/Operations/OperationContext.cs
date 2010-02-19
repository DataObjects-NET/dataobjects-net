// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation context that manages <see cref="IOperation"/> registration.
  /// </summary>
  public sealed class OperationContext : IOperationContext
  {
    private readonly Session session;
    private readonly IOperationContext parentOperationContext;
    private List<IOperation> operations;
    internal bool completed;

    /// <inheritdoc/>
    public bool AreNormalOperationAccepted {get { return true; }}

    /// <inheritdoc/>
    public bool DisableNested { get; private set; }

    /// <inheritdoc/>
    public void Add(IOperation operation)
    {
      if (operations == null)
          operations = new List<IOperation>();
      operations.Add(operation);
    }

    /// <inheritdoc/>
    public void Add(IOperation operation, bool highPriority)
    {
      Add(operation);
    }

    /// <inheritdoc/>
    public void Complete()
    {
      completed = true;
    }
    
    /// <inheritdoc/>
    public IEnumerator<IOperation> GetEnumerator()
    {
      if (operations==null)
        yield break;
      foreach (var operation in operations)
        yield return operation;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="disableNested">Disable nested operation contexts.</param>
    internal OperationContext(Session session, bool disableNested)
    {
      this.session = session;
      parentOperationContext = session.CurrentOperationContext;
      session.CurrentOperationContext = this;
      DisableNested = disableNested;
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
      session.CurrentOperationContext = parentOperationContext;
      if (completed && operations != null)
        foreach (var operation in operations)
          session.NotifyOperationCompleted(operation);
    }
  }
}