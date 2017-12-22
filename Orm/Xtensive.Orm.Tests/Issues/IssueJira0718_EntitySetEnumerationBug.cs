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
  public class IssueJira0718_EntitySetEnumerationBug : AutoBuildTest
  {
    private readonly int defaultCustomerOrderItemsCount = 5;
    private readonly List<SessionOptions> profiles = new List<SessionOptions>() {
      SessionOptions.ServerProfile,
      SessionOptions.ClientProfile
    };

    [Test]
    public void Test01() 
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate())
        using (var transaction = session.OpenTransaction()) {
          var customerOrder = CreateCustomerOrderWithItems(session);

          Assert.DoesNotThrow(() => {
            foreach (var orderItem in customerOrder.Items)
              session.SaveChanges();
          });
        }
      }
    }

    [Test]
    public void Test02()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Add(new CustomerOrderItem(customerOrder));
              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction())
            Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, defaultCustomerOrderItemsCount + 1);
        }
      }
    }

    [Test]
    public void Test03()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              new CustomerOrderItem(customerOrder);
              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction())
            Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, defaultCustomerOrderItemsCount + 1);
        }
      }
    }

    [Test]
    public void Test04()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;
            var customerOrderItem = customerOrderItems.First();

            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItems.Remove(customerOrderItem);
              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction()) 
            Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, defaultCustomerOrderItemsCount - 1);
        }
      }
    }

    [Test]
    public void Test05()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;
            var customerOrderItem = customerOrderItems.First();

            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems)
                session.SaveChanges();

              customerOrderItem.Remove();
              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction())
            Assert.AreEqual(session.Query.All<CustomerOrder>().First().Items.Count, defaultCustomerOrderItemsCount - 1);
        }
      }
    }

    [Test]
    public void Test06()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems)
                new SomeOtherEntity();

              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction())
            Assert.AreEqual(session.Query.All<SomeOtherEntity>().Count(), defaultCustomerOrderItemsCount);
        }
      }
    }

    [Test]
    public void Test07()
    {
      foreach (var profile in profiles){
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            var someOtherEntity = new SomeOtherEntity();
            Assert.DoesNotThrow(() => {
              foreach (var orderItem in customerOrderItems) 
                 new SomeOtherEntitySet(someOtherEntity);
              
              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }

          using (var transaction = session.OpenTransaction())
            Assert.AreEqual(session.Query.All<SomeOtherEntity>().Single().Items.Count(), defaultCustomerOrderItemsCount);
        }
      }
    }


    [Test]
    public void Test08()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            var someOtherEntity = new SomeOtherEntity();
            session.SaveChanges();

            Assert.DoesNotThrow(() =>{
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
    }


    [Test]
    public void Test09()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            var someOtherEntity = new SomeOtherEntity();
            var someOtherEntitySet = new SomeOtherEntitySet(someOtherEntity);
            session.SaveChanges();

            Assert.DoesNotThrow(() => {
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
    }

    [Test]
    public void Test10(){
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            Assert.DoesNotThrow(() =>{
              foreach (var item in customerOrderItems)
                foreach (var item2 in customerOrderItems)
                  session.SaveChanges();

              PersistIfClientProfile(session);
            });
            transaction.Complete();
          }
        }
      }
    }

    [Test]
    public void Test11()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile)))
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = CreateCustomerOrderWithItems(session);
            var customerOrderItems = customerOrder.Items;

            Assert.Throws<InvalidOperationException>(() => {
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
    }


    [Test]
    public void Test12()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrderItems)
                  new CustomerOrderItem(customerOrder);
              });
            }
          }
        }
      }
    }

    [Test]
    public void Test13()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrderItems) {
                  session.SaveChanges();
                  new CustomerOrderItem(customerOrder);
                }
              });
            }
          }
        }
      }
    }

    [Test]
    public void Test14()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrderItems)
                  orderItem.Remove();

              });
            }
          }
        }
      }
    }

    [Test]
    public void Test15()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrderItems) {
                  session.SaveChanges();
                  orderItem.Remove();
                }
              });
            }
          }
        }
      }
    }

    [Test]
    public void Test16()
    {
      var expectedProductName = "GTX 1080 ti";
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.DoesNotThrow(() => {
                foreach (var orderItem in customerOrderItems){
                  orderItem.Product = expectedProductName;
                  session.SaveChanges();
                }
              });
              transaction.Complete();
            }

            using (var transaction = session.OpenTransaction()) {
              var customerOrder = session.Query.All<CustomerOrder>().Single();
              Assert.IsTrue(customerOrder.Items.All( i => i.Product==expectedProductName));
            }
          }
        }
      }
    }

    [Test]
    public void Test17() {
      var expectedProductName = "GTX 1080 ti";
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = CreateCustomerOrderWithItems(session);
              var customerOrderItems = customerOrder.Items;
              PersistIfClientProfile(session);

              Assert.DoesNotThrow(() => {
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
    }

    [Test]
    public void Test18()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = new CustomerOrder();
              var customerOrderItem = new CustomerOrderItem(customerOrder);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrder.Items) {
                  session.SaveChanges();
                  customerOrder.Remove();
                }
              });
            }
          }
        }
      }
    }

    [Test]
    public void Test19()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = new CustomerOrder();
              var customerOrderItem = new CustomerOrderItem(customerOrder);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrder.Items)
                  customerOrder.Remove();
              });
              transaction.Complete();
            }
          }
        }
      }
    }

    [Test]
    public void Test20()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate()) {
            using (var transaction = session.OpenTransaction()) {
              var customerOrder = new CustomerOrder();
              var customerOrderItem = new CustomerOrderItem(customerOrder);
              PersistIfClientProfile(session);

              Assert.Throws<InvalidOperationException>(() => {
                foreach (var orderItem in customerOrder.Items)
                  customerOrder.Items.Remove(customerOrderItem);
              });
            }
          }
        }
      }
    }

    [Test]
    public void Test21()
    {
      RemoveTestableTypes();
      using (var session = Domain.OpenSession()) {
        using (session.Activate()) {
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = new CustomerOrder();
            var customerOrderItem = new CustomerOrderItem(customerOrder);

            Assert.Throws<InvalidOperationException>(() => {
              foreach (var orderItem in customerOrder.Items) {
                customerOrder.Items.Add(new CustomerOrderItem(customerOrder));
              }
            });
          }
        }
      }
    }

    [Test]
    public void Test22()
    {
      foreach (var profile in profiles) {
        RemoveTestableTypes();
        using (var session = Domain.OpenSession(new SessionConfiguration(profile))) {
          using (session.Activate())
          using (var transaction = session.OpenTransaction()) {
            var customerOrder = new CustomerOrder();
            var customerOrderItem = new CustomerOrderItem(customerOrder);

            PersistIfClientProfile(session);
            Assert.Throws<InvalidOperationException>(() => {
              foreach (var order in customerOrder.Items) {
                customerOrder.Items.Clear();
              }
            });
          }
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration() {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (CustomerOrderItem).Assembly, typeof (CustomerOrderItem).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    private void RemoveTestableTypes()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var customerOrders = session.Query.All<CustomerOrder>();
        var someOtherEntities = session.Query.All<SomeOtherEntity>();

        if (customerOrders.Any())
          session.Remove(customerOrders);
        if (someOtherEntities.Any())
          session.Remove(someOtherEntities);
        transaction.Complete();
      }
    }

    private CustomerOrder CreateCustomerOrderWithItems(Session session)
    {
      var customerOrder = new CustomerOrder();
      for (var i = 0; i < defaultCustomerOrderItemsCount; i++)
        new CustomerOrderItem(customerOrder);

      PersistIfClientProfile(session);
      return customerOrder;
    }

    private void PersistIfClientProfile(Session sesssion)
    {
      if (Session.Current.Configuration.Options.HasFlag(SessionOptions.NonTransactionalEntityStates))
        sesssion.SaveChanges();
    }
  }
}
