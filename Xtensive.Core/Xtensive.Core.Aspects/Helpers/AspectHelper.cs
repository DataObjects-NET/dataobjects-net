// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

using System;
using System.Reflection;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Reflection;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Helps to validate common aspect application errors.
  /// </summary>
  public static class AspectHelper
  {
    #region FormatXxx methods

    /// <summary>
    /// Formats the type.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>String representation of the type.</returns>
    public static string FormatType(Type type)
    {
      if (type==null)
        return string.Empty;
      else
        return type.GetShortName();
    }

    /// <summary>
    /// Formats the method.
    /// </summary>
    /// <param name="type">The type where member is declared.
    /// <see langword="null"/> means no type name must be included into the format string.</param>
    /// <param name="returnType">Type of the constructed value.</param>
    /// <param name="parameterTypes">The method parameter types.</param>
    /// <returns>String representation of the member.</returns>
    public static string FormatConstructor(Type type, Type returnType, params Type[] parameterTypes)
    {
      return FormatMethod(type, returnType, WellKnown.CtorName, parameterTypes);
    }

    /// <summary>
    /// Formats the method.
    /// </summary>
    /// <param name="type">The type where member is declared.
    /// <see langword="null"/> means no type name must be included into the format string.</param>
    /// <param name="returnType">Type of the return value.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="parameterTypes">The method parameter types.</param>
    /// <returns>String representation of the member.</returns>
    public static string FormatMethod(Type type, Type returnType, string methodName, params Type[] parameterTypes)
    {
      string namePrefix = type==null ?
        string.Empty :
        (FormatType(type) + ".");
      
      return String.Format(Strings.MethodFormat,
        FormatType(returnType),
        namePrefix + methodName,
        parameterTypes.Select(p => FormatType(p)).ToCommaDelimitedString());
    }

    /// <summary>
    /// Formats the member (field or property).
    /// </summary>
    /// <param name="type">The type where member is declared.
    /// <see langword="null"/> means no type name must be included into the format string.</param>
    /// <param name="returnType">Type of the member.</param>
    /// <param name="name">Name of the member.</param>
    /// <returns>String representation of the member.</returns>
    public static string FormatMember(Type type, Type returnType, string name)
    {
      string namePrefix = type==null ?
        string.Empty :
        (FormatType(type) + ".");
      
      return String.Format(Strings.MemberFormat,
        FormatType(returnType),
        namePrefix + name);
    }

    /// <summary>
    /// Formats the member name.
    /// </summary>
    /// <param name="type">The type where member is declared.
    /// <see langword="null" /> means no type name must be included into the format string.</param>
    /// <param name="member">The member to format.</param>
    /// <returns>String representation of the member.</returns>
    public static string FormatMember(Type type, MemberInfo member)
    {
      var ti = member as Type;
      var mi = member as MethodInfo;
      var ci = member as ConstructorInfo;
      var fi = member as FieldInfo;
      var pi = member as PropertyInfo;
      if (ti!=null)
        return FormatType(type);
      if (ci!=null)
        return FormatConstructor(type, ci.DeclaringType, ci.GetParameters().Select(p => p.ParameterType).ToArray());
      if (mi!=null)
        return FormatMethod(type, mi.ReturnType, mi.Name, mi.GetParameters().Select(p => p.ParameterType).ToArray());
      if (fi!=null)
        return FormatMember(type, fi.FieldType, member.Name);
      if (pi!=null)
        return FormatMember(type, pi.PropertyType, member.Name);
      return FormatMember(type, typeof(void), member.Name);
    }

    #endregion

    #region GetStandardMessage method

    /// <summary>
    /// Gets the standard localized message.
    /// </summary>
    /// <param name="messageType">Type of the message to get.</param>
    /// <returns>Standard localized message.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageType"/> is out of range.</exception>
    public static string GetStandardMessage(AspectMessageType messageType)
    {
      switch (messageType) {
      case AspectMessageType.AspectPossiblyMissapplied:
        return Strings.AspectExPossiblyMissapplied;
      case AspectMessageType.AspectRequiresToBe:
        return Strings.AspectExRequiresToBe;
      case AspectMessageType.AspectRequiresToHave:
        return Strings.AspectExRequiresToHave;
      case AspectMessageType.AspectMustBeSingle:
        return Strings.AspectExMustBeSingle;
      case AspectMessageType.AutoProperty:
        return Strings.AutoProperty;
      case AspectMessageType.PropertyAccessor:
        return Strings.PropertyAccessor;
      case AspectMessageType.Getter:
        return Strings.Getter;
      case AspectMessageType.Setter:
        return Strings.Setter;
      case AspectMessageType.Public:
        return Strings.Public;
      case AspectMessageType.NonPublic:
        return Strings.NonPublic;
      case AspectMessageType.Not:
        return Strings.Not;
      default:
        throw new ArgumentOutOfRangeException("messageType");
      }
    }

    #endregion

    #region AddStandardRequirements method

    /// <summary>
    /// Adds the standard requirements to the specified <paramref name="requirements"/>.
    /// </summary>
    /// <param name="requirements">The requirements to modify.</param>
    public static void AddStandardRequirements(PostSharpRequirements requirements)
    {
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
    }

    #endregion

    /// <summary>
    /// Validates the type of the member.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="member">The member to validate the type of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, member type 
    /// must contain <paramref name="memberTypes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="memberTypes">Expected (or not) type(s) of the member.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateMemberType(Attribute aspect, SeverityType severityType, 
      MemberInfo member, bool containsFlags, MemberTypes memberTypes)
    {
      if (((member.MemberType & memberTypes)!=0) != containsFlags) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToBe,
          FormatType(aspect.GetType()),
          FormatMember(member.DeclaringType, member),
          containsFlags ? string.Empty : Strings.Not,
          memberTypes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates presence of the attribute.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="member">The member to validate the presence of attribute on.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, member type 
    /// must have <typeparamref name="TAttribute"/> applied;
    /// otherwise, it must not have it.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static TAttribute ValidateMemberAttribute<TAttribute>(Attribute aspect, SeverityType severityType, 
      MemberInfo member, bool mustHave, bool inherit)
      where TAttribute: Attribute
    {
      TAttribute attribute = member.GetAttribute<TAttribute>(inherit);
      if ((attribute!=null) != mustHave) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToBe,
          FormatType(aspect.GetType()),
          FormatMember(member.DeclaringType, member),
          mustHave ? string.Empty : Strings.Not,
          FormatType(typeof(TAttribute)));
      }
      return attribute;
    }

    /// <summary>
    /// Validates the implementation of <paramref name="baseType"/> 
    /// (e.g. interface) on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="type">The type to validate for the implementation of <paramref name="baseType"/>.</param>
    /// <param name="mustImplement">If set to <see langword="true"/>, type 
    /// must implement <paramref name="baseType"/>;
    /// otherwise, it must not implement it.</param>
    /// <param name="baseType">The base type to validate the implementation of.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateBaseType(Attribute aspect, SeverityType severityType, 
      Type type, bool mustImplement, Type baseType)
    {
      if ((baseType.IsAssignableFrom(type)) != mustImplement) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToImplement,
          FormatType(aspect.GetType()), 
          FormatType(type),
          mustImplement ? string.Empty : Strings.Not,
          FormatType(baseType));
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the attributes of the field.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="field">The field to validate the attributes of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, field attributes
    /// must contain <paramref name="fieldAttributes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="fieldAttributes">Expected (or not) attributes of the field.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateFieldAttributes(Attribute aspect, SeverityType severityType, 
      FieldInfo field, bool containsFlags, FieldAttributes fieldAttributes)
    {
      if (((field.Attributes & fieldAttributes)!=0) != containsFlags) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToBe,
          FormatType(aspect.GetType()),
          FormatMember(field.DeclaringType, field),
          containsFlags ? string.Empty : Strings.Not,
          fieldAttributes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the accessor of the property.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="property">The property to validate the accessor of.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, property 
    /// must contain setter or getter;
    /// otherwise, it must not contain it.</param>
    /// <param name="nonPublic">Indicates whether expected accessor must be non-public or not.
    /// <see langword="null" /> means this does not matter.</param>
    /// <param name="setter">If <see langword="true" />, property setter will be checked;
    /// otherwise, getter.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidatePropertyAccessor(Attribute aspect, SeverityType severityType, 
      PropertyInfo property, bool mustHave, bool? nonPublic, bool setter)
    {
      MethodInfo accessor = null;
      string sVisibility  = string.Empty;
      string sAccessor    = setter ? Strings.Setter : Strings.Getter;
      if (nonPublic.HasValue) {
        accessor = setter ? 
          property.GetSetMethod(nonPublic.Value) : 
          property.GetGetMethod(nonPublic.Value);
        sVisibility = (nonPublic.Value ? Strings.NonPublic : Strings.Public) + " ";
      }
      else {
        accessor = setter ? 
          property.GetSetMethod() : 
          property.GetGetMethod();
      }
      if ((accessor!=null) != mustHave) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToHave,
          FormatType(aspect.GetType()),
          FormatMember(property.DeclaringType, property),
          mustHave ? string.Empty : Strings.Not,
          sVisibility+sAccessor);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the attributes of the method.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="method">The method to validate the attributes of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, method attributes
    /// must contain <paramref name="methodAttributes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="methodAttributes">Expected (or not) attributes of the method.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateMethodAttributes(Attribute aspect, SeverityType severityType, 
      MethodBase method, bool containsFlags, MethodAttributes methodAttributes)
    {
      if (((method.Attributes & methodAttributes)!=0) != containsFlags) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToBe,
          FormatType(aspect.GetType()),
          FormatMember(method.DeclaringType, method),
          containsFlags ? string.Empty : Strings.Not,
          methodAttributes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the presence of specified method on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="type">The type to get the method of.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, type
    /// must have specified method;
    /// otherwise, it must not have it.</param>
    /// <param name="bindingFlags">Binding flags.</param>
    /// <param name="returnType">The return type of the method.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameterTypes">The types of method arguments.</param>
    /// <param name="method">The found method, or <see langword="null" /> if specified method was not found.</param>
    /// <returns>
    /// <see langword="true" /> if validation has passed; otherwise, <see langword="false" />.    
    /// </returns>
    public static bool ValidateMethod(Attribute aspect, SeverityType severityType, 
      Type type, bool mustHave, BindingFlags bindingFlags, Type returnType, string name, 
      Type[] parameterTypes, out MethodInfo method)
    {
      method = null;
      try {
        method = type.GetMethod(name, bindingFlags, null, parameterTypes, null);
      }
      catch (NullReferenceException) { }
      catch (ArgumentNullException) { }
      catch (AmbiguousMatchException) { }

      if (method!=null && method.ReturnType!=returnType)
        method = null;      
      
      if ((method!=null) != mustHave) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToHave,
          FormatType(aspect.GetType()), 
          FormatType(type), 
          mustHave ? string.Empty : Strings.Not,
          FormatMethod(null, returnType, name, parameterTypes));

        return false;
      }
      return true;      
    }

    /// <summary>
    /// Validates the method of <see cref="IContextBound{TContext}"/> object.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="aspect">The aspect.</param>
    /// <param name="method">The method.</param>
    /// <returns>
    /// <see langword="true" /> if validation has passed; otherwise, <see langword="false" />.    
    /// </returns>
    public static bool ValidateContextBoundMethod<TContext>(Attribute aspect, MethodBase method)
      where TContext : class, IContext
    {
      foreach (var attribute in method.GetAttributes<SuppressActivationAttribute>(false))
        if (attribute.ContextType == typeof(TContext))
          return false;

      if (!ValidateMemberType(aspect, SeverityType.Error,
        method, false, MemberTypes.Constructor))
        return false;
      if (!ValidateMethodAttributes(aspect, SeverityType.Error,
        method, false, MethodAttributes.Static))
        return false;
      if (!ValidateMethodAttributes(aspect, SeverityType.Error,
        method, false, MethodAttributes.Abstract))
        return false;
      if (!ValidateBaseType(aspect, SeverityType.Error,
        method.DeclaringType, true, typeof(IContextBound<TContext>)))
        return false;

      return true;
    }

    /// <summary>
    /// Validates the presence of specified constructor on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="severityType">The severity of the message to write to <see cref="ErrorLog"/>.</param>
    /// <param name="type">The type to get the constructor of.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, type
    /// must have specified constructor;
    /// otherwise, it must not have it.</param>
    /// <param name="bindingFlags">Binding flags.</param>
    /// <param name="parameterTypes">The types of constructor arguments.</param>
    /// <param name="constructor">The found constructor, or <see langword="null" /> if specified method was not found.</param>
    /// <returns>
    /// <see langword="true" /> if validation has passed; otherwise, <see langword="false" />.    
    /// </returns>
    public static bool ValidateConstructor(Attribute aspect, SeverityType severityType, 
      Type type, bool mustHave, BindingFlags bindingFlags, Type[] parameterTypes, out ConstructorInfo constructor)
    {
      constructor = null;
      try {
        constructor = type.GetConstructor(
          bindingFlags | BindingFlags.CreateInstance,
          null,
          parameterTypes,
          null);
      }
      catch (NullReferenceException) { }
      catch (ArgumentNullException) { }
      catch (AmbiguousMatchException) { }
      
      if ((constructor!=null) != mustHave) {
        ErrorLog.Write(severityType, Strings.AspectExRequiresToHave,
          FormatType(aspect.GetType()), 
          FormatType(type), 
          mustHave ? string.Empty : Strings.Not,
          FormatConstructor(null, type, parameterTypes));

        return false;
      }
      return true;
    }
  }
}