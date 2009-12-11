// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Collections.Generic;
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
    /// <param name="options">Attribute search options.</param>
    /// <returns>An array of attributes of specified type.</returns>
    public static TAttribute[] GetAttributes<TAttribute>(this MemberInfo member, AttributeSearchOptions options)
    {
      var attributes = member.GetCustomAttributes(typeof (TAttribute), false).Cast<object, TAttribute>();
      if (attributes==null || attributes.Length==0) {
        if ((options & AttributeSearchOptions.InheritFromPropertyOrEvent)!=0) {
          MethodInfo m = member as MethodInfo;
          if (m!=null) {
            var poe = (MemberInfo) m.GetProperty() ?? m.GetEvent();
            if (poe!=null)
              attributes = poe.GetAttributes<TAttribute>(AttributeSearchOptions.Default);
          }
        }
        if ((options & AttributeSearchOptions.InheritFromBase)!=0 &&
            (options & AttributeSearchOptions.InheritFromAllBase)==0) {
          MemberInfo bm = member.GetBaseMember();
          if (bm!=null)
            attributes = bm.GetAttributes<TAttribute>(options);
        }
      }

      if ((options & AttributeSearchOptions.InheritFromAllBase)!=0
          && member.DeclaringType!=typeof(object)) {
        MemberInfo bm = member.GetBaseMember();
        if (bm!=null) {
          var list = new List<TAttribute>();
          if (attributes!=null)
            list.AddRange(attributes);
          attributes = bm.GetAttributes<TAttribute>(options);
          if (attributes!=null)
            list.AddRange(attributes);
          attributes = list.ToArray();
        }
      }

      return attributes;
    }

    /// <summary>
    /// A version of <see cref="GetAttributes{TAttribute}(MemberInfo, AttributeSearchOptions)"/> 
    /// returning just one attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
    /// <param name="member">Member to get attribute of.</param>
    /// <param name="options">Attribute search options.</param>
    /// <returns>An attribute of specified type;
    /// <see langword="null"/>, if there is no such attribute;
    /// throws <see cref="InvalidOperationException"/>, if there is more then one attribute of specified type found.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if there is more then one attribute of specified type found.</exception>
    public static TAttribute GetAttribute<TAttribute>(this MemberInfo member, AttributeSearchOptions options)
      where TAttribute: Attribute
    {
      object[] attributes = member.GetAttributes<TAttribute>(options);
      if (attributes==null || attributes.Length<1)
        return null;
      if (attributes.Length>1)
        throw new InvalidOperationException(string.Format(Strings.ExMultipleAttributesOfTypeXAreNotAllowedHere,
          member.GetShortName(true),
          typeof(TAttribute).GetShortName()
          ));
      return (TAttribute) attributes[0];
    }
  }
}