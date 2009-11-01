// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23


using System;

namespace Xtensive.Core.Testing
{
  [Serializable]
  internal class NullableInstanceGenerator<T> : WrappingInstanceGenerator<T?, T>
    where T: struct
  {
    private const int nullProbabilityFactor = 100;

    public override T? GetInstance(Random random)
    {
      if (random.Next(nullProbabilityFactor)==0)
        return default(T);
      else
        return BaseGenerator.GetInstance(random);
    }


    // Constructors

    public NullableInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}