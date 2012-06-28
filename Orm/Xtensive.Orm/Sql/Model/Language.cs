// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.25

using System;

namespace Xtensive.Sql.Model
{
  [Serializable]
  public class Language : Node
  {
    public Language(string name)
    {
      Name = name;
    }
  }
}