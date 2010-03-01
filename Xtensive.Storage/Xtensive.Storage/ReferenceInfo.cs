// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  [Serializable]
  public struct ReferenceInfo
  {
    public Entity ReferencingEntity { get; private set; }

    public Entity ReferencedEntity { get; private set; }

    public AssociationInfo Association { get; private set; }

    public ReferenceInfo(Entity referencingEntity, Entity referencedEntity, AssociationInfo association)
      : this()
    {
      ReferencingEntity = referencingEntity;
      ReferencedEntity = referencedEntity;
      Association = association;
    }
  }
}