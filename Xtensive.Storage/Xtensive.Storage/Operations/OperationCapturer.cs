// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
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

    private void OutermostOperationCompleted(object sender, OperationEventArgs e)
    {
      Operations.Log(e.Operation);
    }

    #endregion

    #region Private methods

    private void AttachEventHandlers()
    {
      Session.OutermostOperationCompleted += OutermostOperationCompleted;
    }

    private void DetachEventHandlers()
    {
      Session.OutermostOperationCompleted -= OutermostOperationCompleted;
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
    
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}