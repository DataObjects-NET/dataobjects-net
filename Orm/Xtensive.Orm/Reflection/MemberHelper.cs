// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Reflection;
using System.Security;
using System.Text;
using Xtensive.Reflection;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;


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
      var flags = default(BindingFlags);
      if (member is MethodInfo mi) {
        flags |= mi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        flags |= mi.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (member is PropertyInfo pi) {
        var getter = pi.GetGetMethod(true);
        var setter = pi.GetSetMethod(true);
        var method = getter ?? setter;
        flags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        if (getter!=null) {
          flags |= getter.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        }
        if (setter!=null) {
          flags |= setter.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        }
        return flags;
      }
      if (member is FieldInfo fi) {
        flags |= fi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        flags |= fi.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        return flags;
      }
      if (member is EventInfo ei) {
        var add = ei.GetAddMethod(true);
        var remove = ei.GetRemoveMethod(true);
        var method = add ?? remove;
        flags |= method.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
        if (add!=null) {
          flags |= add.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        }
        if (remove!=null) {
          flags |= remove.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
        }
        return flags;
      }
      if (member is ConstructorInfo ci) {
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
        if (member is PropertyInfo pi) {
          var anyAccessor = (pi.GetGetMethod(true) ?? pi.GetSetMethod(true));
          var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetBaseMember();
          return anyInterfaceAccessor == null
            ? null
            : (MemberInfo) anyInterfaceAccessor.GetProperty();
        }
        if (member is FieldInfo fi) {
          return null;
        }
        if (member is MethodInfo mi) {
          if (mi.DeclaringType.IsInterface) {
            return null;
          }
          if (mi.IsExplicitImplementation()) {
            return mi.GetInterfaceMember();
          }
          if (mi.IsAbstract || !mi.IsVirtual) {
            return null;
          }
          //          var bmi = mi.GetBaseDefinition();
          //          return bmi==mi ? null : bmi;
          var bmi = mi.DeclaringType.UnderlyingSystemType.BaseType
            .GetMethod(mi.Name, mi.GetBindingFlags() | BindingFlags.ExactBinding, null, mi.GetParameterTypes(), null);
          return bmi ?? mi.GetInterfaceMember();
        }
        if (member is ConstructorInfo ci) {
          return null;
        }
        if (member is EventInfo ei) {
          var anyAccessor = (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true));
          var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetBaseMember();
          if (anyInterfaceAccessor==null) {
            return null;
          }
          return anyInterfaceAccessor.GetEvent();
        }
        if (member is Type ti) {
          return ti.BaseType;
        }
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
      if (member is MethodInfo mi) {
        return mi.IsPublic
          ? false
          : mi.GetInterfaceMember()==null
            ? false
            : mi.IsPrivate;
      }
      if (member is PropertyInfo pi) {
        return (pi.GetGetMethod(true) ?? pi.GetSetMethod(true)).IsExplicitImplementation();
      }
      if (member is EventInfo ei) {
        return (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true)).IsExplicitImplementation();
      }
      return false;
    }

    /// <summary>
    /// Gets the interface member implementation in its implementor.
    /// </summary>
    /// <param name="member">The member to get the implementation member for.</param>
    /// <param name="implementor">A <see cref="Type"/> implementing an <see langword="interface"/>
    /// where the specified <paramref name="member"/> is declared.</param>
    /// <returns>Implementation member;
    /// <see langword="null" />, if no implementation member maps to the specified one.</returns>
    public static MemberInfo GetImplementation(this MemberInfo member, Type implementor)
    {
      if (implementor.IsInterface) {
        return null;
      }
      if (member is MethodInfo mi) {
        var iType = mi.DeclaringType.UnderlyingSystemType;
        var map = implementor.GetInterfaceMapFast(iType);
        var mapInterfaceMethods = map.InterfaceMethods;
        for (int i = 0, count = mapInterfaceMethods.Count; i < count; i++) {
          if (mi == mapInterfaceMethods[i]) {
            return map.TargetMethods[i];
          }
        }
        return null;
      }
      if (member is PropertyInfo pi) {
        var anyAccessor = (pi.GetGetMethod(true) ?? pi.GetSetMethod(true));
        var anyAccessorImplementor = (MethodInfo) anyAccessor.GetImplementation(implementor);
        return anyAccessorImplementor == null
          ? null
          : (MemberInfo) anyAccessorImplementor.GetProperty();
      }
      if (member is EventInfo ei) {
        var anyAccessor = (ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true));
        var anyAccessorImplementor = (MethodInfo) anyAccessor.GetImplementation(implementor);
        return anyAccessorImplementor == null ? null : (MemberInfo) anyAccessorImplementor.GetEvent();
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
      if (member.DeclaringType.IsInterface) {
        return null;
      }
      if (member is MethodInfo mi) {
        var type = mi.DeclaringType.UnderlyingSystemType;
        var methodInfoType = mi.GetType();
        var isRuntimeMethodInfo = methodInfoType.FullName == WellKnown.RuntimeMethodInfoName;
        foreach (var iType in TypeHelper.GetInterfacesUnordered(type)) {
          var map = type.GetInterfaceMapFast(iType.UnderlyingSystemType);
          var mapInterfaceMethods = map.InterfaceMethods;
          var targetMethods = map.TargetMethods;
          for (int i = 0, count = map.InterfaceMethods.Count; i < count; i++) {
            var tmi = targetMethods[i];
            if (mi == tmi) {
              return mapInterfaceMethods[i];
            }
            var targetMethodInfoType = tmi.GetType();
            if (isRuntimeMethodInfo && methodInfoType == targetMethodInfoType &&
                mi.MethodHandle.Value == tmi.MethodHandle.Value) {
              return mapInterfaceMethods[i];
            }
          }
        }
        return null;
      }
      if (member is PropertyInfo pi) {
        var anyAccessor = pi.GetGetMethod(true) ?? pi.GetSetMethod(true);
        var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetInterfaceMember();
        return anyInterfaceAccessor == null
          ? null
          : (MemberInfo) anyInterfaceAccessor.GetProperty();
      }
      if (member is EventInfo ei) {
        var anyAccessor = ei.GetAddMethod(true) ?? ei.GetRemoveMethod(true);
        var anyInterfaceAccessor = (MethodInfo) anyAccessor.GetInterfaceMember();
        return anyInterfaceAccessor == null
          ? null
          : (MemberInfo) anyInterfaceAccessor.GetEvent();
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
      if (member == null) {
        return null;
      }

      if (!includeTypeName) {
        if (!member.IsExplicitImplementation()) {
          return member.Name;
        }

        member = member.GetInterfaceMember();
      }
      var sb = new ValueStringBuilder(stackalloc char[256]);
      sb.Append(member.DeclaringType.GetFullName());
      sb.Append(".");
      sb.Append(member.GetFullName(false));
      return sb.ToString();
    }

    /// <summary>
    /// Builds correct short generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <param name="includeTypeName">Indicates whether type name must be included or not.</param>
    /// <returns>Short member name.</returns>
    public static string GetShortName(this MemberInfo member, bool includeTypeName)
    {
      if (member == null) {
        return null;
      }
      var name = member.Name;
      if (!includeTypeName) {
        var dotIndex = name.LastIndexOf('.');
        if (dotIndex <= 0) {
          return name;
        }
        dotIndex = name.LastIndexOf('.', dotIndex - 1);
        return dotIndex <= 0 ? name : name.Substring(dotIndex + 1);
      }
      var sb = new ValueStringBuilder(stackalloc char[256]);
      sb.Append(member.DeclaringType.GetShortName());
      sb.Append(".");
      sb.Append(member.GetShortName(false));
      return sb.ToString();
    }
  }
}
