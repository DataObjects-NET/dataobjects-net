// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
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
      private static readonly Type attributeType = typeof(TAttribute);
      public static readonly ConcurrentDictionary<PerAttributeKey, TAttribute[]> Dictionary = new();

      public static readonly Func<PerAttributeKey, TAttribute[]> AttributesExtractor = ExtractAttributesByKey;

      private static TAttribute[] ExtractAttributesByKey(PerAttributeKey key)
      {
        var (member, options) = key;

        var attributesAsObjects = member.GetCustomAttributes(attributeType, false);
        var attributesCount = attributesAsObjects.Length;

        var attributes = attributesCount > 0
          ? attributesAsObjects.Cast<TAttribute>().ToList(attributesCount)
          : null;

        if (options != AttributeSearchOptions.InheritNone) {
          if (attributesCount == 0) {
            if ((options & AttributeSearchOptions.InheritFromPropertyOrEvent) != 0
                && member is MethodInfo m
                && ((MemberInfo) m.GetProperty() ?? m.GetEvent()) is MemberInfo poe) {
              attributes = GetAttributesAsNewList(poe);
            }
            if ((options & AttributeSearchOptions.InheritFromBase) != 0
                && (options & AttributeSearchOptions.InheritFromAllBase) == 0) {
              AddAttributesFromBase(ref attributes, member, options);
            }
          }

          if ((options & AttributeSearchOptions.InheritFromAllBase) != 0
              && member.DeclaringType != WellKnownTypes.Object) {
            AddAttributesFromBase(ref attributes, member, options);
          }
        }

        return attributes?.ToArray(attributes.Count) ?? Array.Empty<TAttribute>();
      }

      private static List<TAttribute> GetAttributesAsNewList(MemberInfo member)
      {
        var attrObjects = member.GetCustomAttributes(attributeType, false);
        var attrs = new List<TAttribute>(attrObjects.Length);
        for (int i = 0, count = attrObjects.Length; i < count; ++i) {
          attrs.Add((TAttribute) attrObjects[i]);
        }
        return attrs;
      }

      private static void AddAttributesFromBase(ref List<TAttribute> attributes, MemberInfo member, AttributeSearchOptions options)
      {
        if (member.GetBaseMember() is MemberInfo bm) {
          var attrsToAdd = bm.GetAttributes<TAttribute>(options);
          if (attrsToAdd.Count > 0) {
            (attributes ??= new List<TAttribute>(attrsToAdd.Count)).AddRange(attrsToAdd);
          }
        }
      }
    }

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
      AttributeDictionary<TAttribute>.Dictionary.GetOrAdd(new PerAttributeKey(member, options), AttributeDictionary<TAttribute>.AttributesExtractor);

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
  }
}
