// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23


using System;

namespace Xtensive.Testing
{
  [Serializable]
  internal class EnumInstanceGenerator<TEnum, TSystem> : WrappingInstanceGenerator<TEnum, TSystem>
    where TEnum: struct
    where TSystem: struct
  {
    private static readonly Array values = Enum.GetValues(typeof(TEnum));

    public override TEnum GetInstance(Random random)
    {
      return (TEnum) values.GetValue(random.Next(values.Length));
    }


    // Constructors

    public EnumInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}