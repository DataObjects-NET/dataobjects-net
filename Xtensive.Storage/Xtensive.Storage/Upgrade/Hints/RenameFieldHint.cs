// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Rename field hint.
  /// </summary>
  [Serializable]
  public sealed class RenameFieldHint : TargetTypeHintBase, 
    IEquatable<RenameFieldHint>
  {
    /// <summary>
    /// Gets new field name.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Gets the old field name.
    /// </summary>    
    public string OldFieldName { get; private set; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(RenameFieldHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return 
        other.FieldName==FieldName 
          && other.OldFieldName==OldFieldName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (RenameFieldHint))
        return false;
      return Equals((RenameFieldHint) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return 
          ((FieldName!=null ? FieldName.GetHashCode() : 0) * 397) ^ 
            (OldFieldName!=null ? OldFieldName.GetHashCode() : 0);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorEq" copy="true"/>
    /// </summary>
    public static bool operator ==(RenameFieldHint left, RenameFieldHint right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true"/>
    /// </summary>
    public static bool operator !=(RenameFieldHint left, RenameFieldHint right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}: {1} -> {2}", 
        "Rename field", OldFieldName, FieldName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetType">The current type.</param>
    /// <param name="fieldName">Current field name.</param>
    /// <param name="oldFieldName">Old field name.</param>
    public RenameFieldHint(Type targetType, string fieldName, string oldFieldName)
      : base(targetType)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "fieldName");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(oldFieldName, "oldFieldName");
      OldFieldName = oldFieldName;
      FieldName = fieldName;
    }
  }
}