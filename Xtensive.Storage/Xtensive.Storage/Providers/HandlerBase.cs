// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Abstract base class for any storage handler.
  /// </summary>
  public abstract class HandlerBase
  {
    /// <summary>
    /// Gets the <see cref="HandlerAccessor"/> providing other available handlers.
    /// </summary>
    protected internal HandlerAccessor Accessor { get; set; }
  }
}