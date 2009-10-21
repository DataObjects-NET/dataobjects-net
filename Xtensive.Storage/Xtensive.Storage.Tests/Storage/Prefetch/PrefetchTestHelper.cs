// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.10

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Storage.Prefetch
{
  public static class PrefetchTestHelper
  {
    public static void AssertOnlySpecifiedColumnsAreLoaded(Key key, TypeInfo type, Session session,
      Func<FieldInfo, bool> fieldSelector)
    {
      var state = session.EntityStateCache[key, true];
      var realType = state.Key.Type;
      Assert.IsTrue(realType.Equals(type) 
        || realType.GetAncestors().Contains(type) 
        || (type.IsInterface && realType.GetInterfaces(true).Contains(type)));
      var tuple = state.Tuple;
      Assert.IsNotNull(tuple);
      foreach (var field in type.Fields) {
        var isFieldSelected = fieldSelector.Invoke(field);
        foreach (var column in field.Columns) {
          var isAvailable = tuple.GetFieldState(type.Columns.IndexOf(column)).IsAvailable();
          if (isFieldSelected)
            Assert.IsTrue(isAvailable);
          else
            Assert.IsFalse(isAvailable);
        }
      }
    }

    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
    }

    public static void AssertReferencedEntityIsLoaded(Key key, Session session, FieldInfo referencingField)
    {
      var tuple = session.EntityStateCache[key, true].Tuple;
      var foreignKeyValue = referencingField.Association.ExtractForeignKey(tuple);
      var foreignKey = Key.Create(session.Domain, referencingField.Association.TargetType.Hierarchy.Root, TypeReferenceAccuracy.BaseType, foreignKeyValue);
      AssertOnlySpecifiedColumnsAreLoaded(foreignKey, referencingField.Association.TargetType.Hierarchy.Root,
        session, IsFieldToBeLoadedByDefault);
    }
  }
}