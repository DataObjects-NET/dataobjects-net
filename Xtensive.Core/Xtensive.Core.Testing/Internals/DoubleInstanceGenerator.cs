// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class DoubleInstanceGenerator : InstanceGeneratorBase<double>
  {
    public override double GetInstance(Random random)
    {
      return random.NextDouble();
    }


    // Constructors

    public DoubleInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}