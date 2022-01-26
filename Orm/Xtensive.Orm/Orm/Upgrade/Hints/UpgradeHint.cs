// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Abstract base class for any upgrade hint.
  /// </summary>
  [Serializable]
  public abstract class UpgradeHint :
    IEquatable<UpgradeHint>
  {
    /// <inheritdoc/>
    public virtual bool Equals(UpgradeHint other)
    {
      return ReferenceEquals(this, other);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
        || obj is UpgradeHint other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }


    // Constructors

    internal UpgradeHint()
    {
    }
  }
}
