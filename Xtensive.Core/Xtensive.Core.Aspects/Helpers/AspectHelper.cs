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
        (type.GetShortName() + ".");
      
      return String.Format(Strings.MethodFormat,
        returnType.GetShortName(),
        namePrefix + methodName,
        parameterTypes.Select(p => p.GetShortName()).ToCommaDelimitedString());
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
        (type.GetShortName() + ".");
      
      return String.Format(Strings.MemberFormat,
        returnType.GetShortName(),
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
      var mi = member as MethodInfo;
      var ci = member as ConstructorInfo;
      var fi = member as FieldInfo;
      var pi = member as PropertyInfo;
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

    /// <summary>
    /// Validates the implementation of <paramref name="baseType"/> 
    /// (e.g. interface) on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="type">The type to validate for the implementation of <paramref name="baseType"/>.</param>
    /// <param name="mustImplement">If set to <see langword="true"/>, type 
    /// must implement <paramref name="baseType"/>;
    /// otherwise, it must not implement it.</param>
    /// <param name="baseType">The base type to validate the implementation of.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateBaseType(Attribute aspect, Type type, bool mustImplement, Type baseType)
    {
      if ((baseType.IsAssignableFrom(type)) != mustImplement) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToImplement,
          aspect.GetType().GetShortName(), 
          type.GetShortName(),
          mustImplement ? string.Empty : Strings.Not,
          baseType.GetShortName());
        return false;
      }
      return true;
    }


    /// <summary>
    /// Validates the type of the member.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="member">The member to validate the type of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, member type 
    /// must contain <paramref name="memberTypes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="memberTypes">Expected (or not) type(s) of the member.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateMemberType(Attribute aspect, MemberInfo member, bool containsFlags, MemberTypes memberTypes)
    {
      ErrorLog.Debug(member.MemberType.ToString());
      if (((member.MemberType & memberTypes)!=0) != containsFlags) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToBe,
          aspect.GetType().GetShortName(),
          FormatMember(member.DeclaringType, member),
          containsFlags ? string.Empty : Strings.Not,
          memberTypes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the attributes of the field.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="field">The field to validate the attributes of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, field attributes
    /// must contain <paramref name="fieldAttributes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="fieldAttributes">Expected (or not) attributes of the field.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateFieldAttributes(Attribute aspect, FieldInfo field, bool containsFlags, FieldAttributes fieldAttributes)
    {
      if (((field.Attributes & fieldAttributes)!=0) != containsFlags) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToBe,
          aspect.GetType().GetShortName(),
          FormatMember(field.DeclaringType, field),
          containsFlags ? string.Empty : Strings.Not,
          fieldAttributes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the attributes of the method.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="method">The method to validate the attributes of.</param>
    /// <param name="containsFlags">If set to <see langword="true"/>, method attributes
    /// must contain <paramref name="methodAttributes"/> flags;
    /// otherwise, it must not contain them.</param>
    /// <param name="methodAttributes">Expected (or not) attributes of the method.</param>
    /// <returns><see langword="true" /> if validation has passed;
    /// otherwise, <see langword="false" />.</returns>
    public static bool ValidateMethodAttributes(Attribute aspect, MethodBase method, bool containsFlags, MethodAttributes methodAttributes)
    {
      if (((method.Attributes & methodAttributes)!=0) != containsFlags) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToBe,
          aspect.GetType().GetShortName(),
          FormatMember(method.DeclaringType, method),
          containsFlags ? string.Empty : Strings.Not,
          methodAttributes);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Validates the presence of specified constructor on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="type">The type to get the constructor of.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, type 
    /// must have specified constructor;
    /// otherwise, it must not have it.</param>
    /// <param name="bindingFlags">Binding flags.</param>
    /// <param name="parameterTypes">The types of constructor arguments.</param>
    /// <returns>Found constructor, if validation has passed;
    /// otherwise, <see langword="null" />.</returns>
    public static ConstructorInfo ValidateConstructor(Attribute aspect, Type type, bool mustHave, BindingFlags bindingFlags, params Type[] parameterTypes)
    {
      ConstructorInfo constructor = null;
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
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToHave,
          aspect.GetType().GetShortName(), 
          type.GetShortName(), 
          mustHave ? string.Empty : Strings.Not,
          FormatConstructor(null, type, parameterTypes));
      }
      return constructor;
    }

    /// <summary>
    /// Validates the presence of specified method on the <paramref name="type"/>.
    /// </summary>
    /// <param name="aspect">The aspect.</param>
    /// <param name="type">The type to get the method of.</param>
    /// <param name="mustHave">If set to <see langword="true"/>, type 
    /// must have specified method;
    /// otherwise, it must not have it.</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="bindingFlags">Binding flags.</param>
    /// <param name="parameterTypes">The types of method arguments.</param>
    /// <returns>Found method, if validation has passed;
    /// otherwise, <see langword="null" />.</returns>
    public static MethodInfo ValidateMethod(Attribute aspect, Type type, bool mustHave, string name, BindingFlags bindingFlags, params Type[] parameterTypes)
    {
      MethodInfo method = null;
      try {
        method = type.GetMethod(name, bindingFlags, null, parameterTypes, null);
      }
      catch (NullReferenceException) { }
      catch (ArgumentNullException) { }
      catch (AmbiguousMatchException) { }
      
      if ((method!=null) != mustHave) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToHave,
          aspect.GetType().GetShortName(), 
          type.GetShortName(), 
          mustHave ? string.Empty : Strings.Not,
          FormatMethod(null, type, name, parameterTypes));
      }
      return method;
    }
  }
}