// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Aspects
{
  /// <summary>
  /// Base class for all property-constraints attributes.
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
    /// Gets or sets the message of exception to show if propery value is invalid.
    /// </summary>
    public string Message { get; set;}

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
            .MakeGenericMethod(new[] {_this.Property.DeclaringType, _this.Property.PropertyType})
            .Invoke(_this, null)
            as Func<IValidationAware, object>, 
          this)
        .Invoke(target);
    }


    /// <summary>
    /// Determines whether the specified <paramref name="valueType"/> 
    /// is supported by this constraint.
    /// </summary>
    /// <param name="valueType">The value type to check.</param>
    public abstract bool IsSupported(Type valueType);

    /// <summary>
    /// Method called at compile-time by the weaver to validate the application of this
    /// custom attribute on a specific target element.
    /// </summary>
    /// <param name="element">Element (<see cref="T:System.Reflection.MethodBase"/>, <see cref="T:System.Reflection.FieldInfo"/>
    /// or <see cref="T:System.Type"/> on which this instance is applied.</param>
    /// <returns>
    ///   <b>true</b> in case of success, <b>false</b> in case of error.
    /// </returns>
    /// <inheritdoc/>
    public override sealed bool CompileTimeValidate(object element)
    {
      Property = (PropertyInfo) element;

      if (Property.GetSetMethod()==null) {
        ErrorLog.Write(SeverityType.Error, Strings.FieldConstraintCanNotBeAppliedToReadOnlyProperty);
        return false;
      }

      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error, Property.DeclaringType, true, typeof (IValidationAware)))
        return false;

      if (!IsSupported(Property.PropertyType)) { 
        ErrorLog.Write(SeverityType.Error, 
          Strings.XDoesNotSupportYValueType, GetType().Name, Property.PropertyType.Name);
        return false;
      }

      // Specifiec constraint properties validation.
      try {
        ValidateConstraintProperties();
      }
      catch (Exception exception) {
        ErrorLog.Write(SeverityType.Error,
          "Appling [{0}] to property '{1}' failed. {2}",
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(Property.DeclaringType, Property),
          exception.Message);
        return false;
      }

      return true;
    }

    /// <summary>
    /// Validates the constraint properties.
    /// </summary>
    protected virtual void ValidateConstraintProperties()
    {
    }

    /// <inheritdoc/>
    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      collection.AddAspect(Property.GetSetMethod(true), 
        new ImplementPropertyConstraintAspect(this));
    }

    /// <summary>
    /// Determines whether the specified value is valid.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool IsValid(object value);

    private bool isInitialized = false;

    /// <summary>
    /// Initializes the constraint.
    /// </summary>
    protected virtual void Initialize()
    {
    }

    /// <summary>
    /// Checks the specified value, if not throws <see cref="ConstraintViolationException"/>.
    /// </summary>
    /// <param name="target">The validation target.</param>
    /// <param name="value">The property value.</param>
    /// <exception cref="ConstraintViolationException">Value is not valid.</exception>
    public void CheckValue(IValidationAware target, object value)
    {
      if (!isInitialized) {
        ValidateConstraintProperties();
        if (string.IsNullOrEmpty(Message))
          Message = GetDefaultMessage();
        Initialize();
        isInitialized = true;
      }

      if (IsValid(value))
        return;

      string message = Message;
      var parameters = GetMessageParams()
        .AddOne(new KeyValuePair<string, string>("PropertyName", Property.Name))
        .AddOne(new KeyValuePair<string, string>("value", (value ?? "null").ToString()));

      foreach (var param in parameters) {
        message = message.Replace("{" + param.Key + "}", param.Value);
      }
      throw new ConstraintViolationException(message, target.GetType(), Property, value);
    }

    protected virtual IEnumerable<KeyValuePair<string, string>> GetMessageParams()
    {
      return Enumerable.Empty<KeyValuePair<string, string>>();
    }

    protected abstract string GetDefaultMessage();


    #region Private \ internal methods

    internal void OnRuntimeInitialize()
    {
      ConstraintRegistry.RegisterConstraint(Property.ReflectedType, this);
    }

    internal void OnSetValue(IValidationAware target, object value)
    {
      var context = target.Context;
      bool immediate = Mode==ValidationMode.Immediate || context==null || context.IsConsistent;
      if (immediate)
        CheckValue(target, value);
      else
        context.EnqueueValidate(target, _target => Check(_target));   
    }

// ReSharper disable UnusedPrivateMember
    protected Func<IValidationAware, object> GetPropertyGetter<TOwner, TProperty>()
// ReSharper restore UnusedPrivateMember
    {
      var typedGetter = DelegateHelper.CreateGetMemberDelegate<TOwner, TProperty>(Property.Name);
      return validationAware => (object) typedGetter.Invoke((TOwner) validationAware);
    }

    #endregion


    // Constructor
    
    protected PropertyConstraintAspect()
    {
      Mode = ValidationMode.Default;
    }
  }
}