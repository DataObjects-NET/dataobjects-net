// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.15

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.Rse.AnimalModel;


namespace Xtensive.Orm.Tests.Rse.AnimalModel
{
  [Serializable]
  [DebuggerDisplay("Name = '{Name}'")]
  [HierarchyRoot]
  [Index("Name")]
  public class Animal : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public int Name { get; set; }
  }

  [Serializable]
  [DebuggerDisplay("Name = '{Name}'")]
  public class Cat : Animal
  {
  }


  [Serializable]
  [DebuggerDisplay("Name = '{Name}'; TailLength = {TailLength}")]
  [Index("Name", "TailLength")]
  public class Dog : Animal
  {
    [Field]
    public int? TailLength { get; set; }
  }

  [Serializable]
  [DebuggerDisplay("Name = '{Name}'; Airspeed = {Airspeed}; Lifetime = {Lifetime}")]
  [Index("Name", "Airspeed", "Lifetime")]
  public class Bird : Animal
  {
    [Field]
    public int? Airspeed { get; set; }

    [Field(Length = 255)]
    public int? Lifetime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Rse
{
  [Ignore("Not implemented.")]
  [Serializable]
  [TestFixture, Category("Rse")]
  public class ComplexRangeTest : AutoBuildTest
  {
    private RecordQuery rsCat;
    private RecordQuery rsDog;
    private RecordQuery rsBird;
    private const int CatCount = 100;
    private const int DogCount = 100;
    private const int BirdCount = 100;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration(); 
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Rse.AnimalModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Xtensive.Orm.Domain result = base.BuildDomain(configuration);
      return result;
    }

    [Test]
    public void LineTest()
    {
      Fill();
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsCat, range, CatCount);
      TestRange(rsDog, range, DogCount);
      TestRange(rsBird, range, BirdCount);
    }

    [Test]
    public void Ray1Test()
    {
      Fill();
      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10)));
      TestRange(rsCat, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10),Direction.Negative));
      TestRange(rsCat, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10), Direction.Positive));
      TestRange(rsCat, range, 10);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5)));
      TestRange(rsDog, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(5, 10)));
      TestRange(rsDog, range, 4);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5), Direction.Negative));
      TestRange(rsDog, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(5, 10), Direction.Negative));
      TestRange(rsDog, range, 4);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5), Direction.Positive));
      TestRange(rsDog, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(5, 10), Direction.Positive));
      TestRange(rsDog, range, 4);
      
      //Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5, 3)));
      TestRange(rsBird, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 3, 5)));
      TestRange(rsBird, range, 9); 
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(3, 10, 5)));
      TestRange(rsBird, range, 2);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5, 3), Direction.Negative));
      TestRange(rsBird, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Negative));
      TestRange(rsBird, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(3, 10, 5), Direction.Negative));
      TestRange(rsBird, range, 2);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 5, 3), Direction.Positive));
      TestRange(rsBird, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Positive));
      TestRange(rsBird, range, 9);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(InfinityType.Negative), new Entire<Tuple>(Tuple.Create(3, 10, 5), Direction.Positive));
      TestRange(rsBird, range, 2);
    }

    [Test]
    public void Ray2Test()
    {
      Fill();
      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsCat, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsCat, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsCat, range, 10);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(95, 90)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 6);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(95, 95), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 6);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(95, 95), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsDog, range, 6);

      //Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95, 98)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 98, 95)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(98, 90, 95)), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 3);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95, 98), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 98, 90), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(98, 90, 95), Direction.Negative), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 3);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 95, 98), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(90, 98, 90), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 11);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(98, 90, 95), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 3);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(98, 95, 98), Direction.Positive), new Entire<Tuple>(InfinityType.Positive));
      TestRange(rsBird, range, 3);
    }

    [Test]
    public void IntervalTest()
    {
      Fill();
      
      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5), Direction.Positive), new Entire<Tuple>(Tuple.Create(10), Direction.Negative));
      TestRange(rsCat, range, 4);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10), Direction.Positive), new Entire<Tuple>(Tuple.Create(5), Direction.Negative));
      //TestRange(rsCat, range, 0);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5,6), Direction.Positive), new Entire<Tuple>(Tuple.Create(10,11), Direction.Negative));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10,11), Direction.Positive), new Entire<Tuple>(Tuple.Create(5,6), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 8), Direction.Negative));
      TestRange(rsCat, range, 3);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 10), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 8), Direction.Negative));
      TestRange(rsCat, range, 2);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 3), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 12), Direction.Negative));
      TestRange(rsCat, range, 2);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 10), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 11), Direction.Negative));
      //TestRange(rsCat, range, 0);

      // Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Negative));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Positive), new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Negative));
      //TestRange(rsCat, range, 0);
    }

    [Test]
    public void SegmentTest()
    {
      Fill();

      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5), Direction.Negative), new Entire<Tuple>(Tuple.Create(10), Direction.Positive));
      TestRange(rsCat, range, 6);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10), Direction.Negative), new Entire<Tuple>(Tuple.Create(5), Direction.Positive));
      //TestRange(rsCat, range, 0);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 11), Direction.Positive));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11), Direction.Negative), new Entire<Tuple>(Tuple.Create(5, 6), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 8), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 3), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 1), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 1), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 7), Direction.Positive));
      //TestRange(rsCat, range, 0);

      // Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Positive));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Negative), new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Positive));
      //TestRange(rsCat, range, 0);
    }

    [Test]
    public void HalfInterval1Test()
    {
      Fill();

      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5), Direction.Positive), new Entire<Tuple>(Tuple.Create(10), Direction.Positive));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10), Direction.Positive), new Entire<Tuple>(Tuple.Create(5), Direction.Positive));
      //TestRange(rsCat, range, 0);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 11), Direction.Positive));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11), Direction.Positive), new Entire<Tuple>(Tuple.Create(5, 6), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 8), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 3), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 1), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 1), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 7), Direction.Positive));
      //TestRange(rsCat, range, 0);

      // Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Positive));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Positive), new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Positive));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Positive), new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Positive));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Positive), new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Positive));
      //TestRange(rsCat, range, 0);
    }

    [Test]
    public void HalfInterval2Test()
    {
      Fill();

      // Cat
      var range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5), Direction.Negative), new Entire<Tuple>(Tuple.Create(10), Direction.Negative));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10), Direction.Negative), new Entire<Tuple>(Tuple.Create(5), Direction.Negative));
      //TestRange(rsCat, range, 0);

      // Dog
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 11), Direction.Negative));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11), Direction.Negative), new Entire<Tuple>(Tuple.Create(5, 6), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 8), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 3), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 1), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 1), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 7), Direction.Negative));
      //TestRange(rsCat, range, 0);

      // Bird
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Negative));
      TestRange(rsCat, range, 5);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 11, 12), Direction.Negative), new Entire<Tuple>(Tuple.Create(5, 6, 7), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 8, 6), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 3, 5), Direction.Negative));
      //TestRange(rsCat, range, 0);
      range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Negative), new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Negative));
      TestRange(rsCat, range, 3);
      //range = new Range<Entire<Tuple>>(new Entire<Tuple>(Tuple.Create(13, 3, 1), Direction.Negative), new Entire<Tuple>(Tuple.Create(10, 7, 5), Direction.Negative));
      //TestRange(rsCat, range, 0);
    } 

    #region Private methods

    private void TestRange(RecordQuery recordQuery, Range<Entire<Tuple>> range, int testCount)
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var parameter = new Parameter<Range<Entire<Tuple>>>();
          RecordQuery result = recordQuery
            .Range(() => parameter.Value)
            .OrderBy(OrderBy.Asc(recordQuery.Header.IndexOf("ID")));

          using (new ParameterContext().Activate()) {
            parameter.Value = range;
            var count = result.Count(Session.Current);
            Assert.AreEqual(testCount, count);
          }
          t.Complete();
        }
      }
    }

    private void Fill()
    {
      TestFixtureTearDown();
      TestFixtureSetUp();

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (int i = 1; i <= CatCount; i++)
          new Cat { Name = i };
        for (int i = 1; i <= DogCount; i++)
          new Dog { Name = i, TailLength = i };
        for (int i = 1; i <= BirdCount; i++)
          new Bird { Name = i, Airspeed = i, Lifetime = i };
        Session.Current.SaveChanges();
        t.Complete();
        GetRecordSets();
      }
    }

    private void GetRecordSets()
    {
      TypeInfo animalType = Domain.Model.Types[typeof(Cat)];
      rsCat = animalType.Indexes.GetIndex("Name").ToRecordQuery();
      TypeInfo dogType = Domain.Model.Types[typeof(Dog)];
      rsDog = dogType.Indexes.GetIndex("Name", "TailLength").ToRecordQuery();
      TypeInfo birdType = Domain.Model.Types[typeof(Bird)];
      rsBird = birdType.Indexes.GetIndex("Name", "Airspeed", "Lifetime").ToRecordQuery();
    }

    #endregion
  }
}