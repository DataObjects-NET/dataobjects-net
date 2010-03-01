// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using Xtensive.Core.IoC;

namespace Xtensive.Storage
{
  /// <summary>
  /// Tagging interface for all the objects that are bound to the <see cref="Session"/> instance,
  /// which methods must be processed by PostSharp
  /// to ensure its <see cref="Session"/> is active inside method bodies.
  /// </summary>
  public interface ISessionBound : IContextBound<Session>
  {
    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    Session Session { get; }
  }
}