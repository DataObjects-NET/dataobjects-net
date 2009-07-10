// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.09

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Information about an action to be executed during the persisting 
  /// of an <see cref="EntityState"/>.
  /// </summary>
  [Serializable]
  public struct EntityStateAction
  {
    /// <summary>
    /// The state to persist.
    /// </summary>
    public readonly EntityState EntityState;

    /// <summary>
    /// The action to be executed.
    /// </summary>
    public readonly PersistAction PersistAction;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="entityState">The value of the <see cref="EntityState"/> field.</param>
    /// <param name="persistAction">The value of the <see cref="PersistAction"/> field.</param>
    public EntityStateAction(EntityState entityState, PersistAction persistAction)
    {
      EntityState = entityState;
      PersistAction = persistAction;
    }
  }
}