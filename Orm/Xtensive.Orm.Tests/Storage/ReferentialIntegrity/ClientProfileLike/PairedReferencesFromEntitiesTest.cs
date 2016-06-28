// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.06.23

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfileLike.PairedReferencesFromEntitiesModel;

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfileLike.PairedReferencesFromEntitiesModel
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public string MidName { get; set; }

    [Field]
    public EntitySet<Book> Books { get; set; }
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set;}

    [Field]
    public DriverLicense DriverLicense { get; set; }
  }

  [HierarchyRoot]
  public class DriverLicense : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [Association(PairTo = "DriverLicense")]
    public Person Person { get; set; }
  }

  [HierarchyRoot]
  public class TableOfContent : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [Association(PairTo="TableOfContent")]
    public EntitySet<TableOfContentItem> Items { get; set; } 
  }

  [HierarchyRoot]
  public class TableOfContentItem : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public int Page { get; set; }

    [Field]
    public TableOfContent TableOfContent { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfileLike
{
  public class PairedReferencesFromEntitiesTest : AutoBuildTest
  {
    [Test]
    public void InitializeOneToOneReferenceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var person = new Person();
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(0));
        var driverLicense = new DriverLicense();
        Assert.That(ReferenceFinder.GetReferencesTo(driverLicense).Count(), Is.EqualTo(0));
        person.DriverLicense = driverLicense;
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(driverLicense).Count(), Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        person = new Person();
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(0));
        driverLicense = new DriverLicense();
        Assert.That(ReferenceFinder.GetReferencesTo(driverLicense).Count(), Is.EqualTo(0));
        driverLicense.Person = person;
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(driverLicense).Count(), Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      }
    }

    [Test]
    public void InitializeOneToManyReferenceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var tableOfContent = new TableOfContent();
        var tableOfContentItem1 = new TableOfContentItem();
        var tableOfContentItem2 = new TableOfContentItem();
        var tableOfContentItem3 = new TableOfContentItem();
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContent.Items.Add(tableOfContentItem1);
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContent.Items.Add(tableOfContentItem2);
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContent.Items.Add(tableOfContentItem3);
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContent = new TableOfContent();
        tableOfContentItem1 = new TableOfContentItem();
        tableOfContentItem2 = new TableOfContentItem();
        tableOfContentItem3 = new TableOfContentItem();
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContentItem1.TableOfContent = tableOfContent;
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContentItem2.TableOfContent = tableOfContent;
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        tableOfContentItem3.TableOfContent = tableOfContent;
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContentItem3).Count(), Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      }
    }

    [Test]
    public void InitializeManyToManyReferenceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var author1 = new Author();
        var author2 = new Author();
        var author3 = new Author();
        var book1 = new Book();
        var book2 = new Book();
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(author1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(author2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(author3).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(book1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(book2).Count(), Is.EqualTo(0));


        book1.Authors.Add(author1);
        book1.Authors.Add(author2);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(author1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(author2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(author3).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(book1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(book2).Count(), Is.EqualTo(0));

        book2.Authors.Add(author2);
        book2.Authors.Add(author3);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(author1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(author2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(author3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(book1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(book2).Count(), Is.EqualTo(2));
      }
    }

    [Test]
    public void ChangeSavedOneToOneReferenceTest()
    {
      Key person1Key, person2Key;
      Key license1Key, license2Key;
      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var person = new Person() {DriverLicense = new DriverLicense()};
        person1Key = person.Key;
        license1Key = person.DriverLicense.Key;

        person = new Person() {DriverLicense = new DriverLicense()};
        person2Key = person.Key;
        license2Key = person.DriverLicense.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var person1 = session.Query.Single<Person>(person1Key);
        var person2 = session.Query.Single<Person>(person2Key);
        var license1 = person1.DriverLicense;
        var license2 = person2.DriverLicense;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(person2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license2).Count(), Is.EqualTo(1));

        var newlicense = new DriverLicense();
        person1.DriverLicense = newlicense;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(person2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(license2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newlicense).Count(), Is.EqualTo(1));

        var newPerson = new Person();
        license2.Person = newPerson;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(person2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newPerson).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(license2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newlicense).Count(), Is.EqualTo(1));

        person1.DriverLicense = null;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newPerson).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(license2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newlicense).Count(), Is.EqualTo(0));

        license2.Person = null;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newPerson).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(license1).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(license2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newlicense).Count(), Is.EqualTo(0));
      }
    }

    [Test]
    public void ChangeSavedOneToManyReferenceTest()
    {
      Key tableOfContentKey;
      Key item1Key, item2Key, item3Key;
      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var tableOfContent = new TableOfContent();
        var item1 = new TableOfContentItem();
        var item2 = new TableOfContentItem();
        var item3 = new TableOfContentItem();
        tableOfContentKey = tableOfContent.Key;
        item1Key = item1.Key;
        item2Key = item2.Key;
        item3Key = item3.Key;
        tableOfContent.Items.Add(item1);
        tableOfContent.Items.Add(item2);
        tableOfContent.Items.Add(item3);
        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var tableOfContent = session.Query.Single<TableOfContent>(tableOfContentKey);
        var item1 = session.Query.Single<TableOfContentItem>(item1Key);
        var item2 = session.Query.Single<TableOfContentItem>(item2Key);
        var item3 = session.Query.Single<TableOfContentItem>(item3Key);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));

        var newTableOfContent = new TableOfContent();
        item1.TableOfContent = newTableOfContent;

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newTableOfContent).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));

        item2.TableOfContent = null;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newTableOfContent).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
      }
    }

    [Test]
    public void ChangeReferenceToSameObjectTest()
    {
      Key person1Key, person2Key;
      Key license1Key, license2Key;
      Key tableOfContentKey, item1Key, item2Key;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var person1 = new Person {DriverLicense = new DriverLicense()};
        var person2 = new Person {DriverLicense = new DriverLicense()};
        person1Key = person1.Key;
        person2Key = person2.Key;
        license1Key = person1.DriverLicense.Key;
        license2Key = person2.DriverLicense.Key;

        var tableOfContent = new TableOfContent();
        tableOfContentKey = tableOfContent.Key;

        var item1 = new TableOfContentItem();
        item1Key = item1.Key;
        tableOfContent.Items.Add(item1);

        var item2 = new TableOfContentItem();
        item2Key = item2.Key;
        tableOfContent.Items.Add(item2);

        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var person = session.Query.Single<Person>(person1Key);
        var license = session.Query.Single<DriverLicense>(license1Key);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license).Count(), Is.EqualTo(1));
        person.DriverLicense = license;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license).Count(), Is.EqualTo(1));

        person = session.Query.Single<Person>(person2Key);
        license = session.Query.Single<DriverLicense>(license2Key);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license).Count(), Is.EqualTo(1));

        license.Person = person;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(person).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(license).Count(), Is.EqualTo(1));

        var item1 = session.Query.Single<TableOfContentItem>(item1Key);
        var item2 = session.Query.Single<TableOfContentItem>(item2Key);
        var tableOfContent = session.Query.Single<TableOfContent>(tableOfContentKey);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));

        item1.TableOfContent = tableOfContent;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));

        item2.TableOfContent = tableOfContent;
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableOfContent).Count(), Is.EqualTo(2));
      }
    }

    [Test]
    public void AddEntityToEntitySetTest()
    {
      Key tableWithItemsKey;
      Key item1Key, item2Key, item3Key, item4Key;
      Key book1Key, book2Key, book3Key;
      Key author1Key, author2Key, author3Key;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var tableWithItems = new TableOfContent();
        tableWithItemsKey = tableWithItems.Key;
        var item1 = new TableOfContentItem();
        var item2 = new TableOfContentItem();
        item1Key = item1.Key;
        item2Key = item2.Key;
        tableWithItems.Items.Add(item1);
        tableWithItems.Items.Add(item2);

        var item3 = new TableOfContentItem();
        item3Key = item3.Key;
        var item4 = new TableOfContentItem();
        item4Key = item4.Key;

        var book1 = new Book();
        book1Key = book1.Key;
        var book2 = new Book();
        book2Key = book2.Key;
        var book3 = new Book();
        book3Key = book3.Key;

        var author1 = new Author();
        author1Key = author1.Key;
        var author2 = new Author();
        author2Key = author2.Key;
        var author3 = new Author();
        author3Key = author3.Key;

        author1.Books.Add(book1);
        author2.Books.Add(book2);
        author3.Books.Add(book3);

        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var tableWithItems = session.Query.Single<TableOfContent>(tableWithItemsKey);
        var item1 = session.Query.Single<TableOfContentItem>(item1Key);
        var item2 = session.Query.Single<TableOfContentItem>(item2Key);
        var item3 = session.Query.Single<TableOfContentItem>(item3Key);
        var item4 = session.Query.Single<TableOfContentItem>(item4Key);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item4).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(2));

        tableWithItems.Items.Add(item3);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item4).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(3));

        var newTableWithItems = new TableOfContent();
        newTableWithItems.Items.Add(item4);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item4).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newTableWithItems).Count(), Is.EqualTo(1));

        var anotherNewTableWithItems = new TableOfContent();
        var item5 = new TableOfContentItem();
        var item6 = new TableOfContentItem();
        anotherNewTableWithItems.Items.Add(item5);
        anotherNewTableWithItems.Items.Add(item6);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item4).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item5).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item6).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newTableWithItems).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(anotherNewTableWithItems).Count(), Is.EqualTo(2));

        var item7 = new TableOfContentItem();
        var item8 = new TableOfContentItem();
        tableWithItems.Items.Add(item7);
        tableWithItems.Items.Add(item8);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item4).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item5).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item6).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item7).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item8).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(5));
        Assert.That(ReferenceFinder.GetReferencesTo(newTableWithItems).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(anotherNewTableWithItems).Count(), Is.EqualTo(2));

        var oldBook1 = session.Query.Single<Book>(book1Key);
        var oldBook2 = session.Query.Single<Book>(book2Key);
        var oldBook3 = session.Query.Single<Book>(book3Key);

        var oldAutor1 = session.Query.Single<Author>(author1Key);
        var oldAutor2 = session.Query.Single<Author>(author2Key);
        var oldAutor3 = session.Query.Single<Author>(author3Key);

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor3).Count(), Is.EqualTo(1));

        oldAutor3.Books.Add(oldBook1);
        oldBook3.Authors.Add(oldAutor2);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook3).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor3).Count(), Is.EqualTo(2));


        var newAuthor = new Author();
        var newBook = new Book();
        oldAutor2.Books.Add(newBook);
        newBook.Authors.Add(oldAutor1);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook3).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor3).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(0));

        oldBook2.Authors.Add(newAuthor);
        newAuthor.Books.Add(oldBook3);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook3).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor3).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(2));

        newBook.Authors.Add(newAuthor);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook3).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor3).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(3));
      }
    }

    [Test]
    public void RemoveEntityToEntitySetTest()
    {
      Key tableWithItemsKey;
      Key item1Key, item2Key;
      Key book1Key, book2Key;
      Key author1Key, author2Key;

      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var tableWithItems = new TableOfContent();
        tableWithItemsKey = tableWithItems.Key;
        var item1 = new TableOfContentItem();
        var item2 = new TableOfContentItem();
        item1Key = item1.Key;
        item2Key = item2.Key;
        tableWithItems.Items.Add(item1);
        tableWithItems.Items.Add(item2);

        var book1 = new Book();
        book1Key = book1.Key;
        var book2 = new Book();
        book2Key = book2.Key;

        var author1 = new Author();
        author1Key = author1.Key;
        var author2 = new Author();
        author2Key = author2.Key;

        author1.Books.Add(book1);
        author1.Books.Add(book2);
        author2.Books.Add(book1);
        author2.Books.Add(book2);

        transaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      using (session.DisableSaveChanges()) {
        var tableWithItems = session.Query.Single<TableOfContent>(tableWithItemsKey);
        var item1 = session.Query.Single<TableOfContentItem>(item1Key);
        var item2 = session.Query.Single<TableOfContentItem>(item2Key);
        var item3 = new TableOfContentItem();
        tableWithItems.Items.Add(item3);

        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(3));

        tableWithItems.Items.Remove(item3);
        tableWithItems.Items.Remove(item2);
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(item2).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(item3).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(tableWithItems).Count(), Is.EqualTo(1));

        var oldBook1 = session.Query.Single<Book>(book1Key);
        var oldBook2 = session.Query.Single<Book>(book2Key);
        var newBook = new Book();

        var oldAutor1 = session.Query.Single<Author>(author1Key);
        var oldAutor2 = session.Query.Single<Author>(author2Key);
        var newAuthor = new Author();

        newAuthor.Books.Add(oldBook1);
        newAuthor.Books.Add(oldBook2);
        newAuthor.Books.Add(newBook);

        newBook.Authors.Add(oldAutor1);
        newBook.Authors.Add(oldAutor2);

        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(3));

        oldAutor1.Books.Remove(oldBook1);
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(3));

        oldBook2.Authors.Remove(oldAutor2);
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(3));

        oldAutor1.Books.Remove(newBook);
        newBook.Authors.Remove(oldAutor2);
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(3));

        newBook.Authors.Remove(newAuthor);
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook1).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldBook2).Count(), Is.EqualTo(2));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor1).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(oldAutor2).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(newBook).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(newAuthor).Count(), Is.EqualTo(2));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TableOfContent).Assembly, typeof (TableOfContent).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
