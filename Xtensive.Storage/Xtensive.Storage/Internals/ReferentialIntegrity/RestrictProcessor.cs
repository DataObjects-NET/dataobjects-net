// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class RestrictProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity referencingObject, Entity referencedObject)
    {
      if (!context.RemovalQueue.Contains(referencingObject.State))
        throw new ReferentialIntegrityException(referencedObject);
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          referencingObject.SetFieldValue<Entity>(association.ReferencingField, null, context.Notify);
          break;
      }
    }
  }
}