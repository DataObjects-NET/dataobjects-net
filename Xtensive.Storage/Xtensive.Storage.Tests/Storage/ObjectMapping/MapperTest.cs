// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Storage.ObjectMapping;
using Xtensive.Storage.Tests.Storage.ObjectMapping.Model;

namespace Xtensive.Storage.Tests.Storage.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [Test]
    public void SimpleEntitiesMappingTest()
    {
      PersonalProductDto productDto;
      var mapper = new Mapper();
      mapper.MapType<PersonalProduct, PersonalProductDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<Employee, EmployeeDto, string>(e => e.Key.Format(), e => e.Key).Complete();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var employee = new Employee {Age = 25, Name = "A", Position = "B"};
        var product = new PersonalProduct {Employee = employee, Name = "C"};
        productDto = (PersonalProductDto) mapper.Transform(product);
        tx.Complete();
      }
      
      var modifiedProductDto = (PersonalProductDto) productDto.Clone();
      var productNewName = modifiedProductDto.Name + "!!!";
      modifiedProductDto.Name = productNewName;
      const string newEmployeeName = "NewEmployee";
      const int newEmployeeAge = 26;
      const string newEmployeePosition = "F";
      modifiedProductDto.Employee = new EmployeeDto {
        Age = newEmployeeAge, Name = newEmployeeName,
        Position = newEmployeePosition, Key = Guid.NewGuid().ToString()
      };

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var modificationSet = mapper.Compare(productDto, modifiedProductDto);
        modificationSet.Apply();
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var product = Query.All<PersonalProduct>().Single();
        Assert.AreEqual(1, Query.All<Employee>().Count());
        Assert.AreEqual(productNewName, product.Name);
        Assert.AreNotEqual(productDto.Employee.Key, product.Employee.Key.Format());
        Assert.AreEqual(newEmployeeAge, product.Employee.Age);
        Assert.AreEqual(newEmployeeName, product.Employee.Name);
        Assert.AreEqual(newEmployeePosition, product.Employee.Position);
      }
    }

    [Test]
    public void CollectionMappingTest()
    {
      PublisherDto publisherDto;
      var mapper = new Mapper();
      mapper.MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<BookShop, BookShopDto, string>(b => b.Key.Format(), b => b.Key).Complete();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var bookShop0 = new BookShop {Name = "B0"};
        var bookShop1 = new BookShop {Name = "B1"};
        var bookShop2 = new BookShop {Name = "B2"};
        var publisher = new Publisher {Country = "ABC"};
        publisher.Distributors.Add(bookShop0);
        publisher.Distributors.Add(bookShop1);
        publisher.Distributors.Add(bookShop2);
        publisherDto = (PublisherDto) mapper.Transform(publisher);
        tx.Complete();
      }

      var removedBookShop = publisherDto.Distributors.First();
      var modifiedPublisherDto = Clone(publisherDto);
      var newBookShop0 = new BookShopDto {Key = Guid.NewGuid().ToString(), Name = "NB0"};
      var newBookShop1 = new BookShopDto {
        Key = Guid.NewGuid().ToString(), Name = "NB1", Suppliers = new[] {modifiedPublisherDto}
      };
      modifiedPublisherDto.Distributors.Add(newBookShop0);
      modifiedPublisherDto.Distributors.Add(newBookShop1);
      Assert.AreEqual(1, modifiedPublisherDto.Distributors.RemoveWhere(b => b.Key==removedBookShop.Key));
      Assert.IsNotNull(modifiedPublisherDto);

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var modifications = mapper.Compare(publisherDto, modifiedPublisherDto);
        modifications.Apply();
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var publisher = Query.All<Publisher>().Single();
        Assert.AreEqual(4, Query.All<BookShop>().Count());
        Assert.AreEqual(modifiedPublisherDto.Key, publisher.Key.Format());
        Assert.AreEqual(modifiedPublisherDto.Country, publisher.Country);
        var expectedBookShops = modifiedPublisherDto.Distributors.ToDictionary(d => d.Name, d => d);
        Assert.AreEqual(expectedBookShops.Count, publisher.Distributors.Count);
        foreach (var bookShop in publisher.Distributors.AsEnumerable().Cast<BookShop>()) {
          var expectedBookShop = expectedBookShops[bookShop.Name];
          if (expectedBookShop.Key != newBookShop0.Key && expectedBookShop.Key != newBookShop1.Key)
            Assert.AreEqual(expectedBookShop.Key, bookShop.Key.Format());
          if (expectedBookShop.Key != newBookShop0.Key) {
            Assert.AreEqual(1, expectedBookShop.Suppliers.Length);
            Assert.AreEqual(expectedBookShop.Suppliers[0].Key, bookShop.Suppliers.Single().Key.Format());
          }
          Assert.AreEqual(expectedBookShop.Url, bookShop.Url);
        }
      }
    }

    [Test]
    public void CustomEntitySetMappingTest()
    {
      var mapper = new Mapper();
      mapper.MapType<AnotherBookShop, BookShopDto, string>(abs => abs.Key.Format(), bs => bs.Key)
          .Ignore(bs => bs.Name).Ignore(bs => bs.Url)
        .MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
          .Ignore(p => p.Distributors).Complete();
      BookShopDto bookShopDto;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        foreach (var publisher in Query.All<Publisher>())
          publisher.Remove();
        foreach (var bookShop in Query.All<BookShop>())
          bookShop.Remove();
        var anotherBookShop = new AnotherBookShop();
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "A"});
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "B"});
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "C"});
        bookShopDto = (BookShopDto) mapper.Transform(anotherBookShop);
        tx.Complete();
      }

      Assert.IsNotNull(bookShopDto);
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="A"));
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="B"));
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="C"));
      var modifiedBookShopDto = Clone(bookShopDto);
      var newPublisherDto = new PublisherDto {Key = Guid.NewGuid().ToString(), Trademark = "D"};
      var newSuppliers = new PublisherDto[4];
      Array.Copy(modifiedBookShopDto.Suppliers, newSuppliers, modifiedBookShopDto.Suppliers.Length);
      modifiedBookShopDto.Suppliers = newSuppliers;
      modifiedBookShopDto.Suppliers[3] = newPublisherDto;

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var operations = mapper.Compare(bookShopDto, modifiedBookShopDto);
        operations.Apply();
        var newPublisher = Query.All<Publisher>().Where(p => p.Trademark==newPublisherDto.Trademark).Single();
        Assert.AreEqual("D", newPublisher.Trademark);
        var bookShop = Query.All<AnotherBookShop>().Single();
        Assert.AreEqual(4, bookShop.Suppliers.Count());
        tx.Complete();
      }
    }

    private static T Clone<T>(T obj)
    {
      var serializer = new BinaryFormatter();
      using (var stream = new MemoryStream()) {
        serializer.Serialize(stream, obj);
        stream.Seek(0, SeekOrigin.Begin);
        return (T) serializer.Deserialize(stream);
      }
    }
  }
}