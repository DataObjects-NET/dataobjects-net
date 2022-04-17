// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AttributesKey = System.ValueTuple<System.Reflection.MemberInfo, System.Type, Xtensive.Reflection.AttributeSearchOptions>;
using PerAttributeKey = System.ValueTuple<System.Reflection.MemberInfo, Xtensive.Reflection.AttributeSearchOptions>;

namespace Xtensive.Reflection
{
  /// <summary>
  /// <see cref="Attribute"/> related helper \ extension methods.
  /// </summary>
  public static class AttributeHelper
  {
    private static class AttributeDictionary<TAttribute> where TAttribute : Attribute
    {
      public static readonly ConcurrentDictionary<PerAttributeKey, TAttribute[]> Dictionary
        = new ConcurrentDictionary<PerAttributeKey, TAttribute[]>();
    }

    private static readonly ConcurrentDictionary<AttributesKey, Attribute[]> attributesByMemberInfoAndSearchOptions
      = new ConcurrentDictionary<AttributesKey, Attribute[]>();

    /// <summary>
    /// A shortcut to <see cref="MemberInfo.GetCustomAttributes(Type,bool)"/> method.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attributes to get.</typeparam>
    /// <param name="member">Member to get attributes of.</param>
    /// <param name="options">Attribute search options.</param>
    /// <returns>An array of attributes of specified type.</returns>
    ///
    public static IReadOnlyList<TAttribute> GetAttributes<TAttribute>(this MemberInfo member, AttributeSearchOptions options = AttributeSearchOptions.InheritNone)
        where TAttribute : Attribute =>
      AttributeDictionary<TAttribute>.Dictionary.GetOrAdd(
        new PerAttributeKey(member, options),
        key => GetAttributes(key.Item1, typeof(TAttribute), key.Item2).Cast<TAttribute>().ToArray()
      );

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
    public static TAttribute GetAttribute<TAttribute>(this MemberInfo member, AttributeSearchOptions options = AttributeSearchOptions.InheritNone)
      where TAttribute : Attribute
    {
      var attributes = member.GetAttributes<TAttribute>(options);
      return attributes.Count switch {
        0 => null,
        1 => attributes[0],
        _ => throw new InvalidOperationException(string.Format(Strings.ExMultipleAttributesOfTypeXAreNotAllowedHere,
          member.GetShortName(true),
          typeof(TAttribute).GetShortName()))
      };
    }

    private static IReadOnlyList<Attribute> GetAttributes(MemberInfo member, Type attributeType, AttributeSearchOptions options) =>
      attributesByMemberInfoAndSearchOptions.GetOrAdd(
        new AttributesKey(member, attributeType, options),
        t => ExtractAttributes(t).ToArray()
      );

    private static Attribute[] GetAttributes(this MemberInfo member, Type attributeType)
    {
      var attrObjects = member.GetCustomAttributes(attributeType, false);
      var attrs = new Attribute[attrObjects.Length];
      for (int i = attrObjects.Length; i-- > 0;) {
        attrs[i] = (Attribute) attrObjects[i];
      }
      return attrs;
    }

    private static IEnumerable<Attribute> ExtractAttributes((MemberInfo member, Type attributeType, AttributeSearchOptions options) t) {
      (var member, var attributeType, var options) = t;

      var attributes = member.GetCustomAttributes(attributeType, false).Cast<Attribute>().ToList();
      if (options == AttributeSearchOptions.InheritNone)
        return attributes;
      if (attributes.Count == 0) {
        if ((options & AttributeSearchOptions.InheritFromPropertyOrEvent) != 0
            && member is MethodInfo m
            && ((MemberInfo) m.GetProperty() ?? m.GetEvent()) is MemberInfo poe) {
          attributes = poe.GetAttributes(attributeType).ToList();
        }
        if ((options & AttributeSearchOptions.InheritFromBase) != 0
            && (options & AttributeSearchOptions.InheritRecursively) == 0
            && member.GetBaseMember() is MemberInfo bm) {
          attributes.AddRange(GetAttributes(bm, attributeType, options));
          return attributes;
        }
      }

      if ((options & AttributeSearchOptions.InheritFromAllBase) == AttributeSearchOptions.InheritFromAllBase
          && member.DeclaringType != WellKnownTypes.Object
          && member.GetBaseMember() is MemberInfo bm2) {
        attributes.AddRange(GetAttributes(bm2, attributeType, options));
      }

      return attributes;
    }
  }
}
