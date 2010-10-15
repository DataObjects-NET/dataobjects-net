// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;
using Xtensive.Storage.Tests.Issues.Issue0676.Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0676.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Animal : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    [Association(OnTargetRemove = OnRemoveAction.None)]
    public Animal Mate { get; set; }

    [Field(Nullable = false)]
    public Animal MateDenyRemove { get; set; }

    [Field]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }

    public Animal(string name)
    {
      using (Session.Pin(this)) {
        Name = name;
        Mate = this;
        MateDenyRemove = this;
      }
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity,
    IHasNullEntity
  {
    private sealed class KeyExtension
    {
      public Key Key { get; set; }
    }

    #region "Null entity" pattern implementation

    public const string NullName = "<None>";
    private static object @lock = new object();

    public static Person Null { 
      get {
        return Query.Single<Person>(NullKey);
      }
    }

    protected static Key NullKey {
      get {
        var domain = Domain.Current;
        if (domain==null)
          return null;
        var extensions = domain.Extensions;
        var keyExtension = extensions.Get<KeyExtension>();
        if (keyExtension==null) 
          lock (@lock) {
            keyExtension = extensions.Get<KeyExtension>();
            if (keyExtension==null) {
              var nullPerson = Query.All<Person>().Where(p => p.Name==NullName).SingleOrDefault();
              if (nullPerson!=null) {
                keyExtension = new KeyExtension {Key = nullPerson.Key};
                extensions.Set(keyExtension);
              }
            }
          }
        return keyExtension==null ? null : keyExtension.Key;
      }
    }

    IEntity IHasNullEntity.NullEntity {
      get { return Null; }
    }

    #endregion

    [Field, Key]
    public int Id { get; private set; }

    [Field(Nullable = false)]
    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    public Person Mate { get; set; }

    [Field]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }

    public Person(string name)
    {
      using (Session.Pin(this)) {
        Name = name;
        Mate = name==NullName ? this : Null;
      }
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0676_NonNullableReferenceBug : AutoBuildTest
  {
    private const string VersionFieldName = "Version";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return configuration;
    }

    [Test]
    public void StandardTest()
    {
      using (var session = Session.Open(Domain)) {
        var tAnimal = session.Domain.Model.Types[typeof (Animal)];
        var fMate = tAnimal.Fields["Mate"];
        Assert.AreEqual(OnRemoveAction.None, fMate.GetAssociation(tAnimal).OnOwnerRemove);
        Assert.AreEqual(OnRemoveAction.None, fMate.GetAssociation(tAnimal).OnTargetRemove);
        var fMateDenyRemove = tAnimal.Fields["MateDenyRemove"];
        Assert.AreEqual(OnRemoveAction.None, fMateDenyRemove.GetAssociation(tAnimal).OnOwnerRemove);
        Assert.AreEqual(OnRemoveAction.Deny, fMateDenyRemove.GetAssociation(tAnimal).OnTargetRemove);

        var a = new Animal("A");
        var b = new Animal("B") { Mate = a, MateDenyRemove = a };
        var aKey = a.Key;
        AssertEx.Throws<ReferentialIntegrityException>(() => 
          a.Remove());
        b.MateDenyRemove = b;
        a.Remove();
        
        using (var tx = Transaction.Open()) {
          var dpa = session.Services.Demand<DirectPersistentAccessor>();
          Assert.AreEqual(aKey, dpa.GetReferenceKey(b, fMate));
          tx.Complete();
        }
        b.Remove();
      }
    }

    [Test]
    public void HasNullEntityTest()
    {
      using (var session = Session.Open(Domain)) {
        var tPerson = session.Domain.Model.Types[typeof(Person)];
        var fMate = tPerson.Fields["Mate"];
        Assert.AreEqual(OnRemoveAction.None, fMate.GetAssociation(tPerson).OnOwnerRemove);
        Assert.AreEqual(OnRemoveAction.Clear, fMate.GetAssociation(tPerson).OnTargetRemove);

        var nullPerson = new Person(Person.NullName);
        Assert.AreSame(nullPerson, Person.Null);

        var a = new Person("A");
        var b = new Person("B") { Mate = a };
        a.Remove();
        Assert.AreSame(nullPerson, b.Mate);
      }
    }
  }
}