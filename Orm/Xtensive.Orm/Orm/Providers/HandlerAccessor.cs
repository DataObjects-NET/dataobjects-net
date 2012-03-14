// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Storage handler accessor.
  /// Provided by protected members, such as <see cref="DomainBoundHandler.Handlers"/> 
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
    public HandlerFactory Factory { get; internal set; }

    /// <summary>
    /// Gets the storage driver.
    /// </summary>
    public StorageDriver StorageDriver { get; internal set; }

    /// <summary>
    /// Gets storage provider info.
    /// </summary>
    public ProviderInfo ProviderInfo { get { return StorageDriver.ProviderInfo; } }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Domain"/> handler.
    /// </summary>
    public DomainHandler DomainHandler { get; internal set; }

    /// <summary>
    /// Gets the <see cref="SchemaResolver"/> instance.
    /// </summary>
    internal SchemaResolver SchemaResolver { get; set; }

    /// <summary>
    /// Gets the <see cref="SequenceQueryBuilder"/> instance.
    /// </summary>
    internal SequenceQueryBuilder SequenceQueryBuilder { get; set; }

    internal THandler Create<THandler>()
      where THandler : DomainBoundHandler
    {
      var handler = Factory.CreateHandler<THandler>();
      handler.Initialize(this);
      return handler;
    }

    // Constructors

    internal HandlerAccessor(Domain domain)
    {
      Domain = domain;
    }
  }
}