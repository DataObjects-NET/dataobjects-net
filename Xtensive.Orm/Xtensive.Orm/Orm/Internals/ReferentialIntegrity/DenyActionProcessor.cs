// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Orm.Model;

namespace Xtensive.Orm.ReferentialIntegrity
{
  internal class DenyActionProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity removingObject, Entity target, Entity referencingObject, Entity referencedObject)
    {
      if (!context.Contains(target) && referencingObject!=referencedObject)
        throw new ReferentialIntegrityException(association, removingObject, referencingObject, referencedObject);
    }
  }
}