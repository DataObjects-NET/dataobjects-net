// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.DisconnectedTests.Model;
using Xtensive.Storage.Operations;

#region Model

namespace Xtensive.Storage.DisconnectedTests.Model
{
  [HierarchyRoot]
  public class Simple : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field]
    public int VersionId { get; set;}

    [Field]
    public string Value { get; set;}
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }
    
    [Field, Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; private set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
    
    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }
  }

  public class Customer : Person
  {
    [Field, Association(PairTo = "Customer")]
    public EntitySet<Order> Orders { get; private set; }
  }

  public class AdvancedCustomer : Customer
  {
    [Field]
    public float Discount { get; set; }
  }

  public class Supplier : Person
  {
    [Field, Association(PairTo = "Supplier")]
    public EntitySet<Product> Products { get; private set;}
  }

  public class Employee : Person
  {
    [Field, Association(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, Association(PairTo = "Order")]
    public EntitySet<OrderDetail> Details { get; private set; }

    [Serializable]
    class CreateOrderItemOperation : IOperation
    {
      private SerializableKey productKey;
      private SerializableKey orderKey;
      private int count;

      public void Prepare(OperationExecutionContext context)
      {
        context.Register(productKey.Key);
        context.Register(orderKey.Key);
      }

      public void Execute(OperationExecutionContext context)
      {
        var p = Query<Product>.Single(productKey.Key);
        var o = Query<Order>.Single(orderKey.Key);
        var orderItem = new OrderDetail { Product = p, Order = o, Count = count };
      }

      public CreateOrderItemOperation(Product product, Order order, int count)
      {
        productKey = product.Key;
        orderKey = order.Key;
        this.count = count;
      }
    }

    public void CreateOrderItem(Product product, int count)
    {
      using (var context = OpenOperationContext(true)) {
        if (context.IsEnabled()) {
          var productKey = (SerializableKey)product.Key;
          var orderKey = (SerializableKey)Key;
          var operation = new CreateOrderItemOperation(product, this, count);
          context.Add(operation);
        }
        context.Complete();
      }
    }
  }

  [HierarchyRoot]
  public class OrderDetail : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    public Product Product { get; set; }

    [Field]
    public int Count { get; set; }
  }

  [HierarchyRoot]
  public class Product : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Supplier Supplier { get; set; }
  }

  [HierarchyRoot]
  public class Container : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Item> ManyToZero { get; private set; }

    [Field, Association(PairTo = "OneToMany")]
    public EntitySet<Item> ManyToOne { get; private set; }

    [Field, Association(PairTo = "ManyToMany")]
    public EntitySet<Item> ManyToMany { get; private set; }
  }

  [HierarchyRoot]
  public class Item : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Container ZeroToOne { get; set; }

    [Field]
    public Container OneToMany { get; set; }

    [Field]
    public Container ManyToMany { get; set; }
  }

  [HierarchyRoot]
  public class A : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }
  }

  [HierarchyRoot]
  public class B : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public A Root { get; set; }
  }
}

