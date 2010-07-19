// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Built-in implementation of both <see cref="IOperationLogger"/>
  /// and <see cref="IOperationSequence"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationLog : IOperationLogger, 
    IOperationSequence
  {
    private readonly List<IOperation> operations = new List<IOperation>();
    private HashSet<IUniqueOperation> uniqueOperations;

    /// <inheritdoc/>
    public long Count {
      get { return operations.Count; }
    }

    /// <inheritdoc/>
    public void Log(IOperation operation)
    {
      operations.Add(operation);
      TryAppendUniqueOperation(operation);
    }

    /// <inheritdoc/>
    public void Log(IEnumerable<IOperation> source)
    {
      foreach (var operation in source) {
        operations.Add(operation);
        TryAppendUniqueOperation(operation);
      }
    }

    /// <inheritdoc/>
    public KeyMapping Replay()
    {
      return Replay(Session.Demand());
    }

    /// <inheritdoc/>
    public KeyMapping Replay(Session session)
    {
      var operationContext = new OperationExecutionContext(session);
      KeyMapping keyMapping = null;
      Transaction transaction = null;
      using (session.Activate()) {
        try {
          using (var tx = Transaction.Open(TransactionOpenMode.New)) {
            transaction = tx.Transaction;

            foreach (var operation in operations)
              operation.Prepare(operationContext);

            operationContext.KeysToPrefetch
              .Prefetch<Entity, Key>(key => key)
              .Execute();

            foreach (var operation in operations)
              operation.Execute(operationContext);

            keyMapping = new KeyMapping(operationContext.KeyMapping);

            tx.Complete();
          }
        }
        finally {
          bool requiresRemapping = keyMapping!=null && keyMapping.Map.Count!=0;
          if (requiresRemapping && transaction!=null && transaction.State==TransactionState.Committed && transaction.IsNested) {
            session.RemapEntityKeys(keyMapping);
            session.NotifyChanged();
          }
        }
        return keyMapping;
      }
    }

    /// <inheritdoc/>
    public object Replay(object target)
    {
      return Replay((Session) target);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder("{0}:\r\n".FormatWith(Strings.Operations));
      foreach (var o in operations) {
        sb.Append("  ");
        sb.AppendLine(o.ToString());
      }
      return sb.ToString().Trim();
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

    #region Private \ internal methods

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

    #endregion
  }
}