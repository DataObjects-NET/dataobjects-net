// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class CascadeActionProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity removingObject, Entity target, Entity referencingObject, Entity referencedObject)
    {
      if (context.Items.Contains(target))
        return;

      switch (association.Multiplicity) {
      case Multiplicity.ZeroToMany:
      case Multiplicity.OneToMany:
      case Multiplicity.ManyToMany:
        AssociationActionProvider.RemoveReferenceAction(association, referencingObject, referencedObject);
        break;
      }
      target.Remove();
    }
  }
}