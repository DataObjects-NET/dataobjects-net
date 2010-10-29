// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;
using System.Collections.Generic;

namespace Xtensive.Testing
{
  [Serializable]
  internal class KeyValuePairInstanceGenerator<T1, T2>: WrappingInstanceGenerator<KeyValuePair<T1, T2>, T1, T2>
  {
    public override KeyValuePair<T1, T2> GetInstance(Random random)
    {
      return new KeyValuePair<T1, T2>(
        BaseGenerator1.GetInstance(random),
        BaseGenerator2.GetInstance(random));
    }


    // Constructors

    public KeyValuePairInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}