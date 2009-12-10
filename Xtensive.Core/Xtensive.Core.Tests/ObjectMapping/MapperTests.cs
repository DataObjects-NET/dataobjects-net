// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using NUnit.Framework;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.Tests.ObjectMapping.SourceModel;
using Xtensive.Core.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Core.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTests
  {
    [Test]
    public void DefaultConversionOfPrimitivePropertiesTest()
    {
      var source = GetSourcePerson();
      var mapper = GetPersonBookMapper();
      var target = (PersonDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      AssertAreEqual(source, target);
    }

    [Test]
    public void DefaultConversionOfComplexPropertiesTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonBookMapper();
      var target = (OrderDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      Assert.AreEqual(source.Id, target.Key);
      Assert.AreEqual(source.ShipDate, target.ShipDate);
      AssertAreEqual(source.Customer, target.Customer);
    }

    [Test]
    public void ComparisonOfObjectsContainingPrimitivePropertiesOnlyTest()
    {
      var source = GetSourcePerson();
      var mapper = GetPersonBookMapper();
      var target = (PersonDto) mapper.Transform(source);
      var clone = (PersonDto) target.Clone();
      var modifiedDate = clone.BirthDate.AddDays(23);
      clone.BirthDate = modifiedDate;
      var modifiedFirstName = clone.FirstName + "!";
      clone.FirstName = modifiedFirstName;
      var eventRaisingCount = 0;
      mapper.ObjectModified += descriptor => {
        eventRaisingCount++;
        switch (descriptor.Property.Name) {
        case "BirthDate":
          Assert.AreEqual(modifiedDate, descriptor.NewValue);
          break;
        case "FirstName":
          Assert.AreEqual(modifiedFirstName, descriptor.NewValue);
          break;
        default:
          Assert.Fail();
          break;
        }
      };
      mapper.Compare(target, clone);
      Assert.AreEqual(2, eventRaisingCount);
    }

    [Test]
    public void ComparisonOfObjectsWhenOnlyPrimitivePropertiesHaveBeenModifiedTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonBookMapper();
      var target = (OrderDto) mapper.Transform(source);
      var clone = (OrderDto) target.Clone();
      var modifiedShipDate = clone.ShipDate.AddDays(-5);
      clone.ShipDate = modifiedShipDate;
      var modifiedFirstName = clone.Customer.FirstName + "!";
      clone.Customer.FirstName = modifiedFirstName;
      var eventRaisingCount = 0;
      mapper.ObjectModified += descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Source)) {
          Assert.AreEqual("ShipDate", descriptor.Property.Name);
          Assert.AreEqual(modifiedShipDate, descriptor.NewValue);
        }
        else if (ReferenceEquals(target.Customer, descriptor.Source)) {
          Assert.AreEqual("FirstName", descriptor.Property.Name);
          Assert.AreEqual(modifiedFirstName, descriptor.NewValue);
        }
        else
          Assert.Fail();
      };
      mapper.Compare(target, clone);
      Assert.AreEqual(2, eventRaisingCount);
    }

    [Test]
    public void ComparisonOfObjectsWhenReferencedObjectHasBeenReplacedTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonBookMapper();
      var target = (OrderDto) mapper.Transform(source);
      var clone = (OrderDto) target.Clone();
      var modifiedShipDate = clone.ShipDate.AddDays(-5);
      clone.ShipDate = modifiedShipDate;
      var modifiedCustomer = new PersonDto {
        BirthDate = DateTime.Now.AddYears(25), FirstName = "Stewart", LastName = "Smith", Id = 4
      };
      clone.Customer = modifiedCustomer;
      var eventRaisingCount = 0;
      mapper.ObjectModified += descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Source)) {
          Assert.AreSame(target, descriptor.Source);
          switch (descriptor.Property.Name) {
          case "ShipDate":
            Assert.AreEqual(modifiedShipDate, descriptor.NewValue);
            break;
          case "Customer":
            Assert.AreSame(modifiedCustomer, descriptor.NewValue);
            var descriptorCustomer = (PersonDto) descriptor.NewValue;
            Assert.AreEqual(modifiedCustomer.BirthDate, descriptorCustomer.BirthDate);
            Assert.AreEqual(modifiedCustomer.FirstName, descriptorCustomer.FirstName);
            Assert.AreEqual(modifiedCustomer.Id, descriptorCustomer.Id);
            Assert.AreEqual(modifiedCustomer.LastName, descriptorCustomer.LastName);
            break;
          default:
            Assert.Fail();
            break;
          }
        }
        else if (ReferenceEquals(modifiedCustomer, descriptor.Source)) {
          Assert.AreEqual(ModificationType.CreateObject, descriptor.Type);
          Assert.IsNull(descriptor.Property);
          Assert.IsNull(descriptor.NewValue);
        }
        else if (ReferenceEquals(target.Customer, descriptor.Source)) {
          Assert.AreEqual(ModificationType.RemoveObject, descriptor.Type);
          Assert.IsNull(descriptor.Property);
          Assert.IsNull(descriptor.NewValue);
        }
        else
          Assert.Fail();
      };
      mapper.Compare(target, clone);
      Assert.AreEqual(4, eventRaisingCount);
    }

    [Test]
    public void TransformationUsingCustomMapping()
    {
      var source = GetSourceAuthor();
      var mapper = GetAuthorBookMapper();
      var target = (AuthorDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      Assert.AreEqual(source.Name + "!!!", target.Name);
      Assert.AreEqual(source.Book.ISBN, target.Book.ISBN);
      Assert.AreEqual(source.Book.Title.Text, target.Book.TitleText);
      Assert.AreEqual(source.Book.Title.Id, target.Book.Title.Id);
    }

    private static DefaultMapper GetPersonBookMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Person, int, PersonDto, int>(p => p.Id, p => p.Id)
        .MapType<Order, Guid, OrderDto, Guid>(o => o.Id, o => o.Key).Complete();
      return result;
    }

    private static DefaultMapper GetAuthorBookMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Author, Guid, AuthorDto, Guid>(a => a.Id, a => a.Id)
          .Map(a => a.Name + "!!!", a => a.Name)
        .MapType<Book, string, BookDto, string>(b => b.ISBN, b => b.ISBN)
          .Map(b => b.Title.Text, b => b.TitleText)
          .Map(b => new TitleDto {Id = b.Title.Id, Text = b.Title.Text}, b => b.Title)
        .MapType<Title, Guid, TitleDto, Guid>(t => t.Id, t => t.Id).Complete();
      return result;
    }

    private static Person GetSourcePerson()
    {
      return new Person {
        BirthDate = DateTime.Now.AddYears(-20), FirstName = "John", LastName = "Smith", Id = 3
      };
    }

    private static Order GetSourceOrder()
    {
      return new Order {
        Customer = GetSourcePerson(), Id = Guid.NewGuid(), ShipDate = DateTime.Today.AddMonths(3)
      };
    }

    private static Author GetSourceAuthor()
    {
      var title = new Title {Id = Guid.NewGuid(), Text = "T"};
      var book = new Book {ISBN = Guid.NewGuid().ToString(), Price = 25.0, Title = title};
      return new Author {Book = book, Id = Guid.NewGuid(), Name = "A"};
    }

    private static void AssertAreEqual(Person source, PersonDto target)
    {
      Assert.AreEqual(source.BirthDate, target.BirthDate);
      Assert.AreEqual(source.FirstName, target.FirstName);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.LastName, target.LastName);
    }
  }
}