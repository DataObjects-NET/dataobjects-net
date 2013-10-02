// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

namespace Xtensive.Core
{
  /// <summary>
  /// Interface for all objects that are bound to some <see cref="Context"/>
  /// instance.
  /// </summary>
  /// <typeparam name="TContext">The type of the context.</typeparam>
  public interface IContextBound<TContext> 
    where TContext : class
  {
    /// <summary>
    /// Gets <see cref="Context"/> to which current instance is bound.
    /// </summary>
    TContext Context { get; }
  }
}