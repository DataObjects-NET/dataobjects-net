// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.19

using System;

namespace Xtensive.Orm.Building.Builders
{
  internal interface ITypeIdProvider
  {
    int GetTypeId(Type type);
  }
}