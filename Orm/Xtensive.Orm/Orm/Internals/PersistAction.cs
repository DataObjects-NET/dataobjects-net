// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.09

using System;

namespace Xtensive.Orm.Internals
{
  internal readonly struct PersistAction
  {
    public readonly StorageNode Node;
    public readonly EntityState EntityState;
    public readonly PersistActionKind ActionKind;


    // Constructors

    public PersistAction(StorageNode node, EntityState entityState, PersistActionKind persistActionKind)
    {
      Node = node;
      EntityState = entityState;
      ActionKind = persistActionKind;
    }
  }
}