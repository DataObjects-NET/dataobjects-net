// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.19

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class TypeIdProvider : ITypeIdProvider
  {
    private readonly UpgradeContext context;

    public int GetTypeId(Type type)
    {
      var typeId = TypeInfo.NoTypeId;
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null && context.ExtractedTypeMap==null)
        return typeId;

      // type has been renamed?
      var fullName = type.GetFullName();
      var renamer = context.Hints
        .OfType<RenameTypeHint>()
        .SingleOrDefault(hint => hint.NewType.GetFullName()==fullName);
      if (renamer!=null) {
        if (context.ExtractedTypeMap.TryGetValue(renamer.OldType, out typeId))
          return typeId;
        if (oldModel!=null) {
          var oldType = oldModel.Types.SingleOrDefault(t => t.UnderlyingType==renamer.OldType);
          if (oldType!=null)
            return oldType.TypeId;
        }
        // If RenameTypeHint is specified
        // but we can't find old type in any available source fail immediately
        throw new InvalidOperationException(string.Format(Strings.ExTypeXIsNotFound, renamer.OldType));
      }

      // type has been preserved
      if (context.ExtractedTypeMap.TryGetValue(fullName, out typeId))
        return typeId;
      if (oldModel!=null) {
        var oldType = oldModel.Types.SingleOrDefault(t => t.UnderlyingType==fullName);
        if (oldType!=null)
          return oldType.TypeId;
      }

      return TypeInfo.NoTypeId;
    }

    // Constructors

    public TypeIdProvider(UpgradeContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(context, "context");
      this.context = context;
    }
  }
}