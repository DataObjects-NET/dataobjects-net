// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.06

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using TypeAttributes=System.Reflection.TypeAttributes;

namespace Xtensive.Storage.Internals
{
  internal static class EntitySetHelper
  {
    private static readonly object @lock = new object();
    private static AssemblyBuilder assemblyBuilder;
    private static ModuleBuilder moduleBuilder;

    public static Type BuildReferenceType(AssociationInfo association)
    {
      if (association.ReferencingField.IsEntitySet && association.IsMaster) {
        Type baseType = typeof (EntitySetReference<,>).MakeGenericType(association.ReferencingType.UnderlyingType, association.ReferencedType.UnderlyingType);
        string name = BuildingContext.Current.NameBuilder.Build(association, true);
        return TypeHelper.CreateInheritedDummyType(name, baseType);
      }
      return null;
    }

  }
}