// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using Xtensive.Collections;
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
    private const string ToStringFormat = "Remove type: {0}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public ReadOnlyList<string> AffectedTables { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(RemoveTypeHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other) 
        && other.Type == Type;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as RemoveTypeHint);
    }

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
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    public RemoveTypeHint(string type)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(type, "sourceType");
      Type = type;
      AffectedTables = ReadOnlyList<string>.Empty;
    }
  }
}