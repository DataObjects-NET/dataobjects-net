// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.28

using System;
using Xtensive.Core;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Describes a field that is a part of a primary key.
  /// </summary>
  [Serializable]
  public sealed class KeyField : Node
  {
    private Type valueType;
    private int hashCode;

    /// <summary>
    /// Gets or sets the type of the field.
    /// </summary>
    public Type ValueType
    {
      get { return valueType; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        valueType = value;
      }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (!IsLocked)
        return base.GetHashCode();

      if (hashCode == 0)
        hashCode = Name.GetHashCode() ^ valueType.GetHashCode();

      return hashCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyField"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="valueType">Type of the field value.</param>
    public KeyField(string name, Type valueType) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueType, "valueType");
      this.valueType = valueType;
    }
  }
}