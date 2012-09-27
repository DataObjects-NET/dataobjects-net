// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Aspects;
using Xtensive.Core;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// A service listening to operation-related events in <see cref="Session"/>
  /// and writing their sequence to <see cref="Operations"/> instance 
  /// (<see cref="IOperationLogger"/>).
  /// </summary>
  [Infrastructure]
  public sealed class OperationCapturer : SessionBound, 
    IDisposable
  {
    /// <summary>
    /// Gets the operation set updated by this service.
    /// </summary>
    public IOperationLogger Operations { get; private set; }

    #region Session event handlers

    private void OperationStarting(object sender, OperationEventArgs e)
    {
      var operation = e.Operation;
      switch (Operations.LogType) {
      case OperationLogType.SystemOperationLog:
        if (operation.Type==OperationType.System)
          Operations.Log(operation.Clone(false));
        break;
      case OperationLogType.UndoOperationLog:
        Operations.Log(e.Operation.Clone(false));
        break;
      default:
        // Unused in this case
        break;
      }
    }

    private void OperationCompleted(object sender, OperationCompletedEventArgs e)
    {
      var operation = e.Operation;
      if (!e.IsCompleted)
        return;

      switch (Operations.LogType) {
      case OperationLogType.OutermostOperationLog:
        Operations.Log(operation.Clone(true));
        break;
      default:
        // Unused in this case
        break;
      }
    }

    #endregion

    #region Private methods

    private void AttachEventHandlers()
    {
      switch (Operations.LogType) {
      case OperationLogType.OutermostOperationLog:
        Session.Operations.OutermostOperationCompleted += OperationCompleted;
        break;
      case OperationLogType.SystemOperationLog:
        Session.Operations.OutermostOperationStarting += OperationStarting;
        Session.Operations.NestedOperationStarting += OperationStarting;
        break;
      case OperationLogType.UndoOperationLog:
        Session.Operations.UndoOperation += OperationStarting;
        break;
      default:
        break;
      }
    }

    private void DetachEventHandlers()
    {
      switch (Operations.LogType) {
      case OperationLogType.OutermostOperationLog:
        Session.Operations.OutermostOperationCompleted -= OperationCompleted;
        break;
      case OperationLogType.SystemOperationLog:
        Session.Operations.OutermostOperationStarting -= OperationStarting;
        Session.Operations.NestedOperationStarting -= OperationStarting;
        break;
      case OperationLogType.UndoOperationLog:
        Session.Operations.UndoOperation -= OperationStarting;
        break;
      default:
        break;
      }
    }

    #endregion

    // Factory methods

    /// <summary>
    /// Attaches the operation capturer to the current session.
    /// </summary>
    /// <param name="operations">The operation logger to append captured operations to.</param>
    /// <returns>
    /// A newly created <see cref="OperationCapturer"/> attached
    /// to the current session.
    /// </returns>
    [Obsolete("Use Attach(Session, IOperationLogger) instead")]
    public static OperationCapturer Attach(IOperationLogger operations)
    {
      return Attach(Session.Demand(), operations);
    }

    /// <summary>
    /// Attaches the operation capturer to the specified session.
    /// </summary>
    /// <param name="session">The session to attach the capturer to.</param>
    /// <param name="operations">The operation logger to append captured operations to.</param>
    /// <returns>
    /// A newly created <see cref="OperationCapturer"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static OperationCapturer Attach(Session session, IOperationLogger operations)
    {
      return new OperationCapturer(session, operations);
    }


    // Constructors

    private OperationCapturer(Session session, IOperationLogger operations)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(operations, "operations");
      Operations = operations;
      AttachEventHandlers();
    }

    // Dispose

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}