// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.27

using System;
using System.Linq;
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
    /// Gets constructor by names \ types of its parameters.
    /// </summary>
    /// <param name="type">Type to search constructor in.</param>
    /// <param name="bindingFlags">Binding attributes.</param>
    /// <param name="parameterTypes">Either strings or <see cref="Type"/>s of parameters (mixing is allowed).</param>
    /// <returns>Found constructor, if match was found;
    /// otherwise, <see langword="null"/>.</returns>
    public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingFlags, object[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      if (parameterTypes == null)
        parameterTypes = ArrayUtils<object>.EmptyArray;

      if (parameterTypes.All(o => o is Type))
        return type.GetConstructor(bindingFlags, null,
          parameterTypes.Select(o => (Type) o).ToArray(), null);

      var genericArgumentNames = ArrayUtils<string>.EmptyArray;
      ConstructorInfo lastMatch = null;

      foreach (ConstructorInfo ci in type.GetConstructors(bindingFlags)) {
        if (CheckMethod(ci, type, WellKnown.CtorName, genericArgumentNames, parameterTypes)) {
          if (lastMatch!=null)
            throw new AmbiguousMatchException();
          lastMatch = ci;
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
    /// Gets the types of constructor parameters.
    /// </summary>
    /// <param name="ctor">The constructor to get the types of parameters of.</param>
    /// <returns>The array of types of constructor parameters.</returns>
    public static Type[] GetParameterTypes(this ConstructorInfo ctor)
    {
      var parameters = ctor.GetParameters();
      var types = new Type[parameters.Length];
      for (int i = 0; i < parameters.Length; i++)
        types[i] = parameters[i].ParameterType;
      return types;
    }

    #region Property-related methods

    /// <summary>
    /// Determines whether the specified method is a property accessor.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is property accessor; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPropertyAccessor(this MethodInfo method)
    {
      return method.IsGetter() || method.IsSetter();
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
      return method.Name!=TryCutMethodNamePrefix(method.Name, WellKnown.GetterPrefix);
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
      return method.Name!=TryCutMethodNamePrefix(method.Name, WellKnown.SetterPrefix);
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
      string name = method.Name;
      string propertyName = TryCutMethodNamePrefix(name, WellKnown.GetterPrefix);
      var bindingFlags = method.GetBindingFlags() | BindingFlags.Public;
      if (method.IsExplicitImplementation())
        bindingFlags |= BindingFlags.Public;
      if (propertyName!=name)
        return type.GetProperty(propertyName, bindingFlags);
      propertyName = TryCutMethodNamePrefix(name, WellKnown.SetterPrefix);
      if (propertyName!=name)
        return type.GetProperty(propertyName, bindingFlags);
      return null;
    }

    #endregion

    #region Event-related methods

    /// <summary>
    /// Determines whether the specified method is an event accessor.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is event accessor; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsEventAccessor(this MethodInfo method)
    {
      return method.IsAddEventHandler() || method.IsRemoveEventHandler();
    }

    /// <summary>
    /// Determines whether the specified method is "add event handler" method.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is "add event handler" method; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsAddEventHandler(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return false;
      return method.Name!=TryCutMethodNamePrefix(method.Name, WellKnown.AddEventHandlerPrefix);
    }

    /// <summary>
    /// Determines whether the specified method is "remove event handler" method.
    /// </summary>
    /// <param name="method">The method to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified method is "remove event handler" method; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRemoveEventHandler(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return false;
      return method.Name!=TryCutMethodNamePrefix(method.Name, WellKnown.RemoveEventHandlerPrefix);
    }

    /// <summary>
    /// Gets the event to which <paramref name="method"/> belongs.
    /// </summary>
    /// <param name="method">The method to get the event for.</param>
    /// <returns>Found event;
    /// <see langword="null" />, if no event is associated with the method.</returns>
    public static EventInfo GetEvent(this MethodInfo method)
    {
      if (!method.IsSpecialName)
        return null;
      var type = method.DeclaringType.UnderlyingSystemType;
      string name = method.Name;
      string eventName = TryCutMethodNamePrefix(name, WellKnown.AddEventHandlerPrefix);
      var bindingFlags = method.GetBindingFlags() | BindingFlags.Public;
      if (method.IsExplicitImplementation())
        bindingFlags |= BindingFlags.Public;
      if (eventName!=name)
        return type.GetEvent(eventName, bindingFlags);
      eventName = TryCutMethodNamePrefix(name, WellKnown.RemoveEventHandlerPrefix);
      if (eventName!=name)
        return type.GetEvent(eventName, bindingFlags);
      return null;
    }

    #endregion

    #region Private \ internal methods

    private static bool CheckMethod(MethodBase m, Type type, string name, string[] genericArgumentNames, object[] parameterTypes)
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
          if (!genericArgumentNames[i].IsNullOrEmpty() && genericArgumentNames[i]!=genericArguments[i].Name)
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
        if (!parameterTypeName.IsNullOrEmpty())
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

    private static string TryCutMethodNamePrefix(string methodName, string prefixToCut)
    {
      var result = methodName.TryCutPrefix(prefixToCut);
      if (result!=methodName)
        return result;
      int i = methodName.LastIndexOf('.');
      if (i>=0) {
        result = methodName.Substring(0, i+1) + methodName.Substring(i+1).TryCutPrefix(prefixToCut);
        if (result!=methodName)
          return result;
      }
      return methodName;
    }

    #endregion
  }
}