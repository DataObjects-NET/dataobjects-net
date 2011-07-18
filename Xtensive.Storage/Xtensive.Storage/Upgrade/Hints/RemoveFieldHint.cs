// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Remove field hint.
  /// </summary>
  [Serializable]
  public class RemoveFieldHint : UpgradeHint,
    IEquatable<RemoveFieldHint>
  {
    private const string ToStringFormat = "Remove field: {0}.{1}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the source field.
    /// </summary>
    public string Field { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is explicit.
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// Gets affected column paths.
    /// </summary>
    public ReadOnlyList<string> AffectedColumns { get; internal set; }

    /// <inheritdoc/>
    public bool Equals(RemoveFieldHint other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return base.Equals(other) 
        && other.Type==Type 
        && other.Field==Field;
    }

    /// <inheritdoc/>
    public override bool Equals(UpgradeHint other)
    {
      return Equals(other as RemoveFieldHint);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result * 397) ^ (Type!=null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ (Field!=null ? Field.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, Type, Field);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">Value for <see cref="Type"/>.</param>
    /// <param name="field">Value for <see cref="Field"/>.</param>
    public RemoveFieldHint(string type, string field)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(type, "sourceType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(field, "sourceField");
      Type = type;
      Field = field;
      AffectedColumns = new ReadOnlyList<string>(new List<string>());
    }

    /// <summary>
    /// Creates the instance of this hint.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="propertyAccessExpression">The field access expression.</param>
    /// <returns>The newly created instance of this hint.</returns>
    public static RemoveFieldHint Create<T>(Expression<Func<T, object>> propertyAccessExpression)
      where T: Entity
    {
      return new RemoveFieldHint(typeof(T).FullName, propertyAccessExpression.GetProperty().Name);
    }
  }
}