// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal class BooleanInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<bool>(provider)
  {
    public override bool GetInstance(Random random)
    {
      unchecked {
        byte[] randomByte = {(byte)random.Next(0, 2)};

        return BitConverter.ToBoolean(randomByte, 0);
      }
    }
  }
}