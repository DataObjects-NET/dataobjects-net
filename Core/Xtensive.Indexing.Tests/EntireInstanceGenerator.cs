// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23


using System;
using Xtensive.Core;
using Xtensive.Testing;

namespace Xtensive.Indexing.Testing
{
  [Serializable]
  internal class EntireInstanceGenerator<T> : WrappingInstanceGenerator<Entire<T>, T>
    where T: struct
  {
    private const int infinityFactor = 100;
    private const int infinitestimalShiftFactor = 5;

    
    public override Entire<T> GetInstance(Random random)
    {
      if (random.Next(infinityFactor) == 0)        
          return new Entire<T>(random.Next(2)==0 ? InfinityType.Positive : InfinityType.Negative);
      if (random.Next(infinitestimalShiftFactor) == 0)
        return new Entire<T>(BaseGenerator.GetInstance(random),
                             random.Next(2)==0 ? Direction.Positive : Direction.Negative);
      return new Entire<T>(BaseGenerator.GetInstance(random));
    }


    // Constructors

    public EntireInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}