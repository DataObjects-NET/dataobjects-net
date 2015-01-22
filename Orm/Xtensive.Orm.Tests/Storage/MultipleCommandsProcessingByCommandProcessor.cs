// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.12.26

using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using JetBrains.Annotations;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.MultipleCommandsProcessingByCommandProcessorModel;
using Xtensive.Orm.Weaving;
#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Storage.MultipleCommandsProcessingByCommandProcessorModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    [Association(PairTo = "Person")]
    public EntitySet<Contact> Contacts { get; set; }

    public Person(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  public class Contact : Entity
  {
    [Field, Key]
    public int Id { get; set; }
    
    [Field]
    public string Value { get; set; }

    [Field(Nullable = false)]
    public ContactType Type { get; set; }

    [Field]
    public Person Person { get; set; }

    public Contact(Session session)
      : base(session)
    { }
  }

  [HierarchyRoot]
  public class ContactType : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Nullable = false)]
    public string TypeName { get; set; }

    [Field]
    public string IconPath { get; set; }

    public ContactType(Session session)
      : base(session)
    { }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class MultipleCommandsProcessingByCommandProcessor : AutoBuildTest
  {
    [Test]
    public async void PersistBatchAndAsyncQuery()
    {
      Key jessyPinkmanKey;
      Key gregHouseKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var countOfContacts = session.Query.All<Contact>().Count();
        var countOfPersons = session.Query.All<Person>().Count();
        var contactTypes = session.Query.All<ContactType>().ToList();
        var jessyPinkman = new Person(session) {Name = "Jassy", Surname = "Pinkman"};
        jessyPinkmanKey = jessyPinkman.Key;
        jessyPinkman.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[0].TypeName, jessyPinkman.Name, jessyPinkman.Surname), Type = contactTypes[0]});

        var gregHouse = new Person(session) {Name = "Greg", Surname = "House"};
        gregHouseKey = gregHouse.Key;
        gregHouse.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[3].TypeName, gregHouse.Name, gregHouse.Surname), Type = contactTypes[3]});

        var allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var contacts1 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[0]));
        var contacts2 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[1]));
        var contacts3 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[2]));
        var contacts4 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[3]));
        var contacts5 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[4]));
        var contacts6 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[5]));
        var contacts7 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[6]));
        var allPersons = session.Query.ExecuteDelayed(query => query.All<Person>());

        var persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();
        Assert.AreEqual(allContacts.Count(), countOfContacts + 2);
        Assert.AreEqual(5, contacts1.Count());
        Assert.AreEqual(4, contacts2.Count());
        Assert.AreEqual(4, contacts3.Count());
        Assert.AreEqual(5, contacts4.Count());
        Assert.AreEqual(4, contacts5.Count());
        Assert.AreEqual(4, contacts6.Count());
        Assert.AreEqual(4, contacts7.Count());
        Assert.IsTrue(persons.Count!=0);
        Assert.AreEqual(countOfPersons, persons.Count);
        Assert.AreEqual(countOfPersons + 2, allPersons.Count());
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var jessy = session.Query.Single<Person>(jessyPinkmanKey);
        var contacts = jessy.Contacts.ToList();
        jessy.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        jessy.Remove();

        var greg = session.Query.Single<Person>(gregHouseKey);
        contacts = greg.Contacts.ToList();
        greg.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        greg.Remove();
        transaction.Complete();
      }
    }

    [Test]
    public async void BatchPersistAndAsyncQuery()
    {
      Key jessyPinkmanKey;
      Key gregHouseKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var countOfPersons = session.Query.All<Person>().Count();
        var countOfContacts = session.Query.All<Contact>().Count();
        var contactTypes = session.Query.All<ContactType>().ToList();
        var allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var contacts1 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[0]));
        var contacts2 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[1]));
        var contacts3 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[2]));
        var contacts4 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[3]));
        var contacts5 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[4]));
        var contacts6 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[5]));
        var contacts7 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[6]));
        var allPersons = session.Query.ExecuteDelayed(query => query.All<Person>());

        var jessyPinkman = new Person(session) { Name = "Jassy", Surname = "Pinkman" };
        jessyPinkmanKey = jessyPinkman.Key;
        jessyPinkman.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[0].TypeName, jessyPinkman.Name, jessyPinkman.Surname), Type = contactTypes[0]});

        var gregHouse = new Person(session) { Name = "Greg", Surname = "House" };
        gregHouseKey = gregHouse.Key;
        gregHouse.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[3].TypeName, gregHouse.Name, gregHouse.Surname), Type = contactTypes[3]});

        var persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();

        Assert.AreEqual(allContacts.Count(), countOfContacts + 2);
        Assert.AreEqual(5, contacts1.Count());
        Assert.AreEqual(4, contacts2.Count());
        Assert.AreEqual(4, contacts3.Count());
        Assert.AreEqual(5, contacts4.Count());
        Assert.AreEqual(4, contacts5.Count());
        Assert.AreEqual(4, contacts6.Count());
        Assert.AreEqual(4, contacts7.Count());
        Assert.IsTrue(persons.Count!=0);
        Assert.AreEqual(countOfPersons, persons.Count);
        Assert.AreEqual(countOfPersons + 2, allPersons.Count());

        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var jessy = session.Query.Single<Person>(jessyPinkmanKey);
        var contacts = jessy.Contacts.ToList();
        jessy.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        jessy.Remove();

        var greg = session.Query.Single<Person>(gregHouseKey);
        contacts = greg.Contacts.ToList();
        greg.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        greg.Remove();
        transaction.Complete();
      }
    }

    [Test]
    public async void PersistAsyncQuery()
    {
      Key jessyPinkmanKey;
      Key gregHouseKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var countOfContacts = session.Query.All<Contact>().Count();
        var contactTypes = session.Query.All<ContactType>().ToList();
        var countOfPersons = session.Query.All<Person>().Count();
        var allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var jessyPinkman = new Person(session) {Name = "Jassy", Surname = "Pinkman"};
        jessyPinkmanKey = jessyPinkman.Key;
        jessyPinkman.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[0].TypeName, jessyPinkman.Name, jessyPinkman.Surname), Type = contactTypes[0]});

        var gregHouse = new Person(session) {Name = "Greg", Surname = "House"};
        gregHouseKey = gregHouse.Key;
        gregHouse.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[3].TypeName, gregHouse.Name, gregHouse.Surname), Type = contactTypes[3]});

        var persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();

        var allPersons = session.Query.All<Person>().ToList();
        Assert.AreEqual(allContacts.Count(), countOfContacts + 2);
        Assert.IsTrue(persons.Count != 0);
        Assert.AreEqual(countOfPersons, persons.Count);
        Assert.AreEqual(countOfPersons + 2, allPersons.Count());
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var jessy = session.Query.Single<Person>(jessyPinkmanKey);
        var contacts = jessy.Contacts.ToList();
        jessy.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        jessy.Remove();

        var greg = session.Query.Single<Person>(gregHouseKey);
        contacts = greg.Contacts.ToList();
        greg.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        greg.Remove();
        transaction.Complete();
      }
    }

    [Test]
    public async void PersistAsyncQueryPersistBatchAsyncQuery()
    {
      Key jessyPinkmanKey;
      Key gregHouseKey;
      Key nelsonMandelaKey;
      Key gregMorisonKey;
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        //P+AQ
        var countOfContacts = session.Query.All<Contact>().Count();
        var contactTypes = session.Query.All<ContactType>().ToList();
        var countOfPersons = session.Query.All<Person>().Count();
        var allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var jessyPinkman = new Person(session) {Name = "Jassy", Surname = "Pinkman"};
        jessyPinkmanKey = jessyPinkman.Key;
        jessyPinkman.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[0].TypeName, jessyPinkman.Name, jessyPinkman.Surname), Type = contactTypes[0]});

        var gregHouse = new Person(session) {Name = "Greg", Surname = "House"};
        gregHouseKey = gregHouse.Key;
        gregHouse.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[3].TypeName, gregHouse.Name, gregHouse.Surname), Type = contactTypes[3]});

        var persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();

        var allPersons = session.Query.All<Person>().ToList();
        Assert.AreEqual(allContacts.Count(), countOfContacts + 2);
        Assert.IsTrue(persons.Count != 0);
        Assert.AreEqual(countOfPersons, persons.Count);
        Assert.AreEqual(countOfPersons + 2, allPersons.Count());

        var nelsonMandela = new Person(session) {Name = "Nelson", Surname = "Mandela"};
        nelsonMandelaKey = nelsonMandela.Key;
        nelsonMandela.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[0].TypeName, nelsonMandela.Name, nelsonMandela.Surname), Type = contactTypes[0]});

        var gregMorison = new Person(session) {Name = "Greg", Surname = "Morison"};
        gregMorisonKey = gregMorison.Key;
        gregMorison.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactTypes[3].TypeName, gregMorison.Name, gregMorison.Surname), Type = contactTypes[3]});

        allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var contacts1 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[0]));
        var contacts2 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[1]));
        var contacts3 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[2]));
        var contacts4 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[3]));
        var contacts5 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[4]));
        var contacts6 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[5]));
        var contacts7 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type==contactTypes[6]));
        var allPersonsDelayed = session.Query.ExecuteDelayed(query => query.All<Person>());

        persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();
        Assert.AreEqual(allContacts.Count(), countOfContacts + 4);
        Assert.AreEqual(6, contacts1.Count());
        Assert.AreEqual(4, contacts2.Count());
        Assert.AreEqual(4, contacts3.Count());
        Assert.AreEqual(6, contacts4.Count());
        Assert.AreEqual(4, contacts5.Count());
        Assert.AreEqual(4, contacts6.Count());
        Assert.AreEqual(4, contacts7.Count());
        Assert.IsTrue(persons.Count != 0);
        Assert.AreEqual(countOfPersons, persons.Count);
        Assert.AreEqual(countOfPersons + 4, allPersonsDelayed.Count());
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var jessy = session.Query.Single<Person>(jessyPinkmanKey);
        var contacts = jessy.Contacts.ToList();
        jessy.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        jessy.Remove();

        var greg = session.Query.Single<Person>(gregHouseKey);
        contacts = greg.Contacts.ToList();
        greg.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        greg.Remove();

        var nelson = session.Query.Single<Person>(nelsonMandelaKey);
        contacts = nelson.Contacts.ToList();
        nelson.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        nelson.Remove();

        var morison = session.Query.Single<Person>(gregMorisonKey);
        contacts = morison.Contacts.ToList();
        morison.Contacts.Clear();
        foreach (var contact in contacts)
          contact.Remove();
        morison.Remove();
        transaction.Complete();
      }
    }

    [Test]
    public async void BatchAsyncQueryAsyncQuery()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var contactTypes = session.Query.All<ContactType>().ToList();
        var allContacts = session.Query.ExecuteDelayed(query => query.All<Contact>());
        var contacts1 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[0]));
        var contacts2 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[1]));
        var contacts3 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[2]));
        var contacts4 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[3]));
        var contacts5 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[4]));
        var contacts6 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[5]));
        var contacts7 = session.Query.ExecuteDelayed(query => query.All<Contact>().Where(c => c.Type == contactTypes[6]));
        var allPersonsDelayed = session.Query.ExecuteDelayed(query => query.All<Person>());
        var persons = (await session.Query.ExecuteAsync(query => query.All<Person>().Where(p => p.Contacts.Count() > 5))).ToList();
        var contacts = (await session.Query.ExecuteAsync(query => query.All<Contact>().Where(c=>c.Value.Contains("Sparrow")))).ToList();
        transaction.Complete();
      }
    }
    
    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        PopulateContactTypes(session);
        PopulatePersons(session);
        PopulateContacts(session);
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Contact).Assembly, typeof(Contact).Namespace);
      configuration.Sessions["Default"].Options = SessionOptions.ServerProfile;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    private void PopulateContactTypes(Session session)
    {
      new ContactType(session) {TypeName = "MobilePhone"};
      new ContactType(session) {TypeName = "ICQ"};
      new ContactType(session) {TypeName = "WhatsUp"};
      new ContactType(session) {TypeName = "E-mail"};
      new ContactType(session) {TypeName = "Phone"};
      new ContactType(session) {TypeName = "OfficePhone"};
      new ContactType(session) {TypeName = "Skype"};
    }

    private void PopulateContacts(Session session)
    {
      var persons = session.Query.All<Person>();
      var contactTypes = session.Query.All<ContactType>();
      foreach (var person in persons) {
        foreach (var contactType in contactTypes) {
          person.Contacts.Add(new Contact(session) {Value = string.Format("{0} of {1} {2}", contactType.TypeName, person.Name, person.Surname), Type = contactType});
        }
      }
    }

    private void PopulatePersons(Session session)
    {
      new Person(session) {Name = "James", Surname = "Bond"};
      new Person(session) {Name = "Julia", Surname = "Roberts"};
      new Person(session) {Name = "Jonny", Surname = "Depp"};
      new Person(session) {Name = "Jack", Surname = "Sparrow"};
    }
  }
}
#endif