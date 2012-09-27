// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;

using Xtensive.Orm.Operations;


namespace Xtensive.Orm
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
    public OperationLogType LogType { get; private set; }

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
    [Obsolete("Use Replay(Session) instead.")]
    public KeyMapping Replay()
    {
      return Replay(Session.Demand());
    }

    /// <inheritdoc/>
    public KeyMapping Replay(Session session)
    {
      if (session.Operations.IsRegisteringOperation)
        throw new InvalidOperationException(Strings.ExRunningOperationRegistrationMustBeFinished);
      
      var executionContext = new OperationExecutionContext(session);
      bool isSystemOperationLog = LogType==OperationLogType.SystemOperationLog;
      KeyMapping keyMapping = null;
      Transaction transaction = null;

      using (session.Activate()) {
        using (isSystemOperationLog ? session.OpenSystemLogicOnlyRegion() : null) 
        using (var tx = session.OpenTransaction(TransactionOpenMode.New)) {

          foreach (var operation in operations)
            operation.Prepare(executionContext);

          session.Query.Many<Entity>(executionContext.KeysToPrefetch).Run();

          foreach (var operation in operations) {
            var identifierToKey = new Dictionary<string, Key>();
            var handler = new EventHandler<OperationCompletedEventArgs>((sender, e) => {
              foreach (var pair in e.Operation.IdentifiedEntities)
                identifierToKey.Add(pair.Key, pair.Value);
            });

            session.Operations.OutermostOperationCompleted += handler;
            try {
              operation.Execute(executionContext);
            }
            finally {
              session.Operations.OutermostOperationCompleted -= handler;
            }

            foreach (var pair in operation.IdentifiedEntities) {
              string identifier = pair.Key;
              var oldKey = pair.Value;
              var newKey = identifierToKey.GetValueOrDefault(identifier);
              if (newKey!=null)
                executionContext.AddKeyMapping(oldKey, newKey);
            }
          }

          keyMapping = new KeyMapping(executionContext.KeyMapping);

          tx.Complete();
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
      foreach (var o in operations)
        sb.AppendLine(o.ToString().Indent(2));
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


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="logType">Type of the log.</param>
    public OperationLog(OperationLogType logType)
    {
      LogType = logType;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="logType">Type of the log.</param>
    /// <param name="operations">The operations to add (using <see cref="Log"/> method).</param>
    public OperationLog(OperationLogType logType, IEnumerable<IOperation> operations)
      : this(logType)
    {
      Log(operations);
    }
  }
}