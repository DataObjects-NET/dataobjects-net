// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.06

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using System.Linq;
using M=Xtensive.Orm.Model;

#region Model

namespace Xtensive.Orm.Tests.Storage.VersionModel
{
  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(null)]
  public class Address : Entity
  {
    [Key(0), Field(Length = 100)]
    public string City { get; private set; }

    [Key(1), Field(Length = 100)]
    public string RegionCode { get; private set; }

    public Address(string city, string regionCode)
      : base(city, regionCode)
    {
    }
  }

  [Serializable]
  public class Phone : Structure
  {
    [Field]
    public int Code { get; set; }

    [Field]
    public int Number { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  public class Customer : Person
  {
    [Field]
    public Address Address { get; set; }

    [Field]
    public Phone Phone { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public int VersionId { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Phone Phone { get; set; }

    [Field, Association(PairTo = "Authors")]
    public EntitySet<Book> Books { get; private set; }

    [Field, Association(PairTo = "Author")]
    public EntitySet<Comment> Comments { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public int VersionId { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Author> Authors { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Comment : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Author Author { get; set; }
  }


  [Serializable]
  public class VersionStructure : Structure
  {
    [Field]
    [Version]
    public int Version { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class VersionEntity : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class ItemWithStructureVersion : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public VersionStructure VersionId { get; private set;}

    [Field]
    public string Field { get; set;}

    protected override bool UpdateVersion(Entity changedEntity, M.FieldInfo changedField)
    {
      VersionId = new VersionStructure{Version = VersionId.Version + 1};
      return true;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class ItemWithEntityVersion : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version(VersionMode.Manual), Field]
    public VersionEntity VersionId { get; private set;}

    [Field]
    public string Field { get; set;}

    protected override bool UpdateVersion(Entity changedEntity, M.FieldInfo changedField)
    {
      VersionId = new VersionEntity();
      return true;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class ItemWithCustomVersions : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field, Version]
    public long VersionId1 { get; set; }

    [Field]
    public string Field { get; set; }

    protected override bool UpdateVersion(Entity changedEntity, M.FieldInfo changedField)
    {
      VersionId1 = DateTime.Now.Ticks;
      return true;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class ItemWithAutoVersions : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public sbyte VersionId1 { get; set; }

    [Version, Field]
    public byte VersionId2 { get; set; }

    [Version, Field]
    public short VersionId3 { get; set; }

    [Version, Field]
    public ushort VersionId4 { get; set; }

    [Version, Field]
    public int VersionId5 { get; set; }

    [Version, Field]
    public uint VersionId6 { get; set; }

    [Version, Field]
    public long VersionId7 { get; set; }

    [Version, Field]
    public ulong VersionId8 { get; set; }

    [Field]
    public string Field { get; set; }
  }
}

#endregion

namespace Xtensive.Orm.Tests.Storage
{
  using VersionModel;

  [TestFixture]
  public class UpdateVersionTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.VersionModel");
      return config;
    }

    [Test]
    public void GenerateAutoVersionTest()
    {
      Key key;
      VersionInfo versionInfo;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new ItemWithAutoVersions();
          key = instance.Key;
          versionInfo = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithAutoVersions>(key);
          Assert.AreEqual(versionInfo, instance.VersionInfo);
          instance.Field = "New value";
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithAutoVersions>(key);
          Assert.IsFalse(versionInfo==instance.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void GenerateCustomVersionTest()
    {
      Key key;
      VersionInfo versionInfo;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new ItemWithCustomVersions();
          key = instance.Key;
          versionInfo = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithCustomVersions>(key);
          Assert.AreEqual(versionInfo, instance.VersionInfo);
          instance.Field = "New value";
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithCustomVersions>(key);
          Assert.IsFalse(versionInfo==instance.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void GenerateStructureVersionTest()
    {
      Key key;
      VersionInfo version;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new ItemWithStructureVersion();
          key = instance.Key;
          version = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithStructureVersion>(key);
          Assert.IsTrue(version == instance.VersionInfo);
          instance.Field = "NextValue";
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithStructureVersion>(key);
          Assert.IsFalse(version == instance.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void GenerateEntityVersionTest()
    {
      Key key;
      VersionInfo version;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new ItemWithEntityVersion();
          key = instance.Key;
          version = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithEntityVersion>(key);
          Assert.IsTrue(version==instance.VersionInfo);
          instance.Field = "NextValue";
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = session.Query.Single<ItemWithEntityVersion>(key);
          Assert.IsFalse(version==instance.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void FetchFieldOnGetVersionTest()
    {
      Key customerKey;
      VersionInfo customerVersion;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var customer1 = new Customer {
            Name = "Customer1",
            Address = new Address("City1", "Region1"),
            Phone = new Phone {Code = 123, Number = 321}
          };
          customerKey = customer1.Key;
          customerVersion = customer1.VersionInfo;
          transactionScope.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var customer = session.Query.All<Person>().First(person => person.Name=="Customer1") as Customer;
          Assert.IsTrue(customerVersion==customer.VersionInfo);
          customer.Address = new Address("City2", "Region2");
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var customer = session.Query.All<Person>().First(person => person.Name=="Customer1") as Customer;
          Assert.IsFalse(customerVersion==customer.VersionInfo);
          customerVersion = customer.VersionInfo;
          customer.Phone.Number = 0;
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var customer = session.Query.All<Person>().First(person => person.Name=="Customer1") as Customer;
          Assert.IsFalse(customerVersion==customer.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void AutoUpdateVersionTest()
    {
      Key bookKey;
      VersionInfo bookVersion;
      Key authorKey;
      VersionInfo authorVersion;
      Key commentKey;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var book = new Book {Title = "Book1"};
          var author = new Author {Name = "Author1"};
          book.Authors.Add(author);
          Assert.AreEqual(1, book.Authors.Count);
          Assert.AreEqual(1, author.Books.Count);
          bookKey = book.Key;
          bookVersion = book.VersionInfo;
          authorKey = author.Key;
          var comment = new Comment {Author = author};
          commentKey = comment.Key;
          authorVersion = author.VersionInfo;
          
          transactionScope.Complete();
        }
      }

      // Single property changed
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var author = session.Query.Single<Author>(authorKey);
          Assert.IsTrue(authorVersion==author.VersionInfo);
          author.Name = "Author2";
          Assert.IsFalse(authorVersion==author.VersionInfo);
          authorVersion = author.VersionInfo;
          transactionScope.Complete();
        }
      }

      // Structure field changed
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var author = session.Query.Single<Author>(authorKey);
          author.Phone = new Phone{Code = 123, Number = 321};
          Assert.IsFalse(authorVersion==author.VersionInfo);
          authorVersion = author.VersionInfo;
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var author = session.Query.Single<Author>(authorKey);
          author.Phone.Code = 0;
          Assert.IsFalse(authorVersion==author.VersionInfo);
          authorVersion = author.VersionInfo;
          transactionScope.Complete();
        }
      }

      // OneToMany EntitySet changed
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var comment = session.Query.Single<Comment>(commentKey);
          var author = session.Query.Single<Author>(authorKey);
          comment.Author = null;
          Assert.IsFalse(authorVersion==author.VersionInfo);
          authorVersion = author.VersionInfo;
          transactionScope.Complete();
        }
      }

      // ManyToMany EntitySet changed
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var book = session.Query.Single<Book>(bookKey);
          var author = session.Query.Single<Author>(authorKey);
          Assert.IsTrue(bookVersion==book.VersionInfo);
          Assert.IsTrue(authorVersion==author.VersionInfo);
          book.Authors.Remove(author);
          Assert.AreEqual(0, book.Authors.Count);
          Assert.AreEqual(0, author.Books.Count);
          Assert.IsFalse(authorVersion==author.VersionInfo);
          Assert.IsFalse(bookVersion==book.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void SerializeVersionInfoTest()
    {
      VersionInfo versionInfo;

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new ItemWithAutoVersions();
          versionInfo = instance.VersionInfo;
          transactionScope.Complete();
        }
      }
      Assert.IsFalse(versionInfo.IsVoid);
      var clone = Cloner.Clone(versionInfo);
      Assert.IsFalse(clone.IsVoid);
      Assert.IsTrue(versionInfo==clone);
    }
  }
}