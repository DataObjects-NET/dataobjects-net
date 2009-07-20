// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage
{
  /// <summary>
  /// Consumption scope for <see cref="Storage.Session"/>.
  /// </summary>
  public class SessionConsumptionScope: ResourceConsumptionScope<Session, SessionConfiguration>
  {
    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public new static SessionConsumptionScope Current {
      [DebuggerStepThrough]
      get { return ResourceConsumptionScope<Session, SessionConfiguration>.Current as SessionConsumptionScope; }
    }

    /// <summary>
    /// Gets the current session.
    /// </summary>
    public static Session CurrentSession {
      [DebuggerStepThrough]
      get {
        var consumptionScope = Current;
        return consumptionScope==null ? null : consumptionScope.Session;
      }
    }

    /// <summary>
    /// Gets the session for this instance.
    /// </summary>
    public Session Session {
      [DebuggerStepThrough]
      get { return Resource; }
    }

    /// <summary>
    /// Gets the session scope that is controlled by this instance.
    /// </summary>
    private SessionScope SessionScope { get; set; }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="activate">Determines whether session should be activated or not.</param>
    internal SessionConsumptionScope(Session session, bool activate)
      : base(session)
    {
      if (activate)
        SessionScope = session.Activate();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        SessionScope.DisposeSafely();
        SessionScope = null;
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}