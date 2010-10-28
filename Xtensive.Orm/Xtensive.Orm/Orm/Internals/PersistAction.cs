// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.09

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Information about an action to be executed during the persisting 
  /// of an <see cref="EntityState"/>.
  /// </summary>
  [Serializable]
  public struct PersistAction
  {
    /// <summary>
    /// The state to persist.
    /// </summary>
    public readonly EntityState EntityState;

    /// <summary>
    /// The action to be executed.
    /// </summary>
    public readonly PersistActionKind ActionKind;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="entityState">The value of the <see cref="EntityState"/> field.</param>
    /// <param name="persistActionKind">The value of the <see cref="ActionKind"/> field.</param>
    public PersistAction(EntityState entityState, PersistActionKind persistActionKind)
    {
      EntityState = entityState;
      ActionKind = persistActionKind;
    }
  }
}