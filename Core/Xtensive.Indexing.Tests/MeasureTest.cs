// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.15

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Indexing.SizeCalculators;
using Xtensive.Testing;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class MeasureTest
  {
    private readonly Random random = RandomManager.CreateRandom();
    private const int ITEM_AMOUNT = 1024;
    private readonly static string COUNT = CountMeasure<object, long>.CommonName;
    private readonly static string SIZE  = SizeMeasure<object>.CommonName;
    private readonly static string SUM_AGE = "SumAge";
    private readonly static string MIN_AGE = "MinAge";
    private readonly static string MAX_AGE = "MaxAge";

    #region Nested type: Animal

    private struct Animal
    {
      public readonly int Age;

      public Animal(int age)
      {
        Age = age;
      }
    }

    #endregion

    #region Nested type: AnimalCollection

    private class AnimalCollection : MeasurableCollection<Animal>
    {
      public static readonly Converter<Animal, int> AgeExtractor = delegate(Animal item) { return item.Age; };

      public AnimalCollection()
      {
        Measures.Add(new CountMeasure<Animal, int>());
        Measures.Add(new SumMeasure<Animal, int>(SUM_AGE, AgeExtractor));
        Measures.Add(new MinMeasure<Animal, int>(MIN_AGE, AgeExtractor));
        Measures.Add(new MaxMeasure<Animal, int>(MAX_AGE, AgeExtractor));
        Measures.Add(new SizeMeasure<Animal>());

        MeasureResults = new MeasureResultSet<Animal>(Measures);
      }
    }

    #endregion

    #region Nested type: MeasurableCollection

    private class MeasurableCollection<TItem> :
      CollectionBase<TItem>,
      IMeasurable<TItem>,
      IHasMeasureResults<TItem>
    {
      private IMeasureResultSet<TItem> measureResults;
      private IMeasureSet<TItem> measures = new MeasureSet<TItem>();

      protected override void OnInserted(TItem value, int index)
      {
        base.OnInserted(value, index);
        measureResults.Add(value);
      }

      protected override void OnCleared()
      {
        base.OnCleared();
        MeasureResults.Reset();
      }

      protected override void OnRemoved(TItem value, int index)
      {
        base.OnRemoved(value, index);
        if (!measureResults.Subtract(value))
          MeasureUtils<TItem>.BatchRecalculate(measureResults, this);
      }

      long ICountable.Count
      {
        get { return Count; }
      }

      #region IHasMeasuresResults Members

      public IMeasureResultSet<TItem> MeasureResults
      {
        get { return measureResults; }
        protected set { measureResults = value; }
      }

      #endregion

      #region IHasMeasures<TItem> Members

      public bool HasMeasures
      {
        get { return measures.Count != 0; }
      }

      public IMeasureSet<TItem> Measures
      {
        get { return measures; }
      }

      public object GetMeasureResult(string name)
      {
        return measureResults[name].Result;
      }
      
      public object[] GetMeasureResults(params string[] names)
      {
        return MeasureUtils<TItem>.GetMeasurements(measureResults, names);
      }

      #endregion
    }

    #endregion

    [Test]
    public void TestMeasures()
    {
      AnimalCollection animals = new AnimalCollection();
      IEnumerable<byte> ages = InstanceGeneratorProvider.Default.GetInstanceGenerator<byte>().GetInstances(random, ITEM_AMOUNT);
      foreach (byte age in ages)
        animals.Add(new Animal(age));

      CalculateMeasurements(animals);

      for(int index = 0, count = animals.Count/2; index < count; index++) 
        animals.RemoveAt(0);
      CalculateMeasurements(animals);

      for(int index = 0, count = animals.Count/2; index < count; index++) 
        animals.RemoveAt(0);
      CalculateMeasurements(animals);

      for(int index = 0, count = animals.Count/2; index < count; index++) 
        animals.RemoveAt(0);
      CalculateMeasurements(animals);

      animals.Clear();
      CalculateMeasurements(animals);
    }

    private static void CalculateMeasurements(AnimalCollection animals)
    {
      Dictionary<string, int> measurements = new Dictionary<string, int>(4);
      measurements[COUNT] = 0;
      measurements[SUM_AGE] = 0;
      measurements[MIN_AGE] = Int32.MaxValue;
      measurements[MAX_AGE] = Int32.MinValue;
      measurements[SIZE] = 0;

      
      foreach (Animal animal in animals) {
        measurements[COUNT] += 1;
        int age = AnimalCollection.AgeExtractor(animal);
        measurements[SIZE] += (int)SizeCalculator<Animal>.Default.GetValueSize(animal);
        measurements[SUM_AGE] += age;
        if (measurements[MIN_AGE] > age)
          measurements[MIN_AGE] = age;
        if (measurements[MAX_AGE] < age)
          measurements[MAX_AGE] = age;
      }

      foreach (KeyValuePair<string, int> pair in measurements)
        if (animals.Count == 0 && (pair.Key == "MinAge" || pair.Key == "MaxAge"))
          AssertEx.ThrowsInvalidOperationException(delegate { animals.GetMeasureResult(pair.Key); });
        else {
          int result = int.Parse(animals.GetMeasureResult(pair.Key).ToString());
          Assert.AreEqual(pair.Value, result);
        }
    }
  }
}