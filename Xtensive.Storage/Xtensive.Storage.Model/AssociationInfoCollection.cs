// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="AssociationInfo"/> objects.
  /// </summary>
  [Serializable]
  public sealed class AssociationInfoCollection : NodeCollection<AssociationInfo>
  {
    /// <summary>
    /// Finds the associations for the specified <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="type"><see cref="TypeInfo"/> to find associations for.</param>
    /// <returns></returns>
    public IEnumerable<AssociationInfo> Find(TypeInfo type)
    {
      var candidates = new HashSet<TypeInfo>(type.GetAncestors());
      candidates.UnionWith(type.GetInterfaces(true));
      candidates.Add(type);
      return this.Where(a => (candidates.Contains(a.TargetType) || candidates.Contains(a.OwnerType)));
    }

    /// <summary>
    /// Finds the associations for the specified <see cref="TypeInfo"/>.
    /// </summary>
    /// <param name="type"><see cref="TypeInfo"/> to find outgoing associations for.</param>
    /// <param name="target">if set to <see langword="true"/> then only target associations will be returned; otherwise only owner associations.</param>
    /// <returns></returns>
    public IEnumerable<AssociationInfo> Find(TypeInfo type, bool target)
    {
      var candidates = new HashSet<TypeInfo>(type.GetAncestors());
      candidates.UnionWith(type.GetInterfaces(true));
      candidates.Add(type);

      var filter = target ? (Func<AssociationInfo, TypeInfo>) (a => a.TargetType) : (a => a.OwnerType);
      return this.Where(a => candidates.Contains(filter(a)));
    }


    // Constructors

    /// <inheritdoc/>
    public AssociationInfoCollection(Node owner, string name)
      : base(owner, name)
    {
    }
  }
}