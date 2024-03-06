// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.05.29

using System;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Copy field hint.
  /// </summary>
  [Serializable]
  public class CopyFieldHint : UpgradeHint,
    IEquatable<CopyFieldHint>
  {
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
      if (other is null)
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
    public override bool Equals(UpgradeHint other) => Equals(other as CopyFieldHint);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
        result = (result * 397) ^ (SourceField != null ? SourceField.GetHashCode() : 0);
        result = (result * 397) ^ (TargetType != null ? TargetType.GetHashCode() : 0);
        result = (result * 397) ^ (TargetField != null ? TargetField.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString() =>
      $"Copy field: {SourceType}.{SourceField} -> {TargetType.GetFullName()}.{TargetField}";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="sourceTypeName">The source type full name.</param>
    /// <param name="sourceFieldName">Name of source field.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="targetFieldName">Name of target field.</param>
    public CopyFieldHint(string sourceTypeName, string sourceFieldName, Type targetType, string targetFieldName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceTypeName, nameof(sourceTypeName));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceFieldName, nameof(sourceFieldName));
      ArgumentValidator.EnsureArgumentNotNull(targetType, nameof(targetType));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetFieldName, nameof(targetFieldName));
      SourceType = sourceTypeName;
      SourceField = sourceFieldName;
      TargetType = targetType;
      TargetField = targetFieldName;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="fieldName">Name of field in both source and target types.</param>
    /// <param name="targetType">The target type.</param>
    public CopyFieldHint(Type sourceType, string fieldName, Type targetType)
      : this(sourceType, fieldName, targetType, fieldName)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="sourceFieldName">The source field name.</param>
    /// <param name="targetType">The target type name.</param>
    /// <param name="targetFieldName">The target field name.</param>
    public CopyFieldHint(Type sourceType, string sourceFieldName, Type targetType, string targetFieldName)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, nameof(sourceType));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceFieldName, nameof(sourceFieldName));
      ArgumentValidator.EnsureArgumentNotNull(targetType, nameof(targetType));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(targetFieldName, nameof(targetFieldName));
      
      SourceType = sourceType.FullName;
      SourceField = sourceFieldName;
      TargetType = targetType;
      TargetField = targetFieldName;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="sourceTypeName">The source type full name.</param>
    /// <param name="fieldName">Field name in both source and target types.</param>
    /// <param name="targetType">The target type.</param>
    public CopyFieldHint(string sourceTypeName, string fieldName, Type targetType)
      : this(sourceTypeName, fieldName, targetType, fieldName)
    {
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourcePropertyAccessExpression">The source field access expression.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static CopyFieldHint Create<TSource, TTarget>(
      Expression<Func<TSource, object>> sourcePropertyAccessExpression,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TSource: Entity
      where TTarget: Entity
    {
      return new CopyFieldHint(
        typeof(TSource), sourcePropertyAccessExpression.GetProperty().Name, 
        typeof(TTarget), targetPropertyAccessExpression.GetProperty().Name);
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourceTypeName">Full name of source type.</param>
    /// <param name="sourceFieldName">The source field name.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static CopyFieldHint Create<TTarget>(
      string sourceTypeName, string sourceFieldName,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TTarget: Entity
    {
      return new CopyFieldHint(
        sourceTypeName, sourceFieldName, 
        typeof(TTarget), targetPropertyAccessExpression.GetProperty().Name);
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    /// <param name="sourceTypeName">Full name of source type.</param>
    /// <param name="targetPropertyAccessExpression">The target field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static CopyFieldHint Create<TTarget>(
      string sourceTypeName,
      Expression<Func<TTarget, object>> targetPropertyAccessExpression)
      where TTarget: Entity
    {
      var targetField = targetPropertyAccessExpression.GetProperty().Name;
      return new CopyFieldHint(
        sourceTypeName, targetField, 
        typeof(TTarget), targetField);
    }
  }
}