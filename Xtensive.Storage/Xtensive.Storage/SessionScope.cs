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


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session to activate.</param>
    /// <param name="toDispose"><see cref="IDisposable"/> that should be disposed when this scope is disposing.</param>
    public SessionScope(Session session, IDisposable toDispose)
      : base(session)
    {
      this.toDispose = toDispose;
    }

    // Desctructor

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