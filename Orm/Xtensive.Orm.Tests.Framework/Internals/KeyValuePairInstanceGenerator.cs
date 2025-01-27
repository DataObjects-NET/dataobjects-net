// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class ArrayInstanceGenerator<T>: WrappingInstanceGenerator<T[], T>
  {
    public const int ArrayLength = 100;

    public override T[] GetInstance(Random random)
    {
      T[] result = new T[ArrayLength];
      int i = 0;
      foreach (T t in BaseGenerator.GetInstances(random, ArrayLength)) {
        result[i] = t;
        i++;
      }
      return result;
    }


    // Constructors

    public ArrayInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}