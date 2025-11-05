// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Remove type hint.
  /// </summary>
  [Serializable]
  public class RemoveTypeHint : UpgradeHint,
    IEquatable<RemoveTypeHint>
  {
    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public IReadOnlyList<string> AffectedTables { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(RemoveTypeHint other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other)
        && other.Type == Type;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other) => Equals(other as RemoveTypeHint);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (Type != null ? Type.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString() => $"Remove type: {Type}";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="typeName">Full name of type.</param>
    public RemoveTypeHint(string typeName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeName, nameof(typeName));
      Type = typeName;
      AffectedTables = Array.Empty<string>();
    }
  }
}