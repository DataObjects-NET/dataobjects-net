// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.06

using System;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class EntitySetHelper
  {
    public static Type BuildReferenceType(AssociationInfo association)
    {
      if (association.ReferencingField.IsEntitySet && association.IsMaster) {
        Type baseType = typeof (EntitySetReference<,>).MakeGenericType(association.ReferencingType.UnderlyingType, association.ReferencedType.UnderlyingType);
        string name = BuildingContext.Current.NameBuilder.Build(association);
        return TypeHelper.CreateDummyType(name, baseType);
      }
      return null;
    }

    public static void DefineReferenceType(AssociationInfo association)
    {
      TypeDef typeDef = TypeBuilder.DefineType(association.EntityType);
      typeDef.DefineField("Entity1", association.ReferencingType.UnderlyingType);
      typeDef.DefineField("Entity2", association.ReferencedType.UnderlyingType);
      typeDef.Name = association.Name;
      BuildingContext.Current.Definition.Types.Add(typeDef);
      IndexBuilder.DefineIndexes(typeDef);
      TypeBuilder.BuildType(typeDef);
    }

    public static bool IsEntitySetRef(TypeInfo type)
    {
      Type underlyingBaseType = type.UnderlyingType.BaseType;
      return underlyingBaseType!=null
        && underlyingBaseType.IsGenericType
          && underlyingBaseType.GetGenericTypeDefinition()==typeof (EntitySetReference<,>);
    }
  }
}