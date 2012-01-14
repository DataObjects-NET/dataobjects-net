// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.15

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.ReferentialIntegrity;

namespace Xtensive.Orm.PairIntegrity
{
  [Serializable]
  internal struct SyncAction
  {
    public Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> Action { get; private set; }

    public AssociationInfo Association { get; private set; }

    public IEntity Owner { get; private set; }

    public IEntity Target { get; private set; }

    public SyncAction(Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> action,
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