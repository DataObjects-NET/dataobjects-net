// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Model;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;

namespace Xtensive.Storage.Building.Definitions
{
  [DebuggerDisplay("{Name}; Attributes = {Attributes}")]
  [Serializable]
  public class FieldDef : MappingNode
  {
    private readonly PropertyInfo underlyingProperty;
    private FieldAttributes attributes;
    private ReferentialAction onRemove;
    private string pairTo;
    private int? length;

    /// <summary>
    /// Gets or sets a value indicating whether this instance should be loaded on demand.
    /// </summary>
    public bool LazyLoad
    {
      get { return (attributes & FieldAttributes.LazyLoad) != 0; }
      set { attributes = value ? attributes | FieldAttributes.LazyLoad : attributes & ~FieldAttributes.LazyLoad; }
    }

    /// <summary>
    /// Gets or sets the length of the field.
    /// </summary>
    public int? Length
    {
      get { return length; }
      set
      {
        if (value.HasValue)
          ArgumentValidator.EnsureArgumentIsInRange(value.Value, 1, Int32.MaxValue, "Length");
        length = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is nullable.
    /// </summary>
    public bool IsNullable
    {
      get { return (attributes & FieldAttributes.Nullable) != 0; }
      internal set
      {
        FieldBuilder.ValidateIsNullable(ValueType);
        attributes = value ? attributes | FieldAttributes.Nullable : attributes & ~FieldAttributes.Nullable;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsTranslatable
    {
      get { return (attributes & FieldAttributes.Translatable) != 0; }
      set { attributes = value ? attributes | FieldAttributes.Translatable : attributes & ~FieldAttributes.Translatable; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether property is translatable.
    /// </summary>
    public bool IsCollatable
    {
      get { return (attributes & FieldAttributes.Collatable) != 0; }
      set { attributes = value ? attributes | FieldAttributes.Collatable : attributes & ~FieldAttributes.Collatable; }
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
    }

    /// <summary>
    /// Gets or sets the referential action.
    /// </summary>
    /// <exception cref="InvalidOperationException">Field is not reference to entity.</exception>
    public ReferentialAction OnRemove
    {
      get { return onRemove; }
      set
      {
        if (!(IsEntity || IsEntitySet))
          throw new InvalidOperationException("Field is not an entity reference or entity set.");
        onRemove = value;
      }
    }

    /// <summary>
    /// Gets or sets the referential action.
    /// </summary>
    /// <exception cref="InvalidOperationException">Field is not reference to entity.</exception>
    public string PairTo
    {
      get { return pairTo; }
      set
      {
        if (IsPrimitive || IsStructure)
          throw new InvalidOperationException("Field is not an entity reference nor is entity set.");
        pairTo = value;
      }
    }

    /// <summary>
    /// Performs additional custom processes before setting new name to this instance.
    /// </summary>
    /// <param name="nameToValidate">The new name of this instance.</param>
    protected override void Validate(string nameToValidate)
    {
      base.Validate(nameToValidate);

      if (!Validator.IsNameValid(nameToValidate, ValidationRule.Field))
        throw new ArgumentOutOfRangeException(nameToValidate);
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
      IsEntity = valueType == typeof(Entity) || valueType.IsSubclassOf(typeof(Entity)) || typeof(IEntity).IsAssignableFrom(valueType);
      if (valueType.IsClass && !IsStructure)
        attributes |= FieldAttributes.Nullable;
      ValueType = valueType;
      if (valueType.IsGenericType) {
        Type genericType = valueType.GetGenericTypeDefinition();
        IsEntitySet = genericType.IsOfGenericType(typeof(EntitySet<>));
        if (IsEntitySet)
          ItemType = valueType.GetGenericArguments()[0];
        if (genericType == typeof(Nullable<>)) {
          ValueType = Nullable.GetUnderlyingType(valueType);
          attributes |= FieldAttributes.Nullable;
        }
      }
      LazyLoad = false;
    }
  }
}