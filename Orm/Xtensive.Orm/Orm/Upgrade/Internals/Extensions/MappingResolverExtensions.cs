// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.01.22

using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Upgrade.Internals.Extensions
{
  internal static class MappingResolverExtensions
  {
    public static string GetTableName(this MappingResolver resolver, StoredTypeInfo type)
    {
      return resolver.GetNodeName(
        type.MappingDatabase, type.MappingSchema, type.MappingName);
    }

    private static string GetTablePath(this MappingResolver resolver,StoredTypeInfo type)
    {
      var nodeName = resolver.GetTableName(type);
      return string.Format("Tables/{0}", nodeName);
    }
  }
}