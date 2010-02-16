// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using IObjectMappingOperationSet=Xtensive.Core.ObjectMapping.IOperationSet;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Built-in implementation of <see cref="IOperationSet"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationSet : IOperationSet, 
    IObjectMappingOperationSet
  {
    private readonly List<IOperation> log = new List<IOperation>();

    /// <inheritdoc/>
    public long Count {
      get { return log.Count; }
    }

    /// <inheritdoc/>
    public bool IsEmpty {
      get { return log.Count==0; }
    }

    /// <inheritdoc/>
    public void Append(IOperation operation)
    {
      log.Add(operation);
    }

    /// <inheritdoc/>
    public void Append(IOperationSet source)
    {
      log.AddRange(source);
    }

    /// <inheritdoc/>
    void IObjectMappingOperationSet.Apply()
    {
      Apply();
    }

    /// <inheritdoc/>
    public KeyMapping Apply()
    {
      return Apply(Session.Demand());
    }

    /// <inheritdoc/>
    public KeyMapping Apply(Session session)
    {
      var operationContext = new OperationExecutionContext(session);

      using (session.Activate())
      using (var ts = Transaction.Open(TransactionOpenMode.New)) { 
        foreach (var operation in log)
          operation.Prepare(operationContext);

        operationContext.KeysToPrefetch
          .Prefetch<Entity,Key>(key => key)
          .Execute();

        foreach (var operation in log)
          operation.Execute(operationContext);

        ts.Complete();
      }

      return new KeyMapping(operationContext.KeyMapping);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      log.Clear();
    }

    #region IEnumerable<...> implementation

    /// <inheritdoc/>
    public IEnumerator<IOperation> GetEnumerator()
    {
      return log.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}