﻿// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;

namespace Xtensive.Core.Testing
{
  [Serializable]
  internal class PairInstanceGenerator<T1, T2>: WrappingInstanceGenerator<Pair<T1, T2>, T1, T2>
  {
    public override Pair<T1, T2> GetInstance(Random random)
    {
      return new Pair<T1, T2>(
        BaseGenerator1.GetInstance(random),
        BaseGenerator2.GetInstance(random));
    }


    // Constructors

    public PairInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}