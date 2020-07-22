// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
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
}

namespace Xtensive.Orm.Tests.Linq.ContainsOverKeysCollectionTest
{
  [TestFixture]
  public class ContainsOverKeysCollectionTest : AutoBuildTest
  {
    private IEnumerable<Key> keyListAsEnumerable;
    private IEnumerable<Key> keyArrayAsEnumerable;
    private ICollection<Key> keyListAsCollection;
    private ICollection<Key> keyArrayAsCollection;
    private List<Key> keyList;
    private IList<Key> keyListAsInteface;
    private Key[] keyArray;

    private IEnumerable<Key> complexKeyListAsEnumerable;
    private IEnumerable<Key> complexKeyArrayAsEnumerable;
    private ICollection<Key> complexKeyListAsCollection;
    private ICollection<Key> complexKeyArrayAsCollection;
    private List<Key> complexKeyList;
    private IList<Key> complexKeyListAsInteface;
    private Key[] complexKeyArray;

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
    public void ComplexKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<ComplexKeyEntity> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyList.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyList.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyListAsEnumerable.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyListAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyListAsCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyListAsCollection.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyListAsInteface.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyListAsInteface.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyArray.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyArrayAsEnumerable.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyArrayAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<ComplexKeyEntity>().Where(e => complexKeyArrayAsCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(5));
        Assert.That(result.All(x => complexKeyArrayAsCollection.Contains(x.Key)), Is.True);
      }
    }

    [Test]
    public void DirectKeyContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyList.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyListAsEnumerable.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyListAsCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyListAsInteface.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyArray.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyArrayAsEnumerable.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => keyArrayAsCollection.Contains(e.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Key)), Is.True);
      }
    }

    [Test]
    public void DirectKeyInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyList)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyListAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyListAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyListAsInteface)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyArrayAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity>().Where(e => e.Key.In(keyArrayAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Key)), Is.True);
      }
    }

    [Test]
    public void OneLevelContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity2> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyList.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyListAsEnumerable.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyListAsCollection.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyListAsInteface.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyArray.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyArrayAsEnumerable.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => keyArrayAsCollection.Contains(e.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void OneLevelInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity2> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyList)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyListAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyListAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyListAsInteface)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyArrayAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity2>().Where(e => e.Entity.Key.In(keyArrayAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void TwoLevelContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyList.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyListAsEnumerable.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyListAsCollection.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyListAsInteface.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyArray.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyArrayAsEnumerable.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => keyArrayAsCollection.Contains(e.Entity.Entity.Key)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Entity.Entity.Key)), Is.True);
      }
    }

    [Test]
    public void TwoLevelInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result = null;
        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyList)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyList.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyListAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsEnumerable.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyListAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsCollection.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyListAsInteface)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyListAsInteface.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyArray)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArray.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyArrayAsEnumerable)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsEnumerable.Contains(x.Entity.Entity.Key)), Is.True);

        Assert.DoesNotThrow(() =>
          result = session.Query.All<MyEntity3>().Where(e => e.Entity.Entity.Key.In(keyArrayAsCollection)).ToList());
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.All(x => keyArrayAsCollection.Contains(x.Entity.Entity.Key)), Is.True);
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
          .Where(e => keyList.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyListAsEnumerable.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyListAsCollection.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyListAsInteface.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyArray.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyArrayAsEnumerable.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => keyArrayAsCollection.Contains(session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key))
          .ToList());
      }
    }

    [Test]
    public void SubQueryInTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<MyEntity3> result;

        Assert.Throws<QueryTranslationException>(()=>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyList))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyListAsEnumerable))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyListAsCollection))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyListAsInteface))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyArray))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyArrayAsEnumerable))
          .ToList());

        Assert.Throws<QueryTranslationException>(() =>
        result = session.Query.All<MyEntity3>()
          .Where(e => session.Query.All<MyEntity>().First(el => el.Key == e.Entity.Entity.Key).Key.In(keyArrayAsCollection))
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
            .Where(e => keyListAsEnumerable.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyList.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyListAsInteface.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyListAsCollection.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyArrayAsEnumerable.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyArray.Contains(new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => keyArrayAsCollection.Contains(new {
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
            }.DoubleEntityKey.In(keyList)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(keyListAsEnumerable)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(keyListAsInteface)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(keyListAsCollection)).ToList());

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
           }.DoubleEntityKey.In(keyArrayAsEnumerable)).ToList());

        Assert.Throws<QueryTranslationException>(() =>
          result = session.Query.All<MyEntity3>()
            .Where(e => new {
              Key = e.Key,
              EntityKey = e.Entity.Key,
              DoubleEntityKey = e.Entity.Entity.Key
            }.DoubleEntityKey.In(keyArrayAsCollection)).ToList());
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

      new ComplexKeyEntity(1, 1) { Name = "1 1" };
      new ComplexKeyEntity(2, 2) { Name = "2 2" };
      new ComplexKeyEntity(3, 3) { Name = "3 3" };
      new ComplexKeyEntity(4, 4) { Name = "4 4" };
      new ComplexKeyEntity(5, 5) { Name = "5 5" };
      new ComplexKeyEntity(6, 6) { Name = "6 6" };
      new ComplexKeyEntity(7, 7) { Name = "7 7" };
      new ComplexKeyEntity(8, 8) { Name = "8 8" };
    }

    private void PopulateKeysCollections(Session session)
    {
      keyList = session.Query.All<MyEntity>().AsEnumerable().Take(3).Select(e => e.Key).ToList();
      keyListAsEnumerable = new List<Key>(keyList);
      keyListAsCollection = new List<Key>(keyList);
      keyListAsInteface = new List<Key>(keyList);

      keyArray = session.Query.All<MyEntity>().AsEnumerable().Take(3).Select(e => e.Key).ToArray();
      keyArrayAsEnumerable = keyArray.AsEnumerable().ToArray();
      keyArrayAsCollection = keyArray.AsEnumerable().ToArray();

      complexKeyList = session.Query.All<ComplexKeyEntity>().Take(5).Select(e => e.Key).ToList();
      complexKeyListAsEnumerable = new List<Key>(complexKeyList);
      complexKeyListAsCollection = new List<Key>(complexKeyList);
      complexKeyListAsInteface = new List<Key>(complexKeyList);

      complexKeyArray = session.Query.All<ComplexKeyEntity>().Take(5).Select(e => e.Key).ToArray();
      complexKeyArrayAsEnumerable = complexKeyArray.AsEnumerable().ToArray();
      complexKeyArrayAsCollection = complexKeyArray.AsEnumerable().ToArray();
    }
  }
}
