// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Reflection;
using Xtensive.Orm.Model;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Localization
{
  internal class TypeLocalizationInfo
  {
    public Type LocalizableType { get; private set; }

    public Type LocalizationType { get { return LocalizationTypeInfo.UnderlyingType; } }

    public TypeInfo LocalizationTypeInfo { get; private set; }

    public PropertyInfo LocalizationSetProperty { get; private set; }


    // Constructor

    public TypeLocalizationInfo(Type localizableType, TypeInfo localizationType)
    {
      LocalizableType = localizableType;
      LocalizationTypeInfo = localizationType;
      LocalizationSetProperty = localizableType.GetProperty("Localizations");
    }
  }
}