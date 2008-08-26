// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.20

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Atomicity;

namespace Xtensive.Storage
{
  /// <summary>
  /// The context describing current atomic operation.
  /// </summary>
  public class AtomicityContext : AtomicityContextBase
  {
    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    public AtomicityContext(Session session)
      : this(session, AtomicityContextOptions.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="operationLog">The operation log.</param>
    public AtomicityContext(Session session, IOperationLog operationLog)
      : this(session, AtomicityContextOptions.Default, operationLog)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="options">The atomicity context options.</param>
    public AtomicityContext(Session session, AtomicityContextOptions options)
      : this(session, options, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="options">The atomicity context options.</param>
    /// <param name="operationLog">The operation log.</param>
    public AtomicityContext(Session session, AtomicityContextOptions options, IOperationLog operationLog)
      : base(options, operationLog)
    {
      Session = session;
    }
  }
}