// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Rename type hint.
  /// </summary>
  [Serializable]
  public sealed class RenameTypeHint : UpgradeHint,
    IEquatable<RenameTypeHint>
  {
    private const string ToStringFormat = "Rename type: {0} -> {1}";

    /// <summary>
    /// Gets the new type.
    /// </summary>
    public Type NewType { get; private set; }

    /// <summary>
    /// Gets the name of old type.
    /// </summary>
    public string OldType { get; private set; }

    /// <inheritdoc/>
    public bool Equals(RenameTypeHint other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other)
        && other.NewType == NewType
        && other.OldType == OldType;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other) => Equals(other as RenameTypeHint);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (NewType != null ? NewType.GetHashCode() : 0);
        result = (result * 397) ^ (OldType != null ? OldType.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString() =>
      $"Rename type: {OldType} -> {NewType.GetFullName()}";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="oldType">The old type name (if short name provided then <paramref name="newType"/> namespace will be used).</param>
    /// <param name="newType">The new type.</param>
    public RenameTypeHint(string oldType, Type newType)
    {
      ArgumentNullException.ThrowIfNull(newType, nameof(newType));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldType, nameof(oldType));

      if (!oldType.Contains(".", StringComparison.Ordinal))
        oldType = newType.Namespace + "." + oldType;
      OldType = oldType;
      NewType = newType;
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="T">The new type.</typeparam>
    /// <param name="oldName">The old type name (if short name provided then <typeparamref name="T"/> namespace will be used).</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static RenameTypeHint Create<T>(string oldName)
      where T: Entity
    {
      return new RenameTypeHint(oldName, typeof(T));
    }
  }
}