// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.10

using System;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public sealed class FieldInfo : MappingNode
  {
    private PropertyInfo underlyingProperty;
    private FieldAttributes attributes;
    private Type valueType;
    private int? length;
    private TypeInfo reflectedType;
    private TypeInfo declaringType;
    private readonly FieldInfoCollection fields;
    private FieldInfo parent;
    private ColumnInfo column;
    private Segment<int> mappingInfo;
    private AssociationInfo association;
    private CultureInfo cultureInfo = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets <see cref="MappingInfo"/> for current field.
    /// </summary>
    public Segment<int> MappingInfo
    {
      get
      {
        if (!IsLocked)
          throw new InvalidOperationException();
        return mappingInfo;
      }
    }

    /// <summary>
    /// Gets or sets the length of the property.
    /// </summary>
    public int? Length
    {
      get { return length; }
      set
      {
        this.EnsureNotLocked();
        length = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is nullable.
    /// </summary>
    public bool IsNullable
    {
      get { return (attributes & FieldAttributes.Nullable) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property will be loaded on demand.
    /// </summary>
    public bool LazyLoad
    {
      get { return (attributes & FieldAttributes.LazyLoad) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsTranslatable
    {
      get { return (attributes & FieldAttributes.Translatable) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsCollatable
    {
      get { return (attributes & FieldAttributes.Collatable) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is structure field.
    /// </summary>
    public bool IsStructure
    {
      get { return (attributes & FieldAttributes.Structure) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to EntitySet.
    /// </summary>
    public bool IsEntitySet
    {
      get { return (attributes & FieldAttributes.EntitySet) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is primitive field.
    /// </summary>
    public bool IsPrimitive
    {
      get { return !IsStructure && !IsEntity && !IsEntitySet; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to Entity.
    /// </summary>
    public bool IsEntity
    {
      get { return (attributes & FieldAttributes.Entity) != 0; }
    }

    /// <summary>
    /// Gets the underlying system property.
    /// </summary>
    public PropertyInfo UnderlyingProperty
    {
      get { return underlyingProperty; }
      set
      {
        this.EnsureNotLocked();
        underlyingProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the type of the value of this instance.
    /// </summary>
    public Type ValueType
    {
      get { return valueType; }
      set
      {
        this.EnsureNotLocked();
        valueType = value;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public FieldAttributes Attributes
    {
      get { return attributes; }
    }

    /// <summary>
    /// Gets or sets the parent field for nested fields.
    /// </summary>
    /// <remarks>
    /// For not nested fields return value is <see langword="null"/>.
    /// </remarks>
    public FieldInfo Parent
    {
      get { return parent; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Parent");
        parent = value;
        parent.Fields.Add(this);
        reflectedType = value.ReflectedType;
        declaringType = value.DeclaringType;
        IsDeclared = value.IsDeclared;
        IsPrimaryKey = value.IsPrimaryKey;
        association = value.association;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is declared in <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsDeclared
    {
      get { return (Attributes & FieldAttributes.Declared) > 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value
                       ? (Attributes | FieldAttributes.Declared) & ~FieldAttributes.Inherited
                       : (Attributes & ~FieldAttributes.Declared) | FieldAttributes.Inherited;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inherited from parent <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsInherited
    {
      get { return (Attributes & FieldAttributes.Inherited) > 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value
                       ? (Attributes | FieldAttributes.Inherited) & ~FieldAttributes.Declared
                       : (Attributes & ~FieldAttributes.Inherited) | FieldAttributes.Declared;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is contained by primary key.
    /// </summary>
    public bool IsPrimaryKey
    {
      get { return (Attributes & FieldAttributes.PrimaryKey) != 0; }
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.PrimaryKey : Attributes & ~FieldAttributes.PrimaryKey;
        if (column != null)
          column.IsPrimaryKey = true;
        else
          foreach (FieldInfo childField in fields)
            childField.IsPrimaryKey = true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is contained by primary key.
    /// </summary>
    public bool IsNested
    {
      get { return (Attributes & FieldAttributes.Nested) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.Nested : Attributes & ~FieldAttributes.Nested;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is system.
    /// </summary>
    public bool IsSystem
    {
      get { return (attributes & FieldAttributes.System) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.System : Attributes & ~FieldAttributes.System;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property explicitly implemented.
    /// </summary>
    public bool IsExplicit
    {
      get { return (attributes & FieldAttributes.Explicit) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.Explicit : Attributes & ~FieldAttributes.Explicit;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property implements property of one or more interfaces.
    /// </summary>
    public bool IsInterfaceImplementation
    {
      get { return (attributes & FieldAttributes.InterfaceImplementation) != 0; }
      set
      {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.InterfaceImplementation : Attributes & ~FieldAttributes.InterfaceImplementation;
      }
    }

    /// <summary>
    /// Gets the type that was used to obtain this instance.
    /// </summary>
    public TypeInfo ReflectedType
    {
      get { return reflectedType; }
      set
      {
        this.EnsureNotLocked();
        reflectedType = value;
      }
    }

    public TypeInfo DeclaringType
    {
      get { return declaringType; }
      set
      {
        this.EnsureNotLocked();
        declaringType = value;
      }
    }

    /// <summary>
    /// Gets the nested fields.
    /// </summary>
    public FieldInfoCollection Fields
    {
      get { return fields; }
    }

    /// <summary>
    /// Gets or sets the column associated with this instance.
    /// </summary>
    public ColumnInfo Column
    {
      get { return column; }
      set
      {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <summary>
    /// Gets or sets the field association.
    /// </summary>
    public AssociationInfo Association
    {
      get { return association; }
      set
      {
        this.EnsureNotLocked();
        association = value;
      }
    }

    /// <summary>
    /// Gets or sets field <see cref="CultureInfo"/> info.
    /// </summary>
    public CultureInfo CultureInfo
    {
      get { return cultureInfo; }
      set
      {
        this.EnsureNotLocked();
        cultureInfo = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      fields.Lock(true);
      if (column!=null) {
        column.Lock(true);
        if (reflectedType.IsStructure)
          mappingInfo = new Segment<int>(reflectedType.Columns.IndexOf(column), 1);
        else
          mappingInfo = new Segment<int>(reflectedType.Indexes.PrimaryIndex.Columns.IndexOf(column), 1);
      }
      else if (fields.Count > 0)
        mappingInfo = new Segment<int>(fields.First().MappingInfo.Offset, fields.Sum(info => info.MappingInfo.Length));
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    public FieldInfo Clone()
    {
      FieldInfo clone= new FieldInfo(declaringType, reflectedType, attributes);
      clone.Name = Name;
      clone.MappingName = MappingName;
      clone.underlyingProperty = underlyingProperty;
      clone.valueType = valueType;
      clone.length = length;
      clone.association = association;

      return clone;
    }

    /// <inheritdoc/>
    public bool Equals(FieldInfo obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.declaringType, declaringType) && Equals(obj.valueType, valueType) && Equals(obj.Name, Name);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (FieldInfo))
        return false;
      return Equals((FieldInfo) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return ((declaringType.GetHashCode() * 397) ^ valueType.GetHashCode() * 631) ^ Name.GetHashCode();
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="attributes">The attributes.</param>
    public FieldInfo(TypeInfo type, FieldAttributes attributes) : this(type, type, attributes)
    {
      this.attributes |= FieldAttributes.Declared;
    }

    private FieldInfo(TypeInfo declaringType, TypeInfo reflectedType, FieldAttributes attributes)
    {
      this.attributes = attributes;
      this.declaringType = declaringType;
      this.reflectedType = reflectedType;
      fields = IsEntity || IsStructure ? new FieldInfoCollection() : FieldInfoCollection.Empty;
    }
  }
}