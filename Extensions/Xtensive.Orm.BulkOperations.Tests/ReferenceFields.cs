using System;
using System.Linq;
using NUnit.Framework;
using TestCommon.Model;

namespace Xtensive.Orm.BulkOperations.Tests
{
  internal class ReferenceFields : AutoBuildTest
  {
    [Test]
    public void InsertClientSideReferenceField()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar(session);
          var foo = new Foo(session);

          Key key = session.Query.Insert(() => new Foo(session) {Bar = bar, Name = "test"});
          var foo2 = session.Query.Single<Foo>(key);
          Assert.That(foo2.Bar, Is.EqualTo(bar));

          key = session.Query.Insert(() => new Foo(session) {Bar = null, Name = "test1"});
          foo2 = session.Query.Single<Foo>(key);
          Assert.That(foo2.Bar, Is.Null);

          trx.Complete();
        }
      }
    }

    [Test]
    public void InsertClientSideReferenceField2()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar2(session, DateTime.Now, Guid.NewGuid());
          var foo = new Foo2(session, 1, "1");

          Key key = session.Query.Insert(() => new Foo2(null, 0, null) {Bar = bar, Id = 0, Id2 = "test", Name = "test"});
          var foo2 = session.Query.Single<Foo2>(key);
          Assert.That(bar, Is.EqualTo(foo2.Bar));

          key = session.Query.Insert(() => new Foo2(null, 0, null) {Bar = null, Id = 0, Id2 = "test1", Name = "test1"});
          foo2 = session.Query.Single<Foo2>(key);
          Assert.That(foo2.Bar, Is.Null);

          trx.Complete();
        }
      }
    }

    [Test]
    public void UpdateServerSideReferenceField()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar(session);
          var foo = new Foo(session);
          var bara = new Bar(session);
          session.SaveChanges();
          Assert.That(foo.Bar, Is.Null);
          int one = 1;

          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.Single(Key.Create(Domain, typeof (Bar), 1))).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.Single<Bar>(Key.Create<Bar>(Domain, 1))).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1), () => session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.Single<Bar>(1)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(
                a => a.Bar, a => session.Query.SingleOrDefault(Key.Create(Domain, typeof (Bar), 1))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(
                a => a.Bar, a => session.Query.SingleOrDefault<Bar>(Key.Create<Bar>(Domain, 1))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () => session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.SingleOrDefault<Bar>(1)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().Where(b => b.Id==one).First()).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () => session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().First(b => b.Id==1)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(
                a => a.Bar, a => session.Query.All<Bar>().Where(b => b.Id==1).FirstOrDefault()).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().FirstOrDefault(b => b.Id==1)).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().Where(b => b.Id==1).Single()).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () => session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().Single(b => b.Id==1)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(
                a => a.Bar, a => session.Query.All<Bar>().Where(b => b.Id==1).SingleOrDefault()).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().SingleOrDefault(b => b.Id==1)).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));
          trx.Complete();
        }
      }
    }

    [Test]
    public void InsertServerSideReferenceField()
    {
      using (Session session = Domain.OpenSession())
      {
        using (TransactionScope trx = session.OpenTransaction())
        {
          var bar = new Bar(session);
          var foo1 = new Foo(session);
          session.SaveChanges();
          Assert.That(foo1.Bar, Is.Null);
          int one = 1;
          Key key = null;

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = (Bar)session.Query.Single(Key.Create(Domain, typeof(Bar), 1)), Name = "test" })
          );
          var foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.Single<Bar>(Key.Create<Bar>(Domain, 1)), Name = "test1" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.Single<Bar>(1), Name = "test2" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = (Bar)session.Query.SingleOrDefault(Key.Create(Domain, typeof(Bar), 1)), Name = "test3" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = null, Name = "test4" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(foo.Bar, Is.Null);

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.SingleOrDefault<Bar>(1), Name = "test5" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().Where(b => b.Id == one).First(), Name = "test6" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().First(b => b.Id == 1), Name = "test7" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().Where(b => b.Id == 1).FirstOrDefault(), Name = "test8" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().FirstOrDefault(b => b.Id == 1), Name = "test9" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().Where(b => b.Id == 1).Single(), Name = "test10" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().Single(b => b.Id == 1), Name = "test11" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().Where(b => b.Id == 1).SingleOrDefault(), Name = "test12" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.AssertCommandCount(
            Is.EqualTo(1),
            () => key = session.Query.Insert(() => new Foo(session) { Bar = session.Query.All<Bar>().SingleOrDefault(b => b.Id == 1), Name = "test13" })
          );
          foo = session.Query.Single<Foo>(key);
          Assert.That(bar, Is.EqualTo(foo.Bar));

          trx.Complete();
        }
      }
    }

    [Test]
    public void UpdateServerSideReferenceField2()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          DateTime date = DateTime.Now;
          Guid id = Guid.NewGuid();
          var bar = new Bar2(session, date, id);
          var foo = new Foo2(session, 1, "1");
          var bar2 = new Bar2(session, DateTime.Now, Guid.NewGuid());
          session.SaveChanges();
          Assert.That(foo.Bar, Is.Null);

          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.Single(Key.Create(Domain, typeof (Bar2), date, id))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.Single<Bar2>(Key.Create<Bar2>(Domain, date, id))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () => session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.Single<Bar2>(date, id)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.SingleOrDefault(Key.Create(Domain, typeof (Bar2), date, id))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.SingleOrDefault<Bar2>(Key.Create<Bar2>(Domain, date, id))).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () => session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.SingleOrDefault<Bar2>(date, id)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().Where(b => b.Id2==id).First()).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().First(b => b.Id2==id)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.All<Bar2>().Where(b => b.Id2==id).FirstOrDefault()).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().FirstOrDefault(b => b.Id2==id)).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().Where(b => b.Id2==id).Single()).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().Single(b => b.Id2==id)).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(
                a => a.Bar, a => session.Query.All<Bar2>().Where(b => b.Id2==id).SingleOrDefault()).Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));

          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.AssertCommandCount(
            Is.EqualTo(1),
            () =>
              session.Query.All<Foo2>().Set(a => a.Bar, a => session.Query.All<Bar2>().SingleOrDefault(b => b.Id2==id)).
                Update());
          Assert.That(bar, Is.EqualTo(foo.Bar));
          trx.Complete();
        }
      }
    }

    [Test]
    public void UpdateClientSideReferenceField()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar(session);
          var foo = new Foo(session);
          var bar2 = new Bar(session);
          session.Query.All<Foo>().Set(a => a.Bar, bar).Update();
          Assert.That(bar, Is.EqualTo(foo.Bar));
          session.Query.All<Foo>().Set(a => a.Bar, (Bar) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.Query.All<Foo>().Update(a => new Foo(session) {Bar = bar});
          Assert.That(bar, Is.EqualTo(foo.Bar));
          session.Query.All<Foo>().Update(a => new Foo(session) {Bar = null});
          Assert.That(foo.Bar, Is.Null);
          trx.Complete();
        }
      }
    }

    [Test]
    public void UpdateClientSideReferenceField2()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar2(session, DateTime.Now, Guid.NewGuid());
          var foo = new Foo2(session, 1, "1");
          var bar2 = new Bar2(session, DateTime.Now, Guid.NewGuid());
          session.Query.All<Foo2>().Set(a => a.Bar, bar).Update();
          Assert.That(bar, Is.EqualTo(foo.Bar));
          session.Query.All<Foo2>().Set(a => a.Bar, (Bar2) null).Update();
          Assert.That(foo.Bar, Is.Null);
          session.Query.All<Foo2>().Update(a => new Foo2(null, 0, null) {Bar = bar});
          Assert.That(bar, Is.EqualTo(foo.Bar));
          session.Query.All<Foo2>().Update(a => new Foo2(null, 0, null) {Bar = null});
          Assert.That(foo.Bar, Is.Null);
          trx.Complete();
        }
      }
    }
  }
}