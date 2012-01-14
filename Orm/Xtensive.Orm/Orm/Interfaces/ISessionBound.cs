// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.14

using Xtensive.Aspects;
using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for all the objects that are bound to the <see cref="Session"/> instance.
  /// Methods of implementors of this interface must be processed by PostSharp 
  /// to ensure their own <see cref="Session"/> is activated inside their method bodies, 
  /// and transaction is already opened there.
  /// </summary>
  /// <remarks>
  /// Only public and protected methods and properties are processed by
  /// <see cref="TransactionalTypeAttribute"/> aspect.
  /// To override the default behavior, use <see cref="TransactionalAttribute"/> and
  /// <see cref="InfrastructureAttribute"/>.
  /// </remarks>
  [TransactionalType(TransactionalBehavior.Auto)]
  public interface ISessionBound : IContextBound<Session>
  {
    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    Session Session { get; }
  }
}