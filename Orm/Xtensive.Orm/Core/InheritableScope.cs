// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.05

using System;
using System.Security;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Core
{
  /// <summary>
  /// Inheritable <see cref="Scope{TContext}"/> version.
  /// </summary>
  /// <typeparam name="TContext">The type of the context.</typeparam>
  /// <typeparam name="TBaseAncestor">The type of the very base ancestor.</typeparam>
  [Serializable]
  public class InheritableScope<TContext, TBaseAncestor> : Scope<TContext>
    where TContext : class
    where TBaseAncestor: Scope<TContext>
  {
    // Constructors

    /// <inheritdoc/>
    protected internal InheritableScope(TContext context)
      : this()
    {
      Activate(context);
    }

    /// <inheritdoc/>
    /// <exception cref="SecurityException">Only one ancestor of each instance
    /// of this generic type is allowed.</exception>
    protected internal InheritableScope()
      : base(false)
    {
// Must be replaced to more effecient check from the point of performance.
//
//      var type = GetType();
//      if (allowedType==null) lock (@lock) if (allowedType==null)
//        allowedType = type;
//      if (allowedType!=type && (this as TBaseAncestor)==null)
//        throw new SecurityException(
//          Strings.ExOnlyOneAncestorOfEachInstanceOfThisGenericTypeIsAllowed);
    }

    // Type initializer

    /// <summary>
    /// <see cref="ClassDocTemplate.TypeInitializer" copy="true"/>
    /// </summary>
    /// <exception cref="SecurityException">Only one ancestor of each instance
    /// of this generic type is allowed.</exception>
    static InheritableScope()
    {
      var ancestorType = typeof(TBaseAncestor);
      if (allowedType==null) lock (@lock) if (allowedType==null)
        allowedType = ancestorType;
      if (allowedType!=ancestorType)
        throw new SecurityException(
          Strings.ExOnlyOneAncestorOfEachInstanceOfThisGenericTypeIsAllowed);
    }
  }
}