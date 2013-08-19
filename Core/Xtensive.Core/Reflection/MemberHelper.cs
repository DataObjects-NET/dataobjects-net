// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Reflection;
using System.Security;
using System.Text;
using Xtensive.Reflection;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Resources;

namespace Xtensive.Reflection
{
  /// <summary>
  /// <see cref="MemberInfo"/> related helper \ extension methods.
  /// </summary>
  public static class MemberHelper
  {
    /// <summary>
    /// Gets the binding flags for the specified member.
    /// </summary>
    /// <param name="member">The member to get binding flags for.</param>
    /// <returns>Binding flags for the specified member.</returns>
    public static BindingFlags GetBindingFlags(this MemberInfo member)
    {
      var mi = member as MethodInfo;
      var ci = member as ConstructorInfo;
      var fi = member as FieldInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      var flags = default(BindingFlags);
      if (mi!=null) {
        flags |= mi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        flags |= mi.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (pi!=null) {
        var getter = pi.GetGetMethod(true);
        var setter = pi.GetSetMethod(true);
        var method = getter ?? setter;
        flags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        if (getter!=null)
          flags |= getter.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        if (setter!=null)
          flags |= setter.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (fi!=null) {
        flags |= fi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        flags |= fi.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (ei!=null) {
        var add = ei.GetAddMethod(true);
        var remove = ei.GetRemoveMethod(true);
        var method = add ?? remove;
        flags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        if (add!=null)
          flags |= add.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        if (remove!=null)
          flags |= remove.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (ci!=null) {
        flags |= ci.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        flags |= ci.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      return flags;
    }

    /// <summary>
    /// Gets the base member for the specified <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The member to get base member for.</param>
    /// <returns>Base member;
    /// <see langword="null" />, if it does not exist.</returns>
    public static MemberInfo GetBaseMember(this MemberInfo member)
    {
      try {
        // Items are ordered by probability of their reflection
        var pi = member as PropertyInfo;
        if (pi!=null) {
          var anyAccessor = (pi.GetGetMethod(true) ?? pi.GetSetMethod(true));
          var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetBaseMember();
          if (anyInterfaceAccessor==null)
            return null;
          return anyInterfaceAccessor.GetProperty();
        }
        var fi = member as FieldInfo;
        if (fi!=null)
          return null;
        var mi = member as MethodInfo;
        if (mi!=null) {
          if (mi.DeclaringType.IsInterface)
            return null;
          if (mi.IsExplicitImplementation())
            return mi.GetInterfaceMember();
          if (mi.IsAbstract || !mi.IsVirtual)
            return null;
//          var bmi = mi.GetBaseDefinition();
//          return bmi==mi ? null : bmi;
          var bmi = mi.DeclaringType.UnderlyingSystemType.BaseType
            .GetMethod(mi.Name, mi.GetBindingFlags() | BindingFlags.ExactBinding, null, mi.GetParameterTypes(), null);
          return bmi ?? mi.GetInterfaceMember();
        }
        var ci = member as ConstructorInfo;
        if (ci!=null)
          return null;
        var ei = member as EventInfo;
        if (ei!=null) {
          var anyAccessor = (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true));
          var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetBaseMember();
          if (anyInterfaceAccessor==null)
            return null;
          return anyInterfaceAccessor.GetEvent();
        }
        var ti = member as Type;
        if (ti!=null)
          return ti.BaseType;
      }
      catch {
      }
      return null;
    }

    #region Interface related methods

    /// <summary>
    /// Determines whether the specified <paramref name="member"/>
    /// is explicit implementation of some interface member.
    /// </summary>
    /// <param name="member">The member to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsExplicitImplementation(this MemberInfo member)
    {
      var mi = member as MethodInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      if (mi!=null) {
        if (mi.IsPublic)
          return false;
        if (mi.GetInterfaceMember()==null)
          return false;
        return mi.IsPrivate;
      }
      if (pi!=null)
        return (pi.GetGetMethod(true) ?? pi.GetSetMethod(true)).IsExplicitImplementation();
      if (ei!=null)
        return (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true)).IsExplicitImplementation();
      return false;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="member"/>
    /// is explicit or implicit implementation of some interface member.
    /// </summary>
    /// <param name="member">The member to check.</param>
    /// <returns>Check result.</returns>
    public static bool IsImplementation(this MemberInfo member)
    {
      var mi = member as MethodInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      if (mi!=null)
        return mi.GetInterfaceMember()!=null;
      if (pi!=null)
        return (pi.GetGetMethod(true) ?? pi.GetSetMethod(true)).IsImplementation();
      if (ei!=null)
        return (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true)).IsImplementation();
      return false;
    }

    /// <summary>
    /// Gets the interface member implementation in its implementor.
    /// </summary>
    /// <param name="member">The member to get the implementation member for.</param>
    /// <returns>Implementation member;
    /// <see langword="null" />, if no implementation member maps to the specified one.</returns>
    public static MemberInfo GetImplementation(this MemberInfo member, Type implementor)
    {
      if (implementor.IsInterface)
        return null;
      var mi = member as MethodInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      if (mi!=null) {
        var iType = mi.DeclaringType.UnderlyingSystemType;
        var map = implementor.GetInterfaceMapFast(iType);
        for (int i = 0; i < map.InterfaceMethods.Count; i++) {
          if (mi==map.InterfaceMethods[i])
            return map.TargetMethods[i];
        }
        return null;
      }
      if (pi!=null) {
        var anyAccessor = (pi.GetGetMethod(true) ?? pi.GetSetMethod(true));
        var anyAccessorImplementor = (MethodInfo) anyAccessor.GetImplementation(implementor);
        if (anyAccessorImplementor==null)
          return null;
        return anyAccessorImplementor.GetProperty();
      }
      if (ei!=null) {
        var anyAccessor = (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true));
        var anyAccessorImplementor = (MethodInfo) anyAccessor.GetImplementation(implementor);
        if (anyAccessorImplementor==null)
          return null;
        return anyAccessorImplementor.GetEvent();
      }
      return null;
    }

    /// <summary>
    /// Gets the interface member for its explicit or implicit implementor.
    /// </summary>
    /// <param name="member">The member to get the interface member for.</param>
    /// <returns>Interface member;
    /// <see langword="null" />, if no interface member maps to the specified one,
    /// or the member itself is declared in interface.</returns>
    [SecuritySafeCritical]
    public static MemberInfo GetInterfaceMember(this MemberInfo member)
    {
      if (member.DeclaringType.IsInterface)
        return null;
      var mi = member as MethodInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      if (mi!=null) {
        var type = mi.DeclaringType.UnderlyingSystemType;
        foreach (var iType in type.GetInterfaces()) {
          var map = type.GetInterfaceMapFast(iType.UnderlyingSystemType);
          for (int i = 0; i < map.InterfaceMethods.Count; i++) {
            var tmi = map.TargetMethods[i];
            if (mi==tmi)
              return map.InterfaceMethods[i];
            if (mi.GetType().FullName==WellKnown.RuntimeMethodInfoName && 
                tmi.GetType().FullName==WellKnown.RuntimeMethodInfoName &&
                mi.MethodHandle.Value==tmi.MethodHandle.Value)
                return map.InterfaceMethods[i];
          }
        }
        return null;
      }
      if (pi!=null) {
        var anyAccessor = (pi.GetGetMethod(true) ?? pi.GetSetMethod(true));
        var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetInterfaceMember();
        if (anyInterfaceAccessor==null)
          return null;
        return anyInterfaceAccessor.GetProperty();
      }
      if (ei!=null) {
        var anyAccessor = (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true));
        var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetInterfaceMember();
        if (anyInterfaceAccessor==null)
          return null;
        return anyInterfaceAccessor.GetEvent();
      }
      return null;
    }

    #endregion

    /// <summary>
    /// Builds correct full generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <param name="includeTypeName">Indicates whether type name must be included or not.</param>
    /// <returns>Full member name.</returns>
    public static string GetFullName(this MemberInfo member, bool includeTypeName)
    {
      if (member==null)
        return null;
      if (!includeTypeName) {
        if (!member.IsExplicitImplementation())
          return member.Name;
        var im = member.GetInterfaceMember();
        return new StringBuilder()
          .Append(im.DeclaringType.GetFullName())
          .Append(".")
          .Append(im.GetFullName(false))
          .ToString();
      }
      return new StringBuilder()
        .Append(member.DeclaringType.GetFullName())
        .Append(".")
        .Append(member.GetFullName(false))
        .ToString();
    }

    /// <summary>
    /// Builds correct short generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <param name="includeTypeName">Indicates whether type name must be included or not.</param>
    /// <returns>Short member name.</returns>
    public static string GetShortName(this MemberInfo member, bool includeTypeName)
    {
      if (member==null)
        return null;
      var name = member.Name;
      if (!includeTypeName) {
        int i = name.LastIndexOf('.');
        if (i<=0)
          return name;
        i = name.LastIndexOf('.', i-1);
        if (i<=0)
          return name;
        return name.Substring(i + 1);
      }
      return new StringBuilder()
        .Append(member.DeclaringType.GetShortName())
        .Append(".")
        .Append(member.GetShortName(false))
        .ToString();
    }

    /// <summary>
    /// Gets the type of the member.
    /// </summary>
    /// <param name="mi">The <see cref="MemberInfo"/>.</param>
    public static Type GetMemberType(MemberInfo mi)
    {
      var fi = mi as FieldInfo;
      if (fi != null)
        return fi.FieldType;
      var pi = mi as PropertyInfo;
      if (pi != null)
        return pi.PropertyType;
      var ei = mi as EventInfo;
      if (ei != null)
        return ei.EventHandlerType;
      return null;
    }
  }
}