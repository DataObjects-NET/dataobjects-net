// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.22

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  internal struct EntitySetItemDesc
  {
    public Key OwnerKey { get; private set; }

    public FieldInfo Field { get; private set; }

    public Key ItemKey { get; private set; }

    public EntitySetItemDesc(Key ownerKey, FieldInfo field, Key itemKey)
      : this()
    {
      OwnerKey = ownerKey;
      Field = field;
      ItemKey = itemKey;
    }
  }
}