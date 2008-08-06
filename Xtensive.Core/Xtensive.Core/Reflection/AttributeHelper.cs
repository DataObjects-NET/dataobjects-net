// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// <see cref="Attribute"/> related helper \ extension methods.
  /// </summary>
  public static class AttributeHelper
  {
    /// <summary>
    /// A shortcut to <see cref="MemberInfo.GetCustomAttributes(Type,bool)"/> method.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attributes to get.</typeparam>
    /// <param name="member">Member to get attributes of.</param>
    /// <param name="inherit">Indicates whether to inherit the attributes, or not.</param>
    /// <returns>An array of attributes of specified type.</returns>
    public static TAttribute[] GetAttributes<TAttribute>(this MemberInfo member, bool inherit)
    {
      return member.GetCustomAttributes(typeof (TAttribute), inherit).Cast<object, TAttribute>();
    }

    /// <summary>
    /// A version of <see cref="GetAttributes{TAttribute}(MemberInfo, bool)"/> 
    /// returning just one attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
    /// <param name="member">Member to get attribute of.</param>
    /// <param name="inherit">Indicates whether to inherit the attribute, or not.</param>
    /// <returns>An attribute of specified type;
    /// <see langword="null"/>, if there is no such attribute;
    /// throws <see cref="InvalidOperationException"/>, if there is more then one attribute of specified type found.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if there is more then one attribute of specified type found.</exception>
    public static TAttribute GetAttribute<TAttribute>(this MemberInfo member, bool inherit)
      where TAttribute: Attribute
    {
      object[] attributes = member.GetCustomAttributes(typeof (TAttribute), inherit);
      if (attributes==null || attributes.Length<1)
        return null;
      if (attributes.Length>1)
        throw new InvalidOperationException(String.Format(Strings.ExMultipleAttributesOfTypeXAreNotAllowedHere,
          member.GetShortName(),
          typeof(TAttribute).GetShortName()
          ));
      return (TAttribute) attributes[0];
    }

    /// <summary>
    /// A shortcut to <see cref="Type.GetCustomAttributes(Type,bool)"/> method.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attributes to get.</typeparam>
    /// <param name="type">Type to get attributes of.</param>
    /// <param name="inherit">Indicates whether to inherit the attributes, or not.</param>
    /// <returns>An array of attributes of specified type.</returns>
    public static TAttribute[] GetAttributes<TAttribute>(this Type type, bool inherit)
    {
      return type.GetCustomAttributes(typeof (TAttribute), inherit).Cast<object, TAttribute>();
    }

    /// <summary>
    /// A version of <see cref="GetAttributes{TAttribute}(Type, bool)"/> 
    /// returning just one attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
    /// <param name="type">Type to get attribute of.</param>
    /// <param name="inherit">Indicates whether to inherit the attribute, or not.</param>
    /// <returns>An attribute of specified type;
    /// <see langword="null"/>, if there is no such attribute;
    /// throws <see cref="InvalidOperationException"/>, if there is more then one attribute of specified type found.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if there is more then one attribute of specified type found.</exception>
    public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit)
      where TAttribute: Attribute
    {
      object[] attributes = type.GetCustomAttributes(typeof (TAttribute), inherit);
      if (attributes==null || attributes.Length<1)
        return null;
      if (attributes.Length>1)
        throw new InvalidOperationException(String.Format(Strings.ExMultipleAttributesOfTypeXAreNotAllowedHere,
          type.GetShortName(),
          typeof(TAttribute).GetShortName()
          ));
      return (TAttribute) attributes[0];
    }
  }
}