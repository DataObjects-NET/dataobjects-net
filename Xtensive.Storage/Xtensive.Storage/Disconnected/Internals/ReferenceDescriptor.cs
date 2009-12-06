// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.22

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  internal struct ReferenceDescriptor
  {
    public Key TargetKey { get; private set; }

    public FieldInfo Field { get; private set; }

    public Key ReferencingKey { get; private set; }

    public ReferenceDescriptor(Key referencedKey, FieldInfo field, Key referencingKey)
      : this()
    {
      TargetKey = referencedKey;
      Field = field;
      ReferencingKey = referencingKey;
    }
  }
}