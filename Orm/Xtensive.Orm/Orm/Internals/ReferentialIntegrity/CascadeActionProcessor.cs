// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.ReferentialIntegrity
{
  internal class CascadeActionProcessor : ActionProcessor
  {
    public override void Process(RemovalContext context, AssociationInfo association, Entity removingObject, Entity target, Entity referencingObject, Entity referencedObject)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          ReferentialActions.RemoveReference(association, referencingObject, referencedObject, null, context);
          break;
      }
      target.RemoveLaterInternal(EntityRemoveReason.Association);
    }
  }
}