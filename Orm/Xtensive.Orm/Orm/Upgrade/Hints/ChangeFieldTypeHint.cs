// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.06.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;


namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Change field type enforced (ignore type conversion verification) hint.
  /// </summary>
  [Serializable]
  public sealed class ChangeFieldTypeHint : UpgradeHint,
    IEquatable<ChangeFieldTypeHint>
  {
    /// <summary>
    /// Gets the target type.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the target field name.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public IReadOnlyList<string> AffectedColumns { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(ChangeFieldTypeHint other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      var comparer = StringComparer.OrdinalIgnoreCase;
      return base.Equals(other)
        && other.Type == Type
        && comparer.Equals(other.FieldName, FieldName);
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other) => Equals(other as ChangeFieldTypeHint);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (Type != null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ (FieldName != null ? FieldName.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString() => $"Change type of field: {Type}.{FieldName}";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type field belongs to.</param>
    /// <param name="fieldName">Name of field that changes type.</param>
    public ChangeFieldTypeHint(Type type, string fieldName)
    {
      ArgumentNullException.ThrowIfNull(type, nameof(type));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, nameof(fieldName));

      Type = type;
      FieldName = fieldName;
      AffectedColumns = Array.Empty<string>();
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="propertyAccessExpression">The field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static ChangeFieldTypeHint Create<T>(Expression<Func<T, object>> propertyAccessExpression)
      where T: Entity
    {
      return new ChangeFieldTypeHint(typeof(T), propertyAccessExpression.GetProperty().Name);
    }
  }
}