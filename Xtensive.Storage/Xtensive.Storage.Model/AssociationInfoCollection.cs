// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class AssociationInfoCollection : NodeCollection<AssociationInfo>
  {
    private readonly Dictionary<TypeInfo, List<AssociationInfo>> typeIndex = new Dictionary<TypeInfo, List<AssociationInfo>>();

    /// <summary>
    /// Finds the associations for the specified <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="referencedType"><see cref="TypeInfo"/> to find associations for.</param>
    /// <returns></returns>
    public IEnumerable<AssociationInfo> Find(TypeInfo referencedType)
    {
      List<AssociationInfo> associations;
      if (typeIndex.TryGetValue(referencedType, out associations))
        foreach (AssociationInfo association in associations)
          yield return association;

      foreach (TypeInfo ancestor in referencedType.GetAncestors()) {
        if (!typeIndex.TryGetValue(ancestor, out associations))
          continue;
        foreach (AssociationInfo association in associations)
          yield return association;
      }
    }

    /// <inheritdoc/>
    protected override void OnInserted(AssociationInfo value, int index)
    {
      base.OnInserted(value, index);
      if (!typeIndex.ContainsKey(value.ReferencedType))
        typeIndex[value.ReferencedType] = new List<AssociationInfo>(1);
      typeIndex[value.ReferencedType].Add(value);
    }
  }
}