// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class UInt32InstanceGenerator(IInstanceGeneratorProvider provider)
    : WrappingInstanceGenerator<uint, int>(provider)
  {
    public override uint GetInstance(Random random)
    {
      unchecked {
        return (uint)BaseGenerator.GetInstance(random);
      }
    }
  }
}