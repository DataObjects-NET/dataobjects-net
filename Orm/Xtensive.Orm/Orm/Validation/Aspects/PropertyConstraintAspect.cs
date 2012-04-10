// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Internals;
using PostSharp.Extensibility;
using Xtensive.Aspects;
using Xtensive.Aspects.Helpers;
using Xtensive.Collections;

using Xtensive.Reflection;
using Xtensive.Orm.Validation;
using Xtensive.Threading;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Base class for all property-constraints attributes.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
  [MulticastAttributeUsage(MulticastTargets.Method, TargetMemberAttributes = MulticastAttributes.Instance, 
    PersistMetaData = true)]
  [ProvideAspectRole(StandardRoles.Validation)]
  [AspectRoleDependency(AspectDependencyAction.Commute, StandardRoles.Validation)]
  [AspectTypeDependency(AspectDependencyAction.Conflict, typeof(InconsistentRegionAttribute))]
  [AspectTypeDependency(AspectDependencyAction.Order, AspectDependencyPosition.After, typeof(ReplaceAutoProperty))]
  public abstract class PropertyConstraintAspect : OnMethodBoundaryAspect
  {
    private const string MessageResourceNamePropertyName = "MessageResourceName";
    private const string MessageResourceTypePropertyName = "MessageResourceType";
    private const string MessageParameterFormat = "{{{0}}}";
    private const string PropertyNameParameter = "PropertyName";
    private const string ValueParameter = "value";

    private ThreadSafeCached<Func<IValidationAware, object>> getter = ThreadSafeCached<Func<IValidationAware, object>>.Create(new object());

    /// <summary>
    /// Gets the validated property.
    /// </summary>    
    public PropertyInfo Property { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="ConstrainMode"/> to be used on setting property value.
    /// </summary>
    public ConstrainMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the message of exception to show if property value is invalid.
    /// </summary>
    /// <remarks>
    /// You use the <see cref="MessageResourceName"/> and <see cref="MessageResourceType"/> properties to provide localizable error messages. 
    /// To provide a non-localizable error message, use the <see cref="Message"/> property.
    /// </remarks>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the property name on the resource type that provides the localizable error message.
    /// </summary>
    /// <remarks>
    /// You use the <see cref="MessageResourceName"/> and <see cref="MessageResourceType"/> properties to provide localizable error messages. 
    /// To provide a non-localizable error message, use the <see cref="Message"/> property.
    /// </remarks>
    /// <seealso cref="MessageResourceType"/>
    public string MessageResourceName { get; set; }

    /// <summary>
    /// Gets or sets the resource type that provides the localizable error message.
    /// </summary>
    /// <remarks>
    /// You use the <see cref="MessageResourceName"/> and <see cref="MessageResourceType"/> properties to provide localizable error messages. 
    /// To provide a non-localizable error message, use the <see cref="Message"/> property.
    /// </remarks>
    /// <see cref="MessageResourceName"/>
    public Type MessageResourceType { get; set; }

    
    [MethodExecutionAdviceOptimization(MethodExecutionAdviceOptimizations.IgnoreAllEventArgsMembers & ~(MethodExecutionAdviceOptimizations.IgnoreGetInstance | MethodExecutionAdviceOptimizations.IgnoreGetArguments))]
    public sealed override void OnEntry(MethodExecutionArgs args)
    {
      var target = (IValidationAware) args.Instance;
      var context = target.Context;
      var immediate = Mode==ConstrainMode.OnSetValue || context==null || context.IsConsistent;
      if (immediate)
        CheckValue(target, args.Arguments[0]);
      else
        context.EnqueueValidate(target, Check);

    }

    
    public override sealed bool CompileTimeValidate(MethodBase target)
    {
      var methodInfo = target as MethodInfo;
      Property = methodInfo.GetProperty();
      var setMethod = Property.GetSetMethod();
      
      // Skip getters
      if (methodInfo != setMethod)
        return false;

      if (setMethod==null) {
        ErrorLog.Write(
          MessageLocation.Of(Property),
          SeverityType.Error,
          string.Format(Strings.AspectExFieldConstraintCanNotBeAppliedToReadOnlyPropertyX,
          AspectHelper.FormatMember(Property.DeclaringType, Property)));
        return false;
      }

      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error, Property.DeclaringType, true, typeof (IValidationAware)))
        return false;

      if (!IsSupported(Property.PropertyType)) { 
        ErrorLog.Write(
          MessageLocation.Of(Property),
          SeverityType.Error, 
          Strings.AspectExXDoesNotSupportYValueTypeLocationZ, 
          GetType().Name, 
          Property.PropertyType.Name, 
          AspectHelper.FormatMember(Property.DeclaringType, Property));
        return false;
      }

      // Specifiec constraint properties validation.
      try {
        ValidateSelf(true);
      }
      catch (Exception exception) {
        ErrorLog.Write(
          MessageLocation.Of(Property),
          SeverityType.Error,
          Strings.AspectExApplyingXToPropertyYFailedZ,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(Property.DeclaringType, Property),
          exception.Message);
        return false;
      }

      if (string.IsNullOrEmpty(MessageResourceName) ^ MessageResourceType==null)
        ErrorLog.Write(
          MessageLocation.Of(Property),
          SeverityType.Error,
          string.Format(Strings.AspectExXAndYPropertiesMustBeUsedTogetherLocationZ, 
          MessageResourceNamePropertyName, 
          MessageResourceTypePropertyName,
          AspectHelper.FormatMember(Property.DeclaringType, Property)));

      if (!string.IsNullOrEmpty(Message) && !string.IsNullOrEmpty(MessageResourceName))
        ErrorLog.Write(
          MessageLocation.Of(Property),
          SeverityType.Error, 
          Strings.AspectExBothLocalizableMessageResourceAndNotLocalizableMessageCanNotBeSpecifiedAtOnceLocationX,
          AspectHelper.FormatMember(Property.DeclaringType, Property));

      return true;
    }

    public override void RuntimeInitialize(MethodBase method)
    {
      OnRuntimeInitialize();
    }

    /// <summary>
    /// Determines whether the specified <paramref name="valueType"/> 
    /// is supported by this constraint.
    /// </summary>
    /// <param name="valueType">The value type to check.</param>
    public abstract bool IsSupported(Type valueType);

    /// <summary>
    /// Validates itself.
    /// </summary>
    /// <param name="compileTime">Indicates whether this method is invoked 
    /// in compile time or in runtime.</param>
    protected virtual void ValidateSelf(bool compileTime)
    {
    }

    /// <summary>
    /// Validates the <paramref name="target"/> against this constraint.
    /// </summary>
    /// <param name="target">The validation target.</param>
    public void Check(IValidationAware target)
    {
      CheckValue(target, GetPropertyValue(target));
    }

    #region Check value methods

    /// <summary>
    /// Validates the specified value. 
    /// Throws <see cref="ConstraintViolationException"/> on failure.
    /// </summary>
    /// <param name="target">The validation target.</param>
    /// <param name="value">The property value.</param>
    /// <exception cref="ConstraintViolationException">Value is not valid.</exception>
    public virtual void CheckValue(IValidationAware target, object value)
    {
      if (CheckValue(value))
        return;

      // We've got an error. Let's format its message.
      string message = Message;
      var parameters = new Dictionary<string, object> {
        { PropertyNameParameter, Property.Name }, 
        { ValueParameter, value }
      };
      AddCustomMessageParameters(parameters);
      foreach (var p in parameters)
        message = message.Replace(
          string.Format(MessageParameterFormat, p.Key), 
          p.Value==null ? Strings.Null : p.Value.ToString());

      throw new ConstraintViolationException(message, target.GetType(), Property, value);
    }

    /// <summary>
    /// Validates the specified value.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value is valid; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool CheckValue(object value);

    #endregion

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

    #region Message related methods

    /// <summary>
    /// Gets the default message.
    /// </summary>
    /// <returns>Default message.</returns>
    protected abstract string GetDefaultMessage();

    /// <summary>
    /// Adds custom message parameters.
    /// </summary>
    /// <param name="parameters">The parameters to add to.</param>
    protected virtual void AddCustomMessageParameters(Dictionary<string, object> parameters)
    {
    }

    #endregion

    #region Private \ internal methods

    internal void OnRuntimeInitialize()
    {
      ConstraintRegistry.RegisterConstraint(Property.ReflectedType, this);
      ValidateSelf(false);
      Initialize();

      // Getting message
      if (MessageResourceType!=null && !string.IsNullOrEmpty(MessageResourceName))
        Message = ResourceHelper.GetStringResource(MessageResourceType, MessageResourceName);
      if (string.IsNullOrEmpty(Message))
        Message = GetDefaultMessage();
    }

// ReSharper disable UnusedPrivateMember
    internal Func<IValidationAware, object> GetPropertyGetter<TOwner, TProperty>()
// ReSharper restore UnusedPrivateMember
    {
      var typedGetter = DelegateHelper.CreateGetMemberDelegate<TOwner, TProperty>(Property.Name);
      return validationAware => (object) typedGetter.Invoke((TOwner) validationAware);
    }

    #endregion

    /// <summary>
    /// Initializes this instance in runtime.
    /// </summary>
    protected virtual void Initialize()
    {      
    }


    // Constructor

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    protected PropertyConstraintAspect()
    {
      Mode = ConstrainMode.Default;
    }
  }
}