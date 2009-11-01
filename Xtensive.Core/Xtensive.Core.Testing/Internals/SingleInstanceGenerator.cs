﻿// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Core.Testing
{
  [Serializable]
  internal class SingleInstanceGenerator : InstanceGeneratorBase<float>
  {

    public override float GetInstance(Random random)
    {
      unchecked {
        return (float)random.NextDouble();
      }
    }


    // Constructors

    public SingleInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}