// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.04.12

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Issues.Issue0631_DisconnectedStateBugs
{
  [TestFixture]
  public class DisconnectedTest : AutoBuildTest
  {
    private Guid _guid = new Guid("A4CB133D-257C-4FA9-B827-8915627D4F9F");
    private Guid _guid2 = new Guid("4CE5B488-33B7-48CB-A42B-B92463B0DCE3");
    private Guid _guid3 = new Guid("4027ECA5-5476-467B-9457-3ECD6F08B165");
    private Guid _guid4 = new Guid("A1631DEC-A06F-4A48-831B-7527DEDF669C");

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (EntityBase).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var s = Session.Open(domain))
      {
        using (var ts = Transaction.Open())
        {
          var fe = new FilterEntity(Guid.NewGuid()) { Date = DateTime.Now, Integer = 1 };
          fe.ItemGroup.Add(new AnotherEntity(_guid) { Name = "a" });
          fe.ItemGroup.Add(new AnotherEntity(_guid2) { Name = "b" });
          fe.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
          fe.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));

          var ff = new FilterEntity(Guid.NewGuid()) { Date = DateTime.Now, Integer = 2 };
          ff.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
          ff.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
          ff.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
          ff.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));

          ts.Complete();
        }
      }
      return domain;
    }


    [Test]
    public void BFDsTest()
    {
      var state = new DisconnectedState() { MergeMode = MergeMode.PreferTarget }; 
      // MergeMode.PreferSource makes no sense

      // Добавить строки С и Д
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            FilterEntity doc;
            using (state.Connect())
            {
              doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Single();
              doc.ItemGroup.Add(new AnotherEntity(_guid4) { Name = "c" });
              doc.ItemGroup.Add(new AnotherEntity(_guid3) { Name = "d" });
            }

            transactionScope.Complete();
          }
        }
      }

      // Удалить строки А и Д
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Single();
              //Тут падает ReferentialIntegrity o_O
              doc.ItemGroup.ToList().Single(a => a.Id == _guid3).Remove();
              // Тут падает SequenceContainsNoElements
              doc.ItemGroup.Single(a => a.Id == _guid3).Remove();
              Query.All<AnotherEntity>().Single(a => a.Id == _guid3).Remove();
              Query.All<AnotherEntity>().Single(a => a.Id == _guid).Remove();
            }

            transactionScope.Complete();
          }
        }
      }

      // Изменить строки Б и С и откатить транзакцию
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();

              Query.All<AnotherEntity>().Single(a => a.Id == _guid4).Name = "c2";
              Query.All<AnotherEntity>().Single(a => a.Id == _guid2).Name = "b2";
            }
          }
        }
      }

      // Проверка
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();

              Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid4).Name, "c");
              Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid2).Name, "b");
            }
          }
        }
      }

      // Изменить строки Б и С
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();

              Query.All<AnotherEntity>().Single(a => a.Id == _guid4).Name = "c2";
              Query.All<AnotherEntity>().Single(a => a.Id == _guid2).Name = "b2";
            }

            transactionScope.Complete();
          }
        }
      }

      // Проверка
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();

              Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid4).Name, "c2");
              Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid2).Name, "b2");
            }
          }
        }
      }

      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          state.ApplyChanges();
        }
      }

      // Проверка записи
      using (var session = Session.Open(Domain))
      {
        Assert.IsNull(Query.All<AnotherEntity>().SingleOrDefault(a => a.Id == _guid3));
        Assert.IsNull(Query.All<AnotherEntity>().SingleOrDefault(a => a.Id == _guid));
        Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid4).Name, "c2");
        Assert.AreEqual(Query.All<AnotherEntity>().Single(a => a.Id == _guid2).Name, "b2");
      }
    }

    [Test]
    public void PartialSelectTest()
    {
      var state = new DisconnectedState();
      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              var qwe = Query.All<FilterEntity>().Select(u => new { u.Date }).ToList();
              //Exception here
              var aaa = Query.All<FilterEntity>().ToList();
            }

            transactionScope.Complete();
          }
        }
      }
    }

    [Test]
    public void SameEntitySelect()
    {
      var state = new DisconnectedState();

      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              //SomeMethod();
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();
              //var qwe = Query.All<User>().Select(u => new { u.Created }).ToList();


              var subs = doc.ItemGroup.Where(g => true).ToList();

              foreach (var sub in subs)
              {
                sub.Name = "1111";
              }

              doc.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
              doc.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));
              doc.ItemGroup.Add(new AnotherEntity(Guid.NewGuid()));

              //var uuu = Query.All<User>().Select(u => new { u.Created }).OrderBy(a => a.Created).First();
            }

            var fe = new FilterEntity(Guid.NewGuid()) { Date = DateTime.Now, Integer = 123 };
            //var ss = Query.All<FilterEntity>().Where(f => f.NullableGuid.HasValue).Single();
            fe.Integer = 2222;

            transactionScope.Complete();
          }
        }
      }

      using (var session = Session.Open(Domain))
      {
        using (state.Attach(session))
        {
          using (var transactionScope = Transaction.Open())
          {
            using (state.Connect())
            {
              // Exception here
              var doc = Query.All<FilterEntity>().Where(f => f.Integer == 1).Prefetch(s => s.ItemGroup).Single();

              var subs = doc.ItemGroup.Where(g => true).ToList();

              foreach (var sub in subs)
              {
                sub.Name = "22222";
              }
            }

            transactionScope.Complete();
          }
        }

        state.ApplyChanges();
      }
    }
  }
}
