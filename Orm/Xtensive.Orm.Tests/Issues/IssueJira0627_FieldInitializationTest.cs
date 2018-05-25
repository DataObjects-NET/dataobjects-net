// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.05.08

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.GroupByNullableFieldTestModel;

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class NullGroupHasNoElementsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Battery));
      configuration.Types.Register(typeof(Product));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        List<Battery> batt = new List<Battery>() {
          null,
          new Battery() {Amps = 5, Cell = 10, Volts = 10.4},
          new Battery() {Amps = 8, Cell = 12, Volts = 12.4},
          new Battery() {Amps = 2, Cell = 5, Volts = 3.7}
        };

        for (int i = 0; i < 10; i++) {
          new Product {
            Name = "Name:" + i % 3,
            CreationDate = DateTime.UtcNow.AddDays(-(i % 3)),
            Description = i % 3!=0 ? "Lap:" + i % 3 : null,
            Comment = i % 3!=0 ? "Comment:" + i % 3 : null,
            Price = new Money() {CurrencyCode = 100 + i % 3, Value = (decimal) 12.3},
            Battery = batt[i % 3]
          };
        }
        transaction.Complete();
      }
    }

    [Test]
    public void GroupByPersistentField()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => g.Battery)
          .AsEnumerable()
          .Select(g => new {Key = g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => g.Battery)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByStructureFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => g.Price)
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => g.Price)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByStructureFieldTest2()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => g.Price2)
          .AsEnumerable()
          .Select(g => new { g.Key, Cnt = g.Count() })
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => g.Price2)
            .Select(g => new { g.Key, Cnt = g.Count() });

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key == e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByStructureFieldTest3()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => g.Price2.CurrencyCode)
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => g.Price2.CurrencyCode)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByStructureFieldTest4()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {A = g.Price2.Value, B = g.Price2.Value})
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {A = g.Price2.Value, B = g.Price2.Value})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.A==e.Key.A && i.Key.B==e.Key.B);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GrouoByNonNullableFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(p => p.Name)
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(p => p.Name)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByNullableFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(p => p.Description)
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(p => p.Description)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithNullableFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(p => new {p.Description, p.Comment})
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(p => new {p.Description, p.Comment})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Description==e.Key.Description && i.Key.Comment==e.Key.Comment);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithNonNullableFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(p => new {p.Name, p.CreationDate})
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(p => new {p.Name, p.CreationDate})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Name==e.Key.Name && i.Key.CreationDate==e.Key.CreationDate);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithMixedFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(p => new { p.Name, p.Description })
          .AsEnumerable()
          .Select(g => new { g.Key, Cnt = g.Count() })
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(p => new { p.Name, p.Description })
            .Select(g => new { g.Key, Cnt = g.Count() });

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Name==e.Key.Name && i.Key.Description==e.Key.Description);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByIdTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => g.Battery.Id)
          .AsEnumerable()
          .Select(g => new {g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => g.Battery.Id)
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key==e.Key);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithPersistentFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {Battery = g.Battery})
          .AsEnumerable()
          .Select(
            g => new {
              g.Key,
              Cnt = g.Count()
            })
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {Battery = g.Battery})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Battery==e.Key.Battery);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithPersistentAndNullableFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {Battery = g.Battery, Description = g.Description})
          .AsEnumerable()
          .Select(
            g => new {
              g.Key,
              Cnt = g.Count()
            })
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {Battery = g.Battery, Description = g.Description})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Battery==e.Key.Battery && i.Key.Description==e.Key.Description);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithPersistentAndNonNullableFielsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {Battery = g.Battery, Description = g.Name})
          .AsEnumerable()
          .Select(
            g => new {
              g.Key,
              Cnt = g.Count()
            })
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {Battery = g.Battery, Description = g.Name})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Battery==e.Key.Battery && i.Key.Description==e.Key.Description);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithStructureFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {g.Price})
          .AsEnumerable()
          .Select(g => new {Key = g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {g.Price})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Price==e.Key.Price);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithStructureAndNullableFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {g.Price, g.Description})
          .AsEnumerable()
          .Select(g => new {Key = g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {g.Price, g.Description})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Price==e.Key.Price && i.Key.Description==e.Key.Description);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }

    [Test]
    public void GroupByAnonymousTypeWithStructureAndNonNullableFieldsTest()
    {
      using (var session = Domain.OpenSession())
      using (var logger = new QueryLogger(session))
      using (var transaction = session.OpenTransaction()) {
        var expectedCounts = Query.All<Product>()
          .GroupBy(g => new {g.Price, g.Name})
          .AsEnumerable()
          .Select(g => new {Key = g.Key, Cnt = g.Count()})
          .ToList();

        using (logger.Attach()) {
          var x = Query.All<Product>()
            .GroupBy(g => new {g.Price, g.Name})
            .Select(g => new {g.Key, Cnt = g.Count()});

          foreach (var e in x) {
            var entry = expectedCounts.FirstOrDefault(i => i.Key.Price==e.Key.Price && i.Key.Name==e.Key.Name);
            Assert.That(entry, Is.Not.Null);
            Assert.That(e.Cnt, Is.EqualTo(entry.Cnt));
          }
        }
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.GroupByNullableFieldTestModel
{
  [HierarchyRoot]
  public class Battery : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public double Volts { get; set; }
    [Field]
    public int Amps { get; set; }
    [Field]
    public int Cell { get; set; }
  }

  [HierarchyRoot]
  public class Product : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }

    [Field]
    public string Comment { get; set; }

    [Field(Nullable = false)]
    public Battery Battery { get; set; }

    [Field]
    public Money Price { get; set; }

    [Field]
    public Money2 Price2 { get; set; }
  }

  public class Money : Structure
  {
    [Field]
    public decimal Value { get; set; }

    [Field]
    public int CurrencyCode { get; set; }
  }

  public class Money2 : Structure
  {
    [Field]
    public decimal Value { get; set; }

    [Field]
    public Currency CurrencyCode { get; set; }
  }

  [HierarchyRoot]
  public class Currency : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  public class QueryLogger : IDisposable
  {
    private Session session;
    private bool isAttached;

    public IDisposable Attach()
    {
      if (isAttached)
        new Disposable((a) => Detach());
      session.Events.QueryExecuting += EventsOnQueryExecuting;
      session.Events.DbCommandExecuting += EventsOnDbCommandExecuting;
      return new Disposable((a) => Detach());
    }

    private void EventsOnDbCommandExecuting(object sender, DbCommandEventArgs dbCommandEventArgs)
    {
      LogQueryTextToConsole(dbCommandEventArgs.Command);
    }

    private void EventsOnQueryExecuting(object sender, QueryEventArgs queryEventArgs)
    {
      LogExpressionToConsole(queryEventArgs.Expression);
    }

    public void Detach()
    {
      if (!isAttached)
        return;
      session.Events.QueryExecuting -= EventsOnQueryExecuting;
      session.Events.DbCommandExecuting -= EventsOnDbCommandExecuting;
    }

    public void Dispose()
    {
      Detach();
    }

    private void LogExpressionToConsole(Expression expression)
    {
      Console.WriteLine(expression.ToString());
    }

    private void LogQueryTextToConsole(DbCommand command)
    {
      Console.WriteLine(command.CommandText);
    }

    
    public QueryLogger(Session session)
    {
      this.session = session;
    }
  }
}
