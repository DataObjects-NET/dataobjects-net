// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.18


using System;

namespace Xtensive.Orm.Tests
{
  internal sealed class CharInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<char>(provider)
  {
    public override char GetInstance(Random random)
    {
      var  instance = (char)random.Next(char.MinValue, char.MaxValue + 1);
      // Prevent surrogate characters.
      while(char.IsSurrogate(instance)) {
        instance = (char)random.Next(char.MinValue, char.MaxValue + 1);
      }
      return instance;
    }
  }
}