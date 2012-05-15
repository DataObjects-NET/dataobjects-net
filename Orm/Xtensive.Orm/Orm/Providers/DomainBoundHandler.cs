// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Abstract base class for any storage handler.
  /// </summary>
  public abstract class DomainBoundHandler : Handler
  {
    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> providing other available handlers.
    /// </summary>
    public HandlerAccessor Handlers { get; private set; }

    internal void Initialize(HandlerAccessor handlers)
    {
      Handlers = handlers;
      Initialize();
    }

    protected virtual void Initialize()
    {
    }
  }
}