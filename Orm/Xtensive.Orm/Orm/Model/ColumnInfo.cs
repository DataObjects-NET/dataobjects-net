// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.06

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Helpers;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a single column.
  /// </summary>
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public sealed class ColumnInfo : Node, 
    ICloneable
  {
    private ColumnAttributes attributes;
    private Type   valueType;
    private int?   length;
    private int?   scale;
    private int?   precision;
    private object defaultValue;
    private FieldInfo field;
    private NodeCollection<IndexInfo> indexes;
    private CultureInfo cultureInfo = CultureInfo.InvariantCulture;

    #region IsXxx properties

    /// <summary>
    /// Gets or sets a value indicating whether this column is system.
    /// </summary>
    public bool IsSystem {
      [DebuggerStepThrough]
      get { return (attributes & ColumnAttributes.System) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.System : Attributes & ~ColumnAttributes.System;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is declared in <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsDeclared
    {
      [DebuggerStepThrough]
      get { return (attributes & ColumnAttributes.Declared) > 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? 
                             (attributes | ColumnAttributes.Declared) & ~ColumnAttributes.Inherited :
                                                                                                      attributes & ~ColumnAttributes.Declared | ColumnAttributes.Inherited;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inherited from parent <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsInherited {
      [DebuggerStepThrough]
      get { return (attributes & ColumnAttributes.Inherited) > 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? 
                             (attributes | ColumnAttributes.Inherited) & ~ColumnAttributes.Declared :
                                                                                                      attributes & ~ColumnAttributes.Inherited | ColumnAttributes.Declared;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this column is contained by primary key.
    /// </summary>
    public bool IsPrimaryKey {
      [DebuggerStepThrough]
      get { return (Attributes & ColumnAttributes.PrimaryKey) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.PrimaryKey : Attributes & ~ColumnAttributes.PrimaryKey;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether column is nullable.
    /// </summary>
    public bool IsNullable {
      [DebuggerStepThrough]
      get { return (attributes & ColumnAttributes.Nullable) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.Nullable : Attributes & ~ColumnAttributes.Nullable;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether  property will be loaded on demand.
    /// </summary>
    public bool IsLazyLoad {
      [DebuggerStepThrough]
      get { return (attributes & ColumnAttributes.LazyLoad) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.LazyLoad : Attributes & ~ColumnAttributes.LazyLoad;
      }
    }

    #endregion

    /// <summary>
    /// Gets or sets corresponding field.
    /// </summary>
    public FieldInfo Field {
      [DebuggerStepThrough]
      get { return field; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        field = value;
      }
    }

    /// <summary>
    /// Specifies the type that should be used to store the
    /// value of the field (available for properties that can be mapped
    /// to multiple data types).
    /// </summary>
    public Type ValueType {
      [DebuggerStepThrough]
      get { return valueType; }
    }

    /// <summary>
    /// Gets or sets the length of the column.
    /// </summary>
    public int? Length {
      [DebuggerStepThrough]
      get { return length; }
    }

    /// <summary>
    /// Gets or sets the scale of the column.
    /// </summary>
    public int? Scale {
      [DebuggerStepThrough]
      get { return scale; }
    }

    /// <summary>
    /// Gets or sets the precision of the column.
    /// </summary>
    public int? Precision {
      [DebuggerStepThrough]
      get { return precision; }
    }

    /// <summary>
    /// Gets or sets the default value for this field.
    /// <see langword="null" /> indicates default value is provided automatically.
    /// </summary>
    public object DefaultValue {
      [DebuggerStepThrough]
      get { return defaultValue; }
      [DebuggerStepThrough]
      set { defaultValue = value; }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public ColumnAttributes Attributes {
      [DebuggerStepThrough]
      get { return attributes; }
    }

    /// <summary>
    /// Gets or sets column <see cref="CultureInfo"/> info.
    /// </summary>
    public CultureInfo CultureInfo {
      [DebuggerStepThrough]
      get { return cultureInfo; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked(); 
        cultureInfo = value;
      }
    }

    /// <summary>
    /// Gets or the indexes this field is included to.
    /// </summary>
    public NodeCollection<IndexInfo> Indexes {
      [DebuggerStepThrough]
      get { return indexes; } 
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        indexes = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="IComparer"/> instance.
    /// </summary>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> object.</param>
    /// <returns>The instance in <see cref="IComparer"/> to compare values of type <see cref="ValueType"/>.</returns>
    public IComparer GetComparer(CultureInfo cultureInfo)
    {
      return ComparerProvider.GetComparer(ValueType, cultureInfo);
    }

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(ColumnInfo obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return field.Equals(obj.field);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (ColumnInfo))
        return false;
      return Equals((ColumnInfo) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return field.GetHashCode();
    }

    #endregion

    #region ICloneable methods

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    public ColumnInfo Clone()
    {
      ColumnInfo clone = new ColumnInfo(field);
      clone.Name = Name;
      clone.attributes = attributes;
      clone.valueType = valueType;
      clone.length = length;
      clone.scale = scale;
      clone.precision = precision;
      clone.defaultValue = defaultValue;
      clone.indexes = indexes;
      clone.cultureInfo = cultureInfo;

      return clone;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field">The <see cref="Field"/> property value.</param>
    public ColumnInfo(FieldInfo field)
    {
      indexes = NodeCollection<IndexInfo>.Empty;
      this.field = field;
      IsSystem = field.IsSystem;
      IsDeclared = true;
      IsNullable = field.IsNullable;
      IsLazyLoad = field.IsLazyLoad;
      valueType = field.ValueType;
      if (valueType.IsGenericType) {
        var genericType = valueType.GetGenericTypeDefinition();
        if (genericType == typeof(Nullable<>))
          valueType = Nullable.GetUnderlyingType(valueType);
      }
      if (field.IsEnum) {
        valueType = Enum.GetUnderlyingType(valueType);
        if (field.DefaultValue != null)
          defaultValue = Convert.ChangeType(field.DefaultValue, valueType);
      }
      else {
        valueType = valueType;
        defaultValue = field.DefaultValue;
      }
      length = field.Length;
      scale = field.Scale;
      precision = field.Precision;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="field"><see cref="Field"/> property value.</param>
    /// <param name="valueType"><see cref="ValueType"/> property value.</param>
    public ColumnInfo(FieldInfo field, Type valueType)
    {
      indexes = NodeCollection<IndexInfo>.Empty;
      this.field = field;
      this.valueType = valueType;
      IsSystem = field.IsSystem;
      IsDeclared = true;
      IsNullable = field.IsNullable;
      IsLazyLoad = field.IsLazyLoad;
      length = field.Length;
      scale = field.Scale;
      precision = field.Precision;
      defaultValue = field.DefaultValue;
    }
  }
}