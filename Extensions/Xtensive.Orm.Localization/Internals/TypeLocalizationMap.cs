// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Collections.Generic;
using Xtensive.Orm.Localization.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm;
using Xtensive.Reflection;

namespace Xtensive.Orm.Localization
{
  internal class TypeLocalizationMap
  {
    private readonly Dictionary<Type, TypeLocalizationInfo> map = new Dictionary<Type, TypeLocalizationInfo>();

    public LocalizationConfiguration Configuration { get; private set; }

    public static void Initialize(Domain domain)
    {
      if (domain == null)
        throw new ArgumentNullException("domain");
      if (domain.Extensions.Get<TypeLocalizationMap>() != null)
        return;

      var map = new TypeLocalizationMap() {
        Configuration = LocalizationConfiguration.Load()
      };
      foreach (var localizableTypeInfo in domain.Model.Types.Entities) {
        var type = localizableTypeInfo.UnderlyingType;
        if (!type.IsOfGenericInterface(typeof (ILocalizable<>)))
          continue;
        var localizationType = type.GetInterface("ILocalizable`1").GetGenericArguments()[0];
        map.Register(type, domain.Model.Types[localizationType]);
      }
      domain.Extensions.Set(map);
    }

    public TypeLocalizationInfo Get(Type localizableType)
    {
      TypeLocalizationInfo result;
      if (map.TryGetValue(localizableType, out result))
        return result;
      return null;
    }

    private void Register(Type localizableType, TypeInfo localizationType)
    {
      map[localizableType] = new TypeLocalizationInfo(localizableType, localizationType);
    }
  }
}