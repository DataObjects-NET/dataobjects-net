// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.10

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.PairIntegrity
{
  internal struct SyncActionSet
  {
    public Func<AssociationInfo, IEntity, IEntity> GetValue { get; private set; }
    public Action<AssociationInfo, IEntity, IEntity, SyncContext> Break { get; private set; }
    public Action<AssociationInfo, IEntity, IEntity, SyncContext> Create { get; private set; }


    // Constructors

    public SyncActionSet(
      Func<AssociationInfo, IEntity, IEntity> getValue,
      Action<AssociationInfo, IEntity, IEntity, SyncContext> @break,
      Action<AssociationInfo, IEntity, IEntity, SyncContext> create)
      : this()
    {
      GetValue = getValue;
      Break = @break;
      Create = create;
    }
  }
}