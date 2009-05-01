// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Rename type hint.
  /// </summary>
  [Serializable]
  public sealed class RenameTypeHint : TargetTypeHintBase,
    IEquatable<RenameTypeHint>
  {
    /// <summary>
    /// Gets the old type name.
    /// </summary>
    public string OldName { get; private set; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(RenameTypeHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return 
        other.TargetType==TargetType && 
          other.OldName==OldName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (RenameTypeHint))
        return false;
      return Equals((RenameTypeHint) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return 
        ((TargetType!=null ? TargetType.GetHashCode() : 0) * 397) ^ 
          (OldName!=null ? OldName.GetHashCode() : 0);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorEq" copy="true"/>
    /// </summary>
    public static bool operator ==(RenameTypeHint left, RenameTypeHint right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true"/>
    /// </summary>
    public static bool operator !=(RenameTypeHint left, RenameTypeHint right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override void Translate(HintSet target)
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}: {1} -> {2}", 
        "Rename type", OldName, TargetType.GetFullName());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">The current type.</param>
    /// <param name="oldName">The old type name.</param>
    public RenameTypeHint(Type targetType, string oldName)
      : base(targetType)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldName, "oldName");
      if (!oldName.Contains("."))
        oldName = targetType.Namespace + "." + oldName;
      OldName = oldName;
    }
  }
}