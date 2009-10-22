// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.22

using System.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class PrefetchHelper
  {
    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
    }

    public static PrefetchFieldDescriptor[] CreateDescriptorsForFieldsLoadedByDefault(TypeInfo type)
    {
      return type.Fields.Where(field => field.Parent==null && IsFieldToBeLoadedByDefault(field))
        .Select(field => new PrefetchFieldDescriptor(field, false)).ToArray();
    }
  }
}