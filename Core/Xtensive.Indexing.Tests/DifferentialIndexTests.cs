// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.13

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing.Composite;
using Xtensive.Indexing.Differential;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class DifferentialIndexTests
  {
    private static IInstanceGenerator<int> instanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<int>();

    [Test]
    public void TestConstruction()
    {
      AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      //PopulateIndex(origin);
      
      //for (int i=1; i<=5; i++)
      //  origin.Add(i);

      //for (int i = 1; i <= 15; i = i + 3)
      //  origin.Add(i);


      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      DifferentialIndex<int, int, SortedListIndex<int, int>> index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);

      Assert.IsNotNull(index);

      //#region  Add, Remove, Clear

      //index.Clear();
      //for (int i = 1; i <= 15; i = i + 3)
      //  index.Add(i);
      //index.Add(2);
      //index.Add(3);
      //index.Add(5);
      ////for (int i = 1; i <= 6; i++)
      //  index.Remove(4);
      //  index.Remove(6);
      //  index.Add(6);

      //int current;
      //DifferentialIndexReader<int, int, SortedListIndex<int, int>> reader = new DifferentialIndexReader<int, int, SortedListIndex<int, int>>(index, Range<IEntire<int>>.Full);
      //reader.MoveTo(Entire<int>.Create(1));
      //while (reader.MoveNext())
      //  current = reader.Current;
      //reader.MoveTo(Entire<int>.Create(2));
      //reader.MoveNext();
      //current = reader.Current;
      //reader.MoveTo(Entire<int>.Create(4));
      //reader.MoveNext();
      //current = reader.Current;
      //reader.MoveTo(Entire<int>.Create(6));
      //reader.MoveNext();
      //current = reader.Current;


      //#endregion

      //for (int i = 1; i <= 5; i++)
      //{
      //  index.GetItem(i);
      //}





      IndexTest.TestIndex(index,
           new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100));



      //AdvancedComparer<IEntire<Tuple>> comparer =
      //  AdvancedComparer<IEntire<Tuple>>.Default.Cast<IEntire<Tuple>>();
      //Composite.IndexConfiguration<Tuple,Tuple> compositeIndexConfig = new Composite.IndexConfiguration<Tuple,Tuple>();
      //compositeIndexConfig.KeyExtractor = delegate(Tuple item) { return new SegmentTransform(false, item.Descriptor, new Segment<int>(0,1))
      //                                                                        .Apply(TupleTransformType.TransformedTuple, item); };
      //compositeIndexConfig.KeyComparer = AdvancedComparer<Tuple>.Default;

      //IndexSegmentConfiguration<Tuple, Tuple> segmentConfig1 = new IndexSegmentConfiguration<Tuple, Tuple>("first");
      //compositeIndexConfig.Segments.Add(segmentConfig1);

      //IndexSegmentConfiguration<Tuple, Tuple> segmentConfig2 = new IndexSegmentConfiguration<Tuple, Tuple>("second");
      //compositeIndexConfig.Segments.Add(segmentConfig2);

      //CompositeIndex<Tuple, Tuple> compositeIndex =
      //  IndexFactory.CreateComposite<Tuple, Tuple, SortedListIndex<Tuple, Tuple>>(compositeIndexConfig);
      //Assert.IsNotNull(compositeIndex);

      //Assert.AreEqual(2, compositeIndex.Segments.Count);

      //IndexSegment<Tuple, Tuple> indexSegment1 = compositeIndex.Segments["first"];
      //IndexSegment<Tuple, Tuple> indexSegment2 = compositeIndex.Segments["second"];

      //Assert.IsNotNull(indexSegment1);
      //Assert.IsNotNull(indexSegment2);

      //PopulateIndex(indexSegment1);
      //int count1 = (int) indexSegment1.Count;
      //Assert.AreEqual(0, indexSegment2.Count);
      //PopulateIndex(indexSegment2);
      //int count2 = (int) indexSegment2.Count;
      //Assert.AreEqual(count1, indexSegment1.Count);
      //indexSegment1.Clear();
      //Assert.AreEqual(0, indexSegment1.Count);
      //Assert.AreEqual(count2, indexSegment2.Count);
      //indexSegment2.Clear();
      //Assert.AreEqual(0, indexSegment1.Count);
      //Assert.AreEqual(0, indexSegment2.Count);

      //PopulateIndex(indexSegment1);
      //count1 = (int) indexSegment1.Count;
      //IndexTest.TestIndex(indexSegment1,
      //                    new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100));

    }

    private static void PopulateIndex(IIndex<int, int> index)
    {
      IEnumerator<int> enumerator = instanceGenerator.GetInstances(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100).GetEnumerator();
      while (enumerator.MoveNext())
      {
        int item = enumerator.Current;
          if (!index.Contains(item))
            index.Add(item);
      }
      
    }
  }
}
