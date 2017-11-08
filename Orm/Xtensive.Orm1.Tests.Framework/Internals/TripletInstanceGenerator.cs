// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25


using System;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  [Serializable]
  internal class TripletInstanceGenerator<T1, T2, T3>: WrappingInstanceGenerator<Triplet<T1, T2, T3>, T1, T2, T3>
  {
    public override Triplet<T1, T2, T3> GetInstance(Random random)
    {
      return new Triplet<T1, T2, T3>(
        BaseGenerator1.GetInstance(random),
        BaseGenerator2.GetInstance(random),
        BaseGenerator3.GetInstance(random));
    }


    // Constructors

    public TripletInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
    }
  }
}