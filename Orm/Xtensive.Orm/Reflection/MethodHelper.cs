// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.27

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Reflection
{
  /// <summary>
  /// <see cref="MethodInfo"/> related helper \ extension methods.
  /// </summary>
  public static class MethodHelper
  {
    private static readonly Type AnyArrayPlaceholderType = typeof(AnyArrayPlaceholder);

    /// <summary>
    /// This class is used internally by <see cref="MethodHelper"/> to denote
    /// an array of any type when matching parameter types
    /// in <see cref="MethodHelper.GetMethodEx"/> and <see cref="MethodHelper.GetConstructorEx"/>.
    /// </summary>
    public struct AnyArrayPlaceholder
    {
    }

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
    [DebuggerStepThrough]
    [CanBeNull]
    public static MethodInfo GetMethodEx(this Type type, string name, BindingFlags bindingFlags, string[] genericArgumentNames, object[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      if (genericArgumentNames == null) {
        genericArgumentNames = Array.Empty<string>();
      }

      if (parameterTypes == null) {
        parameterTypes = Array.Empty<object>();
      }

      var parameterTypesAreFullyDefined = true;
      for (var i = 0; i < parameterTypes.Length; i++) {
        var parameterType = parameterTypes[i] as Type;
        if (parameterType == null || parameterType.IsGenericTypeDefinition || parameterType == AnyArrayPlaceholderType) {
          parameterTypesAreFullyDefined = false;
          break;
        }
      }

      if (parameterTypesAreFullyDefined) {
        // Let's try to find an exact match
        var exactParameterTypes = new Type[parameterTypes.Length];
        for (var i = 0; i < parameterTypes.Length; i++) {
          exactParameterTypes[i] = (Type) parameterTypes[i];
        }
        try {
          var m = type.GetMethod(name, bindingFlags, null, exactParameterTypes, null);
          if (m != null) {
            return CheckMethod(m, name, genericArgumentNames, parameterTypes) ? m : null;
          }
        }
        catch (AmbiguousMatchException) {
        }
      }

      // No exact match... Trying to find the match by checking all methods
      MethodInfo lastMatch = null;
      foreach (var m in type.GetMethods(bindingFlags)) {
        if (CheckMethod(m, name, genericArgumentNames, parameterTypes)) {
          if (lastMatch != null) {
            throw new AmbiguousMatchException();
          }
          lastMatch = m;
        }
      }
      return lastMatch;
    }

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
    [DebuggerStepThrough]
    [Obsolete("Use MethodHelper.GetMethodEx() instead")]
    [CanBeNull]
    public static MethodInfo GetMethod(this Type type,
        string name, BindingFlags bindingFlags, string[] genericArgumentNames, object[] parameterTypes) =>
      GetMethodEx(type, name, bindingFlags, genericArgumentNames, parameterTypes);

    /// <summary>
    /// Gets constructor by names \ types of its parameters.
    /// </summary>
    /// <param name="type">Type to search constructor in.</param>
    /// <param name="bindingFlags">Binding attributes.</param>
    /// <param name="parameterTypes">Either strings or <see cref="Type"/>s of parameters (mixing is allowed).</param>
    /// <returns>Found constructor, if match was found;
    /// otherwise, <see langword="null"/>.</returns>

    [CanBeNull]
    public static ConstructorInfo GetConstructorEx(this Type type, BindingFlags bindingFlags, object[] parameterTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      if (parameterTypes == null) {
        parameterTypes = Array.Empty<object>();
      }
      else if (parameterTypes.All(o => o is Type)) {
        return type.GetConstructor(bindingFlags, null,
          parameterTypes.Select(o => (Type) o).ToArray(parameterTypes.Length), null);
      }

      ConstructorInfo lastMatch = null;

      foreach (var ci in type.GetConstructors(bindingFlags)) {
        if (CheckMethod(ci, WellKnown.CtorName, Array.Empty<string>(), parameterTypes)) {
          if (lastMatch != null) {
            throw new AmbiguousMatchException();
          }

          lastMatch = ci;
        }
      }

      return lastMatch;
    }

    [Obsolete("Use MethodHelper.GetConstructorEx() instead")]
    [CanBeNull]
    public static ConstructorInfo GetConstructor(this Type type, BindingFlags bindingFlags, object[] parameterTypes) =>
      GetConstructorEx(type, bindingFlags, parameterTypes);

    /// <summary>
    /// Gets the types of method parameters.
    /// </summary>
    /// <param name="method">The method to get the types of parameters of.</param>
    /// <returns>The array of types of method parameters.</returns>
    public static Type[] GetParameterTypes(this MethodBase method)
    {
      var parameters = method.GetParameters();
      if (parameters.Length == 0) {
        return Array.Empty<Type>();
      }

      var types = new Type[parameters.Length];
      for (var i = 0; i < parameters.Length; i++) {
        types[i] = parameters[i].ParameterType;
      }
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
    public static bool IsPropertyAccessor(this MethodInfo method) =>
      method.IsGetter() || method.IsSetter();

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
      return method.IsSpecialName
        ? method.Name != TryCutMethodNamePrefix(method.Name, WellKnown.GetterPrefix)
        : false;
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
      return method.IsSpecialName
        ? method.Name != TryCutMethodNamePrefix(method.Name, WellKnown.SetterPrefix)
        : false;
    }

    /// <summary>
    /// Gets the property to which <paramref name="method"/> belongs.
    /// </summary>
    /// <param name="method">The method to get the property for.</param>
    /// <returns>Found property;
    /// <see langword="null" />, if no property is associated with the method.</returns>
    [CanBeNull]
    public static PropertyInfo GetProperty(this MethodInfo method)
    {
      if (!method.IsSpecialName) {
        return null;
      }

      var type = method.DeclaringType.UnderlyingSystemType;
      var name = method.Name;
      var propertyName = TryCutMethodNamePrefix(name, WellKnown.GetterPrefix);
      var bindingFlags = method.GetBindingFlags() | BindingFlags.Public;
      //if (method.IsExplicitImplementation())
      //  bindingFlags |= BindingFlags.Public;
      if (!propertyName.Equals(name, StringComparison.Ordinal)) {
        return GetPropertyRecursive(type, bindingFlags, propertyName);
      }

      propertyName = TryCutMethodNamePrefix(name, WellKnown.SetterPrefix);
      return !propertyName.Equals(name, StringComparison.Ordinal)
        ? GetPropertyRecursive(type, bindingFlags, propertyName)
        : null;
    }

    [CanBeNull]
    private static PropertyInfo GetPropertyRecursive(Type type, BindingFlags bindingFlags, string propertyName)
    {
      if (type.IsInterface) {
        return type.GetProperty(propertyName, bindingFlags);
      }

      bindingFlags |= BindingFlags.DeclaredOnly;
      while (type != null) {
        var property = type.GetProperty(propertyName, bindingFlags);
        if (property != null) {
          return property;
        }
        type = type.BaseType;
      }
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
    public static bool IsEventAccessor(this MethodInfo method) =>
      method.IsAddEventHandler() || method.IsRemoveEventHandler();

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
      return method.IsSpecialName
        ? method.Name != TryCutMethodNamePrefix(method.Name, WellKnown.AddEventHandlerPrefix)
        : false;
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
      return method.IsSpecialName
        ? method.Name != TryCutMethodNamePrefix(method.Name, WellKnown.RemoveEventHandlerPrefix)
        : false;
    }

    /// <summary>
    /// Gets the event to which <paramref name="method"/> belongs.
    /// </summary>
    /// <param name="method">The method to get the event for.</param>
    /// <returns>Found event;
    /// <see langword="null" />, if no event is associated with the method.</returns>
    [CanBeNull]
    public static EventInfo GetEvent(this MethodInfo method)
    {
      if (!method.IsSpecialName) {
        return null;
      }

      var type = method.DeclaringType.UnderlyingSystemType;
      var name = method.Name;
      var eventName = TryCutMethodNamePrefix(name, WellKnown.AddEventHandlerPrefix);
      var bindingFlags = method.GetBindingFlags() | BindingFlags.Public;
      //if (method.IsExplicitImplementation())
      //  bindingFlags |= BindingFlags.Public;
      if (!eventName.Equals(name, StringComparison.Ordinal)) {
        return GetEventRecursive(bindingFlags, eventName, type);
      }

      eventName = TryCutMethodNamePrefix(name, WellKnown.RemoveEventHandlerPrefix);
      return !eventName.Equals(name, StringComparison.Ordinal)
        ? GetEventRecursive(bindingFlags, eventName, type)
        : null;
    }

    [CanBeNull]
    private static EventInfo GetEventRecursive(BindingFlags bindingFlags, string eventName, Type type)
    {
      if (type.IsInterface) {
        return type.GetEvent(eventName, bindingFlags);
      }

      bindingFlags |= BindingFlags.DeclaredOnly;
      while (type != null) {
        var eventInfo = type.GetEvent(eventName, bindingFlags);
        if (eventInfo != null) {
          return eventInfo;
        }

        type = type.BaseType;
      }
      return null;
    }

    #endregion

    #region Private \ internal methods

    private static bool CheckMethod(MethodBase m, string name, string[] genericArgumentNames, object[] parameterTypes)
    {
      // Checking name
      if (!m.Name.Equals(name, StringComparison.Ordinal)) {
        return false;
      }

      var matchCount = 0;
      if (m.IsGenericMethod) {
        // Checking generic arguments (just by their names, if defined)
        var genericArguments = m.GetGenericArguments();
        if (genericArguments.Length!=genericArgumentNames.Length) {
          return false;
        }
        for (var i = 0; i < genericArguments.Length; i++) {
          if (!genericArgumentNames[i].IsNullOrEmpty() && genericArgumentNames[i] != genericArguments[i].Name) {
            break;
          }
          else {
            matchCount++;
          }
        }

        if (matchCount != genericArguments.Length) {
          return false;
        }
      }
      else {
        // Non-generic method
        if (genericArgumentNames.Length != 0) {
          return false;
        }
      }
      // Checking parameter types
      var parameters = m.GetParameters();
      if (parameters.Length!=parameterTypes.Length) {
        return false;
      }

      matchCount = 0;
      for (var i = 0; i<parameters.Length; i++) {
        var requiredParameterTypeName = parameterTypes[i] as string;
        var requiredParameterType = parameterTypes[i] as Type;
        bool isMatch;
        var actualParameterType = parameters[i].ParameterType;
        if (!requiredParameterTypeName.IsNullOrEmpty()) {
          isMatch = actualParameterType.Name==requiredParameterTypeName;
        }
        else if (requiredParameterType != null) {
          isMatch = actualParameterType == requiredParameterType
            || (requiredParameterType.IsGenericTypeDefinition
               && actualParameterType.IsGenericType
               && actualParameterType.CachedGetGenericTypeDefinition()==requiredParameterType)
            || (requiredParameterType == AnyArrayPlaceholderType && actualParameterType.IsArray);
        }
        else {
          isMatch = true;
        }

        if (!isMatch)
          break;
        else
          matchCount++;
      }
      return matchCount == parameters.Length;
    }

    private static string TryCutMethodNamePrefix(ReadOnlySpan<char> methodName, ReadOnlySpan<char> prefixToCut)
    {
      var result = methodName.TryCutPrefix(prefixToCut);
      if (result.Length != methodName.Length) {
        return result.ToString();
      }
      var i = methodName.LastIndexOf('.');
      if (i >= 0) {
        var s1WithDot = methodName[..(i + 1)];
        var s2Cutted = methodName[(i + 1)..].TryCutPrefix(prefixToCut);
        if (s1WithDot.Length + s2Cutted.Length != methodName.Length) {
          return s1WithDot.ToString() + s2Cutted.ToString();
        }
      }
      return methodName.ToString();
    }

    #endregion
  }
}
