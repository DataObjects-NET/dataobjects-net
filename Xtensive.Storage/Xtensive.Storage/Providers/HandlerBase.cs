// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

namespace Xtensive.Storage.Providers
{
  public abstract class HandlerBase
  {
    /// <summary>
    /// Gets the execution context.
    /// </summary>
    protected internal HandlerAccessor HandlerAccessor { get; internal set; }
  }
}