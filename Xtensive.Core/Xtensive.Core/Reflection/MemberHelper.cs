// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Reflection;
using System.Text;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Reflection
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
        var getter = pi.GetGetMethod();
        var setter = pi.GetSetMethod();
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
        var add = ei.GetAddMethod();
        var remove = ei.GetRemoveMethod();
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
      var ti = member as Type;
      var mi = member as MethodInfo;
      var ci = member as ConstructorInfo;
      var fi = member as FieldInfo;
      var pi = member as PropertyInfo;
      var ei = member as EventInfo;
      try {
        if (ti!=null)
          return ti.BaseType;
        if (ci!=null)
          return null;
        if (fi!=null)
          return null;
        if (mi!=null) {
          if (mi.IsAbstract || !mi.IsVirtual)
            return null;
          return mi.DeclaringType.UnderlyingSystemType.BaseType
            .GetMethod(mi.Name, mi.GetBindingFlags() | BindingFlags.ExactBinding, null, mi.GetParameterTypes(), null);
        }
        if (pi!=null) {
          var getter = pi.GetGetMethod(false);
          var setter = pi.GetSetMethod(false);
          if (getter!=null && (getter.IsAbstract || !getter.IsVirtual))
            return null;
          if (setter!=null && (setter.IsAbstract || !setter.IsVirtual))
            return null;
          return pi.DeclaringType.UnderlyingSystemType.BaseType
            .GetProperty(pi.Name, pi.GetBindingFlags());
        }
        if (ei!=null) {
          var add = ei.GetAddMethod(false);
          var remove = ei.GetAddMethod(false);
          if (add!=null && (add.IsAbstract || !add.IsVirtual))
            return null;
          if (remove!=null && (remove.IsAbstract || !remove.IsVirtual))
            return null;
          return ei.DeclaringType.UnderlyingSystemType.BaseType
            .GetEvent(ei.Name, ei.GetBindingFlags());
        }
      }
      catch {
      }
      return null;
    }


    /// <summary>
    /// Builds correct full generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <returns>Full member name.</returns>
    public static string GetFullName(this MemberInfo member)
    {
      return new StringBuilder()
        .Append(member.DeclaringType.GetFullName())
        .Append(".")
        .Append(member.Name)
        .ToString();
    }

    /// <summary>
    /// Builds correct short generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <returns>Short member name.</returns>
    public static string GetShortName(this MemberInfo member)
    {
      return new StringBuilder()
        .Append(member.DeclaringType.GetShortName())
        .Append(".")
        .Append(member.Name)
        .ToString();
    }
  }
}