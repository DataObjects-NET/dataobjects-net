// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.15

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  [Serializable]
  internal struct SyncAction
  {
    public Action<AssociationInfo, IEntity, IEntity> Action { get; private set; }

    public AssociationInfo Association { get; private set; }

    public IEntity Owner { get; private set; }

    public IEntity Target { get; private set; }

    public SyncAction(Action<AssociationInfo, IEntity, IEntity> action,
      AssociationInfo association, IEntity owner, IEntity target)
      : this()
    {
      Action = action;
      Association = association;
      Owner = owner;
      Target = target;
    }
  }
}