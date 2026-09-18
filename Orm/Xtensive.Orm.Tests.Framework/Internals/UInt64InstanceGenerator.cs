// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;


namespace Xtensive.Orm.Tests
{
  internal sealed class UInt64InstanceGenerator(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<ulong, long>(provider)
  {
    public override ulong GetInstance(Random random)
    {
      unchecked {
        return (ulong)BaseGenerator.GetInstance(random);
      }
    }
  }
}