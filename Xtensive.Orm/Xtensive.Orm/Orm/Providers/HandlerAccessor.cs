// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

using Xtensive.Orm;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Storage handler accessor.
  /// Provided by protected members, such as <see cref="HandlerBase.Handlers"/> 
  /// to provide access to other available handlers.
  /// </summary>
  public sealed class HandlerAccessor
  {
    /// <summary>
    /// Gets the <see cref="Orm.Domain"/> 
    /// this handler accessor is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the handler provider 
    /// creating handlers in the <see cref="Domain"/>.
    /// </summary>
    public HandlerFactory HandlerFactory { get; internal set; }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Domain"/> handler.
    /// </summary>
    public DomainHandler DomainHandler { get; internal set; }

    /// <summary>
    /// Gets the <see cref="SchemaUpgradeHandler"/> instance.
    /// </summary>
    public SchemaUpgradeHandler SchemaUpgradeHandler { get; internal set; }

    /// <summary>
    /// Gets the handler of the current <see cref="Session"/>.
    /// </summary>
//    public SessionHandler SessionHandler { get { return Session.Demand().Handler; } }
//    

    // Constructors

    internal HandlerAccessor(Domain domain)
    {
      Domain = domain;
    }
  }
}