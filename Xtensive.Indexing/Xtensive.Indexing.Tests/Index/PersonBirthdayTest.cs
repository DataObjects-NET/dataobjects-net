// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.01

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Aspects;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Reflection;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Indexing.Measures;
using B=Xtensive.Indexing;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class PersonBirthdayTest
  {
    public static readonly DateTime MinBirthday = new DateTime(1800, 1, 1);
    public static readonly DateTime MaxBirthday = new DateTime(2007, 1, 1);
    public static readonly int DaysPerMinMaxBirthday = (int) MaxBirthday.Subtract(MinBirthday).TotalDays;
    public static readonly int DaysPerFilterByBirthday = 365;

    private static int GetAge(IPerson person)
    {
      return new DateTime(1, 1, 1).Add(MaxBirthday - person.Birthday).Year - 1;
    }

    public interface IPerson
    {
      string Name { get; set; }
      DateTime Birthday { get; set; }
    }

    public interface ITestCollection<TItem>: IEnumerable<TItem>
      where TItem: IPerson
    {
      int Count { get; }
      TItem this[string name] { get; }
      TItem this[int index] { get; }
      TItem CreateItem(string name, DateTime age);
      void Add(TItem item);
      IEnumerable<TItem> FilterByBirthday(DateTime from, DateTime to);
      int CountByBirthday(DateTime from, DateTime to);
      double AverageAgeByBirthday(DateTime from, DateTime to);
    }

    public class IndexedPerson: IPerson
    {
      private DateTime age;
      private string name;

      #region IPerson Members

      public string Name
      {
        get { return name; }
        set { name = value; }
      }

      public DateTime Birthday
      {
        get { return age; }
        set { age = value; }
      }

      #endregion

      // Constructor
      public IndexedPerson(string name, DateTime age)
      {
        this.name = name;
        this.age = age;
      }
    }

    //    [Index("IX_Name",     "Name",     typeof (B.Index<,>), Unique = true,  Expose = true)]
    //    [Index("IX_Birthday", "Birthday", typeof (B.Index<,>), Unique = false, Expose = true)]
    public class IndexedPersonCollection: CollectionBase<IndexedPerson>,
      ITestCollection<IndexedPerson>
    {
      private INonUniqueIndex<DateTime, IndexedPerson> birthdayIndex;
      private IUniqueIndex<string, IndexedPerson> nameIndex;
      private readonly CollectionIndexSet<IndexedPerson> indexes;

      public IUniqueIndex<string, IndexedPerson> NameIndex
      {
        get { return nameIndex; }
      }

      public INonUniqueIndex<DateTime, IndexedPerson> BirthdayIndex
      {
        get { return birthdayIndex; }
      }

      private static long GetAgeSumValueDelegate(IndexedPerson item)
      {
        return GetAge(item);
      }

      #region ITestCollection<IndexedPerson> Members

      public IndexedPerson CreateItem(string name, DateTime age)
      {
        return new IndexedPerson(name, age);
      }

      public IndexedPerson this[string name]
      {
        get { return NameIndex.GetItem(name); }
      }

      public IEnumerable<IndexedPerson> FilterByBirthday(DateTime from, DateTime to)
      {
        var range = new Range<Entire<DateTime>>(
          new Entire<DateTime>(from,Direction.Negative),
          new Entire<DateTime>(to,Direction.Positive));
        foreach (IndexedPerson item in BirthdayIndex.GetItems(range))
          yield return item;
      }

      public int CountByBirthday(DateTime from, DateTime to)
      {
        var range = new Range<Entire<DateTime>>(
          new Entire<DateTime>(from,Direction.Negative),
          new Entire<DateTime>(to,Direction.Positive));
        return (int) (long) birthdayIndex.GetMeasureResult(range, CountMeasure<object, long>.CommonName);
      }

      public double AverageAgeByBirthday(DateTime from, DateTime to)
      {
        var range = new Range<Entire<DateTime>>(
          new Entire<DateTime>(from, Direction.Negative),
          new Entire<DateTime>(to, Direction.Positive));
        var measures = birthdayIndex.Measures;
        object[] result = birthdayIndex.GetMeasureResults(range, CountMeasure<object, long>.CommonName, "AgeSum");
        int count = (int) (long) result[0];
        if (count == 0)
          return 0;
        double res = (long) result[1];
        return res / count;
      }

      #endregion

      // Constructor
      public IndexedPersonCollection()
      {
        // Unique index: (Name)
        var nameIndexConfig = new IndexConfiguration<string, IndexedPerson>();
        nameIndexConfig.KeyExtractor = item => item.Name;
        nameIndexConfig.KeyComparer = AdvancedComparer<string>.Default;
        nameIndex = IndexFactory.CreateUnique<string, IndexedPerson, Index<string, IndexedPerson>>(nameIndexConfig);

        // Unique index: (Birthday, Name)
        var descriptor1 = TupleDescriptor.Create<DateTime, string>();
        IndexConfiguration<Tuple, IndexedPerson> bnIndexConfig = 
          new IndexConfiguration<Tuple, IndexedPerson>();
        bnIndexConfig.KeyExtractor = item => Tuple.Create(descriptor1, item.Birthday, item.Name);
        bnIndexConfig.KeyComparer = AdvancedComparer<Tuple>.Default;

        // Non-unique index for above unique index: (Birthday)
        var descriptor2 = TupleDescriptor.Create<DateTime>();
        NonUniqueIndexConfiguration<DateTime, Tuple, IndexedPerson> birthdayIndexConfig = 
          new NonUniqueIndexConfiguration<DateTime, Tuple, IndexedPerson>();
        birthdayIndexConfig.KeyExtractor = item => item.Birthday;
        birthdayIndexConfig.KeyComparer = AdvancedComparer<DateTime>.Default;
        birthdayIndexConfig.EntireConverter = item => new Entire<Tuple>(Tuple.Create(descriptor2, item.Value), item.ValueType);
        birthdayIndexConfig.UniqueIndexConfiguration = bnIndexConfig;
        birthdayIndexConfig.Measures.Add(new SumMeasure<IndexedPerson, long>("AgeSum", GetAgeSumValueDelegate));
        birthdayIndex = IndexFactory.CreateNonUnique<DateTime, Tuple, IndexedPerson, Index<Tuple, IndexedPerson>>(birthdayIndexConfig);
        // birthdayIndex = IndexFactory.CreateNonUnique<DateTime, Tuple, IndexedPerson, SortedListIndex<Tuple, IndexedPerson>>(nonUniqueIndexConfig);

        // IndexSet creation
        indexes = new CollectionIndexSet<IndexedPerson> {
          new CollectionIndex<string, IndexedPerson>("nameIndex", this, nameIndex), 
          new CollectionIndex<DateTime, IndexedPerson>("ageIndex", this, birthdayIndex)
        };
      }
    }

    public class RegularPerson: IPerson
    {
      private DateTime age;
      private string name;

      #region IPerson Members

      public string Name
      {
        get { return name; }
        set { name = value; }
      }

      public DateTime Birthday
      {
        get { return age; }
        set { age = value; }
      }

      #endregion

      // Constructor
      public RegularPerson(string name, DateTime age)
      {
        this.name = name;
        this.age = age;
      }
    }

    public class ManualPersonCollection: List<RegularPerson>,
      ITestCollection<RegularPerson>
    {
      private SortedDictionary<DateTime, Pair<RegularPerson, List<RegularPerson>>> birthdayIndex =
        new SortedDictionary<DateTime, Pair<RegularPerson, List<RegularPerson>>>();

      private SortedDictionary<string, RegularPerson> nameIndex =
        new SortedDictionary<string, RegularPerson>();

      #region ITestCollection<RegularPerson> Members

      public RegularPerson CreateItem(string name, DateTime age)
      {
        return new RegularPerson(name, age);
      }

      public new void Add(RegularPerson item)
      {
        nameIndex.Add(item.Name, item);
        Pair<RegularPerson, List<RegularPerson>> pair;
        if (!birthdayIndex.TryGetValue(item.Birthday, out pair)) {
          pair = new Pair<RegularPerson, List<RegularPerson>>(item, null);
          birthdayIndex[item.Birthday] = pair;
        }
        else {
          if (pair.Second == null) {
            RegularPerson onePerson = pair.First;
            pair = new Pair<RegularPerson, List<RegularPerson>>(null, new List<RegularPerson>());
            pair.Second.Add(onePerson);
            pair.Second.Add(item);
            birthdayIndex[item.Birthday] = pair;
          }
          else {
            pair.Second.Add(item);
          }
        }
        base.Add(item);
      }

      public RegularPerson this[string name]
      {
        get { return nameIndex[name]; }
      }

      public IEnumerable<RegularPerson> FilterByBirthday(DateTime from, DateTime to)
      {
        List<DateTime> birthdays =
          new List<DateTime>((birthdayIndex as IDictionary<DateTime, Pair<RegularPerson, List<RegularPerson>>>).Keys);
        int firstSuitableIndex = birthdays.BinarySearch(from);
        if (firstSuitableIndex < 0)
          firstSuitableIndex = ~firstSuitableIndex;
        else {
          DateTime firstAge = birthdays[firstSuitableIndex];
          while (firstSuitableIndex > 0 && birthdays[--firstSuitableIndex] == firstAge) {
          }
          ;
          if (birthdays[firstSuitableIndex] != firstAge)
            firstSuitableIndex++;
        }
        for (int i = firstSuitableIndex; i < birthdays.Count; i++) {
          DateTime age = birthdays[i];
          if (age > to)
            break;
          Pair<RegularPerson, List<RegularPerson>> pair;
          if (birthdayIndex.TryGetValue(age, out pair)) {
            if (pair.Second == null)
              yield return pair.First;
            else
              foreach (RegularPerson a in pair.Second)
                yield return a;
          }
        }
      }

      public int CountByBirthday(DateTime from, DateTime to)
      {
        int count = 0;
        foreach (RegularPerson person in FilterByBirthday(from, to))
          count++;
        return count;
      }

      public double AverageAgeByBirthday(DateTime from, DateTime to)
      {
        int count = 0;
        double ageSumm = 0;
        foreach (RegularPerson person in FilterByBirthday(from, to)) {
          count++;
          ageSumm += GetAge(person);
        }
        if (count == 0)
          return 0;
        return ageSumm/count;
      }

      public new IEnumerator<RegularPerson> GetEnumerator()
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    public class RegularPersonCollection: List<RegularPerson>,
      ITestCollection<RegularPerson>
    {
      #region ITestCollection<RegularPerson> Members

      public RegularPerson CreateItem(string name, DateTime age)
      {
        return new RegularPerson(name, age);
      }

      public RegularPerson this[string name]
      {
        get
        {
          foreach (RegularPerson a in this) {
            if (a.Name.Equals(name))
              return a;
          }
          return null;
        }
      }

      public IEnumerable<RegularPerson> FilterByBirthday(DateTime from, DateTime to)
      {
        foreach (RegularPerson a in this) {
          if (a.Birthday >= from && a.Birthday <= to)
            yield return a;
        }
      }

      public int CountByBirthday(DateTime from, DateTime to)
      {
        int count = 0;
        foreach (RegularPerson person in FilterByBirthday(from, to)) {
          count++;
        }
        return count;
      }

      public double AverageAgeByBirthday(DateTime from, DateTime to)
      {
        int count = 0;
        double ageSumm = 0;
        foreach (RegularPerson person in FilterByBirthday(from, to)) {
          count++;
          ageSumm += GetAge(person);
        }
        if (count == 0)
          return 0;
        return ageSumm/count;
      }

      #endregion
    }


    private static void InsertTest<TItem>(ITestCollection<TItem> collection, int count)
      where TItem: IPerson
    {
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
      List<TItem> list = new List<TItem>();
      // Pre-create items to exclude Guid & item generation from the measured time
      for (int i = 0; i < count; i++)
        list.Add(collection.CreateItem(
          String.Intern(random.NextDouble().ToString() + ":" + i.ToString()),
          MinBirthday.Add(TimeSpan.FromDays(random.Next(DaysPerMinMaxBirthday)))));
      int localCount = count;
      using (new Measurement(String.Format("Inserting into {0}", collection.GetType().Name), localCount)) {
        for (int i = 0; i < localCount; i++) {
          collection.Add(list[i]);
        }
      }
    }

    private static void IndexerTest<TItem>(ITestCollection<TItem> collection, int count)
      where TItem: IPerson
    {
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
      using (new Measurement(String.Format("Indexing {0}", collection.GetType().Name), count)) {
        for (int i = 0; i < count; i++) {
          TItem item = collection[collection[random.Next(collection.Count)].Name];
        }
      }
    }

    private static int RangeTest<TItem>(ITestCollection<TItem> collection, int count)
      where TItem: IPerson
    {
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
      int totalCount = 0;
      using (new Measurement(String.Format("Filtering {0}", collection.GetType().Name), count)) {
        for (int i = 0; i < count; i++) {
          DateTime minDate = MinBirthday.Add(TimeSpan.FromDays(random.Next(DaysPerMinMaxBirthday)));
          totalCount+=collection.FilterByBirthday(minDate, minDate.Add(TimeSpan.FromDays(random.Next(DaysPerFilterByBirthday)))).Count();
        }
      }
      Indexing.Log.Info("Total count: {0}", totalCount);
      return totalCount;
    }

    private static long RangeCountTest<TItem>(ITestCollection<TItem> collection, int count)
      where TItem: IPerson
    {
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
      long sum = 0;
      int lastCount = 0;
      using (new Measurement(String.Format("Count on ranges of {0}", collection.GetType().Name), count)) {
        for (int i = 0; i < count; i++) {
          DateTime minDate = MinBirthday.Add(TimeSpan.FromDays(random.Next(DaysPerMinMaxBirthday)));
          lastCount = collection.CountByBirthday(minDate,
            minDate.Add(TimeSpan.FromDays(random.Next(DaysPerFilterByBirthday))));
          sum += lastCount;
        }
      }
      Indexing.Log.Info("Last count: {0}, sum(count): {0}", lastCount, sum);
      return sum;
    }

    private static double RangeAvgTest<TItem>(ITestCollection<TItem> collection, int count)
      where TItem: IPerson
    {
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
      double sum = 0;
      double lastAverage = 0;
      using (new Measurement(String.Format("Average on ranges of {0}", collection.GetType().Name), count)) {
        for (int i = 0; i < count; i++) {
          DateTime minDate = MinBirthday.Add(TimeSpan.FromDays(random.Next(DaysPerMinMaxBirthday)));
          lastAverage = collection.AverageAgeByBirthday(minDate,
            minDate.Add(TimeSpan.FromDays(random.Next(DaysPerFilterByBirthday))));
          sum += lastAverage;
        }
      }
      Indexing.Log.Info("Last average(Age): {0}, sum(average(Age)): {0}", lastAverage, sum);
      return sum;
    }

    private static void Test(int count)
    {
      IndexedPersonCollection indexedPersons = new IndexedPersonCollection();
      ManualPersonCollection manualPersons = new ManualPersonCollection();
      RegularPersonCollection regularPersons = new RegularPersonCollection();


      Indexing.Log.Info("{0} items sequence:", count);
      using (new LogIndentScope()) {
        InsertTest(indexedPersons, count);
        InsertTest(manualPersons, count);
        InsertTest(regularPersons, count);

        IndexerTest(indexedPersons, count < 1000 ? count : 1000);
        IndexerTest(manualPersons, count < 1000 ? count : 1000);
        IndexerTest(regularPersons, count < 1000 ? count : 1000);

        object o1 = RangeTest(indexedPersons, 100);
        object o2 = RangeTest(manualPersons, 100);
        object o3 = RangeTest(regularPersons, 100);
        Assert.AreEqual(o1, o2);
        Assert.AreEqual(o1, o3);
        Assert.AreEqual(o2, o3);

        o1 = RangeCountTest(indexedPersons, 100);
        o2 = RangeCountTest(manualPersons, 100);
        o3 = RangeCountTest(regularPersons, 100);
        Assert.AreEqual(o1, o2);
        Assert.AreEqual(o1, o3);
        Assert.AreEqual(o2, o3);

        o1 = RangeAvgTest(indexedPersons, 100);
        o2 = RangeAvgTest(manualPersons, 100);
        o3 = RangeAvgTest(regularPersons, 100);
        Assert.AreEqual(o1, o2);
        Assert.AreEqual(o1, o3);
        Assert.AreEqual(o2, o3);
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      Indexing.Log.Info("Warmup...");
      using (new LogIndentScope()) {
        Test(100000);
      }
      Indexing.Log.Info("Starting actual tests...");
      using (new LogIndentScope()) {
        for (int i = 1000; i <= 1000000; i *= 10)
          Test(i);
      }
    }

    [Test]
    [Category("Debug")]
    public void DebugTest()
    {
      const int count = 5000;
      var indexedPersons = new IndexedPersonCollection();
      var manualPersons  = new ManualPersonCollection();
      InsertTest(indexedPersons, count);
      InsertTest(manualPersons, count);
      Random random = RandomManager.CreateRandom(count, SeedVariatorType.CallingMethod);
        for (int i = 0; i < count; i++) {
          DateTime minDate = MinBirthday.Add(TimeSpan.FromDays(random.Next(DaysPerMinMaxBirthday)));
          DateTime maxDate = minDate.Add(TimeSpan.FromDays(random.Next(DaysPerFilterByBirthday)));

          var indexedFilter = indexedPersons.FilterByBirthday(minDate, maxDate).ToList();
          var manualFilter  = manualPersons.FilterByBirthday(minDate, maxDate).ToList();
          int indexedPersonsCount = indexedFilter.Count();
          int manualPersonsCount  = manualFilter.Count();
          if (indexedPersonsCount!=manualPersonsCount)
            Indexing.Log.Error("indexedFilter.Count()!=manualFilter.Count()");
          Assert.AreEqual(indexedPersonsCount, manualPersonsCount);
        }
    }

    [Test]
    [Explicit, Category("Profile")]
    public void ProfileTest()
    {
      IndexedPersonCollection indexedPersons = new IndexedPersonCollection();
      using (new LogIndentScope())
        InsertTest(indexedPersons, 100000);
    }

    [Test]
    [Explicit, Category("Profile")]
    public void Test()
    {
      for (int i = 0; i < 50; i++)
        Test(i);
      Test(10000);
      Test(100000);
    }
  }
}