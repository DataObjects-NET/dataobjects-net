// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.23

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation context that manages 
  /// <see cref="IOperation"/> logging in <see cref="Session"/>.
  /// </summary>
  public sealed class OperationContext : IOperationContext
  {
    private readonly Session session;
    private readonly OperationContext parentOperationContext;
    private IOperation firstOperation;
    private List<IOperation> operations;
    private Dictionary<string, Key> keyByIdentifier;
    private Dictionary<Key, string> identifierByKey;
    private bool completed;

    internal long CurrentIdentifier;

    /// <inheritdoc/>
    public bool IsLoggingEnabled { get { return true; } }

    /// <inheritdoc/>
    public bool IsIntermediate { get; private set; }

    /// <inheritdoc/>
    public bool IsBlocking { get { return false; } }

    /// <inheritdoc/>
    public void LogOperation(IOperation operation)
    {
      var isPrecondition = operation is IPrecondition;
      if (!IsLoggingEnabled && !isPrecondition)
        return;
      if (!isPrecondition && firstOperation==null)
        firstOperation = operation;
      if (operations == null)
        operations = new List<IOperation>();
      operations.Add(operation);
    }

    /// <inheritdoc/>
    public void LogEntityIdentifier(Key key, string identifier)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      // Initializing dictionaries, if necessary
      if (identifierByKey == null) {
        identifierByKey = new Dictionary<Key, string>();
        keyByIdentifier = new Dictionary<string, Key>();
      }

      // Removing existing records about the same entity or identifier
      string existingIdentifier = identifierByKey.GetValueOrDefault(key);
      if (existingIdentifier != null) {
        keyByIdentifier.Remove(existingIdentifier);
        identifierByKey.Remove(key);
      }
      if (identifier==null)
        return;

      var existingKey = keyByIdentifier.GetValueOrDefault(identifier);
      if (existingKey != null) {
        keyByIdentifier.Remove(identifier);
        identifierByKey.Remove(existingKey);
      }

      keyByIdentifier.Add(identifier, key);
      identifierByKey.Add(key, identifier);

      if (session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXEntityWithKeyYIdentifiedAsZ,
          session, key, identifier);
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

    #region Private \ internal methods

    internal OperationContext GetTopmostOperationContext()
    {
      var currentContext = this;
      while (true) {
        if (currentContext.parentOperationContext==null)
          return currentContext;
        currentContext = currentContext.parentOperationContext;
      }
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
      // Passing entity identifiers to parent context or assigning them to the current operation
      if (parentOperationContext != null) {
        foreach (var pair in identifierByKey)
          parentOperationContext.LogEntityIdentifier(pair.Key, pair.Value);
      }
      else if (firstOperation != null) {
        firstOperation.IdentifiedEntities = new ReadOnlyDictionary<string, Key>(
          keyByIdentifier ?? new Dictionary<string, Key>());
      }

      session.CurrentOperationContext = parentOperationContext;
      if (session.IsOperationLoggingEnabled) {
        if (completed && operations!=null)
          foreach (var operation in operations)
            session.NotifyOutermostOperationCompleted(operation);
      }
    }
  }
}