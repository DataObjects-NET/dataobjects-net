// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.23

using System.Collections.Generic;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal static class AssociationBrowser
  {
    public static IEnumerable<Entity> FindReferencingObjects(AssociationInfo association, Entity referencedObject)
    {
      var key = referencedObject.Key.Value;
      RecordSet rs = association.UnderlyingIndex.ToRecordSet().Range(key, key);

      foreach (Entity referencingObject in rs.ToEntities(association.ReferencingField.DeclaringType.UnderlyingType))
        yield return referencingObject;
    }
  }
}