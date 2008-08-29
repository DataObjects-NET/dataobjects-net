// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System;
using Xtensive.Core;
using Xtensive.Core.Disposable;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="Session"/> activation scope. 
  /// </summary>
  public class SessionScope : Scope<Session>
  {
    private IDisposable toDispose;

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public static Session CurrentSession
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public Session Session
    {
      get { return Context; }
    }

    /// <inheritdoc/>
    public override void Activate(Session newContext)
    {
      base.Activate(newContext);
      toDispose = newContext.Domain.TemporaryData.Activate();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session to activate.</param>
    public SessionScope(Session session)
      : base(session)
    {
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        toDispose.DisposeSafely();
        toDispose = null;
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}