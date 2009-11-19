// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.17

using System;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected session.
  /// </summary>
  public sealed class DisconnectedSession : IDisposable
  {
    /// <summary>
    /// Opens the session in disconnected mode.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>Opened session.</returns>
    public static DisconnectedSession Open(Domain domain)
    {
      var session = Session.Open(domain);
      var state = new DisconnectedState();
      state.Attach(session);
      return new DisconnectedSession(state);
    }
    
    /// <summary>
    /// Opens the session in disconnected mode.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="state">Stored disconnected state.</param>
    /// <returns>Opened session.</returns>
    public static DisconnectedSession Open(Domain domain, DisconnectedState state)
    {
      if (state.IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsAlreadyAttachedToSession);
      var session = Session.Open(domain);
      state.Attach(session);
      return new DisconnectedSession(state);
    }

    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get { return State.Session; } }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public DisconnectedState State { get; private set; }

    /// <summary>
    /// Opens underlying connection.
    /// </summary>
    /// <returns>The scope.</returns>
    /// <exception cref="InvalidOperationException">Transaction is required.</exception>
    public IDisposable Connect()
    {
      State.BeginUnderlyingTransaction();
      return new Disposable<DisconnectedState>(State, (b, state) => state.CloseUnderlyingTransaction());
    }

    /// <summary>
    /// Saves the changes to underlying storage.
    /// </summary>
    public void SaveChanges()
    {
      if (Transaction.Current!=null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsPresent);
      var session = State.Session;
      try {
        State.Detach();
        State.SaveChanges(session);
      }
      finally {
        if (!State.IsAttached)
          State.Attach(session);
      }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (State!=null && State.IsAttached) {
        var session = Session;
        State.Detach();
        session.DisposeSafely();
      }
    }


    // Contructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="state">The state.</param>
    private DisconnectedSession(DisconnectedState state)
    {
      if (!state.IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsAlreadyAttachedToSession);
      State = state;
    }
  }
}