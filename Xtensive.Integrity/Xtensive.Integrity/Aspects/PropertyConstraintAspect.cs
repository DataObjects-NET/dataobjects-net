// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
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
  public abstract class PropertyConstraintAspect : CompoundAspect
  {
    private ThreadSafeCached<Func<IValidationAware, object>> getter =
      ThreadSafeCached<Func<IValidationAware, object>>.Create(new object());

    /// <summary>
    /// Gets the validated property.
    /// </summary>    
    protected PropertyInfo Property { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="ValidationMode"/> to be used on setting property value.
    /// </summary>
    public ValidationMode Mode { get; set; }

    /// <summary>
    /// Validates the <paramref name="target"/> against this constraint.
    /// </summary>
    /// <param name="target">The validation target.</param>
    public void Check(IValidationAware target)
    {
      CheckValue(target, GetPropertyValue(target));
    }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <param name="target">The target to get the property value of.</param>
    /// <returns>Property value.</returns>
    protected object GetPropertyValue(IValidationAware target)
    {
      return 
        getter.GetValue(
          _this => _this
            .GetType()
            .GetMethod("GetPropertyGetter", 
              BindingFlags.NonPublic | BindingFlags.Instance, null, ArrayUtils<Type>.EmptyArray, null)
            .GetGenericMethodDefinition()
            .MakeGenericMethod(new[] {_this.Property.PropertyType})
            .Invoke(_this, null)
            as Func<IValidationAware, object>, 
          this)
        .Invoke(target);
    }

    /// <summary>
    /// Validates the specified field value.
    /// </summary>
    /// <param name="target">The validation target.</param>
    /// <param name="value">The value to validate.</param>
    public abstract void CheckValue(IValidationAware target, object value);    

    /// <summary>
    /// Determines whether the specified <paramref name="valueType"/> 
    /// is supported by this constraint.
    /// </summary>
    /// <param name="valueType">The value type to check.</param>
    public abstract bool IsSupported(Type valueType);

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object element)
    {
      Property = (PropertyInfo) element;

      if (Property.GetSetMethod()==null)
        throw new Exception(Resources.Strings.FieldConstraintCanNotBeAppliedToReadOnlyProperty);

      if (!typeof (IValidationAware).IsAssignableFrom(Property.DeclaringType))
        throw new Exception(
          string.Format(Resources.Strings.XInterfaceShouldBeImplementedToUseFieldConstraints, typeof(IValidationAware).Name));
    
      if (!IsSupported(Property.PropertyType))        
        throw new Exception(
          string.Format(Resources.Strings.XDoesNotSupportYValueType, GetType().Name, Property.PropertyType.Name));
      
      return true;
    }

    /// <inheritdoc/>
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      collection.AddAspect(Property.GetSetMethod(true), new ImplementPropertyConstraintAspect(this));          
    }

    #region Private \ internal methods

    internal void OnRuntimeInitialize()
    {
      ConstraintRegistry.RegisterConstraint(Property.ReflectedType, this);
    }

    internal void OnSetValue(IValidationAware target, object value)
    {
      if (target.Context.IsConsistent)
        CheckValue(target, value);
      else
        target.Validate(_target => Check(_target), Mode);
    }

// ReSharper disable UnusedPrivateMember
    private Func<IValidationAware, object> GetPropertyGetter<T>()
// ReSharper restore UnusedPrivateMember
    {
      var typedGetter = DelegateHelper.CreateGetMemberDelegate<IValidationAware, T>(Property.Name);
      return va => (object) typedGetter;
    }

    #endregion


    // Constructor
    
    protected PropertyConstraintAspect()
    {
      Mode = ValidationMode.Default;
    }
  }
}