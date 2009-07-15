// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.15

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  [Serializable]
  internal struct SyncAction
  {
    public Action<AssociationInfo, object, object> Action { get; private set; }

    public AssociationInfo Association { get; private set; }

    public object Owner { get; private set; }

    public object Target { get; private set; }

    public SyncAction(Action<AssociationInfo, object, object> action, AssociationInfo association, object owner, object target)
      : this()
    {
      Action = action;
      Association = association;
      Owner = owner;
      Target = target;
    }
  }
}