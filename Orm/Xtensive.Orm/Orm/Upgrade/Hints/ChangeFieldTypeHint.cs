// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.05

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Collections;


namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Change field type enforced (ignore type conversion verification) hint.
  /// </summary>
  [Serializable]
  public sealed class ChangeFieldTypeHint : UpgradeHint,
    IEquatable<ChangeFieldTypeHint>
  {
    private const string ToStringFormat = "Change type of field: {0}.{1}";

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
    public ReadOnlyList<string> AffectedColumns { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(ChangeFieldTypeHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      var comparer = StringComparer.OrdinalIgnoreCase;
      return base.Equals(other)
        && other.Type==Type 
        && comparer.Equals(other.FieldName, FieldName);
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as ChangeFieldTypeHint);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (Type!=null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ (FieldName!=null ? FieldName.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type, FieldName);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    /// <param name="fieldName">Value for <see cref="FieldName"/>.</param>
    public ChangeFieldTypeHint(Type type, string fieldName)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(fieldName, "sourceField");

      Type = type;
      FieldName = fieldName;
      AffectedColumns = new ReadOnlyList<string>(new List<string>());
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