// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.06

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public sealed class ColumnInfo : Node
  {
    private ColumnAttributes attributes;
    private Type valueType;
    private int? length;
    private FieldInfo field;
    private NodeCollection<IndexInfo> indexes;
    private CultureInfo cultureInfo = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets or sets a value indicating whether column is nullable.
    /// </summary>
    public bool IsNullable
    {
      get { return (attributes & ColumnAttributes.Nullable) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.Nullable : Attributes & ~ColumnAttributes.Nullable;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether  property will be loaded on demand.
    /// </summary>
    public bool LazyLoad
    {
      get { return (attributes & ColumnAttributes.LazyLoad) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.LazyLoad : Attributes & ~ColumnAttributes.LazyLoad;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether column is translatable.
    /// </summary>
    public bool IsCollatable
    {
      get { return (attributes & ColumnAttributes.Collatable) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.Collatable : Attributes & ~ColumnAttributes.Collatable;
      }
    }

    /// <summary>
    /// Gets or sets corresponding field.
    /// </summary>
    public FieldInfo Field
    {
      get { return field; }
      set
      {
        this.EnsureNotLocked();
        field = value;
      }
    }

    public NodeCollection<IndexInfo> Indexes
    {
      get { return indexes; }
      set
      {
        this.EnsureNotLocked();
        indexes = value;
      }
    }

    /// <summary>
    /// Gets or sets the length of the column.
    /// </summary>
    public int? Length
    {
      get { return length; }
    }

    /// <summary>
    /// Specifies the type that should be used to store the
    /// value of the field (available for properties that can be mapped
    /// to multiple data types).
    /// </summary>
    public Type ValueType
    {
      get { return valueType; }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public ColumnAttributes Attributes
    {
      get { return attributes; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is declared in <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsDeclared
    {
      get { return (attributes & ColumnAttributes.Declared) > 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value
                       ? (attributes | ColumnAttributes.Declared) & ~ColumnAttributes.Inherited
                       : attributes & ~ColumnAttributes.Declared | ColumnAttributes.Inherited;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inherited from parent <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsInherited
    {
      get { return (attributes & ColumnAttributes.Inherited) > 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value
                       ? (attributes | ColumnAttributes.Inherited) & ~ColumnAttributes.Declared
                       : attributes & ~ColumnAttributes.Inherited | ColumnAttributes.Declared;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this column is contained by primary key.
    /// </summary>
    public bool IsPrimaryKey
    {
      get { return (Attributes & ColumnAttributes.PrimaryKey) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.PrimaryKey : Attributes & ~ColumnAttributes.PrimaryKey;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this column is system.
    /// </summary>
    public bool IsSystem
    {
      get { return (attributes & ColumnAttributes.System) != 0; }
      private set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | ColumnAttributes.System : Attributes & ~ColumnAttributes.System;
      }
    }

    /// <summary>
    /// Gets or sets column <see cref="CultureInfo"/> info.
    /// </summary>
    public CultureInfo CultureInfo
    {
      get { return cultureInfo; }
      set {
        this.EnsureNotLocked(); 
        cultureInfo = value;
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
      clone.indexes = indexes;

      return clone;

    }

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
      if (ReferenceEquals(null, obj))
        return false;
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="field">The field.</param>
    public ColumnInfo(FieldInfo field)
    {
      indexes = NodeCollection<IndexInfo>.Empty;
      this.field = field;
      valueType = field.ValueType.IsEnum ? Enum.GetUnderlyingType(field.ValueType) : field.ValueType;
      IsNullable = field.IsNullable;
      LazyLoad = field.LazyLoad;
      IsCollatable = field.IsCollatable;
      length = field.Length;
      IsDeclared = true;
      IsSystem = field.IsSystem;
    }
  }
}