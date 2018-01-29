// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.12.22

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0718_EntitySetEnumerationBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0718_EntitySetEnumerationBugModel
{
  public abstract class BaseEntity : Entity
  {
    [Key, Field]
    public long ID { get; set; }
  }

  [HierarchyRoot]
  public class CustomerOrder : BaseEntity
  {
    [Field]
    public string Name { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear, PairTo = "CustomerOrder")]
    public EntitySet<CustomerOrderItem> Items { get; set; }
  }

  [HierarchyRoot]
  public class CustomerOrderItem : BaseEntity
  {
    [Field]
    public string Product { get; set; }

    [Field]
    public CustomerOrder CustomerOrder { get; private set; }

    public CustomerOrderItem(CustomerOrder order)
    {
      CustomerOrder = order;
    }
  }

  [HierarchyRoot]
  public class SomeOtherEntity : BaseEntity
  {
    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear, PairTo = "SomeOtherEntity")]
    public EntitySet<SomeOtherEntitySet> Items { get; set; }
  }

  [HierarchyRoot]
  public class SomeOtherEntitySet : BaseEntity
  {
    [Field]
    public string Name { get; set; }

    [Field]
    public SomeOtherEntity SomeOtherEntity { get; private set; }


    public SomeOtherEntitySet(SomeOtherEntity entity)
    {
      SomeOtherEntity = entity;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0718_EntitySetEnumeration : AutoBuildTest
  {
    private const int DefaultCustomerOrderItemsCount = 5;

    private readonly SessionConfiguration clientProfile = new SessionConfiguration(SessionOptions.ClientProfile);
    private readonly SessionConfiguration serverProfile = new SessionConfiguration(SessionOptions.ServerProfile);

    [SetUp]
    public void SetUp()
    {
      ClearTables();
    }

    [Test(Description = "Persist on enumeration")]
    public void ServerProfileTest01()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);

        Assert.DoesNotThrow(
          () => {
            foreach (var orderItem in customerOrder.Items)
              session.SaveChanges();
          });
      }
    }

    [Test(Description = "Add item to entity set after enumeration")]
    public void ServerProfileTest02()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Add(new CustomerOrderItem(customerOrder));
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount + 1);
      }
    }

    [Test(Description = "Add item to entity set after")]
    public void ServerProfileTest03()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              new CustomerOrderItem(customerOrder);
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount + 1);
      }
    }

    [Test(Description = "Implicit remove entity after enumeration")]
    public void ServerProfileTest04()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          var customerOrderItem = customerOrderItems.First();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Remove(customerOrderItem);
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount - 1);
      }
    }

    [Test(Description = "Explicit remove entity after enumeration")]
    public void ServerProfileTest05()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          var customerOrderItem = customerOrderItems.First();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItem.Remove();
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount - 1);
      }
    }

    [Test(Description = "Explicit remove enitity from entity set")]
    public void ServerProfileTest06()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                new SomeOtherEntity();

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<SomeOtherEntity>().Count(), DefaultCustomerOrderItemsCount);
      }
    }

    [Test(Description = "Add entity in another entity set")]
    public void ServerProfileTest07()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                new SomeOtherEntitySet(someOtherEntity);

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<SomeOtherEntity>().Single().Items.Count(), DefaultCustomerOrderItemsCount);
      }
    }

    [Test(Description = "Remove entities from enother entity set")]
    public void ServerProfileTest08()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          session.SaveChanges();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems) {
                var fetchedSomeOtherEntity = session.Query.All<SomeOtherEntity>().SingleOrDefault();
                if (fetchedSomeOtherEntity!=null && !fetchedSomeOtherEntity.IsRemoved)
                  fetchedSomeOtherEntity.Remove();
              }

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.IsFalse(session.Query.All<SomeOtherEntity>().Any());
      }
    }

    [Test(Description = "Remove another entity entity set items")]
    public void ServerProfileTest09()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          var someOtherEntitySet = new SomeOtherEntitySet(someOtherEntity);
          session.SaveChanges();

          Assert.DoesNotThrow(
            () => {
              var someOtherEntitySetItems = session.Query.All<SomeOtherEntity>().Single().Items;
              foreach (var orderItem in customerOrderItems)
                session.Remove(someOtherEntitySetItems);

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.IsFalse(session.Query.All<SomeOtherEntity>().Single().Items.Any());
      }
    }

    [Test(Description = "Enumeration inside enumeration")]
    public void ServerProfileTest10()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var item in customerOrderItems)
              foreach (var item2 in customerOrderItems)
                session.SaveChanges();

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }
      }
    }

    [Test(Description = "Explicit add and persist during entity set enumeration")]
    public void ServerProfileTest11()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var item in customerOrderItems)
            foreach (var item2 in customerOrderItems)
              new CustomerOrderItem(customerOrder);

            session.SaveChanges();

            PersistIfClientProfile(session);
          });
        transaction.Complete();
      }
    }

    [Test(Description = "Explicit add item to entity set")]
    public void ServerProfileTest12()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems)
              new CustomerOrderItem(customerOrder);
          });
      }
    }

    [Test(Description = "Explicit persist and add item to entity set")]
    public void ServerProfileTest13()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems) {
              session.SaveChanges();
              new CustomerOrderItem(customerOrder);
            }
          });
      }
    }

    [Test(Description = "Implicitly remove entity from entityset during enumeration")]
    public void ServerProfileTest14()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems)
              orderItem.Remove();
          });
      }
    }

    [Test(Description = "Explicit persist and implicit remove from entity set")]
    public void ServerProfileTest15()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems) {
              session.SaveChanges();
              orderItem.Remove();
            }
          });
      }
    }

    [Test(Description = "Explicit persist and change of entity in entity set")]
    public void ServerProfileTest16()
    {
      var expectedProductName = "GTX 1080 ti";

      using (var session = Domain.OpenSession(serverProfile)) {
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;
            PersistIfClientProfile(session);

            Assert.DoesNotThrow(
              () => {
                foreach (var orderItem in customerOrderItems) {
                  orderItem.Product = expectedProductName;
                  session.SaveChanges();
                }
              });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction()) {
            var customerOrder = session.Query.All<CustomerOrder>().Single();
            Assert.IsTrue(customerOrder.Items.All(i => i.Product==expectedProductName));
          }
        }
      }
    }

    [Test(Description = "Explicit persist and change of entity in entity set")]
    public void ServerProfileTest17()
    {
      var expectedProductName = "GTX 1080 ti";

      ClearTables();
      using (var session = Domain.OpenSession(serverProfile)) {
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;
            PersistIfClientProfile(session);

            Assert.DoesNotThrow(
              () => {
                foreach (var orderItem in customerOrderItems) {
                  session.SaveChanges();
                  orderItem.Product = expectedProductName;
                }
              });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction()) {
            var customerOrder = session.Query.All<CustomerOrder>().Single();
            Assert.IsTrue(customerOrder.Items.All(i => i.Product==expectedProductName));
          }
        }
      }
    }

    [Test(Description = "Emplicit persist and remove entity")]
    public void ServerProfileTest18()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItem = new CustomerOrderItem(customerOrder);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items) {
              session.SaveChanges();
              customerOrder.Remove();
            }
          });
      }
    }

    [Test(Description = "Implicitly remove entity from entity set")]
    public void ServerProfileTest19()
    {
      using (var session = Domain.OpenSession(serverProfile)) {
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);

            Assert.Throws<InvalidOperationException>(
              () => {
                foreach (var orderItem in customerOrder.Items)
                  customerOrder.Remove();
              });
            transaction.Complete();
          }
        }
      }
    }

    [Test(Description = "Explicitly remove entity during enumeration")]
    public void ServerProfileTest20()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        var customerOrderItem = new CustomerOrderItem(customerOrder);
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items)
              customerOrder.Items.Remove(customerOrderItem);
          });
      }
    }

    [Test(Description = "Explicitly add items during enum")]
    public void ServerProfileTest21()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        var customerOrderItem = new CustomerOrderItem(customerOrder);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items) {
              customerOrder.Items.Add(new CustomerOrderItem(customerOrder));
            }
          });
      }
    }

    [Test(Description = "Clears items during transaction")]
    public void ServerProfileTest22()
    {
      using (var session = Domain.OpenSession(serverProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        new CustomerOrderItem(customerOrder);
        new CustomerOrderItem(customerOrder);

        PersistIfClientProfile(session);
        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var order in customerOrder.Items) {
              customerOrder.Items.Clear();
            }
          });
      }
    }

    [Test(Description = "Persist on enumeration")]
    public void ClientProfileTest01()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);

        Assert.DoesNotThrow(
          () => {
            foreach (var orderItem in customerOrder.Items)
              session.SaveChanges();
          });
      }
    }

    [Test(Description = "Add item to entity set after enumeration")]
    public void ClientProfileTest02()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Add(new CustomerOrderItem(customerOrder));
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount + 1);
      }
    }

    [Test(Description = "Add item to entity set after")]
    public void ClientProfileTest03()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              new CustomerOrderItem(customerOrder);
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount + 1);
      }
    }

    [Test(Description = "Implicit remove entity after enumeration")]
    public void ClientProfileTest04()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          var customerOrderItem = customerOrderItems.First();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Remove(customerOrderItem);
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount - 1);
      }
    }

    [Test(Description = "")]
    public void ClientProfileTest05()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          var customerOrderItem = customerOrderItems.First();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItem.Remove();
              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, DefaultCustomerOrderItemsCount - 1);
      }
    }

    [Test(Description = "Explicit remove enitity from entity set")]
    public void ClientProfileTest06()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                new SomeOtherEntity();

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<SomeOtherEntity>().Count(), DefaultCustomerOrderItemsCount);
      }
    }

    [Test(Description = "Add entity in another entity set")]
    public void ClientProfileTest07()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems)
                new SomeOtherEntitySet(someOtherEntity);

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.AreEqual(session.Query.All<SomeOtherEntity>().Single().Items.Count(), DefaultCustomerOrderItemsCount);
      }
    }

    [Test(Description = "Remove entities from enother entity set")]
    public void ClientProfileTest08()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          session.SaveChanges();

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems) {
                var fetchedSomeOtherEntity = session.Query.All<SomeOtherEntity>().SingleOrDefault();
                if (fetchedSomeOtherEntity!=null && !fetchedSomeOtherEntity.IsRemoved)
                  fetchedSomeOtherEntity.Remove();
              }

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.IsFalse(session.Query.All<SomeOtherEntity>().Any());
      }
    }

    [Test(Description = "Remove another entity entity set items")]
    public void ClientProfileTest09()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          var someOtherEntity = new SomeOtherEntity();
          var someOtherEntitySet = new SomeOtherEntitySet(someOtherEntity);
          session.SaveChanges();

          Assert.DoesNotThrow(
            () => {
              var someOtherEntitySetItems = session.Query.All<SomeOtherEntity>().Single().Items;
              foreach (var orderItem in customerOrderItems)
                session.Remove(someOtherEntitySetItems);

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction())
          Assert.IsFalse(session.Query.All<SomeOtherEntity>().Single().Items.Any());
      }
    }

    [Test(Description = "Enumeration inside enumeration")]
    public void ClientProfileTest10()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.DoesNotThrow(
            () => {
              foreach (var item in customerOrderItems)
              foreach (var item2 in customerOrderItems)
                session.SaveChanges();

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }
      }
    }

    [Test(Description = "Explicit add and persist during entity set enumeration")]
    public void ClientProfileTest11()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;

          Assert.Throws<InvalidOperationException>(
            () => {
              foreach (var item in customerOrderItems)
              foreach (var item2 in customerOrderItems)
                new CustomerOrderItem(customerOrder);
              session.SaveChanges();

              PersistIfClientProfile(session);
            });
          transaction.Complete();
        }
      }
    }

    [Test(Description = "Explicit add item to entity set")]
    public void ClientProfileTest12()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems)
              new CustomerOrderItem(customerOrder);
          });
      }
    }

    [Test(Description = "Explicit persist and add item to entity set")]
    public void ClientProfileTest13()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems) {
              session.SaveChanges();
              new CustomerOrderItem(customerOrder);
            }
          });
      }
    }

    [Test(Description = "Implicitly remove entity from entityset during enumeration")]
    public void ClientProfileTest14()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          PersistIfClientProfile(session);

          Assert.Throws<InvalidOperationException>(
            () => {
              foreach (var orderItem in customerOrderItems)
                orderItem.Remove();
            });
        }
      }
    }

    [Test(Description = "Explicit persist and implicit remove from entity set")]
    public void ClientProfileTest15()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);
        var customerOrderItems = customerOrder.Items;
        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrderItems) {
              session.SaveChanges();
              orderItem.Remove();
            }
          });
      }
    }

    [Test(Description = "Explicit persist and change of entity in entity set")]
    public void ClientProfileTest16()
    {
      var expectedProductName = "GTX 1080 ti";

      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          PersistIfClientProfile(session);

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems) {
                orderItem.Product = expectedProductName;
                session.SaveChanges();
              }
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var customerOrder = session.Query.All<CustomerOrder>().Single();
          Assert.IsTrue(customerOrder.Items.All(i => i.Product==expectedProductName));
        }
      }
    }

    [Test(Description = "Explicit persist and change of entity in entity set")]
    public void ClientProfileTest17()
    {
      var expectedProductName = "GTX 1080 ti";

      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate()) {
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);
          var customerOrderItems = customerOrder.Items;
          PersistIfClientProfile(session);

          Assert.DoesNotThrow(
            () => {
              foreach (var orderItem in customerOrderItems) {
                session.SaveChanges();
                orderItem.Product = expectedProductName;
              }
            });
          transaction.Complete();
        }

        using (var transaction = session.OpenTransaction()) {
          var customerOrder = session.Query.All<CustomerOrder>().Single();
          Assert.IsTrue(customerOrder.Items.All(i => i.Product==expectedProductName));
        }
      }
    }

    [Test(Description = "Emplicit persist and remove entity")]
    public void ClientProfileTest18()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = CreateCustomerOrderWithItems(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items) {
              session.SaveChanges();
              customerOrder.Remove();
            }
          });
      }
    }

    [Test(Description = "Implicitly remove entity from entity set")]
    public void ClientProfileTest19()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        var customerOrderItem = new CustomerOrderItem(customerOrder);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items)
              customerOrder.Remove();
          });
        transaction.Complete();
      }
    }

    [Test(Description = "Explicitly remove entity during enumeration")]
    public void ClientProfileTest20()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var items = new List<CustomerOrderItem>();
        var customerOrder = CreateCustomerOrderWithItems(session, ref items);
        var itemToRemove = items[0];

        PersistIfClientProfile(session);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items)
              customerOrder.Items.Remove(itemToRemove);
          });
      }
    }

    [Test(Description = "Explicitly add items during enum")]
    public void ClientProfileTest21()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        var customerOrderItem = new CustomerOrderItem(customerOrder);

        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var orderItem in customerOrder.Items) {
              customerOrder.Items.Add(new CustomerOrderItem(customerOrder));
            }
          });
      }
    }

    [Test(Description = "Clears items during transaction")]
    public void ClientProfileTest22()
    {
      using (var session = Domain.OpenSession(clientProfile))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var customerOrder = new CustomerOrder();
        new CustomerOrderItem(customerOrder);
        new CustomerOrderItem(customerOrder);

        PersistIfClientProfile(session);
        Assert.Throws<InvalidOperationException>(
          () => {
            foreach (var order in customerOrder.Items) {
              customerOrder.Items.Clear();
            }
          });
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(CustomerOrderItem).Assembly, typeof(CustomerOrderItem).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    private void ClearTables()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customerOrders = session.Query.All<CustomerOrder>().ToArray();
        var someOtherEntities = session.Query.All<SomeOtherEntity>().ToArray();

        if (customerOrders.Any())
          session.Remove(customerOrders);
        if (someOtherEntities.Any())
          session.Remove(someOtherEntities);
        transaction.Complete();
      }
    }

    private CustomerOrder CreateCustomerOrderWithItems(Session session)
    {
      List<CustomerOrderItem> list = new List<CustomerOrderItem>();
      return CreateCustomerOrderWithItems(session, ref list);
    }

    private CustomerOrder CreateCustomerOrderWithItems(Session session, ref List<CustomerOrderItem> items)
    {
      var customerOrder = new CustomerOrder();
      for (var i = 0; i < DefaultCustomerOrderItemsCount; i++)
        items.Add(new CustomerOrderItem(customerOrder));

      PersistIfClientProfile(session);
      return customerOrder;
    }

    private void PersistIfClientProfile(Session sesssion)
    {
      if (Session.Current.Configuration.Options.HasFlag(SessionOptions.ClientProfile))
        sesssion.SaveChanges();
    }
  }
}
