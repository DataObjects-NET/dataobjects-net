// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class Int32InstanceGenerator : InstanceGeneratorBase<int>
  {
    public override int GetInstance(Random random)
    {
      return random.Next(int.MinValue, int.MaxValue);
    }


    // Constructors

    public Int32InstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}