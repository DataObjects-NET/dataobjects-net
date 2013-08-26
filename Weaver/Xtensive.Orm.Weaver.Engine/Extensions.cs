// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Xtensive.Orm.Weaver
{
  internal static class Extensions
  {
    public static bool HasAttribute(this ICustomAttributeProvider target, string fullName)
    {
      var comparer = WeavingHelper.TypeNameComparer;
      return target.CustomAttributes.Any(a => comparer.Equals(a.AttributeType.FullName, fullName));
    }

    public static bool IsAutoProperty(this PropertyDefinition property)
    {
      return property.GetMethod!=null && property.GetMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute)
        && property.SetMethod!=null && property.SetMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute);
    }

    public static bool IsStatic(this PropertyDefinition property)
    {
      return property.GetMethod!=null && property.GetMethod.IsStatic
        || property.SetMethod!=null && property.SetMethod.IsStatic;
    }

    public static bool Remove<T>(this Collection<T> items, string name)
      where T : MemberReference
    {
      var index = items.IndexOf(name);
      if (index < 0)
        return false;
      items.RemoveAt(index);
      return true;
    }

    public static int IndexOf<T>(this Collection<T> items, string name)
      where T : MemberReference
    {
      var comparer = WeavingHelper.TypeNameComparer;

      for (int i = 0; i < items.Count; i++)
        if (comparer.Equals(items[i].Name, name))
          return i;

      return -1;
    }
  }
}