// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.ReferentialIntegrity
{
  internal class DenyActionProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity removingObject, Entity target, Entity referencingObject, Entity referencedObject)
    {
      if (!context.Items.Contains(target))
        throw new ReferentialIntegrityException(removingObject);

      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          AssociationActionProvider.ClearReferenceAction(association, referencingObject, null);
          break;
      }
    }
  }
}