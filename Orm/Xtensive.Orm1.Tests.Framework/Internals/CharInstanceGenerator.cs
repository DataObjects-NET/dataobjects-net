// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class CharInstanceGenerator : InstanceGeneratorBase<char>
  {
    public override char GetInstance(Random random)
    {
      char instance = (char)random.Next(char.MinValue, char.MaxValue + 1);
      // Prevent surrogate characters.
      while(char.IsSurrogate(instance)) {
        instance = (char)random.Next(char.MinValue, char.MaxValue + 1);
      }
      return instance;
    }


    // Constructors

    public CharInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}