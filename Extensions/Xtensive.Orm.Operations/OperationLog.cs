// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Operations.Interfaces;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Built-in implementation of both <see cref="IOperationLogger"/>
  /// and <see cref="IOperationSequence"/>.
  /// </summary>
  [Serializable]
  public sealed class OperationLog : IOperationLogger, 
    Interfaces.IOperationSequence
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
    public KeyMapping Replay(Session session)
    {
      if (session.Operations.IsRegisteringOperation)
        throw new InvalidOperationException("Running operation registration must be finished.");
      
      var executionContext = new OperationExecutionContext(session);
      bool isSystemOperationLog = LogType==OperationLogType.SystemOperationLog;
      KeyMapping keyMapping;

      using (session.Activate()) {
        using (isSystemOperationLog ? (IDisposable) AccessOpenSystemLogicOnlyRegionMethod(session) : null)
        using (var tx = session.OpenTransaction(TransactionOpenMode.New)) {

          var executableOperations = operations.OfType<IExecutableOperation>().ToArray();
          foreach (var operation in executableOperations)
            operation.Prepare(executionContext);

          session.Query.Many<Entity>(executionContext.KeysToPrefetch).Run();

          foreach (var operation in executableOperations) {
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
      var sb = new StringBuilder("Operations:");
      _ = sb.AppendLine();
      foreach (var o in operations) {
        _ = sb.AppendLine(o.ToString().Indent(2));
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
          throw new InvalidOperationException(string.Format("Duplicate for Operation {0} is found", uniqueOperation));
      }
    }

    private static IDisposable AccessOpenSystemLogicOnlyRegionMethod(Session session)
    {
      var OpenSystemLogicOnlyRegion = typeof(Session).GetMethod("OpenSystemLogicOnlyRegion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

      return (IDisposable) OpenSystemLogicOnlyRegion.Invoke(session, null);
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
    /// <param name="operations">The operations to add (using <see cref="Log(Xtensive.Orm.IOperation)"/> method).</param>
    public OperationLog(OperationLogType logType, IEnumerable<IOperation> operations)
      : this(logType)
    {
      Log(operations);
    }
  }
}
