// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

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

      // Different entire & tuple test
      Entire<Tuple> entire1 = new Entire<Tuple>(tuple1);
      Entire<Tuple> entire2 = new Entire<Tuple>(tuple2);
      Assert.IsTrue(entire1.Equals(tuple1));
      Assert.IsTrue(entire1.CompareTo(tuple2)  < 0);
      Assert.IsTrue(entire1.CompareTo(entire2) < 0);

      // Exact equals
      Assert.IsTrue(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.Exact)));
      

      // Nearest positive equals
      entire2 = new Entire<Tuple>(tuple2, Direction.Positive);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.Exact)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)));

      // Nearest negative equals
      entire2 = new Entire<Tuple>(tuple2, Direction.Negative);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Positive)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)));

      // Negative infinity equals
      entire2 = new Entire<Tuple>(InfinityType.Negative);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)));

      // Positive infinity equals
      entire2 = new Entire<Tuple>(InfinityType.Positive);
      Assert.IsFalse(entire2.Equals(tuple2));
      Assert.IsTrue(entire2.Equals(entire2));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.Exact)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, Direction.Negative)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2,EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)));
      Assert.IsTrue(entire2.Equals(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)));

      // Exact compare
      entire2 = new Entire<Tuple>(tuple2);
      Assert.IsTrue(entire2.CompareTo(tuple2) == 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.Exact)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)) < 0);

      // Nearest positive compare
      entire2 = new Entire<Tuple>(tuple2, Direction.Positive);
      Assert.IsTrue(entire2.CompareTo(tuple2) > 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Positive)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) < 0);

      // Nearest negative compare
      entire2 = new Entire<Tuple>(tuple2, Direction.Negative);
      Assert.IsTrue(entire2.CompareTo(tuple2) < 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Negative)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) > 0);

      // Negative infinity compare
      entire2 = new Entire<Tuple>(InfinityType.Negative);
      Assert.IsTrue(entire2.CompareTo(tuple2) < 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.Exact)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Positive)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Negative)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Positive)) < 0 );
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Negative)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)) == 0);

      // Positive infinity compare
      entire2 = new Entire<Tuple>(InfinityType.Positive);
      Assert.IsTrue(entire2.CompareTo(tuple2) > 0);
      Assert.IsTrue(entire2.CompareTo(entire2) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.Exact)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Positive)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, Direction.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Positive)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple2, EntireValueType.PositiveInfinity)) == 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinitesimal)) > 0);
      Assert.IsTrue(entire2.CompareTo(new Entire<Tuple>(tuple1, EntireValueType.PositiveInfinity)) == 0);

    }

    [Test]
    public void EntireTest()
    {
      // Exact equals
      Assert.IsTrue(new Entire<int>(1).Equals(1));
      Assert.IsTrue(new Entire<int>(1).Equals(new Entire<int>(1)));
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
  }
}