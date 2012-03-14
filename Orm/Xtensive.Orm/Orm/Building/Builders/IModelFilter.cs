// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.14

using System;
using System.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal interface IModelFilter
  {
    bool IsTypeAvailable(Type type);
    bool IsFieldAvailable(PropertyInfo field);
  }
}