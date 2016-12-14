// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// A flow, allows to specify several terms as located near each other.
  /// </summary>
  public interface IProximityOperandsConstructionFlow
  {
    /// <summary>
    /// Current list of near terms.
    /// </summary>
    IList<IProximityOperand> Operands { get; }

    /// <summary>
    /// Specifies <see cref="ISimpleTerm"/> as near to already added terms.
    /// </summary>
    /// <param name="term">Word or phrase.</param>
    /// <returns>An instance ready to continue additing another terms.</returns>
    IProximityOperandsConstructionFlow NearSimpleTerm(string term);

    /// <summary>
    /// Specifies <see cref="IPrefixTerm"/> as near to already added terms.
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <returns>An instance ready to continue additing another terms.</returns>
    IProximityOperandsConstructionFlow NearPrefixTerm(string prefix);
  }
}