// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.10

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Model;
using FieldAttributes=Xtensive.Orm.Model.FieldAttributes;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines a signle persistent field.
  /// </summary>
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public class FieldDef : MappingNode
  {
    private readonly PropertyInfo           underlyingProperty;
    private FieldAttributes                 attributes;
    private OnRemoveAction?                 onTargetRemove;
    private OnRemoveAction?                 onOwnerRemove;
    private string                          pairTo;
    private int?                            length;
    private int?                            scale;
    private int?                            precision;
    private object                          defaultValue;

    /// <summary>
    /// Gets or sets the maximal length of the field.
    /// </summary>
    public int? Length
    {
      get { return length; }
      set
      {
        if (value.HasValue)
          ArgumentValidator.EnsureArgumentIsGreaterThan(value.Value, 0, "Length");
        length = value;
      }
    }

    /// <summary>
    /// Gets or sets the scale of the field.
    /// </summary>
    public int? Scale
    {
      get { return scale; }
      set
      {
        if (value.HasValue)
          ArgumentValidator.EnsureArgumentIsGreaterThan(value.Value, -1, "Scale");
        scale = value;
      }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    public int? Precision
    {
      get { return precision; }
      set
      {
        if (value.HasValue)
          ArgumentValidator.EnsureArgumentIsGreaterThan(value.Value, 0, "Precision");
        precision = value;
      }
    }

    /// <summary>
    /// Gets or sets the default value for this field.
    /// <see langword="null" /> indicates default value is provided automatically.
    /// </summary>
    public object DefaultValue
    {
      get { return defaultValue; }
      set { defaultValue = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance should be loaded on demand.
    /// </summary>
    public bool IsLazyLoad
    {
      get { return (attributes & FieldAttributes.LazyLoad) != 0; }
      set { attributes = value ? attributes | FieldAttributes.LazyLoad : attributes & ~FieldAttributes.LazyLoad; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is nullable.
    /// </summary>
    public bool IsNullable
    {
      get { return (attributes & FieldAttributes.Nullable) != 0; }
      internal set
      {
        Validator.EnsureIsNullable(ValueType);
        attributes = value ? attributes | FieldAttributes.Nullable : attributes & ~FieldAttributes.Nullable;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this property is structure field.
    /// </summary>
    public bool IsStructure
    {
      get { return (attributes & FieldAttributes.Structure) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.Structure : attributes & ~FieldAttributes.Structure; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is reference to EntitySet.
    /// </summary>
    public bool IsEntitySet
    {
      get { return (attributes & FieldAttributes.EntitySet) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.EntitySet : attributes & ~FieldAttributes.EntitySet; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is primitive field.
    /// </summary>
    public bool IsPrimitive
    {
      get { return !IsStructure && !IsEntity && !IsEntitySet; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is a reference to Entity.
    /// </summary>
    public bool IsEntity
    {
      get { return (attributes & FieldAttributes.Entity) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.Entity : attributes & ~FieldAttributes.Entity; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is system field.
    /// </summary>
    public bool IsSystem
    {
      get { return (attributes & FieldAttributes.System) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.System : attributes & ~FieldAttributes.System; }
    }

    /// <summary>
    /// Gets a value indicating whether this property contains information about Type identifier.
    /// </summary>
    public bool IsTypeId
    {
      get { return (attributes & FieldAttributes.TypeId) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.TypeId | FieldAttributes.System : attributes & ~FieldAttributes.TypeId; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is used as custom type discriminator.
    /// </summary>
    public bool IsTypeDiscriminator
    {
      get { return (attributes & FieldAttributes.TypeDiscriminator) != 0; }
      set { attributes = value ? attributes | FieldAttributes.TypeDiscriminator : attributes & ~FieldAttributes.TypeDiscriminator; }
    }

    /// <summary>
    /// Gets a value indicating whether this property is indexed.
    /// </summary>
    public bool IsIndexed
    {
      get { return (attributes & FieldAttributes.Indexed) != 0; }
      internal set { attributes = value ? attributes | FieldAttributes.Indexed : attributes & ~FieldAttributes.Indexed; }
    }

    /// <summary>
    /// Gets the underlying system property.
    /// </summary>
    public PropertyInfo UnderlyingProperty
    {
      get { return underlyingProperty; }
    }

    /// <summary>
    /// Gets or sets the type of the value of this instance.
    /// </summary>
    public Type ValueType { get; private set; }

    /// <summary>
    /// Gets or sets the item type for field that describes the EntitySet.
    /// </summary>
    public Type ItemType { get; private set; }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public FieldAttributes Attributes
    {
      get { return attributes; }
      internal set { attributes = value; }
    }

    /// <summary>
    /// Gets or sets the <see cref="OnRemoveAction"/> action that will be executed on referenced Entity removal.
    /// </summary>
    /// <exception cref="InvalidOperationException">Field is not reference to entity, nor <see cref="EntitySet{TItem}"/>.</exception>
    public OnRemoveAction? OnTargetRemove
    {
      get { return onTargetRemove; }
      set
      {
        if (!(IsEntity || IsEntitySet))
          throw new InvalidOperationException(String.Format(Resources.Strings.ExFieldXIsNotAnEntityReferenceNorEntitySet, this));
        onTargetRemove = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="OnRemoveAction"/> action that will be executed with referenced Entity on field owner removal.
    /// </summary>
    /// <exception cref="InvalidOperationException">Field is not reference to entity, nor <see cref="EntitySet{TItem}"/>.</exception>
    public OnRemoveAction? OnOwnerRemove
    {
      get { return onOwnerRemove; }
      set
      {
        if (!(IsEntity || IsEntitySet))
          throw new InvalidOperationException(String.Format(Resources.Strings.ExFieldXIsNotAnEntityReferenceNorEntitySet, this));
        onOwnerRemove = value;
      }
    }

    /// <summary>
    /// Gets or sets the name of the paired field.
    /// </summary>
    /// <exception cref="InvalidOperationException">Field is not reference to entity.</exception>
    public string PairTo
    {
      get { return pairTo; }
      set
      {
        if (!(IsEntity || IsEntitySet))
          throw new InvalidOperationException(String.Format(Resources.Strings.ExFieldXIsNotAnEntityReferenceNorEntitySet, this));
        pairTo = value;
      }
    }

    /// <summary>
    /// Performs additional custom processes before setting new name to this instance.
    /// </summary>
    /// <param name="newName">The new name of this instance.</param>
    protected override void ValidateName(string newName)
    {
      base.ValidateName(newName);
      Validator.ValidateName(newName, ValidationRule.Field);
    }


    // Constructors

    internal FieldDef(PropertyInfo property)
      : this(property.PropertyType)
    {
      underlyingProperty = property;
    }

    internal FieldDef(Type valueType)
    {
      IsStructure = valueType.IsSubclassOf(typeof(Structure)) || valueType == typeof(Structure);
      IsEntity = typeof(IEntity).IsAssignableFrom(valueType);
      if ((valueType.IsClass || valueType.IsInterface) && !IsStructure)
        attributes |= FieldAttributes.Nullable;
      ValueType = valueType;

      // Nullable<T>
      if (valueType.IsGenericType && valueType.GetGenericTypeDefinition()==typeof (Nullable<>)) {
        attributes |= FieldAttributes.Nullable;
        return;
      }

      // EntitySet<TEntity>
      var genericTypeDefinition = valueType.GetGenericType(typeof (EntitySet<>));
      if (genericTypeDefinition != null) {
        IsEntitySet = true;
        ItemType = genericTypeDefinition.GetGenericArguments()[0];
      }
    }
  }
}