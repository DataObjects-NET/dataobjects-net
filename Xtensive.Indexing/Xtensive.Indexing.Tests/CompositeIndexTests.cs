// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.03

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing.Composite;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class CompositeIndexTests
  {
    private static IInstanceGenerator<Tuple> instanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();

    [Test]
    public void TestConstruction()
    {
      AdvancedComparer<Entire<Tuple>> comparer =
        AdvancedComparer<Entire<Tuple>>.Default.Cast<Entire<Tuple>>();
      Composite.IndexConfiguration<Tuple,Tuple> compositeIndexConfig = new Composite.IndexConfiguration<Tuple,Tuple>();
      compositeIndexConfig.KeyExtractor = delegate(Tuple item) { return new SegmentTransform(false, item.Descriptor, new Segment<int>(0,1))
                                                                              .Apply(TupleTransformType.TransformedTuple, item); };
      compositeIndexConfig.KeyComparer = AdvancedComparer<Tuple>.Default;

      IndexSegmentConfiguration<Tuple, Tuple> segmentConfig1 = new IndexSegmentConfiguration<Tuple, Tuple>("first");
      compositeIndexConfig.Segments.Add(segmentConfig1);

      IndexSegmentConfiguration<Tuple, Tuple> segmentConfig2 = new IndexSegmentConfiguration<Tuple, Tuple>("second");
      compositeIndexConfig.Segments.Add(segmentConfig2);

      CompositeIndex<Tuple, Tuple> compositeIndex =
        IndexFactory.CreateComposite<Tuple, Tuple, SortedListIndex<Tuple, Tuple>>(compositeIndexConfig);
      Assert.IsNotNull(compositeIndex);

      Assert.AreEqual(2, compositeIndex.Segments.Count);

      IndexSegment<Tuple, Tuple> indexSegment1 = compositeIndex.Segments["first"];
      IndexSegment<Tuple, Tuple> indexSegment2 = compositeIndex.Segments["second"];

      Assert.IsNotNull(indexSegment1);
      Assert.IsNotNull(indexSegment2);

      PopulateIndex(indexSegment1);
      int count1 = (int) indexSegment1.Count;
      Assert.AreEqual(0, indexSegment2.Count);
      PopulateIndex(indexSegment2);
      int count2 = (int) indexSegment2.Count;
      Assert.AreEqual(count1, indexSegment1.Count);
      indexSegment1.Clear();
      Assert.AreEqual(0, indexSegment1.Count);
      Assert.AreEqual(count2, indexSegment2.Count);
      indexSegment2.Clear();
      Assert.AreEqual(0, indexSegment1.Count);
      Assert.AreEqual(0, indexSegment2.Count);

      PopulateIndex(indexSegment1);
      count1 = (int) indexSegment1.Count;
      IndexTest.TestIndex(indexSegment1,
                          new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100));

    }

    private static void PopulateIndex(IIndex<Tuple, Tuple> index)
    {
      IEnumerator<Tuple> enumerator = instanceGenerator.GetInstances(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100).GetEnumerator();
      while (enumerator.MoveNext())
      {
        Tuple item = enumerator.Current;
          if (!index.Contains(item))
            index.Add(item);
      }
      
    }
  }
}
