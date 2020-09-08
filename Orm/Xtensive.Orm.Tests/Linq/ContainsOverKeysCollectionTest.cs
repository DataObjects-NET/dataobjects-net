// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTestModel;

namespace Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTestModel
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Field { get; set; }
  }

  [HierarchyRoot]
  public class MyEntity2 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity Entity { get; set; }
  }
  
  [HierarchyRoot]
  public class MyEntity3 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public MyEntity2 Entity { get; set; }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class ComplexKeyEntity : Entity
  {
    [Field, Key(0)]
    public int Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field]
    public string Name { get; set; }

    public ComplexKeyEntity(int id0, int id1)
      : base(id0, id1)
    {
    }
  }

  public class TestKeyCollection : IReadOnlyCollection<Key>
  {
    private readonly Key[] keys;

    public int Count => keys.Length;

    public IEnumerator<Key> GetEnumerator()
    {
      foreach (var key in keys)
        yield return key;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TestKeyCollection(Key[] array)
    {
      keys = array;
    }
  }

  public class TestGenericCollection<TKey> : IReadOnlyCollection<TKey>
  {
    private readonly TKey[] keys;

    public int Count => keys.Length;

    public IEnumerator<TKey> GetEnumerator()
    {
      foreach (var key in keys)
        yield return key;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TestGenericCollection(TKey[] array)
    {
      keys = array;
    }
  }
}

namespace Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTest
{
  [TestFixture]
  public class ContainsOverKeysCollectionTest : AutoBuildTest
  {
    private Key[] keyArray;
    private TestGenericCollection<Key> genericKeyCollection;
    private TestKeyCollection nonGenericKeyCollection;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        PopulateEntities();
        PopulateKeysCollections(session);
        transaction.Complete();
      }
    }

    [Test]
    public void DirectKeyContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyArray.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => nonGenericKeyCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => genericKeyCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);
      }
    }

    [Test]
    public void DirectKeyInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(nonGenericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(genericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);
      }
    }

    [Test]
    public void OneLevelContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity2> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyArray.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => nonGenericKeyCollection.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => genericKeyCollection.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void OneLevelInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity2> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(nonGenericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(genericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void TwoLevelContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyArray.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => nonGenericKeyCollection.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => genericKeyCollection.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void TwoLevelInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result = null;

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(nonGenericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(genericKeyCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void SubQueryContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result;

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyArray.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
            .ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => nonGenericKeyCollection.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
            .ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => genericKeyCollection.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
            .ToList());
      }
    }

    [Test]
    public void SubQueryInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result;

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyArray))
            .ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(nonGenericKeyCollection))
            .ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(genericKeyCollection))
            .ToList());
      }
    }

    [Test]
    public void AnonymousTypeKeyContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result;

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyArray.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => nonGenericKeyCollection.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => genericKeyCollection.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());
      }
    }

    [Test]
    public void AnonymousTypeKeyInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result;

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(keyArray)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(nonGenericKeyCollection)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(genericKeyCollection)).ToList());
      }
    }

    private void PopulateEntities()
    {
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "ABC" }
        }
      };
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "DEF" }
        }
      };
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "GHI" }
        }
      };
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "KLM" }
        }
      };
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "NOP" }
        }
      };
      new MyEntity3 {
        Entity = new MyEntity2 {
          Entity = new MyEntity { Field = "QRS" }
        }
      };
    }

    private void PopulateKeysCollections(Session session)
    {
      keyArray = session.Query.All<MyEntity>().AsEnumerable().Take(3).Select(e => e.Key).ToArray();
      genericKeyCollection = new TestGenericCollection<Key>(keyArray);
      nonGenericKeyCollection = new TestKeyCollection(keyArray);
    }
  }
}
