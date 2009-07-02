// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class ClearProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity owner, Entity target)
    {
      if (!context.RemovalQueue.Contains(owner.State))
        switch (association.Multiplicity) {
          case Multiplicity.ZeroToOne:
          case Multiplicity.OneToOne:
          case Multiplicity.ManyToOne:
            owner.SetFieldValue<Entity>(association.OwnerField, null);
            break;
          case Multiplicity.ZeroToMany:
          case Multiplicity.OneToMany:
          case Multiplicity.ManyToMany:
            owner.GetProperty<EntitySetBase>(association.OwnerField.Name).Remove(target);
            break;
        }
    }
  }
}