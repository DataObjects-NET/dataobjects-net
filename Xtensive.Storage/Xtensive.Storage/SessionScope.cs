// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.29

using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage
{
  /// <summary>
  /// Represents the implementation of consumable scope pattern for <see cref="Xtensive.Storage.Session"/>.
  /// </summary>
  public class SessionScope: ResourceConsumptionScope<Session, SessionConfiguration>
  {
    private readonly CompilationScope compilationScope;

    /// <summary>
    /// Gets the current scope.
    /// </summary>
    public new static SessionScope Current
    {
      get { return ResourceConsumptionScope<Session, SessionConfiguration>.Current as SessionScope; }
    }

    /// <summary>
    /// Gets the session for this instance.
    /// </summary>
    public Session Session
    {
      get { return Resource; }
    }



    // Constructors

    internal SessionScope(Session session)
      : base(session)
    {
      Resource = session;
      ((IResource)Resource).AddConsumer(this);
      compilationScope = Session.HandlerAccessor.DomainHandler.Compiler.Activate();
    }

    public override void Dispose()
    {
      base.Dispose();
      compilationScope.DisposeSafely();
    }
  }
}