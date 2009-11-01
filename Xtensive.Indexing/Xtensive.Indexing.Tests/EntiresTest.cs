// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class EntiresTest
  {
    [Test]
    public void TupleEntireTest()
    {
      Tuple tuple1 = Tuple.Create(0);
      Tuple tuple2 = Tuple.Create(0, 1);

      TestBehavior(Entire<Tuple>.Create(tuple2), EntireValueType.Exact, EntireValueType.Exact);
      TestBehavior(Entire<Tuple>.Create(InfinityType.Positive), EntireValueType.PositiveInfinity, EntireValueType.PositiveInfinity);
      TestBehavior(Entire<Tuple>.Create(tuple2, Direction.Positive), EntireValueType.Default, EntireValueType.PositiveInfinitesimal);
      TestBehavior(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact), EntireValueType.PositiveInfinitesimal, EntireValueType.Exact);

      // Different entire & tuple test
      IEntire<Tuple> entire1 = Entire<Tuple>.Create(tuple1);
      IEntire<Tuple> entire2 = Entire<Tuple>.Create(tuple2);
      Assert.IsTrue(entire1.Equals(tuple1));
      Assert.IsTrue(entire1.CompareTo(tuple2)  < 0);
      Assert.IsTrue(entire1.CompareTo(entire2) < 0);

      // Exact equals
      Assert.IsTrue(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)));

      // Nearest positive equals
      entire2 = Entire<Tuple>.Create(tuple2, Direction.Positive);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.PositiveInfinitesimal)));

      // Nearest negative equals
      entire2 = Entire<Tuple>.Create(tuple2, Direction.Negative);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Positive)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.NegativeInfinitesimal)));

      // Negative infinity equals
      entire2 = Entire<Tuple>.Create(InfinityType.Negative);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.NegativeInfinity)));

      // Positive infinity equals
      entire2 = Entire<Tuple>.Create(InfinityType.Positive);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, Direction.Negative)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsTrue(entire2.Equals(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.PositiveInfinity)));

      // Exact compare
      entire2 = Entire<Tuple>.Create(tuple2);
      Assert.IsTrue(entire2.CompareTo(tuple2) == 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);

      // Nearest positive compare
      entire2 = Entire<Tuple>.Create(tuple2, Direction.Positive);
      Assert.IsTrue(entire2.CompareTo(tuple2) > 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Positive)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.PositiveInfinitesimal)) < 0);

      // Nearest negative compare
      entire2 = Entire<Tuple>.Create(tuple2, Direction.Negative);
      Assert.IsTrue(entire2.CompareTo(tuple2) < 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Negative)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.NegativeInfinitesimal)) > 0);

      // Negative infinity compare
      entire2 = Entire<Tuple>.Create(InfinityType.Negative);
      Assert.IsTrue(entire2.CompareTo(tuple2) < 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Negative)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Positive)) < 0 );
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Negative)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.NegativeInfinity)) == 0);

      // Positive infinity compare
      entire2 = Entire<Tuple>.Create(InfinityType.Positive);
      Assert.IsTrue(entire2.CompareTo(tuple2) > 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Positive)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Positive)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.Exact, EntireValueType.PositiveInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.Exact)) == 0);
      Assert.IsTrue(entire2.CompareTo(Entire<Tuple>.Create(tuple2, EntireValueType.PositiveInfinity, EntireValueType.PositiveInfinity)) == 0);

    }

    [Test]
    public void EntireTest()
    {
      TestBehavior(Entire<int>.Create(1), EntireValueType.Exact);
      TestBehavior(Entire<int>.Create(InfinityType.Positive), EntireValueType.PositiveInfinity);
      TestBehavior(Entire<int>.Create(1, EntireValueType.PositiveInfinitesimal), EntireValueType.PositiveInfinitesimal);

      // Exact equals
      Assert.IsTrue(new Entire<int>(1).Equals(1));
      Assert.IsTrue(Entire<int>.Create(1).Equals(Entire<int>.Create(1)));
      Assert.IsFalse(new Entire<int>(1).Equals(new Entire<int>(1, Direction.Positive)));
      Assert.IsFalse(new Entire<int>(1).Equals(new Entire<int>(1, Direction.Negative)));
      Assert.IsFalse(new Entire<int>(1).Equals(new Entire<int>(InfinityType.Negative)));
      Assert.IsFalse(new Entire<int>(1).Equals(new Entire<int>(InfinityType.Positive)));

      // Nearest positive equals
      Assert.IsFalse(new Entire<int>(1, Direction.Positive).Equals(2));
      Assert.IsFalse(new Entire<int>(1, Direction.Positive).Equals(new Entire<int>(2)));
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).Equals(new Entire<int>(1, Direction.Positive)));
      Assert.IsFalse(new Entire<int>(1, Direction.Positive).Equals(new Entire<int>(1, Direction.Negative)));
      Assert.IsFalse(new Entire<int>(1, Direction.Positive).Equals(new Entire<int>(InfinityType.Positive)));
      Assert.IsFalse(new Entire<int>(1, Direction.Positive).Equals(new Entire<int>(InfinityType.Negative)));

      // Nearest negative equals
      Assert.IsFalse(new Entire<int>(1, Direction.Negative).Equals(0));
      Assert.IsFalse(new Entire<int>(1, Direction.Negative).Equals(new Entire<int>(0)));
      Assert.IsFalse(new Entire<int>(1, Direction.Negative).Equals(new Entire<int>(1, Direction.Positive)));
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).Equals(new Entire<int>(1, Direction.Negative)));
      Assert.IsFalse(new Entire<int>(1, Direction.Negative).Equals(new Entire<int>(InfinityType.Positive)));
      Assert.IsFalse(new Entire<int>(1, Direction.Negative).Equals(new Entire<int>(InfinityType.Negative)));

      // Infinity equals
      Assert.IsTrue(new Entire<int>(InfinityType.Positive).Equals(new Entire<int>(InfinityType.Positive)));
      Assert.IsTrue(new Entire<int>(InfinityType.Negative).Equals(new Entire<int>(InfinityType.Negative)));
      Assert.IsFalse(new Entire<int>(InfinityType.Positive).Equals(new Entire<int>(InfinityType.Negative)));

      // Exact compare
      Assert.IsTrue(new Entire<int>(1).CompareTo(1) == 0);
      Assert.IsTrue(new Entire<int>(1).CompareTo(new Entire<int>(1)) == 0);
      Assert.IsTrue(new Entire<int>(1).CompareTo(new Entire<int>(1, Direction.Positive)) < 0);
      Assert.IsTrue(new Entire<int>(1).CompareTo(new Entire<int>(1, Direction.Negative)) > 0);
      Assert.IsTrue(new Entire<int>(1).CompareTo(new Entire<int>(InfinityType.Negative)) > 0);
      Assert.IsTrue(new Entire<int>(1).CompareTo(new Entire<int>(InfinityType.Positive)) < 0);

      // Nearest compare
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(2) < 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(1) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(0) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(1) < 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(new Entire<int>(2)) < 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(new Entire<int>(0)) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(new Entire<int>(1, Direction.Positive)) == 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(new Entire<int>(1, Direction.Negative)) == 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(new Entire<int>(1, Direction.Negative)) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(new Entire<int>(InfinityType.Negative)) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Positive).CompareTo(new Entire<int>(InfinityType.Positive)) < 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(new Entire<int>(InfinityType.Negative)) > 0);
      Assert.IsTrue(new Entire<int>(1, Direction.Negative).CompareTo(new Entire<int>(InfinityType.Positive)) < 0);

      // Infinity compare
      Assert.IsTrue(new Entire<int>(InfinityType.Positive).CompareTo(new Entire<int>(InfinityType.Positive)) == 0);
      Assert.IsTrue(new Entire<int>(InfinityType.Negative).CompareTo(new Entire<int>(InfinityType.Negative)) == 0);
      Assert.IsTrue(new Entire<int>(InfinityType.Positive).CompareTo(new Entire<int>(InfinityType.Negative)) > 0);

    }

    private static void TestBehavior<T>(IEntire<T> entire, params EntireValueType[] fieldValueTypes)
    {
      for (int index = 0, count = entire.Count; index < count; index++) {
        AssertEx.ThrowsNotSupportedException(delegate { entire.SetValue(index, 0); });
        AssertEx.ThrowsNotSupportedException(delegate { entire.SetValue(index, 0); });

        EntireValueType fieldValueType = entire.GetValueType(index);
        Assert.AreEqual(fieldValueType, fieldValueTypes[index]);

        if (fieldValueType == EntireValueType.NegativeInfinity || fieldValueType == EntireValueType.PositiveInfinity) {
          AssertEx.ThrowsInvalidOperationException(delegate { entire.GetValue(index); });
          AssertEx.ThrowsInvalidOperationException(delegate { entire.GetValue<bool>(index); });
//          AssertEx.ThrowsInvalidOperationException(delegate { entire.GetValueOrDefault(index); });
//          AssertEx.ThrowsInvalidOperationException(delegate { entire.GetValueOrDefault<bool>(index); });
//          AssertEx.ThrowsInvalidOperationException(delegate { entire.IsNull(index); });
          Assert.IsFalse(entire.IsAvailable(index));
          Assert.IsFalse(entire.HasValue(index));
        }
        else {
          Assert.IsTrue(entire.IsAvailable(index));
          Assert.IsTrue(entire.HasValue(index));
          Assert.IsFalse(entire.IsNull(index));
        }
      }
    }
  }
}