// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.23

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Core.Tests.Tuples
{
  [TestFixture]
  public class DifferentialTupleTest : TupleBehaviorTestBase
  {
    [Test]
    public void DifferentialTest()
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(fieldTypes);
      Tuple t1 = Tuple.Create(descriptor);
      Tuple t2 = t1.Clone();
      PopulateData(fieldTypes, t2, t1);

      DifferentialTuple d = new DifferentialTuple(t1);
      AssertAreSame(t1, d);

      PopulateData(fieldTypes, t1, d);
      AssertAreSame(t1, d);
      DifferentialTuple c = (DifferentialTuple)d.Clone();
      AssertAreSame(d, c);

      d.Reset();
      AssertAreSame(t2, d);
      AssertAreSame(t1, c.ToRegular());
    }

    [Test]
    public void Test()
    {
      base.Test();
    }

    [Test]
    public void BehaviorTest()
    {
      base.BehaviorTest();
    }

    [Test]
    public void EmptyFieldsTest()
    {
      base.EmptyFieldsTest();
    }

    [Test]
    public void RandomTest()
    {
      base.RandomTest();
    }

    protected override Tuple CreateTestTuple(TupleDescriptor descriptor)
    {
      return new DifferentialTuple(Tuple.Create(descriptor));
    }

    protected virtual Tuple CreateTestTuple(Tuple source)
    {
      return new DifferentialTuple(source);
    }
  }
}