#endregion

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class DisconnectedStateTest : AutoBuildTest
  {
    [SetUp]
    public void SetUp()
    {
      Domain.DisposeSafely();
      Domain = BuildDomain(BuildConfiguration());
      FillDataBase();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Simple).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }


    [Test]
    public void TransactionsTest()
    {

      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = new Simple {
            VersionId = 1,
            Value = "some value"
          };          
        }
      }

      using (var session = Session.Open(Domain)) {

        var sessionHandler = session.Handler;

        var disconnectedState = new DisconnectedState();
        using (disconnectedState.Attach(session)) {

          using (var transactionScope = Transaction.Open()) {

            Assert.IsFalse(sessionHandler.TransactionIsStarted);

            using (disconnectedState.Connect()) {

              var objects = Query<Simple>.All.ToList();

              Assert.IsTrue(sessionHandler.TransactionIsStarted);
            }

            Assert.IsFalse(sessionHandler.TransactionIsStarted);
          }
        }
      }
    }


    [Test]
    public void FetchFromCacheTest()
    {
      Key key;

      // Create instance
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = new Simple {
            VersionId = 1,
            Value = "some value"
          };
          key = simple.Key;
          transactionScope.Complete();
        }
      }

      var state = new DisconnectedState();

      // Fetch instance to cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              Query<Simple>.All
                .Prefetch(item => item.Id)
                .Prefetch(item => item.VersionId)
                .Prefetch(item => item.Value)
                .ToList();
            }
          var simple = Query<Simple>.Single(key);
            Assert.IsNotNull(simple);  
            transactionScope.Complete();
          }
        }
      }
      // Change instance in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = Query<Simple>.Single(key);
          simple.Value = "new value";
          transactionScope.Complete();
        }
      }
      // Look for instance in cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var simple = Query<Simple>.Single(key);
            Assert.IsNotNull(simple);
            Assert.AreEqual("some value", simple.Value);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void QueryFromCacheTest()
    {
      Key key;
      // Create instance
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = new Simple {VersionId = 1, Value = "some value"};
          key = simple.Key;
          transactionScope.Complete();
        }
      }
      // Fetch all instances to cache
      var state = new DisconnectedState();
      state.MergeMode = MergeMode.PreferTarget;

      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Simple> list = null;
            using (state.Connect()) {
              list = Query<Simple>.All.ToList();
            }
            Assert.AreEqual(1, list.Count);
            transactionScope.Complete();
          }
        }
      }
      // Change instance in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = Query<Simple>.Single(key);
          simple.Value = "new value";
          transactionScope.Complete();
        }
      }
      // Look instance from cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(Session.Current)) {
          using (var transactionScope = Transaction.Open()) {
            List<Simple> list = null;
            using (state.Connect()) {
              list = Query<Simple>.All.ToList();
            }
            var simple = list.First();
            Assert.IsNotNull(simple);
            Assert.AreEqual("some value", simple.Value);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void QueryWithRemovedTest()
    {
      // Create instances
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple1 = new Simple {VersionId = 1, Value = "Simple1"};
          var simple2 = new Simple {VersionId = 1, Value = "Simple2"};
          transactionScope.Complete();
        }
      }
      // Fetch all instances to cache and remove one instance
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Simple> list = null;
            using (state.Connect()) {
              list = Query<Simple>.All.OrderBy(simple => simple.Id).ToList();
            }
            Assert.AreEqual(2, list.Count);
            list[0].Remove();
            transactionScope.Complete();
          }
        }
      }
      // Query instances from cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Simple> list = null;
            using (state.Connect()) {
              list = Query<Simple>.All.OrderBy(simple => simple.Id).ToList();
            }
            Assert.AreEqual(2, list.Count);
            Assert.IsNull(list[0]);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void PersistToCacheTest()
    {
      Key updatedKey;
      Key removedKey;
      Key insertedKey;

      // Create instances
      using (var session = Session.Open(Domain)) {
        using (var transacton = Transaction.Open()) {
          var simple1 = new Simple {VersionId = 1, Value = "Value1"};
          var simple2 = new Simple {VersionId = 1, Value = "Value2"};
          updatedKey = simple1.Key;
          removedKey = simple2.Key;
          transacton.Complete();
        }
      }
      // Fetch instances to cache
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using(state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              Query<Simple>.All.ToList();
            }
            var simple = Query<Simple>.Single(updatedKey);
            simple.Value = "New Value";
            simple = Query<Simple>.Single(removedKey);
            simple.Remove();
            simple = new Simple {
              VersionId = 1,
              Value = "Value3"
            };
            insertedKey = simple.Key;
            transactionScope.Complete();
          }
        }
      }
      // Check instances in cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var simple = Query<Simple>.Single(updatedKey);
            Assert.AreEqual(1, simple.VersionId);
            Assert.AreEqual("New Value", simple.Value);
            simple = Query<Simple>.Single(insertedKey);
            Assert.AreEqual(1, simple.VersionId);
            Assert.AreEqual("Value3", simple.Value);
            AssertEx.Throws<KeyNotFoundException>(() => Query<Simple>.Single(removedKey));
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ModifyDataTest()
    {
      var state = new DisconnectedState();
      Key order1Key;
      Key newCustomerKey;

      // Modify data
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query<Order>.All
                .Prefetch(o => o.Customer)
                .PrefetchMany(o => o.Details, set => set,
                  od => od.Prefetch(item => item.Product)).ToList();
            }
            var newCustomer = new Customer {
              Name = "NewCustomer"
            };
            newCustomerKey = newCustomer.Key;
            session.Persist();
            newCustomer.Remove();
            session.Persist();

            var order1 = orders.First(order => order.Number==1);
            order1Key = order1.Key;
            Assert.AreEqual(2, order1.Details.Count);
            Product product3 = null;
            using (state.Connect()) {
              product3 = Query<Product>.All.First(product => product.Name=="Product3");
            }
            new OrderDetail() {
              Product = product3,
              Count = 250,
              Order = order1
            };
            var order1Detail1 = order1.Details.ToList().First(detail => detail.Product.Name=="Product1");
            order1Detail1.Product.Name = "Product1.New";
            order1Detail1.Count = 150;
            var order1Detail2 = order1.Details.ToList().First(detail => detail.Product.Name=="Product2");
            order1.Details.Remove(order1Detail2);
            order1Detail2.Remove();
            transactionScope.Complete();
          }
        }
      }

      // Save data to storage
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          var keyMapping = state.SaveChanges();
          order1Key = keyMapping.Remap(order1Key);
          newCustomerKey = keyMapping.Remap(newCustomerKey);
        }
      }

      // Check saved data
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query<Order>.Single(order1Key);
          var details = order1.Details.ToList();
          Assert.AreEqual(2, order1.Details.Count);
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
          AssertEx.Throws<KeyNotFoundException>(() => Query<Customer>.Single(newCustomerKey));
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void ModifyDataAfterSaveChangesTest()
    {
      var state = new DisconnectedState();
      Key order1Key = null;

      // Modify data
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query<Order>.All
                .Prefetch(o => o.Customer)
                .PrefetchMany(o => o.Details, set => set,
                  od => od.Prefetch(item => item.Product)).ToList();
            }

            var order1 = orders.First(order => order.Number==1);
            order1Key = order1.Key;
            Product product3 = null;
            using (state.Connect()) {
              product3 = Query<Product>.All.First(product => product.Name=="Product3");
            }
            new OrderDetail() {
              Product = product3,
              Count = 250,
              Order = order1
            };
            transactionScope.Complete();
          }
          state.SaveChanges();
        }
      }
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query<Order>.Single(order1Key);
            order1.Number = 1000;
            var details = order1.Details.ToList();
            var detail3 = details.First(detail => detail.Count==250);
            detail3.Count = 1250;

            transactionScope.Complete();
          }
          state.SaveChanges();
        }
      }


      // Check saved data
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query<Order>.Single(order1Key);
          Assert.AreEqual(1000, order1.Number);
          var details = order1.Details.ToList();
          Assert.AreEqual(3, order1.Details.Count);
          var detail3 = details.FirstOrDefault(detailt => detailt.Count==1250);
          Assert.IsNotNull(detail3);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void MultipleTransactionsTest()
    {
      var state = new DisconnectedState();

      Key supplier1Key;
      Key newSupplierKey;
      Key product3Key;
      Key newProduct1Key;
      Key newProduct2Key;

      List<Supplier> suppliers = null;
      List<Product> products = null;

      // Modify data step 1 (differences must be cached)
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              suppliers = Query<Supplier>.All.Prefetch(s => s.Products).ToList();
              products = Query<Product>.All.Prefetch(p => p.Supplier).ToList();
            }
            var supplier1 = suppliers.First(employee => employee.Name=="Supplier1");
            supplier1Key = supplier1.Key;
            var newProduct1 = new Product {
              Supplier = supplier1,
              Name = "NewProduct1"
            };
            newProduct1Key = newProduct1.Key;
            var newProduct2 = new Product {
              Supplier = supplier1,
              Name = "NewProduct2"
            };
            newProduct2Key = newProduct2.Key;
            var product3 = products.First(product => product.Name=="Product3");
            product3Key = product3.Key;
            product3.Supplier = supplier1;
            var newSupplier = new Supplier {
              Name = "NewSupplier"
            };
            newSupplierKey = newSupplier.Key;

            transactionScope.Complete();
          }
        }
      }

      // Modify data step 2 (differences must be cached)
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {

            // Check previous changes
            var newProduct1 = Query<Product>.SingleOrDefault(newProduct1Key);
            var newProduct2 = Query<Product>.SingleOrDefault(newProduct2Key);
            var product3 = Query<Product>.SingleOrDefault(product3Key);
            var newSupplier = Query<Supplier>.SingleOrDefault(newSupplierKey);

            Assert.IsNotNull(newProduct1);
            Assert.IsNotNull(newProduct1.Supplier);
            Assert.AreEqual("Supplier1", newProduct1.Supplier.Name);
            Assert.IsNotNull(newProduct2);
            Assert.IsNotNull(newProduct2.Supplier);
            Assert.AreEqual("Supplier1", newProduct2.Supplier.Name);
            Assert.IsNotNull(product3);
            Assert.IsNotNull(product3.Supplier);
            Assert.AreEqual("Supplier1", product3.Supplier.Name);
            Assert.IsNotNull(newSupplier);

            // Modify data
            newProduct2.Supplier = null;
            newProduct2.Remove();
            newProduct1.Supplier = newSupplier;

            transactionScope.Complete();
          }
        }
      }

      // Save changes to storage
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          var keyMapping = state.SaveChanges();
          newProduct1Key = keyMapping.Remap(newProduct1Key);
          newProduct2Key = keyMapping.Remap(newProduct2Key);
          product3Key = keyMapping.Remap(product3Key);
          newSupplierKey = keyMapping.Remap(newSupplierKey);
          supplier1Key = keyMapping.Remap(supplier1Key);
        }
      }

      // Check changes
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var newProduct1 = Query<Product>.SingleOrDefault(newProduct1Key);
          var newProduct2 = Query<Product>.SingleOrDefault(newProduct2Key);
          var product3 = Query<Product>.SingleOrDefault(product3Key);
          var newSupplier = Query<Supplier>.SingleOrDefault(newSupplierKey);
          Assert.IsNotNull(newSupplier);
          Assert.IsNotNull(newProduct1);
          Assert.IsNotNull(newProduct1.Supplier);
          Assert.AreEqual("NewSupplier", newProduct1.Supplier.Name);
          Assert.IsNull(newProduct2);
          Assert.IsNotNull(product3);
          Assert.IsNotNull(product3.Supplier);
          Assert.AreEqual("Supplier1", product3.Supplier.Name);
          
          transactionScope.Complete();
        }
      }
    }
    
    [Test]
    public void RollbackTransactionTest()
    {
      var state = new DisconnectedState();

      Key updatedSupplierKey;
      Key newSupplierKey;

      // Modify data and commit transaction
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            Supplier supplier1 = null;
            using (state.Connect()) {
              supplier1 = Query<Supplier>.All.First(employee => employee.Name=="Supplier1");
            }
            updatedSupplierKey = supplier1.Key;
            supplier1.Name = "UpdatedSupplier1";
            session.Persist();

            transactionScope.Complete();
          }
        }
      }

      // Modify data and rollback transaction
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {

            var supplier1 = Query<Supplier>.Single(updatedSupplierKey);
            supplier1.Name = "UpdatedSupplier2";
            var newSupplier = new Supplier {
              Name = "NewSupplier"
            };
            newSupplierKey = newSupplier.Key;
            session.Persist();

            // Rollback
          }
        }
      }

      // Check data in cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {

            var supplier1 = Query<Supplier>.Single(updatedSupplierKey);
            Assert.AreEqual("UpdatedSupplier1", supplier1.Name);
            AssertEx.Throws<ConnectionRequiredException>(() => Query<Supplier>.Single(newSupplierKey));

            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void NestedTransactionsTest()
    {
      var disconnectedState = new DisconnectedState();
      Key customerKey;
      Key orderKey;

      // nested transactions in connected mode
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (disconnectedState.Connect())
      using (var outer = Transaction.Open()) {
        var customer = new Customer();
        customerKey = customer.Key;
        var order = new Order();
        customer.Orders.Add(order);
        orderKey = order.Key;
        Assert.AreEqual(customer.Orders.Count, 1);
        using (var inner = Transaction.Open(TransactionOpenMode.New)) {
          Assert.AreEqual(customer.Orders.Count, 1);
          customer.Orders.Add(new Order());
          Assert.AreEqual(customer.Orders.Count, 2);
          // rollback
        }
        Assert.AreEqual(customer.Orders.Count, 1);
        outer.Complete();
      }

      // verifying changes
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (Transaction.Open()) {
        var customer = Query<Customer>.Single(customerKey);
        var order = customer.Orders.AsEnumerable().Single();
        Assert.AreEqual(orderKey, order.Key);
      }
      
      // nested transactions in disconnected mode
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (var outer = Transaction.Open()) {
        var customer = Query<Customer>.Single(customerKey);
        using (var inner = Transaction.Open(TransactionOpenMode.New)) {
          customer.Name = "Vasya";
          inner.Complete();
        }
        Assert.AreEqual("Vasya", customer.Name);
        using (var inner = Transaction.Open(TransactionOpenMode.New)) {
          customer.Name = "Joe";
          // rollback
        }
        Assert.AreEqual("Vasya", customer.Name);
        outer.Complete();
      }

      // verifying changes
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (Transaction.Open()) {
        var customer = Query<Customer>.Single(customerKey);
        Assert.AreEqual(customer.Name, "Vasya");
      }
    }

    [Test]
    public void GetEntitySetFromCache()
    {
      var orderType = Domain.Model.Types[typeof (Order)];
      Key orderKey;
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          orderKey = Query<Order>.All.OrderBy(c => c.Id).First().Key;
          transactionScope.Complete();
        }
      }

      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              Query<Order>.All.Prefetch(o => o.Details).ToList();
            }
            transactionScope.Complete();
          }
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query<Order>.Single(orderKey);
            foreach (var detail in order1.Details)
              Assert.IsNotNull(detail);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ModifyEntitySetTest()
    {
      Key order1Key;
      Key order2Key;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query<Order>.All.Prefetch(o => o.Details).ToList();
              Query<Product>.All.ToList(); // Need for OrderDetails removing
            }
            var order1 = orders.First(order => order.Number==1);
            order1Key = order1.Key;
            var order2 = orders.First(order => order.Number==2);
            order2Key = order2.Key;
            var detail1 = order1.Details.ToList().First(detail => detail.Count==100);
            var detail2 = order1.Details.ToList().First(detail => detail.Count==200);
            var detail3 = order2.Details.ToList().First(detail => detail.Count==300);
            var detail4 = order2.Details.ToList().First(detail => detail.Count==400);

            detail1.Order = null;
            detail2.Order = order2;
            new OrderDetail {
              Order = order1,
              Count = 500
            };
            detail3.Count = 350;
            detail4.Order = null;
            detail4.Remove();
            transactionScope.Complete();
          }
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query<Order>.Single(order1Key);
            var order2 = Query<Order>.Single(order2Key);
            Assert.AreEqual(1, order1.Details.Count);
            Assert.AreEqual(2, order2.Details.Count);

            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ModifyManyToManyEntitySetTest()
    {
      var orderType = Domain.Model.Types[typeof (Book)];
      var authorType = Domain.Model.Types[typeof (Author)];
      
      List<Author> authors = null;
      List<Book> books = null;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              authors = Query<Author>.All.Prefetch(a => a.Books).ToList();
              books = Query<Book>.All.Prefetch(b => b.Authors).ToList();
            }
            var author1 = authors.First(author => author.Name=="Author1");
            var author2 = authors.First(author => author.Name=="Author2");
            var book1 = books.First(book => book.Title=="Book1");
            var book2 = books.First(book => book.Title=="Book2");
            var book3 = books.First(book => book.Title=="Book3");

            book1.Authors.Remove(author1);
            book2.Authors.Remove(author1);
            book3.Authors.Add(author1);

            transactionScope.Complete();
          }
          using (var transactionScope = Transaction.Open()) {
            var book1 = books.First(book => book.Title=="Book1");
            var book2 = books.First(book => book.Title=="Book2");
            var book3 = books.First(book => book.Title=="Book3");
            var author1 = authors.First(author => author.Name=="Author1");
            Assert.AreEqual(1, book1.Authors.Count);
            Assert.AreEqual(0, book2.Authors.Count);
            Assert.AreEqual(2, book3.Authors.Count);
            Assert.AreEqual(1, author1.Books.Count);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void OperationLogSerializationTest()
    {
      Key order1Key;
      Key newCustomerKey;

      var log = new OperationSet();
      // Modify data
      using (var session = Session.Open(Domain)) {
        using (new Logger(session, log))
        using (var transactionScope = Transaction.Open()) {
          var orders = Query<Order>.All
            .Prefetch(o => o.Customer)
            .PrefetchMany(o => o.Details, set => set,
              od => od.Prefetch(item => item.Product)).ToList();
          
          var newCustomer = new Customer {Name = "NewCustomer"};
          newCustomerKey = newCustomer.Key;
          session.Persist();
          newCustomer.Remove();
          session.Persist();

          var order1 = orders.First(order => order.Number==1);
          order1Key = order1.Key;
          Assert.AreEqual(2, order1.Details.Count);
          Product product3 = null;
          product3 = Query<Product>.All.First(product => product.Name=="Product3");
          new OrderDetail() {
            Product = product3,
            Count = 250,
            Order = order1
          };
          var product2 = Query<Product>.All.First(product => product.Name == "Product2");
          var order1Detail1 = order1.Details.ToList().First(detail => detail.Product.Name=="Product1");
          order1Detail1.Product.Name = "Product1.New";
          order1Detail1.Count = 150;
          var order1Detail2 = order1.Details.ToList().First(detail => detail.Product.Name=="Product2");
          order1.Details.Remove(order1Detail2);
          order1Detail2.Remove();
          order1.CreateOrderItem(product2, 499);
        }
      }

      // Serialize/Deserialize operationLog
      IOperationSet deserializedSet = null;
      var binaryFormatter = new BinaryFormatter();
      using (var stream = new MemoryStream()) {
        binaryFormatter.Serialize(stream, log);
        stream.Position = 0;
        using (Session.Open(Domain))
          deserializedSet = (IOperationSet)binaryFormatter.Deserialize(stream);
      }
 
      // Save data to storage
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open(session)) {
          deserializedSet.Apply(session);
          transactionScope.Complete();
        }
      }

      // Check saved data
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query<Order>.Single(order1Key);
          var details = order1.Details.ToList();
          Assert.AreEqual(3, order1.Details.Count);
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product2" && detail.Count == 499));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
          AssertEx.Throws<KeyNotFoundException>(() => Query<Customer>.Single(newCustomerKey));
          transactionScope.Complete();
        }
      }
    }
    
    [Test]
    public void SerializationTest()
    {
      var state = new DisconnectedState();
      Key order1Key;
      Key newCustomerKey;

      // Modify data
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query<Order>.All
                .Prefetch(o => o.Customer)
                .PrefetchMany(o => o.Details, set => set,
                  od => od.Prefetch(item => item.Product)).ToList();
            }

            var newCustomer = new Customer {
              Name = "NewCustomer"
            };
            newCustomerKey = newCustomer.Key;
            session.Persist();
            newCustomer.Remove();
            session.Persist();

            var order1 = orders.First(order => order.Number==1);
            order1Key = order1.Key;
            Assert.AreEqual(2, order1.Details.Count);
            Product product3 = null;
            using (state.Connect()) {
              product3 = Query<Product>.All.First(product => product.Name=="Product3");
            }
            new OrderDetail() {
              Product = product3,
              Count = 250,
              Order = order1
            };
            var order1Detail1 = order1.Details.ToList().First(detail => detail.Product.Name=="Product1");
            order1Detail1.Product.Name = "Product1.New";
            order1Detail1.Count = 150;
            var order1Detail2 = order1.Details.ToList().First(detail => detail.Product.Name=="Product2");
            order1.Details.Remove(order1Detail2);
            order1Detail2.Remove();
            transactionScope.Complete();
          }
        }
      }
      
      // Serialize, deserialize
      using (var session = Session.Open(Domain)) {
        using (var stream = new MemoryStream()) {
          state = LegacyBinarySerializer.Instance.Clone(state) as DisconnectedState;
          Assert.IsNotNull(state);
        }
      }
      
      // Check data in cache and save to DB
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query<Order>.Single(order1Key);
            var details = order1.Details.ToList();
            Assert.AreEqual(2, order1.Details.Count);
            Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
            Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
            transactionScope.Complete();
          }
          var keyMapping = state.SaveChanges();
          order1Key = keyMapping.Remap(order1Key);
          newCustomerKey = keyMapping.Remap(newCustomerKey);
        }
      }

      // Check data in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order1 = Query<Order>.Single(order1Key);
          var details = order1.Details.ToList();
          Assert.AreEqual(2, order1.Details.Count);
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void CheckVersionTest()
    {
      var state = new DisconnectedState();
      Key customer1Key = null;

      // Fetch instance to cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              customer1Key = Query<Customer>.All.First(customer => customer.Name=="Customer1").Key;
            }
            transactionScope.Complete();
          }
        }
      }

      // Modify instance in storage
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer1 = Query<Customer>.Single(customer1Key);
          customer1.Name = "NewName1";
          transactionScope.Complete();
        }
      }

      // Modify instance in cache and check version
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var customer1 = Query<Customer>.Single(customer1Key);
            customer1.Name = "NewName2";
            transactionScope.Complete();
          }
          AssertEx.Throws<InvalidOperationException>(() => state.SaveChanges());
        }
      }
    }

    [Test]
    public void CreateEntitySetForNewObjectsTest()
    {
      var disconnectedState = new DisconnectedState();
      Key supplierKey = null;
      var state = new DisconnectedState();

      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var supplier = new Supplier();
            var e = (IEnumerable) supplier.Products;
            var first = e.GetEnumerator().MoveNext();
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ManyToOneReferenceMappingTest()
    {
      var state = new DisconnectedState();
      Key containerKey = null;
      Key itemKey = null;
      
      // With existing instance
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var container = new Container();
          var item = new Item();
          containerKey = container.Key;
          itemKey = item.Key;
          container.ManyToOne.Add(item);
          transactionScope.Complete();
        }
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            Container container = null;
            Item item = null;
            using (state.Connect()) {
              container = Query<Container>.Single(containerKey);
              item = Query<Item>.Single(itemKey);
            }
            AssertEx.Throws<ReferentialIntegrityException>(() => container.Remove());

            transactionScope.Complete();
          }
        }
      }
      
      // With new instance
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var newContainer = new Container();
            newContainer.Remove();
            newContainer = new Container();
            containerKey = newContainer.Key;
            var item = new Item();
            itemKey = item.Key;
            newContainer.ManyToOne.Add(item);

            transactionScope.Complete();
          }

          using (var transactionScope = Transaction.Open()) {
            var newContainer = Query<Container>.Single(containerKey);
            AssertEx.Throws<ReferentialIntegrityException>(() => newContainer.Remove());

            transactionScope.Complete();
          }

        }
      }
    }

    [Test]
    public void ZeroToOneReferenceMappingTest()
    {
      var state = new DisconnectedState();
      Key containerKey = null;
      Key itemKey = null;
      
      // With existing instance
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var container = new Container();
          var item = new Item();
          containerKey = container.Key;
          itemKey = item.Key;
          item.ZeroToOne = container;
          transactionScope.Complete();
        }
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            Container container = null;
            Item item = null;
            using (state.Connect()) {
              container = Query<Container>.Single(containerKey);
              item = Query<Item>.Single(itemKey);
            }
            AssertEx.Throws<ReferentialIntegrityException>(() => container.Remove());

            transactionScope.Complete();
          }

        }
      }
      
      // With new instance
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var newContainer = new Container();
            newContainer.Remove();
            newContainer = new Container();
            containerKey = newContainer.Key;
            var item = new Item();
            itemKey = item.Key;
            item.ZeroToOne = newContainer;

            transactionScope.Complete();
          }

          using (var transactionScope = Transaction.Open()) {
            var newContainer = Query<Container>.Single(containerKey);
            AssertEx.Throws<ReferentialIntegrityException>(() => newContainer.Remove());

            transactionScope.Complete();
          }

        }
      }
    }

    [Test]
    public void ZeroToManyReferenceMappingTest()
    {
      var state = new DisconnectedState();
      Key containerKey = null;
      Key itemKey = null;
      
      // With existing instance
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var container = new Container();
          var item = new Item();
          containerKey = container.Key;
          itemKey = item.Key;
          container.ManyToZero.Add(item);
          transactionScope.Complete();
        }
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            Container container = null;
            Item item = null;
            using (state.Connect()) {
              Query<Container>.All.Prefetch(c => c.ManyToZero).ToList();
              container = Query<Container>.Single(containerKey);
              item = Query<Item>.Single(itemKey);
            }
            AssertEx.Throws<ReferentialIntegrityException>(() => item.Remove());

            transactionScope.Complete();
          }

        }
      }
      
      // With new instance
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var newContainer = new Container();
            newContainer.Remove();
            newContainer = new Container();
            containerKey = newContainer.Key;
            var item = new Item();
            itemKey = item.Key;
            newContainer.ManyToZero.Add(item);

            transactionScope.Complete();
          }

          using (var transactionScope = Transaction.Open()) {
            var item = Query<Item>.Single(itemKey);
            AssertEx.Throws<ReferentialIntegrityException>(() => item.Remove());

            transactionScope.Complete();
          }

        }
      }
    }

    [Test]
    public void ManyToManyReferenceMappingTest()
    {
      var state = new DisconnectedState();

      // With existing instance
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            Author author1 = null;
            using (state.Connect()) {
              author1 = Query<Author>.All
                .Where(author => author.Name=="Author1")
                .Prefetch(author => author.Books)
                .First();
            }
            Assert.IsTrue(author1.Books.Count > 0);
            AssertEx.Throws<ReferentialIntegrityException>(() => author1.Remove());

            transactionScope.Complete();
          }

        }
      }
      
      // With new instance
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          Key customerKey = null;
          using (var transactionScope = Transaction.Open()) {
            var newCustomer = new Customer();
            newCustomer.Remove();
            newCustomer = new Customer();
            var newOrder = new Order {
              Customer = newCustomer
            };
            customerKey = newCustomer.Key;

            AssertEx.Throws<ReferentialIntegrityException>(() => newCustomer.Remove());

            transactionScope.Complete();
          }

          using (var transactionScope = Transaction.Open()) {
            var newCustomer = Query<Customer>.Single(customerKey);
            AssertEx.Throws<ReferentialIntegrityException>(newCustomer.Remove);

            transactionScope.Complete();
          }

        }
      }
    }

    [Test]
    public void FetchInstanceWithExisitingKeyTest()
    {
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              Query<Order>.All.ToList();
              Query<Order>.All.Prefetch(o => o.Customer).ToList();
            }
            transactionScope.Complete();
          }
        }
      }
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Customer> customers;
            using (state.Connect()) {
              customers = Query<Customer>.All.Prefetch(c => c.Orders).ToList();
            }
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ApplyRemoveOperationTest()
    {
      // Create data
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {

          var a = new A();
          a.Number = 1;
          var b1 = new B();
          b1.Root = a;
          var b2 = new B();
          b2.Root = a;
          
          transactionScope.Complete();
        }
      }

      var state = new DisconnectedState();

      // Remove A instance in cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            A a = null;
            List<B> list = null;
            using (state.Connect()) {
              a = Query<A>.All.First();
              list = Query<B>.All.ToList();
            }
            a.Remove();
            Assert.IsTrue(list.All(item => item.Root==null));
            transactionScope.Complete();
          }
        }
      }

      // Add references to A in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var a = Query<A>.All.First();
          var b3 = new B();
          b3.Root = a;
          transactionScope.Complete();
        }
      }

      // Save changes to DB
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          state.SaveChanges();
        }
      }

      // Check data
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Assert.IsNull(Query<A>.All.FirstOrDefault());
          Assert.IsTrue(Query<B>.All.All(item => item.Root==null));
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void MergeModeTest()
    {
      Key key;

      // Create instances
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = new Simple {
            VersionId = 1,
            Value = "Value1"
          };
          key = simple.Key;
          transactionScope.Complete();
        }
      }
      

      // Target state
      var state2 = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state2.Attach(session)) {
          // Load data
          using (var transactionScope = Transaction.Open()) {
            using (state2.Connect()) {
              Query<Simple>.All.ToList();
            }
          }
        }
      }

      // Source state
      var state1 = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state1.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state1.Connect()) {
              Query<Simple>.All.ToList();
            }
            var simple = Query<Simple>.Single(key);
            simple.Value = "Value2";
            Assert.IsNotNull(simple);
            transactionScope.Complete();
          }
          state1.SaveChanges();
        }
      }
      
      // Merge and check
      using (var session = Session.Open(Domain)) {
        using (state2.Attach(session)) {
          AssertEx.Throws<InvalidOperationException>(() => state2.Merge(state1, MergeMode.Restrict));
          state2.Merge(state1, MergeMode.PreferTarget);
          Assert.AreEqual("Value1", Query<Simple>.Single(key).Value);
          state2.Merge(state1, MergeMode.PreferSource);
          Assert.AreEqual("Value2", Query<Simple>.Single(key).Value);
        }
      }
    }
    
    private void FillDataBase()
    {
      using (var sesscionScope = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {

          var customer1 = new Customer {Name = "Customer1"};
          var customer2 = new Customer {Name = "Customer2"};
          var supplier1 = new Supplier {Name = "Supplier1"};
          var supplier2 = new Supplier {Name = "Supplier2"};
          var employee1 = new Employee {Name = "Employee1"};
          var employee2 = new Employee {Name = "Employee2"};
          var product1 = new Product {Name = "Product1", Supplier = supplier1};
          var product2 = new Product {Name = "Product2", Supplier = supplier1};
          var product3 = new Product {Name = "Product3", Supplier = supplier2};
          var product4 = new Product {Name = "Product4", Supplier = supplier2};
          var order1 = new Order {Number = 1, Customer = customer1, Employee = employee1};
          var order1Detail1 = new OrderDetail {Order = order1, Product = product1, Count = 100};
          var order1Detail2 = new OrderDetail {Order = order1, Product = product2, Count = 200};
          var order2 = new Order {Number = 2, Customer = customer2, Employee = employee2};
          var order2Detail1 = new OrderDetail {Order = order2, Product = product3, Count = 300};
          var order2Detail2 = new OrderDetail {Order = order2, Product = product4, Count = 400};
          var author1 = new Author {Name = "Author1"};
          var author2 = new Author {Name = "Author2"};
          var book1 = new Book {Title = "Book1"};
          var book2 = new Book {Title = "Book2"};
          var book3 = new Book {Title = "Book3"};
          book1.Authors.Add(author1);
          book1.Authors.Add(author2);
          book2.Authors.Add(author1);
          book3.Authors.Add(author2);

          transactionScope.Complete();
        }
      }
    }
  }
}