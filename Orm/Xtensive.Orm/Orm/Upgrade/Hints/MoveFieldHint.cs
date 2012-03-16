// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.14

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Move field hint.
  /// </summary>
  [Serializable]
  public class MoveFieldHint : UpgradeHint,
    IEquatable<MoveFieldHint>
  {
    private const string ToStringFormat = "Move field: {0}.{1} -> {2}.{3}";

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
    public bool Equals(MoveFieldHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other)
        && other.SourceType == SourceType
        && other.SourceField == SourceField
        && other.TargetType == TargetType
        && other.TargetField == TargetField;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as MoveFieldHint);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked
      {
        int result = base.GetHashCode();
        result = (result * 397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
        result = (result * 397) ^ (SourceField != null ? SourceField.GetHashCode() : 0);
        result = (result * 397) ^ (TargetType != null ? TargetType.GetHashCode() : 0);
        result = (result * 397) ^ (TargetField != null ? TargetField.GetHashCode() : 0);
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
    public MoveFieldHint(string sourceType, string sourceField, Type targetType, string targetField)
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
    public MoveFieldHint(Type sourceType, string field, Type targetType)
      : this(sourceType, field, targetType, field)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceType">Value for <see cref="SourceType"/>.</param>
    /// <param name="sourceField">Value for <see cref="SourceField"/>.</param>
    /// <param name="targetType">Value for <see cref="TargetType"/>.</param>
    /// <param name="targetField">Value for <see cref="TargetField"/>.</param>
    public MoveFieldHint(Type sourceType, string sourceField, Type targetType, string targetField)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceField, "sourceField");
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetField, "targetField");

      SourceType = sourceType.FullName;
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
    public MoveFieldHint(string sourceType, string field, Type targetType)
      : this(sourceType, field, targetType, field)
    {
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourceType">The source type.</param>
    /// <param name="sourceField">The source field.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static MoveFieldHint Create<TTarget>(
      string sourceType, string sourceField,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TTarget: Entity
    {
      return new MoveFieldHint(
        sourceType, sourceField, 
        typeof(TTarget), targetPropertyAccessExpression.GetProperty().Name);
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourceType">The source type.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static MoveFieldHint Create<TTarget>(
      string sourceType,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TTarget: Entity
    {
      var targetField = targetPropertyAccessExpression.GetProperty().Name;
      return new MoveFieldHint(
        sourceType, targetField, 
        typeof(TTarget), targetField);
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourcePropertyAccessExpression">The source field access expression.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static MoveFieldHint Create<TSource, TTarget>(
      Expression<Func<TSource, object>> sourcePropertyAccessExpression,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TSource : Entity where TTarget: Entity
    {
      var sourceField = sourcePropertyAccessExpression.GetProperty().Name;
      var targetField = targetPropertyAccessExpression.GetProperty().Name;
      return new MoveFieldHint(
        typeof(TSource), sourceField, 
        typeof(TTarget), targetField);
    }
  }
}