// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver
{
  internal static class Extensions
  {
    public static bool HasAttribute(this ICustomAttributeProvider target, string fullName)
    {
      var comparer = TypeIdentity.TypeNameComparer;
      return target.CustomAttributes.Any(a => comparer.Equals(a.AttributeType.FullName, fullName));
    }
  }
}