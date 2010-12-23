// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.18

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.DisconnectedTests.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Providers;

#region Model

namespace Xtensive.Storage.DisconnectedTests.Model
{
  [Serializable]
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

  [Serializable]
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

  [Serializable]
  [HierarchyRoot]
  [Index("Name", Unique = true)]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
    
    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }

//    [Field, Version]
//    public int Version { get; private set; }
  }

  [Serializable]
  public class Customer : Person
  {
    [Field, Association(PairTo = "Customer")]
    public EntitySet<Order> Orders { get; private set; }
  }

  [Serializable]
  public class AdvancedCustomer : Customer
  {
    [Field]
    public float Discount { get; set; }
  }

  [Serializable]
  public class Supplier : Person
  {
    [Field, Association(PairTo = "Supplier")]
    public EntitySet<Product> Products { get; private set;}
  }

  [Serializable]
  public class Employee : Person
  {
    [Field, Association(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }
  }

  [Serializable]
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
    class CreateOrderItemOperation : Operation
    {
      private Ref<Product> product;
      private Ref<Order> order;
      private readonly int count;

      public override string Title {
        get { return "Create order item"; }
      }

      public override string Description {
        get {
          return "{0}, Product = {1}, Order = {2}".FormatWith(Title, product, order);
        }
      }

      protected override void PrepareSelf(OperationExecutionContext context)
      {
        context.RegisterKey(product, false);
        context.RegisterKey(order, false);
      }

      protected override void ExecuteSelf(OperationExecutionContext context)
      {
        var p = Query.Single<Product>(context.TryRemapKey(product));
        var o = Query.Single<Order>(context.TryRemapKey(order));
        var orderItem = new OrderDetail { Product = p, Order = o, Count = count };
      }

      protected override Operation CloneSelf(Operation clone)
      {
        if (clone==null)
          clone = new CreateOrderItemOperation(product, order, count);
        return clone;
      }

      public CreateOrderItemOperation(Product product, Order order, int count)
      {
        this.product = product;
        this.order = order;
        this.count = count;
      }
    }

    public void LogCreateOrderItemOperation(Product product, int count)
    {
      var operations = Session.Operations;
      using (var scope = operations.BeginRegistration(OperationType.Custom)) {
        if (operations.CanRegisterOperation)
          operations.RegisterOperation(new CreateOrderItemOperation(product, this, count), true);
        scope.Complete();
      }
    }
  }

  [Serializable]
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

  [Serializable]
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

  [Serializable]
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

  [Serializable]
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

  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }
  }

  [Serializable]
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

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyExample : Entity
  {
    [Key(0), Field]
    public KeyElementFirst First { get; private set; }

    [Key(1), Field]
    public int Second { get; private set; }

    [Key(2), Field]
    public KeyElementSecond Third { get; private set; }

    [Field]
    public string Aux { get; set; }


    // Constructors

    public CompositeKeyExample(KeyElementFirst first, int second, KeyElementSecond third)
      : base(first, second, third)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class KeyElementFirst : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Aux { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class KeyElementSecond : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Aux { get; private set; }


    // Constructors

    public KeyElementSecond(Guid id)
      : base(id)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class EntityIdentifiedByEntity : Entity
  {
    [Key, Field]
    public Simple Id { get; private set; }

    public int Aux { get; set; }


    // Constructors

    public EntityIdentifiedByEntity(Simple id)
      : base(id)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class EntityIdentifiedByPrimitiveValue : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Aux { get; private set; }


    // Constructors

    public EntityIdentifiedByPrimitiveValue(int id)
      : base(id)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositePrimitiveKeyExample : Entity
  {
    [Key(0), Field]
    public int IdFirst { get; private set; }

    [Key(1), Field]
    public Guid IdSecond { get; private set; }

    [Field]
    public string Aux { get; private set; }


    // Constructors

    public CompositePrimitiveKeyExample(int idFirst, Guid idSecond)
      : base(idFirst, idSecond)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class ChangeSet : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public ChangeSet Parent { get; set; }

    [Field]
    public ChangeSet SecondParent { get; set; }
  }
}

#endregion

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class DisconnectedStateTest : AutoBuildTest
  {
    public override void TestFixtureSetUp()
    {
    }

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

              var objects = Query.All<Simple>().ToList();

              Assert.IsTrue(sessionHandler.TransactionIsStarted);
            }

            Assert.IsFalse(sessionHandler.TransactionIsStarted);
          }
        }
      }
    }

    [Test]
    public void ResolvingRollbackedEntityKeyTest()
    {
      var disconnectedState = new DisconnectedState();

      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (disconnectedState.Connect())
      using (Transaction.Open()) {

        Key key;

        using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {

          var order = new Order();
          session.Persist();
          key = order.Key;
        }

        AssertEx.Throws<KeyNotFoundException>(() =>
          Query.Single(key));
      }
    }

    [Test]
    public void NestedCommitTest()
    {
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          new Customer {
            Name = "1",
            Orders = {
              new Order {Number = 1}
            }
          };
          transactionScope.Complete();
        }
      }

      using (var session = Session.Open(Domain)) {
        var disconnectedState = new DisconnectedState();
        using (disconnectedState.Attach(session))
        using (disconnectedState.Connect()) {

          using (Transaction.Open()) {
            var customer = Query.All<Customer>().First();
            var order = customer.Orders.First();
            using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
              order.Number = 2;
              nestedScope.Complete();
            }
            Assert.AreEqual(order.Number, 2);
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
              Query.All<Simple>()
                .Prefetch(item => item.Id)
                .Prefetch(item => item.VersionId)
                .Prefetch(item => item.Value)
                .ToList();
            }
          var simple = Query.Single<Simple>(key);
            Assert.IsNotNull(simple);  
            transactionScope.Complete();
          }
        }
      }
      // Change instance in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = Query.Single<Simple>(key);
          simple.Value = "new value";
          transactionScope.Complete();
        }
      }
      // Look for instance in cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var simple = Query.Single<Simple>(key);
            Assert.IsNotNull(simple);
            Assert.AreEqual("some value", simple.Value);
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void InvalidChangesTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          new Author {
            Name = "Peter"
          };
          transactionScope.Complete();
        }
      }

      using (var session = Session.Open(Domain)) {
        var disconnectedState = new DisconnectedState();
        using (disconnectedState.Attach(session))
        using (disconnectedState.Connect()) {

          Author author;

          using (var transactionScope = Transaction.Open()) {

            var firstAuthor = Query.All<Author>().First();

            // violating unique index
            author = new Author { Name = firstAuthor.Name };

            transactionScope.Complete();
          }

          // try to save incorrect value twice, two similar errors are expected

          Exception firstError = null;
          Exception secondError = null;

          try {
            disconnectedState.ApplyChanges();
          }
          catch (Exception exception) {
            firstError = exception;
          }
          Assert.IsNotNull(firstError);

          try {
            disconnectedState.ApplyChanges();
          }
          catch (Exception exception) {
            secondError = exception;
          }

          Assert.IsNotNull(secondError);
          // Messages are similar, but keys there (if they are included into text) are different
          Assert.AreEqual(
            firstError.Message.Substring(0, 20), 
            secondError.Message.Substring(0, 20)); 

          // Correct value
          using (var transactionScope = Transaction.Open()) {
            author.Name += " the Second";
            transactionScope.Complete();
          }

          disconnectedState.ApplyChanges();
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
      state.MergeMode = MergeMode.PreferOriginal;

      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Simple> list = null;
            using (state.Connect()) {
              list = Query.All<Simple>().ToList();
            }
            Assert.AreEqual(1, list.Count);
            transactionScope.Complete();
          }
        }
      }
      // Change instance in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = Query.Single<Simple>(key);
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
              list = Query.All<Simple>().ToList();
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
              list = Query.All<Simple>().OrderBy(simple => simple.Id).ToList();
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
              list = Query.All<Simple>().OrderBy(simple => simple.Id).ToList();
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
              Query.All<Simple>().ToList();
            }
            var simple = Query.Single<Simple>(updatedKey);
            simple.Value = "New Value";
            simple = Query.Single<Simple>(removedKey);
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
            var simple = Query.Single<Simple>(updatedKey);
            Assert.AreEqual(1, simple.VersionId);
            Assert.AreEqual("New Value", simple.Value);
            simple = Query.Single<Simple>(insertedKey);
            Assert.AreEqual(1, simple.VersionId);
            Assert.AreEqual("Value3", simple.Value);
            AssertEx.Throws<KeyNotFoundException>(() => Query.Single<Simple>(removedKey));
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ModifyDataTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var state = new DisconnectedState();
      Key order1Key;
      Key newCustomerKey;

      // Modify data
      using (var session = Session.Open(Domain)) {
        var order1 = Query.All<Order>().First(order => order.Number==1);
        order1Key = order1.Key;
        Assert.AreEqual(2, order1.Details.Count);

        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query.All<Order>()
                .Prefetch(o => o.Customer)
                .Prefetch(o => o.Details,
                  od => od.Prefetch(item => item.Product)).ToList();
            }
            var newCustomer = new Customer {
              Name = "NewCustomer"
            };
            newCustomerKey = newCustomer.Key;
            session.Persist();
            newCustomer.Remove();
            session.Persist();
            
            Assert.AreEqual(2, order1.Details.Count);
            Product product3 = null;
            using (state.Connect()) {
              product3 = Query.All<Product>().First(product => product.Name=="Product3");
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
          var keyMapping = state.ApplyChanges();
          order1Key = keyMapping.TryRemapKey(order1Key);
          newCustomerKey = keyMapping.TryRemapKey(newCustomerKey);
        }
      }

      // Check saved data
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query.Single<Order>(order1Key);
          var details = order1.Details.ToList();
          Assert.AreEqual(2, order1.Details.Count);
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
          AssertEx.Throws<KeyNotFoundException>(() => Query.Single<Customer>(newCustomerKey));
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void ModifyDataAfterSaveChangesTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var state = new DisconnectedState();
      Key order1Key = null;

      // Modify data
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query.All<Order>()
                .Prefetch(o => o.Customer)
                .Prefetch(o => o.Details,
                  od => od.Prefetch(item => item.Product)).ToList();
            }

            var order1 = orders.First(order => order.Number==1);
            order1Key = order1.Key;
            Product product3 = null;
            using (state.Connect()) {
              product3 = Query.All<Product>().First(product => product.Name=="Product3");
            }
            new OrderDetail() {
              Product = product3,
              Count = 250,
              Order = order1
            };
            transactionScope.Complete();
          }
          state.ApplyChanges();
        }
      }
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query.Single<Order>(order1Key);
            order1.Number = 1000;
            var details = order1.Details.ToList();
            var detail3 = details.First(detail => detail.Count==250);
            detail3.Count = 1250;

            transactionScope.Complete();
          }
          state.ApplyChanges();
        }
      }


      // Check saved data
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query.Single<Order>(order1Key);
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
    public void EntitySetMiniTest()
    {
      var state = new DisconnectedState();
      Key supplierKey;
      using (var session = Session.Open(Domain))
      using (state.Attach(session))
      using (var transactionScope = Transaction.Open()) {
        var supplier = new Supplier();
        supplierKey = supplier.Key;
        supplier.Products.Add(new Product());
        supplier.Products.Add(new Product());
        transactionScope.Complete();
      }

      using (var session = Session.Open(Domain))
      using (state.Attach(session))
      using (Transaction.Open()) {
        var supplier = Query.Single<Supplier>(supplierKey);
        Assert.AreEqual(2, supplier.Products.Count);
        Assert.IsTrue(supplier.Products.State.IsFullyLoaded);
      }
    }

    [Test]
    public void MultipleTransactionsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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
              suppliers = Query.All<Supplier>().Prefetch(s => s.Products).ToList();
              products = Query.All<Product>().Prefetch(p => p.Supplier).ToList();
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
            var newProduct1 = Query.SingleOrDefault<Product>(newProduct1Key);
            var newProduct2 = Query.SingleOrDefault<Product>(newProduct2Key);
            var product3 = Query.SingleOrDefault<Product>(product3Key);
            var newSupplier = Query.SingleOrDefault<Supplier>(newSupplierKey);

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
          var keyMapping = state.ApplyChanges();
          newProduct1Key = keyMapping.TryRemapKey(newProduct1Key);
          newProduct2Key = keyMapping.TryRemapKey(newProduct2Key);
          product3Key = keyMapping.TryRemapKey(product3Key);
          newSupplierKey = keyMapping.TryRemapKey(newSupplierKey);
          supplier1Key = keyMapping.TryRemapKey(supplier1Key);
        }
      }

      // Check changes
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var newProduct1 = Query.SingleOrDefault<Product>(newProduct1Key);
          var newProduct2 = Query.SingleOrDefault<Product>(newProduct2Key);
          var product3 = Query.SingleOrDefault<Product>(product3Key);
          var newSupplier = Query.SingleOrDefault<Supplier>(newSupplierKey);
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
              supplier1 = Query.All<Supplier>().First(employee => employee.Name=="Supplier1");
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

            var supplier1 = Query.Single<Supplier>(updatedSupplierKey);
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

            var supplier1 = Query.Single<Supplier>(updatedSupplierKey);
            Assert.AreEqual("UpdatedSupplier1", supplier1.Name);
            AssertEx.Throws<ConnectionRequiredException>(() => Query.Single<Supplier>(newSupplierKey));

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
        Assert.AreEqual(1, customer.Orders.Count);
        using (var inner = Transaction.Open(TransactionOpenMode.New)) {
          Assert.AreEqual(1, customer.Orders.Count);
          customer.Orders.Add(new Order());
          Assert.AreEqual(2, customer.Orders.Count);
          // rollback
        }
        Assert.AreEqual(1, customer.Orders.Count);
        outer.Complete();
      }

      // verifying changes
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (Transaction.Open()) {
        var customer = Query.Single<Customer>(customerKey);
        var order = customer.Orders.AsEnumerable().Single();
        Assert.AreEqual(orderKey, order.Key);
      }
      
      // nested transactions in disconnected mode
      using (var session = Session.Open(Domain))
      using (disconnectedState.Attach(session))
      using (var outer = Transaction.Open()) {
        var customer = Query.Single<Customer>(customerKey);
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
        var customer = Query.Single<Customer>(customerKey);
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
          orderKey = Query.All<Order>().OrderBy(c => c.Id).First().Key;
          transactionScope.Complete();
        }
      }

      var state = new DisconnectedState();
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              Query.All<Order>().Prefetch(o => o.Details).ToList();
            }
            transactionScope.Complete();
          }
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query.Single<Order>(orderKey);
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
              orders = Query.All<Order>().Prefetch(o => o.Details).ToList();
              Query.All<Product>().ToList(); // Need for OrderDetails removing
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
            var order1 = Query.Single<Order>(order1Key);
            var order2 = Query.Single<Order>(order2Key);
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
              authors = Query.All<Author>().Prefetch(a => a.Books).ToList();
              books = Query.All<Book>().Prefetch(b => b.Authors).ToList();
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Key order1Key;
      Key newCustomerKey;

      var log = new OperationLog(OperationLogType.OutermostOperationLog);
      // Modify data
      using (var session = Session.Open(Domain)) {
        using (OperationCapturer.Attach(session, log))
        using (var transactionScope = Transaction.Open()) {
          var orders = Query.All<Order>()
            .Prefetch(o => o.Customer)
            .Prefetch(o => o.Details,
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
          product3 = Query.All<Product>().First(product => product.Name=="Product3");
          new OrderDetail() {
            Product = product3,
            Count = 250,
            Order = order1
          };
          var product2 = Query.All<Product>().First(product => product.Name == "Product2");
          var order1Detail1 = order1.Details.ToList().First(detail => detail.Product.Name=="Product1");
          order1Detail1.Product.Name = "Product1.New";
          order1Detail1.Count = 150;
          var order1Detail2 = order1.Details.ToList().First(detail => detail.Product.Name=="Product2");
          order1.Details.Remove(order1Detail2);
          order1Detail2.Remove();
          order1.LogCreateOrderItemOperation(product2, 499);
        }
      }

 
      // Save data to storage
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open(session)) {
          var logCopy = Cloner.Default.Clone(log);
          logCopy.Replay(session);
          transactionScope.Complete();
        }
      }

      // Check saved data
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Order order1 = Query.Single<Order>(order1Key);
          var details = order1.Details.ToList();
          Assert.AreEqual(3, order1.Details.Count);
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product2" && detail.Count == 499));
          Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
          AssertEx.Throws<KeyNotFoundException>(() => Query.Single<Customer>(newCustomerKey));
          transactionScope.Complete();
        }
      }
    }
    
    [Test]
    public void SerializationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var state = new DisconnectedState();
      Key order1Key;
      Key newCustomerKey;

      // Modify data
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            List<Order> orders = null;
            using (state.Connect()) {
              orders = Query.All<Order>()
                .Prefetch(o => o.Customer)
                .Prefetch(o => o.Details,
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
              product3 = Query.All<Product>().First(product => product.Name=="Product3");
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
        state = LegacyBinarySerializer.Instance.Clone(state) as DisconnectedState;
        Assert.IsNotNull(state);
      }
      
      // Check data in cache and save to DB
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var order1 = Query.Single<Order>(order1Key);
            var details = order1.Details.ToList();
            Assert.AreEqual(2, order1.Details.Count);
            Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product1.New"));
            Assert.IsNotNull(details.FirstOrDefault(detail => detail.Product.Name=="Product3"));
            transactionScope.Complete();
          }
          var keyMapping = state.ApplyChanges();
          order1Key = keyMapping.TryRemapKey(order1Key);
          newCustomerKey = keyMapping.TryRemapKey(newCustomerKey);
        }
      }

      // Check data in DB
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order1 = Query.Single<Order>(order1Key);
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var state = new DisconnectedState();
      Key customer1Key = null;

      // Fetch instance to cache
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            using (state.Connect()) {
              customer1Key = Query.All<Customer>().First(customer => customer.Name=="Customer1").Key;
              Log.Info("Key: {0}", customer1Key);
            }
            transactionScope.Complete();
          }
        }
      }

      // Modify instance in storage
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var customer1 = Query.Single<Customer>(customer1Key);
          customer1.Name = "NewName1";
          transactionScope.Complete();
        }
      }

      // Clone DisconnectedState
      DisconnectedState stateClone;
      using (Session.Open(Domain))
        stateClone = (DisconnectedState) LegacyBinarySerializer.Instance.Clone(state);

      // Modify instance in cache and check version
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var customer1 = Query.Single<Customer>(customer1Key);
            customer1.Name = "NewName2";
            transactionScope.Complete();
          }
          AssertEx.Throws<VersionConflictException>(() => state.ApplyChanges());
        }
      }

      // Set the cached instance's field value equal to the field value of instance in the storage
      using (var session = Session.Open(Domain)) {
        using (stateClone.Attach(session)) {
          using (var transactionScope = Transaction.Open()) {
            var customer1 = Query.Single<Customer>(customer1Key);
            customer1.Name = "NewName1";
            transactionScope.Complete();
          }
          // Must throw an exception, but currently doesn't, because
          // change attempt isn't detected (old value "NewName1" = new "NewName1").
          AssertEx.Throws<VersionConflictException>(() => stateClone.ApplyChanges());
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
              container = Query.Single<Container>(containerKey);
              item = Query.Single<Item>(itemKey);
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
            var newContainer = Query.Single<Container>(containerKey);
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
              container = Query.Single<Container>(containerKey);
              item = Query.Single<Item>(itemKey);
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
            var newContainer = Query.Single<Container>(containerKey);
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
              Query.All<Container>().Prefetch(c => c.ManyToZero).ToList();
              container = Query.Single<Container>(containerKey);
              item = Query.Single<Item>(itemKey);
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
            var item = Query.Single<Item>(itemKey);
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
              author1 = Query.All<Author>()
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
            var newCustomer = Query.Single<Customer>(customerKey);
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
              Query.All<Order>().ToList();
              Query.All<Order>().Prefetch(o => o.Customer).ToList();
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
              customers = Query.All<Customer>().Prefetch(c => c.Orders).ToList();
            }
            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void ApplyRemoveOperationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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
              a = Query.All<A>().First();
              list = Query.All<B>().ToList();
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
          var a = Query.All<A>().First();
          var b3 = new B();
          b3.Root = a;
          transactionScope.Complete();
        }
      }

      // Save changes to DB
      using (var session = Session.Open(Domain)) {
        using (state.Attach(session)) {
          state.ApplyChanges();
        }
      }

      // Check data
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          Assert.IsNull(Query.All<A>().FirstOrDefault());
          Assert.IsTrue(Query.All<B>().All(item => item.Root==null));
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void MergeModeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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
              Query.All<Simple>().ToList();
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
              Query.All<Simple>().ToList();
            }
            var simple = Query.Single<Simple>(key);
            simple.Value = "Value2";
            Assert.IsNotNull(simple);
            transactionScope.Complete();
          }
          state1.ApplyChanges();
        }
      }
      
      // Merge and check
      using (var session = Session.Open(Domain)) {
        using (state2.Attach(session)) {
          AssertEx.Throws<VersionConflictException>(() => state2.Merge(state1, MergeMode.Strict));
          state2.Merge(state1, MergeMode.PreferOriginal);
          Assert.AreEqual("Value1", Query.Single<Simple>(key).Value);
          state2.Merge(state1, MergeMode.PreferNew);
          Assert.AreEqual("Value2", Query.Single<Simple>(key).Value);
        }
      }
    }

    [Test]
    public void MapCompositeKeyContainingEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Key localFirstKey;
      Key secondKey;
      Key localCompositeKey;
      KeyMapping keyMapping;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (state.Attach(session)) {
        using (var transactionScope = Transaction.Open()) {
          var firstElement = new KeyElementFirst();
          localFirstKey = firstElement.Key;
          var secondElement = new KeyElementSecond(Guid.NewGuid());
          secondKey = secondElement.Key;
          var composite = new CompositeKeyExample(firstElement, 3, secondElement);
          localCompositeKey = composite.Key;
          transactionScope.Complete();
        }
        keyMapping = state.ApplyChanges();
      }

      using (var session = Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var composite = Query.Single<CompositeKeyExample>(keyMapping.Map[localCompositeKey]);
        Assert.AreEqual(keyMapping.Map[localFirstKey], composite.First.Key);
        Assert.AreEqual(3, composite.Second);
        Assert.AreEqual(secondKey, composite.Third.Key);
        transactionScope.Complete();
      }
    }

    [Test]
    public void MapKeyContainingOtherEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Key localKey;
      Key localSimpleKey;
      KeyMapping keyMapping;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (state.Attach(session)) {
        using (var transactionScope = Transaction.Open()) {
          var simple = new Simple {Value = "Value0"};
          localSimpleKey = simple.Key;
          var entity = new EntityIdentifiedByEntity(simple);
          localKey = entity.Key;
          transactionScope.Complete();
        }
        keyMapping = state.ApplyChanges();
      }

      using (var session = Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var entity = Query.Single<EntityIdentifiedByEntity>(keyMapping.Map[localKey]);
        Assert.AreEqual(keyMapping.Map[localSimpleKey], entity.Id.Key);
        transactionScope.Complete();
      }
    }

    [Test]
    public void MapExplicitlySpecifiedKeyContainingSinglePrimitiveValueTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Key key;
      int value;
      KeyMapping keyMapping;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (state.Attach(session)) {
        using (var transactionScope = Transaction.Open()) {
          value = int.MaxValue - 1;
          var entity = new EntityIdentifiedByPrimitiveValue(value);
          key = entity.Key;
          transactionScope.Complete();
        }
        keyMapping = state.ApplyChanges();
      }

      using (var session = Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var entity = Query.Single<EntityIdentifiedByPrimitiveValue>(key);
        Assert.AreEqual(value, entity.Id);
        Assert.AreEqual(0, keyMapping.Map.Count);
        transactionScope.Complete();
      }
    }

    [Test]
    public void MapCompositeKeyContainingPrimitiveValuesOnlyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Key key;
      int valueFirst;
      Guid valueSecond;
      KeyMapping keyMapping;
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (state.Attach(session)) {
        using (var transactionScope = Transaction.Open()) {
          valueFirst = int.MaxValue - 2;
          valueSecond = Guid.NewGuid();
          var entity = new CompositePrimitiveKeyExample(valueFirst, valueSecond);
          key = entity.Key;
          transactionScope.Complete();
        }
        keyMapping = state.ApplyChanges();
      }

      using (var session = Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var entity = Query.Single<CompositePrimitiveKeyExample>(key);
        Assert.AreEqual(valueFirst, entity.IdFirst);
        Assert.AreEqual(valueSecond, entity.IdSecond);
        Assert.AreEqual(0, keyMapping.Map.Count);
        transactionScope.Complete();
      }
    }

    [Test]
    public void MergeFailedTest()
    {
      var serverData = new DisconnectedState();
      var clientData = new DisconnectedState();

      Key childKey;

      using (Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var child = new ChangeSet {Parent = new ChangeSet()};
        childKey = child.Key;
        transactionScope.Complete();
      }

      using (Session.Open(Domain))
      using (serverData.Attach())
      using (serverData.Connect())
      using (var transactionScope = Transaction.Open()) {
        Query.Single<ChangeSet>(childKey);
      }
      
      using (Session.Open(Domain))
      using (clientData.Attach()) {
        clientData.Merge(serverData);
      }
    }

    private void FillDataBase()
    {
      using (var session = Session.Open(Domain)) {
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