// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.07.28

using System.Diagnostics;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Possible identifier types for <see cref="IEntity.IdentifyAs(Xtensive.Orm.Operations.EntityIdentifierType)"/> method.
  /// </summary>
  public enum EntityIdentifierType
  {
    /// <summary>
    /// Automatically generated indetifier.
    /// </summary>
    Auto,
    /// <summary>
    /// No identifier (i.e. identifier must not be logged).
    /// </summary>
    None,
  }
}