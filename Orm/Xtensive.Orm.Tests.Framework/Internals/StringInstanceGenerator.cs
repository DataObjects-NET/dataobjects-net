// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.22


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class StringInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<string>(provider)
  {
    public override string GetInstance(Random random)
    {
      return "Random String " + random.Next();
    }
  }
}