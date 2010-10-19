// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.17

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Aspects;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests
{
  public class Animal
  {
    private int age;
    private string name;

    public string Name
    {
      get { return name; }
      set { name = value; }
    }

    public int Age
    {
      get { return age; }
      set { age = value; }
    }

    public Animal(string name, int age)
    {
      this.name = name;
      this.age = age;
    }
  }

  public class AnimalInstanceGenerator : InstanceGeneratorBase<Animal>
  {
    public override Animal GetInstance(Random random)
    {
      return new Animal("Animal " + random.Next(0, Int32.MaxValue), random.Next(1, 50));
    }

    public AnimalInstanceGenerator(IInstanceGeneratorProvider provider) : base(provider)
    {
    }
  }

  public class AnimalComparer: AdvancedComparerBase<Animal>
  {
    public override int Compare(Animal x, Animal y)
    {
      int result = StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
      if (result != 0)
        return result;
      return Comparer<int>.Default.Compare(x.Age, y.Age);
    }

    public override bool Equals(Animal x, Animal y)
    {
      return x.Name == y.Name && x.Age == y.Age;
    }

    public override int GetHashCode(Animal obj)
    {
      return obj.Name.GetHashCode() ^ obj.Age.GetHashCode();
    }

    protected override IAdvancedComparer<Animal> CreateNew(ComparisonRules rules)
    {
      return new AnimalComparer(Provider, rules);
    }

    public AnimalComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }

  public class AnimalCollection
    : CollectionBase<Animal>,
      IIndexedCollection<Animal>
  {
    private readonly INonUniqueIndex<int, Animal> ageIndex;
    private readonly CollectionIndexSet<Animal> indexes;
    private readonly IUniqueIndex<string, Animal> nameIndex;
    private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public override object SyncRoot
    {
      get { return syncRoot; }
    }

    public IUniqueIndex<string, Animal> NameIndex
    {
      get { return indexes["nameIndex"] as IUniqueIndex<string, Animal>; }
    }

    public Animal this[string key]
    {
      get { return nameIndex.GetItem(key); }
    }

    public IEnumerable<Animal> GetItems(int key)
    {
      return ageIndex.GetItems(key);
    }

    #region IIndexedCollection<Animal> Members

    public CollectionIndexSet<Animal> Indexes
    {
      get { return indexes; }
    }

    #endregion

    public AnimalCollection()
    {
      // Unique index creation
      var uniqueIndexConfig = new IndexConfigurationBase<string, Animal>();
      uniqueIndexConfig.KeyExtractor = (item => item.Name);
      uniqueIndexConfig.KeyComparer = AdvancedComparer<string>.Default;
      nameIndex = IndexFactory.CreateUnique<string, Animal, DictionaryIndex<string, Animal>>(uniqueIndexConfig);

      // Unique sorted index creation for nonunique index
      TupleDescriptor td = TupleDescriptor.Create(new[] { typeof(int), typeof(string) });
      var orderedIndexConfig = new IndexConfigurationBase<Tuple, Animal>();
      orderedIndexConfig.KeyExtractor = delegate(Animal item) {
                                          Tuple tuple = Tuple.Create(td);
                                          tuple.SetValue(0, item.Age);
                                          tuple.SetValue(1, item.Name);
                                          return tuple;};
      orderedIndexConfig.KeyComparer = AdvancedComparer<Tuple>.Default;

      // Nonunique index creation
      var nonUniqueIndexConfig = new NonUniqueIndexConfiguration<int, Tuple, Animal>();
      nonUniqueIndexConfig.KeyExtractor = (item => item.Age);
      nonUniqueIndexConfig.KeyComparer = AdvancedComparer<int>.Default;
      nonUniqueIndexConfig.EntireConverter = (age => new Entire<Tuple>(Tuple.Create(age.Value), age.ValueType));
      nonUniqueIndexConfig.UniqueIndexConfiguration = orderedIndexConfig;
      ageIndex = IndexFactory.CreateNonUnique<int, Tuple, Animal, SortedListIndex<Tuple, Animal>>(nonUniqueIndexConfig);

      // IndexSet creation
      indexes = new CollectionIndexSet<Animal>();
      indexes.Add(new CollectionIndex<string, Animal>("nameIndex", this, nameIndex));
      indexes.Add(new CollectionIndex<int, Animal>("ageIndex", this, ageIndex));
    }
  }

  public class AnimalSet
    : Set<Animal>,
      IIndexedCollection<Animal>
  {
    private readonly INonUniqueIndex<int, Animal> ageIndex;
    private readonly CollectionIndexSet<Animal> indexes;
    private readonly IUniqueIndex<string, Animal> nameIndex;

    public IUniqueIndex<string, Animal> NameIndex
    {
      get { return indexes["nameIndex"] as IUniqueIndex<string, Animal>; }
    }

    public Animal this[string key]
    {
      get { return nameIndex.GetItem(key); }
    }

    public IEnumerable<Animal> GetItems(int key)
    {
      return ageIndex.GetItems(key);
    }

    #region IIndexedCollection<Animal> Members

    public CollectionIndexSet<Animal> Indexes
    {
      get { return indexes; }
    }

    #endregion

    public AnimalSet()
    {
      // Unique index creation
      var uniqueIndexConfig = new IndexConfigurationBase<string, Animal>();
      uniqueIndexConfig.KeyExtractor = (item => item.Name);
      uniqueIndexConfig.KeyComparer = AdvancedComparer<string>.Default;
      nameIndex = IndexFactory.CreateUnique<string, Animal, DictionaryIndex<string, Animal>>(uniqueIndexConfig);

      // Unique sorted index creation for nonunique index
      var orderedIndexConfig = new IndexConfigurationBase<Tuple, Animal>();
      orderedIndexConfig.KeyExtractor = (item => Tuple.Create(item.Age, item.Name));
      orderedIndexConfig.KeyComparer = AdvancedComparer<Tuple>.Default;

      // Nonunique index creation
      var nonUniqueIndexConfig = new NonUniqueIndexConfiguration<int, Tuple, Animal>();
      nonUniqueIndexConfig.KeyExtractor = (item => item.Age);
      nonUniqueIndexConfig.KeyComparer = AdvancedComparer<int>.Default;
      nonUniqueIndexConfig.EntireConverter = (age => new Entire<Tuple>(Tuple.Create(age.Value), age.ValueType));
      nonUniqueIndexConfig.UniqueIndexConfiguration = orderedIndexConfig;
      ageIndex = IndexFactory.CreateNonUnique<int, Tuple, Animal, SortedListIndex<Tuple, Animal>>(nonUniqueIndexConfig);

      // IndexSet creation
      indexes = new CollectionIndexSet<Animal>();
      indexes.Add(new CollectionIndex<string, Animal>("nameIndex", this, nameIndex));
      indexes.Add(new CollectionIndex<int, Animal>("ageIndex", this, ageIndex));
    }
  }

  [TestFixture]
  public class CollectionIndexTest
  {
    private AnimalCollection collection;
    private IUniqueIndex<string, Animal> collectionIndex;
    private IUniqueIndex<string, Animal> setIndex;
    private AnimalSet set;

    [TestFixtureSetUp]
    public void TestFixtureSetup()
    {
      collection = new AnimalCollection();
      set = new AnimalSet();
      collectionIndex = collection.NameIndex;
      setIndex = set.NameIndex;
    }

    public void PopulateAnimals(ICollection<Animal> animals)
    {
      animals.Clear();
      for (int i = 0; i < 1024; i++)
        animals.Add(new Animal("a" + i, i/10));
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void AddTest()
    {
      collectionIndex.Add(new Animal("x", 0));
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void RemoveTest()
    {
      collectionIndex.Remove(new Animal("x", 0));
    }

    [Test]
    public void KeyViolationAddTest()
    {
      PopulateAnimals(collection);
      try {
        collection.Add(new Animal("a0", 1));
        throw new AssertionException("Test failed");
      }
      catch (InvalidOperationException) {
      }
      Assert.AreEqual(collection.Count, collectionIndex.Count);

      PopulateAnimals(set);
      try {
        set.Add(new Animal("a0", 1));
        throw new AssertionException("Test failed");
      }
      catch (InvalidOperationException) {
      }
      Assert.AreEqual(set.Count, setIndex.Count);
    }

    [Ignore]
    [Test]
    public void KeyViolationSetTest()
    {
      PopulateAnimals(collection);
      try {
        Animal a = collection[0];
        Assert.IsNotNull(a);
        a.Name = collection[1].Name;
        throw new AssertionException("Test failed");
      }
      catch (InvalidOperationException) {
      }

      PopulateAnimals(set);
      try {
        Animal a = set["a0"];
        Assert.IsNotNull(a);
        a.Name = "a1";
        throw new AssertionException("Test failed");
      }
      catch (InvalidOperationException) {
      }
    }

    [Test]
    public void IndexSetTest()
    {
      PopulateAnimals(set);
      PopulateAnimals(collection);

      Assert.AreEqual(2, set.Indexes.Count);
      Assert.IsNotNull(set.Indexes["nameIndex"]);
      Assert.IsNotNull(set.Indexes["ageIndex"]);

      int i = 0;
      foreach (Animal animal in set.GetItems(5)) {
        Assert.AreEqual(5, animal.Age);
        i++;
      }
      Assert.Greater(i, 0);

      Assert.AreEqual(2, collection.Indexes.Count);
      Assert.IsNotNull(collection.Indexes["nameIndex"]);
      Assert.IsNotNull(collection.Indexes["ageIndex"]);

      i = 0;
      foreach (Animal animal in collection.GetItems(5)) {
        Assert.AreEqual(5, animal.Age);
        i++;
      }
      Assert.Greater(i, 0);
    }

    [Test]
    public void EmptyIndexesTest()
    {
      set.Clear();
      collection.Clear();
      Assert.AreEqual(2, set.Indexes.Count);
      Assert.IsNotNull(set.Indexes["nameIndex"]);
      Assert.IsNotNull(set.Indexes["ageIndex"]);

      int i = 0;
      foreach (Animal animal in set.GetItems(5)) {
        Assert.AreEqual(5, animal.Age);
        i++;
      }
      Assert.AreEqual(i, 0);

      Assert.AreEqual(2, collection.Indexes.Count);
      Assert.IsNotNull(set.Indexes["nameIndex"]);
      Assert.IsNotNull(set.Indexes["ageIndex"]);

      i = 0;
      foreach (Animal animal in collection.GetItems(5)) {
        Assert.AreEqual(5, animal.Age);
        i++;
      }
      Assert.AreEqual(i, 0);
    }
  }
}