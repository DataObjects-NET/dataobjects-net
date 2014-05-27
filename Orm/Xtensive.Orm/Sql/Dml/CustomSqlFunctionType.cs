// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class CustomSqlFunctionType
  {
    public readonly string Name;

    // Constructors

    public CustomSqlFunctionType(string name)
    {
      Name = name;
    }
  }
}
