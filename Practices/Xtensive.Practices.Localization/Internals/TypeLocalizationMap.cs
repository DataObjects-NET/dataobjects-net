// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Practices.Localization.Internals
{
  internal class TypeLocalizationMap
  {
    private readonly Dictionary<Type, TypeLocalizationInfo> map = new Dictionary<Type, TypeLocalizationInfo>();

    public void Register(Type localizableType, TypeInfo localizationType)
    {
      map[localizableType] = new TypeLocalizationInfo(localizableType, localizationType);
    }

    public TypeLocalizationInfo Get(Type localizableType)
    {
      TypeLocalizationInfo result;
      if (map.TryGetValue(localizableType, out result))
        return result;
      return null;
    }
  }
}