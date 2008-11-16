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
  internal static class AssociationBrowser
  {
    public static IEnumerable<Entity> FindReferencingObjects(AssociationInfo association, Entity referencedObject)
    {
      IndexInfo index;
      Tuple keyTuple;
      RecordSet rs;
      switch (association.Multiplicity) {
      case Multiplicity.ZeroToOne:
      case Multiplicity.ManyToOne:
        index = association.ReferencingType.Indexes.GetIndex(association.ReferencingField.Name);
        keyTuple = referencedObject.Key.Value;
        rs = index.ToRecordSet().Range(keyTuple, keyTuple);
        return rs.ToEntities(association.ReferencingField.DeclaringType.UnderlyingType);
      case Multiplicity.OneToOne:
      case Multiplicity.OneToMany:
        Key key = referencedObject.GetKey(association.Reversed.ReferencingField);
        if (key!=null)
          return new [] { key.Resolve() };
        break;
      case Multiplicity.ZeroToMany:
      case Multiplicity.ManyToMany:
        index = association.UnderlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(association.IsMaster ? 1 : 0).First();
        keyTuple = referencedObject.Key.Value;
        rs = index.ToRecordSet().Range(keyTuple, keyTuple);
        return rs.ToEntities(association.ReferencingField.DeclaringType.UnderlyingType);
      }
      return null;
    }
  }
}