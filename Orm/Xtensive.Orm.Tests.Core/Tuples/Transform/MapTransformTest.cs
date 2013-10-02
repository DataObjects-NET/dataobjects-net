// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using NUnit.Framework;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples.Transform
{
  [TestFixture]
  public class MapTransformTest
  {
    public void MainTest()
    {
      Xtensive.Tuples.Tuple source = Tuple.Create(1);
      MapTransform transform = new MapTransform(true, TupleDescriptor.Create<byte, int, string>(), new[] {-1, 0});
      Xtensive.Tuples.Tuple result = transform.Apply(TupleTransformType.TransformedTuple, source);
      Assert.AreEqual(TupleFieldState.Default, result.GetFieldState(0));
      Assert.AreEqual(TupleFieldState.Available, result.GetFieldState(1));
      Assert.AreEqual(TupleFieldState.Default, result.GetFieldState(2));
    }
  }
}