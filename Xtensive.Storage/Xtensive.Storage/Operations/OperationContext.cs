// Copyright (C) 2003-2010 Xtensive LLC.
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
  /// Operation context that manages 
  /// <see cref="IOperation"/> logging in <see cref="Session"/>.
  /// </summary>
  public sealed class OperationContext : IOperationContext
  {
    private readonly Session session;
    private readonly IOperationContext parentOperationContext;
    private List<IOperation> operations;
    private bool completed;

    /// <inheritdoc/>
    public bool IsLoggingEnabled { get { return true; } }

    /// <inheritdoc/>
    public bool IsIntermediate { get; private set; }

    /// <inheritdoc/>
    public void LogOperation(IOperation operation)
    {
      if (!IsLoggingEnabled && !(operation is IPrecondition))
        return;
      if (operations == null)
        operations = new List<IOperation>();
      operations.Add(operation);
    }

    /// <inheritdoc/>
    public void Complete()
    {
      completed = true;
    }

    #region IEnumerable<...> members

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

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session this context belongs to.</param>
    /// <param name="isIntermediate"><see cref="IsIntermediate"/> property value.</param>
    internal OperationContext(Session session, bool isIntermediate)
    {
      this.session = session;
      parentOperationContext = session.CurrentOperationContext;
      session.CurrentOperationContext = this;
      IsIntermediate = isIntermediate;
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