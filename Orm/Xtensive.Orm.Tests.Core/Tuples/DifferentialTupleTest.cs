// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.23

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class DifferentialTupleTest : TupleBehaviorTestBase
  {
    [Test]
    public void DifferentialTest()
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(fieldTypes);
      Xtensive.Tuples.Tuple t1 = Tuple.Create(descriptor);
      Xtensive.Tuples.Tuple t2 = t1.Clone();
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
    public new void Test()
    {
      base.Test();
    }

    [Test]
    public new void BehaviorTest()
    {
      base.BehaviorTest();
    }

    [Test]
    public new void EmptyFieldsTest()
    {
      base.EmptyFieldsTest();
    }

    [Test]
    public new void RandomTest()
    {
      base.RandomTest();
    }

    protected override Xtensive.Tuples.Tuple CreateTestTuple(TupleDescriptor descriptor)
    {
      return new DifferentialTuple(Tuple.Create(descriptor));
    }

    protected override Xtensive.Tuples.Tuple CreateTestTuple(Xtensive.Tuples.Tuple source)
    {
      return new DifferentialTuple(source);
    }
  }
}