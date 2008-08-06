// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.27

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// <see cref="MethodInfo"/> related helper \ extension methods.
  /// </summary>
  public static class MethodHelper
  {
    /// <summary>
    /// Gets generic method by names \ types of its arguments.
    /// </summary>
    /// <param name="type">Type to search the method in.</param>
    /// <param name="name">Method name.</param>
    /// <param name="bindingFlags">Binding attributes.</param>
    /// <param name="genericArgumentNames">Generic arguments of the method.</param>
    /// <param name="parameterTypes">Either strings or <see cref="Type"/>s of parameters (mixing is allowed).</param>
    /// <returns>Found method, if match was found;
    /// otherwise, <see langword="null"/>.</returns>
    public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags, string[] genericArgumentNames, object[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      
      if (genericArgumentNames==null)
        genericArgumentNames = ArrayUtils<string>.EmptyArray;
      if (parameterTypes==null)
        parameterTypes = ArrayUtils<object>.EmptyArray;
      bool parameterTypesAreFullyDefined = true;
      for (int i = 0; i<parameterTypes.Length; i++) {
        if (!(parameterTypes[i] is Type)) {
          parameterTypesAreFullyDefined = false;
          break;
        }
      }
      
      if (parameterTypesAreFullyDefined) {
        // Let's try to find an exact match
        Type[] exactParameterTypes = new Type[parameterTypes.Length];
        for (int i = 0; i<parameterTypes.Length; i++)
          exactParameterTypes[i] = (Type)parameterTypes[i];
        try {
          MethodInfo m = type.GetMethod(name, bindingFlags, null, exactParameterTypes, null);
          if (m!=null)
            return CheckMethod(m, type, name, genericArgumentNames, parameterTypes) ?  m : null;
        }
        catch (AmbiguousMatchException) {
        }
      }

      // No exact match... Trying to find the match by checking all methods
      MethodInfo lastMatch = null;
      foreach (MethodInfo m in type.GetMethods(bindingFlags)) {
        if (CheckMethod(m, type, name, genericArgumentNames, parameterTypes)) {
          if (lastMatch!=null)
            throw new AmbiguousMatchException();
          lastMatch = m;
        }
      }
      return lastMatch;
    }

    /// <summary>
    /// Gets the types of method parameters.
    /// </summary>
    /// <param name="method">The method to get the types of parameters of.</param>
    /// <returns>The array of types of method parameters.</returns>
    public static Type[] GetParameterTypes(this MethodInfo method)
    {
      var parameters = method.GetParameters();
      var types = new Type[parameters.Length];
      for (int i = 0; i < parameters.Length; i++)
        types[i] = parameters[i].ParameterType;
      return types;
    }

    /// <summary>
    /// Determines whether the specified method is a property getter.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is getter; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsGetter(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return false;
      return method.Name.StartsWith(WellKnown.GetterPrefix);
    }

    /// <summary>
    /// Determines whether the specified method is a property setter.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is setter; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsSetter(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return false;
      return method.Name.StartsWith(WellKnown.SetterPrefix);
    }

    /// <summary>
    /// Gets the property to which <paramref name="method"/> belongs.
    /// </summary>
    /// <param name="method">The method to get the property for.</param>
    /// <returns>Found property;
    /// <see langword="null" />, if no property is associated with the method.</returns>
    public static PropertyInfo GetProperty(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return null;
      var type = method.DeclaringType.UnderlyingSystemType;
      var bindingFlags = method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
      bindingFlags    |= method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
      string name = method.Name;
      string propertyName = name.TryCutPrefix(WellKnown.GetterPrefix);
      if (propertyName!=name)
        return type.GetProperty(propertyName, bindingFlags);
      propertyName = name.TryCutPrefix(WellKnown.SetterPrefix);
      if (propertyName!=name)
        return type.GetProperty(propertyName, bindingFlags);
      return null;
    }

    #region Private \ internal methods

    private static bool CheckMethod(this MethodInfo m, Type type, string name, string[] genericArgumentNames, object[] parameterTypes)
    {
      // Checking name
      if (m.Name!=name)
        return false;
      int matchCount = 0;
      if (m.IsGenericMethod) {
        // Checking generic arguments (just by their names, if defined)
        Type[] genericArguments = m.GetGenericArguments();
        if (genericArguments.Length!=genericArgumentNames.Length)
          return false;
        for (int i = 0; i < genericArguments.Length; i++)
          if (!String.IsNullOrEmpty(genericArgumentNames[i]) && genericArgumentNames[i]!=genericArguments[i].Name)
            break;
          else
            matchCount++;
        if (matchCount!=genericArguments.Length)
          return false;
      }
      else {
        // Non-generic method
        if (genericArgumentNames.Length!=0)
          return false;
      }
      // Checking parameter types
      ParameterInfo[] parameters = m.GetParameters();
      if (parameters.Length!=parameterTypes.Length)
        return false;
      matchCount = 0;
      for (int i = 0; i<parameters.Length; i++) {
        string parameterTypeName = parameterTypes[i] as string;
        Type parameterType = parameterTypes[i] as Type;
        bool isMatch;
        if (!String.IsNullOrEmpty(parameterTypeName))
          isMatch = parameters[i].ParameterType.Name==parameterTypeName;
        else if (parameterType!=null)
          isMatch = parameters[i].ParameterType==parameterType;
        else
          isMatch = true;
        if (!isMatch)
          break;
        else
          matchCount++;
      }
      return matchCount==parameters.Length;
    }

    #endregion
  }
}