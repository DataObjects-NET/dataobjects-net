// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Storage.Resources;
using IObjectMappingOperationSet=Xtensive.Core.ObjectMapping.IOperationSet;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Built-in implementation of <see cref="IOperationLog"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationLog : IOperationLog, 
    IObjectMappingOperationSet
  {
    private readonly List<IOperation> operations = new List<IOperation>();
    private HashSet<IUniqueOperation> uniqueOperations;

    /// <inheritdoc/>
    public long Count {
      get { return operations.Count; }
    }

    /// <inheritdoc/>
    public bool IsEmpty {
      get { return operations.Count==0; }
    }

    /// <inheritdoc/>
    public void Append(IOperation operation)
    {
      operations.Add(operation);
      TryAppendUniqueOperation(operation);
    }

    /// <inheritdoc/>
    public void Append(IOperationLog source)
    {
      foreach (var operation in source) {
        operations.Add(operation);
        TryAppendUniqueOperation(operation);
      }
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
        foreach (var operation in operations)
          operation.Prepare(operationContext);

        operationContext.KeysToPrefetch
          .Prefetch<Entity,Key>(key => key)
          .Execute();

        foreach (var operation in operations)
          operation.Execute(operationContext);

        ts.Complete();
      }

      return new KeyMapping(operationContext.KeyMapping);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      operations.Clear();
      if (uniqueOperations!=null)
        uniqueOperations.Clear();
    }

    #region IEnumerable<...> implementation

    /// <inheritdoc/>
    public IEnumerator<IOperation> GetEnumerator()
    {
      return operations.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    private void TryAppendUniqueOperation(IOperation operation)
    {
      var uniqueOperation = operation as IUniqueOperation;
      if (uniqueOperation!=null) {
        if (uniqueOperations==null)
          uniqueOperations = new HashSet<IUniqueOperation>();
        if (!uniqueOperations.Add(uniqueOperation) && !uniqueOperation.IgnoreIfDuplicate)
          throw new InvalidOperationException(
            Strings.ExDuplicateForOperationXIsFound.FormatWith(uniqueOperation));
      }
    }
  }
}