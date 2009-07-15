// Copyright (C) 2008 Xtensive LLC.
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
    public Func<AssociationInfo, object, object> GetValue { get; private set; }

    public Action<AssociationInfo, object, object> Break { get; private set; }

    public Action<AssociationInfo, object, object> Create { get; private set; }


    // Constructors

    public SyncActionSet(Func<AssociationInfo, object, object> getValue, 
      Action<AssociationInfo, object, object> @break,
      Action<AssociationInfo, object, object> create)
      : this()
    {
      GetValue = getValue;
      Break = @break;
      Create = create;
    }
  }
}