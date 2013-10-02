// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [TestFixture]
  public class Issue0631_DisconnectedStateBugs : AutoBuildTest
  {
    private static readonly Guid guidA = new Guid("A4CB133D-257C-4FA9-B827-8915627DAAAA");
    private static readonly Guid guidB = new Guid("4CE5B488-33B7-48CB-A42B-B92463B0BBBB");
    private static readonly Guid guidC = new Guid("A1631DEC-A06F-4A48-831B-7527DEDFCCCC");
    private static readonly Guid guidD = new Guid("4027ECA5-5476-467B-9457-3ECD6F08DDDD");

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (EntityBase).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var s = domain.OpenSession()) {
        using (var tx = s.OpenTransaction()) {
          var e = new TestEntity(Guid.NewGuid()) {
            Date = DateTime.Now, 
            Integer = 1
          };
          e.OwnedItems.Add(new OwnedEntity(guidA) { Name = "a" });
          e.OwnedItems.Add(new OwnedEntity(guidB) { Name = "b" });
          e.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
          e.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));

          var ff = new TestEntity(Guid.NewGuid()) {
            Date = DateTime.Now, 
            Integer = 2
          };
          ff.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
          ff.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
          ff.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
          ff.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));

          tx.Complete();
        }
      }
      return domain;
    }


    [Test]
    public void CombinedTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);

      var ds = new DisconnectedState(); 

      // Adding few items to DisconnectedState
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            TestEntity doc;
            using (ds.Connect()) {
              doc = session.Query.All<TestEntity>().Where(f => f.Integer == 1).Single();
              doc.OwnedItems.Add(new OwnedEntity(guidC) { Name = "c" }); // Not a real Add
              doc.OwnedItems.Add(new OwnedEntity(guidD) { Name = "d" }); // Not a real Add
            }
            tx.Complete();
          }
        }
      }

      DisconnectedState dsBackup;

      // Removing one old and one new item
      using (var session = Domain.OpenSession()) {
        dsBackup = ds.Clone(); // Needs active Session to work properly

        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (ds.Connect()) {
              var doc = session.Query.All<TestEntity>().Where(f => f.Integer == 1).Single();
              // Must throw an exception, since there is no real entity
              AssertEx.Throws<InvalidOperationException>(() => doc.OwnedItems.Single(a => a.Id==guidD));
              
              var dItem = doc.OwnedItems.ToList().Single(a => a.Id == guidD);
              // Must throw an exception, because OnRemoveAction on TestEntity.OwnedItems is Deny (default)
              AssertEx.Throws<ReferentialIntegrityException>(dItem.Remove);
              doc.OwnedItems.Remove(dItem);
              dItem.Remove();
              
              var aItem = session.Query.All<OwnedEntity>().Single(a => a.Id == guidA);
              doc.OwnedItems.Remove(aItem);
              aItem.Remove();
            }
            tx.Complete();
          }
        }
      }

      // Rolling everything back by switching back to the clone
      ds = dsBackup;

      // Modifying B and C instances and rollback the local transaction
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (ds.Connect()) {
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems) // Must not affect here
                .Single();
              var anotherEntities = session.Query.All<OwnedEntity>().ToArray();
              session.Query.Single<OwnedEntity>(guidB).Name = "b2";
              session.Query.Single<OwnedEntity>(guidC).Name = "c2";
            }
            // tx.Complete();
          }
        }
      }

      // Testing if above block has successfully completed
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (ds.Connect()) {
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems) // Must not affect here
                .Single();

              // Must throw an exception, since there is no real entity
              AssertEx.ThrowsInvalidOperationException(() => 
                session.Query.All<OwnedEntity>().Single(a => a.Id == guidC));

              // But it esxists in EntitySet
              Assert.AreEqual(session.Query.Single<OwnedEntity>(guidB).Name, "b");
              // And this one is in database
              Assert.AreEqual(session.Query.All<OwnedEntity>().Single(a => a.Id == guidB).Name, "b");
            }
          }
        }
      }

      // Really modifying B and C instances
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (ds.Connect()) {
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems) // Must not affect here
                .Single(); 
              session.Query.Single<OwnedEntity>(guidB).Name = "b2";
              session.Query.Single<OwnedEntity>(guidC).Name = "c2";

              var aItem = session.Query.All<OwnedEntity>().Single(a => a.Id == guidA);
              doc.OwnedItems.Remove(aItem);
              aItem.Remove();
            }
            tx.Complete();
          }
        }
      }

      // Testing if above block has successfully completed
      using (var session = Domain.OpenSession()) {
        using (ds.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (ds.Connect()) {
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems) // Must not affect here
                .Single();

              Assert.AreEqual(session.Query.Single<OwnedEntity>(guidC).Name, "c2");
              Assert.AreEqual(session.Query.Single<OwnedEntity>(guidB).Name, "b2");

              Assert.IsNull(session.Query.SingleOrDefault<OwnedEntity>(guidA));
              var ownedItems = doc.OwnedItems.ToList();
              Assert.IsFalse(ownedItems.Any(i => i.Id == guidA));
            }
          }
        }
      }

      // Applying all the changes
      using (var session = Domain.OpenSession()) {
        ds.ApplyChanges(session);
      }

      // Testing if above block has successfully completed
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          Assert.AreEqual(session.Query.All<OwnedEntity>().Single(i => i.Id == guidB).Name, "b2");
          Assert.AreEqual(session.Query.All<OwnedEntity>().Single(i => i.Id == guidC).Name, "c2");
          Assert.AreEqual(session.Query.All<OwnedEntity>().Single(i => i.Id == guidD).Name, "d");

          Assert.IsNull(session.Query.All<OwnedEntity>().SingleOrDefault(i => i.Id == guidA));
          var doc = session.Query.All<TestEntity>()
            .Where(f => f.Integer == 1)
            .Prefetch(session, s => s.OwnedItems) // Must not affect here
            .Single();
          Assert.IsFalse(doc.OwnedItems.Any(i => i.Id == guidA));
        }
      }
    }

    [Test]
    public void PartialSelectTest()
    {
      var state = new DisconnectedState();
      using (var session = Domain.OpenSession()) {
        using (state.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (state.Connect()) {
              var partialResult = session.Query.All<TestEntity>().Select(u => new { u.Date }).ToList();
              // Exception here, to be fixed
              var fullResult = session.Query.All<TestEntity>().ToList();
            }
            tx.Complete();
          }
        }
      }
    }

    [Test]
    public void SameEntitySelect()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);

      var state = new DisconnectedState();

      using (var session = Domain.OpenSession()) {
        using (state.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (state.Connect()) {
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems)
                .Single();

              var subs = doc.OwnedItems.Where(g => true).ToList();
              foreach (var sub in subs) {
                sub.Name = "1111";
              }

              doc.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
              doc.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
              doc.OwnedItems.Add(new OwnedEntity(Guid.NewGuid()));
            }

            var fe = new TestEntity(Guid.NewGuid()) { Date = DateTime.Now, Integer = 123 };
            fe.Integer = 2222;
            tx.Complete();
          }
        }
      }

      using (var session = Domain.OpenSession()) {
        using (state.Attach(session)) {
          using (var tx = session.OpenTransaction()) {
            using (state.Connect()) {
              // Exception here
              var doc = session.Query.All<TestEntity>()
                .Where(f => f.Integer == 1)
                .Prefetch(session, s => s.OwnedItems)
                .Single();

              var subs = doc.OwnedItems.Where(g => true).ToList();

              foreach (var sub in subs) {
                sub.Name = "22222";
              }
            }
            tx.Complete();
          }
          state.ApplyChanges();
        }
      }
    }
  }
}
