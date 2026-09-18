// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests
{
  internal sealed class ArrayInstanceGenerator<T>(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<T[], T>(provider)
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
  }
}