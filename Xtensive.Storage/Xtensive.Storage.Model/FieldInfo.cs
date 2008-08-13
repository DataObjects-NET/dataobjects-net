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
  public sealed class FieldInfo : MappingNode,
    ICloneable
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
    private ThreadSafeCached<int> cachedHashCode = ThreadSafeCached<int>.Create(new object());

    #region IsXxx properties

    /// <summary>
    /// Gets a value indicating whether this property is system.
    /// </summary>
    public bool IsSystem {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.System) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.System : Attributes & ~FieldAttributes.System;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is declared in <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsDeclared {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Declared) > 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value
          ? (Attributes | FieldAttributes.Declared) & ~FieldAttributes.Inherited
          : (Attributes & ~FieldAttributes.Declared) | FieldAttributes.Inherited;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inherited from parent <see cref="TypeInfo"/> instance.
    /// </summary>
    public bool IsInherited {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Inherited) > 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value
          ? (Attributes | FieldAttributes.Inherited) & ~FieldAttributes.Declared
          : (Attributes & ~FieldAttributes.Inherited) | FieldAttributes.Declared;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is contained by primary key.
    /// </summary>
    public bool IsPrimaryKey {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.PrimaryKey) != 0; }
      [DebuggerStepThrough]
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
    public bool IsNested {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Nested) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.Nested : Attributes & ~FieldAttributes.Nested;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property explicitly implemented.
    /// </summary>
    public bool IsExplicit {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Explicit) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.Explicit : Attributes & ~FieldAttributes.Explicit;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property implements property of one or more interfaces.
    /// </summary>
    public bool IsInterfaceImplementation {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.InterfaceImplementation) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        attributes = value ? Attributes | FieldAttributes.InterfaceImplementation : Attributes & ~FieldAttributes.InterfaceImplementation;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is primitive field.
    /// </summary>
    public bool IsPrimitive {
      [DebuggerStepThrough]
      get { return !IsStructure && !IsEntity && !IsEntitySet; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to Entity.
    /// </summary>
    public bool IsEntity {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Entity) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is structure field.
    /// </summary>
    public bool IsStructure {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Structure) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to EntitySet.
    /// </summary>
    public bool IsEntitySet {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.EntitySet) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is nullable.
    /// </summary>
    public bool IsNullable {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Nullable) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property will be loaded on demand.
    /// </summary>
    public bool IsLazyLoad {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.LazyLoad) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsTranslatable {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Translatable) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsCollatable {
      [DebuggerStepThrough]
      get { return (attributes & FieldAttributes.Collatable) != 0; }
    }

    #endregion

    /// <summary>
    /// Gets or sets the type of the value of this instance.
    /// </summary>
    public Type ValueType {
      [DebuggerStepThrough]
      get { return valueType; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        valueType = value;
      }
    }

    /// <summary>
    /// Gets or sets the length of the property.
    /// </summary>
    public int? Length {
      [DebuggerStepThrough]
      get { return length; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        length = value;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public FieldAttributes Attributes {
      [DebuggerStepThrough]
      get { return attributes; }
    }

    /// <summary>
    /// Gets <see cref="MappingInfo"/> for current field.
    /// </summary>
    public Segment<int> MappingInfo {
      [DebuggerStepThrough]
      get { return mappingInfo; }
    }

    /// <summary>
    /// Gets the underlying system property.
    /// </summary>
    public PropertyInfo UnderlyingProperty {
      [DebuggerStepThrough]
      get { return underlyingProperty; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        underlyingProperty = value;
      }
    }

    /// <summary>
    /// Gets or sets the parent field for nested fields.
    /// </summary>
    /// <remarks>
    /// For not nested fields return value is <see langword="null"/>.
    /// </remarks>
    public FieldInfo Parent {
      [DebuggerStepThrough]
      get { return parent; }
      [DebuggerStepThrough]
      set {
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
    /// Gets the type that was used to obtain this instance.
    /// </summary>
    public TypeInfo ReflectedType {
      [DebuggerStepThrough]
      get { return reflectedType; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        reflectedType = value;
      }
    }

    /// <summary>
    /// Gets the type where the field is declared.
    /// </summary>
    public TypeInfo DeclaringType {
      [DebuggerStepThrough]
      get { return declaringType; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        declaringType = value;
      }
    }

    /// <summary>
    /// Gets the nested fields.
    /// </summary>
    public FieldInfoCollection Fields {
      [DebuggerStepThrough]
      get { return fields; }
    }

    /// <summary>
    /// Gets or sets the column associated with this instance.
    /// </summary>
    public ColumnInfo Column {
      [DebuggerStepThrough]
      get { return column; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <summary>
    /// Gets or sets the field association.
    /// </summary>
    public AssociationInfo Association {
      [DebuggerStepThrough]
      get { return association; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        association = value;
      }
    }

    /// <summary>
    /// Gets or sets field <see cref="CultureInfo"/> info.
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

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(FieldInfo obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return 
        obj.declaringType==declaringType && 
          obj.valueType==valueType && 
            obj.Name==Name;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
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
        return cachedHashCode.GetValue(
          _this =>
            ((_this.declaringType.GetHashCode() * 397) ^ 
              _this.valueType.GetHashCode() * 631) ^ 
                _this.Name.GetHashCode(),
          this);
      }
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

    #endregion


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