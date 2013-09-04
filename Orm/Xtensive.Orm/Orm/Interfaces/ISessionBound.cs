// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for all the objects that are bound to the <see cref="Session"/> instance.
  /// </summary>
  public interface ISessionBound : IContextBound<Session>
  {
    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    Session Session { get; }
  }
}