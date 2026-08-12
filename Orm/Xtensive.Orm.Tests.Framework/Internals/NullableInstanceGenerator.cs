// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.23


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class NullableInstanceGenerator<T>(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<T?, T>(provider)
    where T: struct
  {
    private const int NullProbabilityFactor = 100;

    public override T? GetInstance(Random random) =>
      random.Next(NullProbabilityFactor) == 0
        ? default
        : BaseGenerator.GetInstance(random);
  }
}