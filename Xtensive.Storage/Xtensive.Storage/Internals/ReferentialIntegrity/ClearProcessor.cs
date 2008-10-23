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
    public override void Process(Entity referencedObject, Entity referencingObject, AssociationInfo association)
    {
      switch (association.Multiplicity) {
      case Multiplicity.OneToOne:
      case Multiplicity.OneToMany:
        referencingObject[association.ReferencingField.Name] = null;
        break;
      case Multiplicity.ManyToOne:
      case Multiplicity.ManyToMany:
        referencingObject.GetProperty<EntitySet>(association.ReferencingField.Name).Remove(referencedObject);
        break;
      }
    }
  }
}