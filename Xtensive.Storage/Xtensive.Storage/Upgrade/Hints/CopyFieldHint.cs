// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Copy field hint.
  /// </summary>
  [Serializable]
  public class CopyFieldHint : UpgradeHint,
    IEquatable<CopyFieldHint>
  {
    private const string ToStringFormat = "Copy field: {0}.{1} -> {2}.{3}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string SourceType { get; private set; }
    /// <summary>
    /// Gets the source field.
    /// </summary>
    public string SourceField { get; private set; }
    /// <summary>
    /// Gets the target type.
    /// </summary>
    public Type TargetType { get; private set; }
    /// <summary>
    /// Gets the target field.
    /// </summary>
    public string TargetField { get; private set; }

    /// <inheritdoc/>
    public bool Equals(CopyFieldHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other) 
        && other.SourceType==SourceType
        && other.SourceField==SourceField
        && other.TargetType==TargetType
        && other.TargetField==TargetField;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as CopyFieldHint);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (SourceType!=null ? SourceType.GetHashCode() : 0);
        result = (result * 397) ^ (SourceField!=null ? SourceField.GetHashCode() : 0);
        result = (result * 397) ^ (TargetType!=null ? TargetType.GetHashCode() : 0);
        result = (result * 397) ^ (TargetField!=null ? TargetField.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat,
        SourceType, SourceField, TargetType.GetFullName(), TargetField);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceType">Value for <see cref="SourceType"/>.</param>
    /// <param name="sourceField">Value for <see cref="SourceField"/>.</param>
    /// <param name="targetType">Value for <see cref="TargetType"/>.</param>
    /// <param name="targetField">Value for <see cref="TargetField"/>.</param>
    public CopyFieldHint(string sourceType, string sourceField, Type targetType, string targetField)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceType, "sourceType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceField, "sourceField");
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetField, "targetField");
      SourceType = sourceType;
      SourceField = sourceField;
      TargetType = targetType;
      TargetField = targetField;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceType">Value for <see cref="SourceType"/>.</param>
    /// <param name="field">Value for <see cref="SourceField"/> and <see cref="TargetField"/>.</param>
    /// <param name="targetType">Value for <see cref="TargetType"/>.</param>
    public CopyFieldHint(string sourceType, string field, Type targetType)
      : this(sourceType, field, targetType, field)
    {
    }
  }
}