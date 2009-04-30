// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.23

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal static class AssociationInfoExtensions
  {
    public static IEnumerable<Entity> FindReferencingObjects(this AssociationInfo association, Entity referencedObject)
    {
      IndexInfo index;
      Tuple keyTuple;
      RecordSet recordSet;
      switch (association.Multiplicity) {
      case Multiplicity.ZeroToOne:
      case Multiplicity.ManyToOne:
        index = association.ReferencingType.Indexes.GetIndex(association.ReferencingField.Name);
        keyTuple = referencedObject.Key.Value;
        recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
        foreach(Entity item in recordSet.ToEntities(association.ReferencingField.DeclaringType.UnderlyingType))
          yield return item;
          break;
      case Multiplicity.OneToOne:
      case Multiplicity.OneToMany:
        Key key = referencedObject.GetKey(association.Reversed.ReferencingField);
        if (key!=null)
          yield return key.Resolve();
        break;
      case Multiplicity.ZeroToMany:
      case Multiplicity.ManyToMany:
        index = association.UnderlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(association.IsMaster ? 0 : 1).First();
        keyTuple = referencedObject.Key.Value;
        recordSet = index.ToRecordSet().Range(keyTuple, keyTuple);
        foreach (var item in recordSet)
          yield return Key.Create(association.ReferencingType, association.ExtractForeignKey(item)).Resolve();
          break;
      }
    }
  }
}