// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
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
      if (ReferenceEquals(null, other))
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
      return Equals(other as RenameTypeHint);
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
      return string.Format(ToStringFormat, OldType, NewType.GetFullName());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="oldType">The old type.</param>
    /// <param name="newType">The new type.</param>
    public RenameTypeHint(string oldType, Type newType)
    {
      ArgumentValidator.EnsureArgumentNotNull(newType, "newType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldType, "oldType");

      if (!oldType.Contains("."))
        oldType = newType.Namespace + "." + oldType;
      OldType = oldType;
      NewType = newType;
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="T">The new type.</typeparam>
    /// <param name="oldName">The old type name.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static RenameTypeHint Create<T>(string oldName)
      where T: Entity
    {
      return new RenameTypeHint(oldName, typeof(T));
    }
  }
}