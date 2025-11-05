// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Rename type hint.
  /// </summary>
  [Serializable]
  public sealed class RecycledTypeHint : UpgradeHint,
    IEquatable<RecycledTypeHint>
  {
    /// <summary>
    /// Gets the type.
    /// </summary>
    public Type Type { get; private set; }

    /// <inheritdoc/>
    public bool Equals(RecycledTypeHint other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other)
        && other.Type == Type;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other) => Equals(other as RecycledTypeHint);

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
    public override string ToString() => $"Recycled type: {Type}";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The recycled type.</param>
    public RecycledTypeHint(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, nameof(type));

      Type = type;

      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="T">The recycled type.</typeparam>
    /// <returns>The newly created instance of this hint.</returns>
    public static RecycledTypeHint Create<T>()
      where T: Entity
    {
      return new RecycledTypeHint(typeof(T));
    }
  }
}