// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class AssociationInfoCollection : NodeCollection<AssociationInfo>
  {
    /// <summary>
    /// Finds the associations for the specified <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="type"><see cref="TypeInfo"/> to find associations for.</param>
    /// <returns></returns>
    public IEnumerable<AssociationInfo> Find(TypeInfo type)
    {
      var ancestors = new HashSet<TypeInfo>(type.GetAncestors());
      return this.Where(a => (a.TargetType==type || ancestors.Contains(a.TargetType) ||
        a.OwnerType==type || ancestors.Contains(a.OwnerType)));
    }

    /// <summary>
    /// Finds the associations for the specified <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="type"><see cref="TypeInfo"/> to find outgoing associations for.</param>
    /// <param name="incoming">if set to <see langword="true"/> [incoming].</param>
    /// <returns></returns>
    public IEnumerable<AssociationInfo> Find(TypeInfo type, bool incoming)
    {
      var ancestors = type.GetAncestors();
      Func<AssociationInfo, TypeInfo> accessor;
      accessor = incoming ? (Func<AssociationInfo, TypeInfo>) (a => a.TargetType) : (a => a.OwnerType);
      return this.Where(a => (accessor(a)==type || ancestors.Contains(accessor(a))));
    }
  }
}