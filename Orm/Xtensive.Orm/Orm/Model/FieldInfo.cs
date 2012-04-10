// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;

using Xtensive.Sorting;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a single field.
  /// </summary>
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public sealed class FieldInfo : MappingNode,
    ICloneable
  {
    /// <summary>
    /// "No <see cref="NoFieldId"/>" value (<see cref="NoFieldId"/> is unknown or undefined).
    /// Value is <see langword="0" />.
    /// </summary>
    public const int NoFieldId = 0;

    /// <summary>
    /// Minimal possible <see cref="FieldId"/> value.
    /// Value is <see langword="100" />.
    /// </summary>
    public const int MinFieldId = 1;

    private PropertyInfo                    underlyingProperty;
    private Type                            valueType;
    private int?                            length;
    private int?                            scale;
    private int?                            precision;
    private object                          defaultValue;
    private TypeInfo                        reflectedType;
    private TypeInfo                        declaringType;
    private FieldInfo                       parent;
    private ColumnInfo                      column;
    private NodeCollection<AssociationInfo> associations;
    private Type                            itemType;
    private string                          originalName;
    internal SegmentTransform               valueExtractor;
    private int                             adapterIndex = -1;
    private ColumnInfoCollection            columns;
    private int                             fieldId;
    private int?                            cachedHashCode;
    
    #region IsXxx properties

    /// <summary>
    /// Gets or sets the field identifier uniquely identifying the field
    /// in <see cref="TypeInfo.Fields"/> collection of <see cref="ReflectedType"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">Property is already initialized.</exception>
    public int FieldId {
      [DebuggerStepThrough]
      get { return fieldId; }
      set {
        if (fieldId != NoFieldId)
          throw Exceptions.AlreadyInitialized("FieldId");
        fieldId = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is system.
    /// </summary>
    public bool IsSystem {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.System) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        Attributes = value
          ? (Attributes | FieldAttributes.System)
          : (Attributes & ~FieldAttributes.System);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is not used within an entity version.
    /// </summary>
    public bool SkipVersion
    {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.SkipVersion) != 0; }
      [DebuggerStepThrough]
      set
      {
        this.EnsureNotLocked();
        Attributes = value
          ? (Attributes | FieldAttributes.SkipVersion)
          : (Attributes & ~FieldAttributes.SkipVersion);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property belongs to an entity version. Updated manually.
    /// </summary>
    public bool ManualVersion
    {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.ManualVersion) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property belongs to an entity version. Updated automatically.
    /// </summary>
    public bool AutoVersion
    {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.AutoVersion) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property contains type identifier.
    /// </summary>
    public bool IsTypeId {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.TypeId) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is type discriminator.
    /// </summary>
    public bool IsTypeDiscriminator {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.TypeDiscriminator) != 0; }
      set { Attributes = value ? Attributes | FieldAttributes.TypeDiscriminator : Attributes & ~FieldAttributes.TypeDiscriminator; }
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
        Attributes = value
          ? (Attributes | FieldAttributes.Declared) & ~FieldAttributes.Inherited
          : (Attributes & ~FieldAttributes.Declared) | FieldAttributes.Inherited;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is enum.
    /// </summary>
    public bool IsEnum { 
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Enum) > 0; }
      private set {
        this.EnsureNotLocked();
        Attributes = value
          ? (Attributes | FieldAttributes.Enum)
          : (Attributes & ~FieldAttributes.Enum);
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
        Attributes = value
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
        Attributes = value ? Attributes | FieldAttributes.PrimaryKey : Attributes & ~FieldAttributes.PrimaryKey;
        if (column != null)
          column.IsPrimaryKey = true;
        else
          foreach (FieldInfo childField in Fields)
            childField.IsPrimaryKey = true;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is nested.
    /// </summary>
    public bool IsNested {
      [DebuggerStepThrough]
      get { return Parent != null; }
    }

    /// <summary>
    /// Gets a value indicating whether this property explicitly implemented.
    /// </summary>
    public bool IsExplicit {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Explicit) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        Attributes = value ? Attributes | FieldAttributes.Explicit : Attributes & ~FieldAttributes.Explicit;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property implements property of one or more interfaces.
    /// </summary>
    public bool IsInterfaceImplementation {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.InterfaceImplementation) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        Attributes = value ? Attributes | FieldAttributes.InterfaceImplementation : Attributes & ~FieldAttributes.InterfaceImplementation;
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
      get { return (Attributes & FieldAttributes.Entity) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is structure field.
    /// </summary>
    public bool IsStructure {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Structure) != 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to EntitySet.
    /// </summary>
    public bool IsEntitySet {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.EntitySet) != 0; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is nullable.
    /// </summary>
    public bool IsNullable
    {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.Nullable) != 0; }
      [DebuggerStepThrough]
      private set {
        this.EnsureNotLocked();
        Attributes = value ? Attributes | FieldAttributes.Nullable : Attributes & ~FieldAttributes.Nullable;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property will be loaded on demand.
    /// </summary>
    public bool IsLazyLoad
    {
      [DebuggerStepThrough]
      get { return (Attributes & FieldAttributes.LazyLoad) != 0; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        Attributes = value ? Attributes | FieldAttributes.LazyLoad : Attributes & ~FieldAttributes.LazyLoad;
      }
    }

    #endregion

    /// <summary>
    /// Gets or sets original name of this instance.
    /// </summary>
    public string OriginalName
    {
      get { return originalName; }
      set
      {
        this.EnsureNotLocked();
        ValidateName(value);
        originalName = value;
      }
    }

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
        if (valueType.IsGenericType) {
          if (valueType.GetGenericTypeDefinition() == typeof (Nullable<>))
            IsEnum = Nullable.GetUnderlyingType(valueType).IsEnum;
        }
        else
          IsEnum = value.IsEnum;
      }
    }

    /// <summary>
    /// Gets or sets the item type for field that describes the EntitySet.
    /// </summary>
    public Type ItemType {
      [DebuggerStepThrough]
      get { return itemType; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        itemType = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximal length of the field.
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
    /// Gets or sets the scale of the field.
    /// </summary>
    public int? Scale {
      [DebuggerStepThrough]
      get { return scale; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        scale = value;
      }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    public int? Precision {
      [DebuggerStepThrough]
      get { return precision; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        precision = value;
      }
    }

    /// <summary>
    /// Gets or sets the default value for this field.
    /// <see langword="null" /> indicates default value is provided automatically.
    /// </summary>
    public object DefaultValue {
      [DebuggerStepThrough]
      get { return defaultValue; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        defaultValue = value;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public FieldAttributes Attributes { get; private set; }

    /// <summary>
    /// Gets <see cref="MappingInfo"/> for current field.
    /// </summary>
    public Segment<int> MappingInfo { get; private set; }

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
    /// Gets or sets the declaring field.
    /// </summary>
    public FieldInfo DeclaringField { get; private set; }

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
        if (value.IsEntity)
          IsNullable = value.IsNullable;
//        associations = value.associations;
        itemType = value.itemType;
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
    public FieldInfoCollection Fields { get; private set; }

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
    /// <param name="targetType"></param>
    public AssociationInfo GetAssociation(TypeInfo targetType)
    {
      if (associations.Count == 0)
        return null;

      if (associations.Count == 1)
        return associations[0];

      var ordered = IsLocked
        ? associations
        : associations.Reorder();

      return ordered.FirstOrDefault(
        a => a.TargetType.UnderlyingType.IsAssignableFrom(targetType.UnderlyingType));
    }

    public NodeCollection<AssociationInfo> Associations
    {
      get { return associations; }
    }

    /// <summary>
    /// Gets or sets field's adapter index.
    /// </summary>
    public int AdapterIndex {
      [DebuggerStepThrough]
      get { return adapterIndex; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        adapterIndex = value;
      }
    }

    /// <summary>
    /// Extracts the field value from the specified <see cref="Tuple"/>.
    /// </summary>
    /// <param name="tuple">The tuple to extract value from.</param>
    /// <returns><see cref="Tuple"/> instance with the extracted value.</returns>
    public Tuple ExtractValue (Tuple tuple)
    {
      return valueExtractor.Apply(TupleTransformType.TransformedTuple, tuple);
    }

    /// <summary>
    /// Gets field columns.
    /// </summary>
    public ColumnInfoCollection Columns {
      get {
        if (columns != null)
          return columns;

        var result = new ColumnInfoCollection(this, "Columns");
        GetColumns(result);
        return result;
      }
    }

    private void GetColumns(ColumnInfoCollection result)
    {
      if (Column != null)
        result.Add(Column);
      else
        if (!IsPrimitive)
          foreach (var item in Fields.Where(f => f.Column!=null).Select(f => f.Column))
            result.Add(item);
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      if (!recursive)
        return;
      Fields.UpdateState(true);
      if (column!=null) 
        column.UpdateState(true);
      columns = new ColumnInfoCollection(this, "Columns");
      GetColumns(columns);

      CreateMappingInfo();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Fields.Lock(true);
      if (column!=null) 
        column.Lock(true);
      if (associations.Count > 1) {
        var sorted = associations.Reorder();
        associations = new NodeCollection<AssociationInfo>(associations.Owner, associations.Name);
        associations.AddRange(sorted);
      }
      associations.Lock(false);
    }

    private void CreateMappingInfo()
    {
      if (column!=null) {
        if (reflectedType.IsStructure)
          MappingInfo = new Segment<int>(reflectedType.Columns.IndexOf(column), 1);
        else {
          var primaryIndex = reflectedType.Indexes.PrimaryIndex;
          var indexColumn = primaryIndex.Columns.Where(c => c.Name==column.Name).FirstOrDefault();
          if (indexColumn == null)
            throw new InvalidOperationException();
          MappingInfo = new Segment<int>(primaryIndex.Columns.IndexOf(indexColumn), 1);
        }
      }
      else 
        if (Fields.Count > 0)
          MappingInfo = new Segment<int>(
            Fields.First().MappingInfo.Offset, Fields.Sum(f => f.IsPrimitive ? f.MappingInfo.Length : 0));

      if (IsEntity || IsStructure) {
        valueExtractor = new SegmentTransform(
          false, reflectedType.TupleDescriptor, new Segment<int>(MappingInfo.Offset, MappingInfo.Length));
      }
    }

    #region Equals, GetHashCode methods

    /// <inheritdoc/>
    public bool Equals(FieldInfo obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      FieldInfo mappedField;
      return
        obj.declaringType == declaringType &&
        obj.valueType == valueType &&
        obj.Name == Name;
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
      if (cachedHashCode.HasValue)
        return cachedHashCode.Value;
      if (!IsLocked)
        return CalculateHashCode();
      lock (this) {
        if (cachedHashCode.HasValue)
          return cachedHashCode.Value;
        cachedHashCode = CalculateHashCode();
        return cachedHashCode.Value;
      }
    }

    private int CalculateHashCode()
    {
      unchecked {
       return 
         (declaringType.GetHashCode() * 397) ^ 
         (valueType.GetHashCode() * 631) ^
         Name.GetHashCode();
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
      var clone= new FieldInfo(declaringType, reflectedType, Attributes)
        {
          Name = Name, 
          OriginalName = OriginalName,
          MappingName = MappingName, 
          underlyingProperty = underlyingProperty, 
          valueType = valueType, 
          itemType = itemType,
          length = length, 
          scale = scale,
          precision = precision,
          defaultValue = defaultValue,
          DeclaringField = DeclaringField
        };
      clone.Associations.AddRange(associations);
      return clone;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="attributes">The attributes.</param>
    public FieldInfo(TypeInfo type, FieldAttributes attributes) : this(type, type, attributes)
    {
      Attributes |= FieldAttributes.Declared;
      DeclaringField = this;
    }

    private FieldInfo(TypeInfo declaringType, TypeInfo reflectedType, FieldAttributes attributes)
    {
      Attributes = attributes;
      this.declaringType = declaringType;
      this.reflectedType = reflectedType;
      Fields = IsEntity || IsStructure 
        ? new FieldInfoCollection(this, "Fields") 
        : FieldInfoCollection.Empty;
      associations = new NodeCollection<AssociationInfo>(this, "Associations");
    }
  }
}