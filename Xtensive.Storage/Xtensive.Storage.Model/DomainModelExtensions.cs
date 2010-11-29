// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.24

using System.Collections.Generic;
using Xtensive.Core.Sorting;
using Xtensive.Storage.Model.Stored;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Extension methods related to <see cref="DomainModel"/>.
  /// </summary>
  public static class DomainModelExtensions
  {
    /// <summary>
    /// Converts speicified <see cref="DomainModel"/> to corresponding <see cref="StoredDomainModel"/>.
    /// </summary>
    /// <param name="model">The model to convert.</param>
    /// <returns>A result of conversion.</returns>
    public static StoredDomainModel ToStoredModel(this DomainModel model)
    {
      return new ConverterToStoredModel().Convert(model);
    }

    /// <summary>
    /// Reorders the specified sequence of <see cref="AssociationInfo"/> using <see cref="TopologicalSorter"/>.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <returns>A reordered sequence.</returns>
    public static IEnumerable<AssociationInfo> Reorder(this IEnumerable<AssociationInfo> origin)
    {
      return TopologicalSorter.Sort(origin,
        (f, s) => s.TargetType != f.TargetType && s.TargetType.UnderlyingType.IsAssignableFrom(f.TargetType.UnderlyingType));
    }
  }
}