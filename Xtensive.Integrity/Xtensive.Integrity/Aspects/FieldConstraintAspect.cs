// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects
{
  /// <summary>
  /// Base class for all field-constraints attributes.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
  [MulticastAttributeUsage(MulticastTargets.Property)]
  public abstract class FieldConstraintAspect : CompoundAspect
  {
    /// <summary>
    /// Gets the validated property.
    /// </summary>    
    protected PropertyInfo Property { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="ValidationMode"/> to be used on setting property value.
    /// </summary>
    /// <value>The mode.</value>
    public ValidationMode Mode { get; set; }

    /// <summary>
    /// Validates the specified field value.
    /// </summary>
    /// <param name="target">The validation target.</param>
    /// <param name="value">The value to validate.</param>
    public abstract void ValidateValue(IValidatable target, object value);    

    /// <summary>
    /// Determines whether the specified value type is type supported.
    /// </summary>
    /// <param name="type">The type.</param>
    public abstract bool IsTypeSupported(Type type);

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object element)
    {
      Property = (PropertyInfo) element;
    
      return 
        typeof (IValidatable).IsAssignableFrom(Property.DeclaringType) &&
        IsTypeSupported(Property.PropertyType);
    }

    /// <inheritdoc/>
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      Property = (PropertyInfo) element;      

      collection.AddAspect(
        Property.GetSetMethod(true),
        new FieldSetterConstraintAspect(this));          
    }

    internal void RuntimeInitialize()
    {
      ConstraintsRegistry.RegisterConstraint(Property.ReflectedType, this);
    }

    /// <summary> 
    /// Called when property value is to be set.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="value">The value.</param>
    internal void OnSetValue(IValidatable target, object value)
    {
      target.Validate(Mode);
    }

    internal void OnValidate(IValidatable target)
    {
      ValidateValue(target, Property.GetGetMethod().Invoke(target, new object[] {} ));
    }

    // Constructor
    
    protected FieldConstraintAspect()
    {
      Mode = ValidationMode.Default;
    }
  }
}