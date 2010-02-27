// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Operations.Internals;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// A service listening to operation-related events in <see cref="Session"/>
  /// and writing their sequence to <see cref="Operations"/> instance 
  /// (<see cref="IOperationLog"/>).
  /// </summary>
  [Infrastructure]
  public sealed class OperationLogger : SessionBound, 
    IDisposable
  {
    /// <summary>
    /// Gets the operation set updated by this service.
    /// </summary>
    public IOperationLog Operations { get; private set; }

    #region Session event handlers

    private void KeyGenerated(object sender, KeyEventArgs e)
    {
      var keyGenerator = Session.Domain.KeyGenerators[e.Key.TypeRef.Type.Hierarchy.Key];
      Operations.Append(
        new KeyGenerateOperation(e.Key));
    }

    private void OperationCompleted(object sender, OperationEventArgs e)
    {
      Operations.Append(e.Operation);
    }

    #endregion

    #region Private methods

    private void AttachEventHandlers()
    {
      Session.KeyGenerated += KeyGenerated;
      Session.OperationCompleted += OperationCompleted;
    }

    private void DetachEventHandlers()
    {
      Session.KeyGenerated -= KeyGenerated;
      Session.OperationCompleted -= OperationCompleted;
    }

    #endregion

    // Factory method

    /// <summary>
    /// Attaches the operation logger to the specified session.
    /// </summary>
    /// <param name="session">The session to attach validator to.</param>
    /// <param name="operations">The operation set to write the operation sequence to.</param>
    /// <returns>
    /// A newly created <see cref="OperationLogger"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static OperationLogger Attach(Session session, IOperationLog operations)
    {
      return new OperationLogger(session, operations);
    }


    // Constructors

    private OperationLogger(Session session, IOperationLog operations)
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