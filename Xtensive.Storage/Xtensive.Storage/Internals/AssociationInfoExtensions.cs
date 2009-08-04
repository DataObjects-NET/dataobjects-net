// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.23

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal static class AssociationInfoExtensions
  {
    public static IEnumerable<Entity> FindReferencingObjects(this AssociationInfo association, Entity target)
    {
      IndexInfo index;
      Tuple keyTuple;
      RecordSet recordSet;

      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne:
          index = association.OwnerType.Indexes.GetIndex(association.OwnerField.Name);
          keyTuple = target.Key.Value;
          recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
          foreach (var item in recordSet.ToEntities(0))
            yield return item;
          break;
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany:
          Key key = target.GetReferenceKey(association.Reversed.OwnerField);
          if (key!=null)
            yield return Query.SingleOrDefault(key);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          if (association.IsMaster)
            index = association.AuxiliaryType.Indexes.Where(indexInfo => indexInfo.IsSecondary).First();
          else
            index = association.Master.AuxiliaryType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
          keyTuple = target.Key.Value;
          recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
          foreach (var item in recordSet)
            yield return Query.SingleOrDefault(Key.Create(association.OwnerType, association.ExtractForeignKey(item)));
          break;
      }
    }

    public static IEnumerable<Entity> FindReferencedObjects(this AssociationInfo association, Entity owner)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          var target = owner.GetFieldValue<Entity>(association.OwnerField);
          if (target != null)
            yield return target;
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          var targets = owner.GetFieldValue<EntitySetBase>(association.OwnerField);
          foreach (var item in targets.Entities)
            yield return item;
          break;
      }

    }
  }
}