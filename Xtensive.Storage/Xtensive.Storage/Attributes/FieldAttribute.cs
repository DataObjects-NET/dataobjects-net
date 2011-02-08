// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that property is persistent field
  /// and defines its persistence-related properties.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class FieldAttribute : StorageAttribute
  {
    internal int? length;
    internal int? scale;
    internal int? precision;
    internal bool? nullable;
    internal bool  nullableOnUpgrade;
    internal bool? indexed;

    /// <summary>
    /// Gets or sets whether the field should be indexed.
    /// </summary>
    public bool Indexed
    {
      get { return indexed.HasValue ? indexed.Value : false; }
      set { indexed = value; }
    }

    /// <summary>
    /// Gets or sets the length of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="string"/> or array of <see cref="byte"/> fields.
    /// </remarks>
    public int Length
    {
      get { return length.HasValue ? length.Value : 0; }
      set { length = value; }
    }

    /// <summary>
    /// Gets or sets the scale of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Scale
    {
      get { return scale.HasValue ? scale.Value : 0; }
      set { scale = value; }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Precision
    {
      get { return precision.HasValue ? precision.Value : 0; }
      set { precision = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field is nullable.
    /// </summary>
    /// <remarks>
    /// Note that this should be applied to reference fields only. For value-based fields
    /// consider using <see cref="System.Nullable{T}"/> approach.
    /// </remarks>
    /// <value>
    /// <see langword="true"/> if field nullable; otherwise, <see langword="false"/>.
    /// </value>
    public bool Nullable
    {
      get { return nullable.HasValue ? nullable.Value : false; }
      set { nullable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field must be <see cref="Nullable"/> during upgrade.
    /// </summary>
    public bool NullableOnUpgrade
    {
      get { return nullableOnUpgrade; }
      set { nullableOnUpgrade = value; }
    }

    /// <summary>
    /// Gets or sets the default value for this property.
    /// <see langword="null" /> indicates default value is provided automatically.
    /// </summary>
    public object DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether value of this field should be loaded on demand.
    /// </summary>
    /// <remarks>
    /// Usually lazy loading is used for byte-arrays, large string fields or <see cref="Structure">structures</see>.
    /// <see cref="Entity"/> and <see cref="EntitySet{TItem}"/> fields are always loaded on demand.
    /// </remarks>
    public bool LazyLoad { get; set; }
  }
}