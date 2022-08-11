// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  public sealed class MergeTypeHint : UpgradeHint,
    IEquatable<MergeTypeHint>
  {
    private const string ToStringFormat = "Merge type: {0} -> {1}";

    /// <summary>
    /// Gets the new type.
    /// </summary>
    public Type NewType { get; private set; }

    /// <summary>
    /// Gets the old type.
    /// </summary>
    public Type OldType { get; private set; }

    /// <inheritdoc/>
    public bool Equals(MergeTypeHint other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other) 
        && other.NewType==NewType
        && other.OldType==OldType;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as MergeTypeHint);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (NewType!=null ? NewType.GetHashCode() : 0);
        result = (result * 397) ^ (OldType!=null ? OldType.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, OldType.GetFullName(), NewType.GetFullName());
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="oldType">The old type.</param>
    /// <param name="newType">The new type.</param>
    public MergeTypeHint(Type oldType, Type newType)
    {
      ArgumentValidator.EnsureArgumentNotNull(newType, "newType");
      ArgumentValidator.EnsureArgumentNotNull(oldType, "oldType");

      OldType = oldType;
      NewType = newType;

      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TOld">The old type.</typeparam>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <returns>The newly created instance of this hint.</returns>
    public static MergeTypeHint Create<TOld, TNew>()
      where TOld: Entity
      where TNew: Entity
    {
      return new MergeTypeHint(typeof(TOld), typeof(TNew));
    }
  }
}