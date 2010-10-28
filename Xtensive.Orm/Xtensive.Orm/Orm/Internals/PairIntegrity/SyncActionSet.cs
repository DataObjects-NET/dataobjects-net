// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.10

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.ReferentialIntegrity;

namespace Xtensive.Orm.PairIntegrity
{
  internal struct SyncActionSet
  {
    public Func<AssociationInfo, IEntity, IEntity> GetValue { get; private set; }
    public Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> Break { get; private set; }
    public Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> Create { get; private set; }


    // Constructors

    public SyncActionSet(
      Func<AssociationInfo, IEntity, IEntity> getValue,
      Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> @break,
      Action<AssociationInfo, IEntity, IEntity, SyncContext, RemovalContext> create)
      : this()
    {
      GetValue = getValue;
      Break = @break;
      Create = create;
    }
  }
}