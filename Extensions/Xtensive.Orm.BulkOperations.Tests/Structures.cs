using System.Linq;
using NUnit.Framework;
using TestCommon.Model;

namespace Xtensive.Orm.BulkOperations.Tests
{
  internal class Structures : AutoBuildTest
  {
    [Test]
    public void StructuresSet()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar(session) {Count = 5};

          session.Query.All<Bar>().Set(a => a.Rectangle, new Rectangle(session) {BorderWidth = 1}).Update();
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(1));

          session.Query.All<Bar>().Set(a => a.Rectangle, new Rectangle(session)).Update();
          Assert.That(bar.Rectangle.BorderWidth, Is.Null);

          session.Query.All<Bar>().Set(
            a => a.Rectangle, new Rectangle(session) {BorderWidth = 2, First = new Point(session) {X = 3, Y = 4}}).Update(
            );
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(2));
          Assert.That(bar.Rectangle.First.X, Is.EqualTo(3));
          Assert.That(bar.Rectangle.First.Y, Is.EqualTo(4));
          Assert.That(bar.Rectangle.Second.X, Is.Null);
          bar.Rectangle = new Rectangle(session);

          session.Query.All<Bar>().Set(a => a.Rectangle.First.X, 1).Update();
          Assert.That(bar.Rectangle.First.X, Is.EqualTo(1));
          Assert.That(bar.Rectangle.Second.X, Is.Null);
          bar.Rectangle = new Rectangle(session);

          /*var bar2 = new Bar(session);
            session.SaveChanges();
          session.AssertCommandCount(Is.EqualTo(1), () => {
            session.Query.All<Bar>().Where(a => a.Id==bar2.Id).Set(
              a => a.Rectangle,
              a => session.Query.All<Bar>().Where(b => a.Id==bar.Id).Select(b => b.Rectangle).First()).
              Update();
          });
            Assert.That( bar2.Rectangle.First.X, Is.EqualTo(1));
            Assert.That(bar2.Rectangle.Second.X, Is.Null);
            bar2.Remove();*/

          session.Query.All<Bar>().Set(a => a.Rectangle.BorderWidth, a => a.Count * 2).Update();
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(10));

          bar.Rectangle = new Rectangle(session) {First = new Point(session) {X = 1, Y = 2}, Second = new Point(session) {X = 3, Y = 4}};
          session.Query.All<Bar>().Set(a => a.Rectangle.BorderWidth, 1).Set(
            a => a.Rectangle.First, a => new Point(session) {X = 2}).Update();
          Assert.That(
            bar.Rectangle,
            Is.EqualTo(
              new Rectangle(session) {
                BorderWidth = 1,
                First = new Point(session) {X = 2, Y = 2},
                Second = new Point(session) {X = 3, Y = 4}
              }));
          bar.Rectangle = new Rectangle(session);

          bar.Rectangle = new Rectangle(session) {First = new Point(session) {X = 1, Y = 2}, Second = new Point(session) {X = 3, Y = 4}};
          session.Query.All<Bar>().Set(a => a.Rectangle.BorderWidth, 1).Set(
            a => a.Rectangle.First, new Point(session) {X = 2}).Update();
          Assert.That(
            bar.Rectangle,
            Is.EqualTo(
              new Rectangle(session) {
                BorderWidth = 1,
                First = new Point(session) {X = 2, Y = null},
                Second = new Point(session) {X = 3, Y = 4}
              }));
          bar.Rectangle = new Rectangle(session);
          trx.Complete();
        }
      }
    }

    [Test]
    public void StructuresUpdate()
    {
      using (Session session = Domain.OpenSession()) {
        using (TransactionScope trx = session.OpenTransaction()) {
          var bar = new Bar(session) {Count = 5};

          session.Query.All<Bar>().Update(
            a => new Bar(null) {Rectangle = new Rectangle(session) {BorderWidth = 1}});
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(1));

          session.Query.All<Bar>().Update(a => new Bar(null) {Rectangle = new Rectangle(session)});
          Assert.That(bar.Rectangle.BorderWidth, Is.Null);

          session.Query.All<Bar>().Update(
            a =>
              new Bar(null) {Rectangle = new Rectangle(session) {BorderWidth = 2, First = new Point(session) {X = 3, Y = 4}}});
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(2));
          Assert.That(bar.Rectangle.First.X, Is.EqualTo(3));
          Assert.That(bar.Rectangle.First.Y, Is.EqualTo(4));
          Assert.That(bar.Rectangle.Second.X, Is.Null);
          bar.Rectangle = new Rectangle(session);

          session.Query.All<Bar>().Update(
            a => new Bar(null) {Rectangle = new Rectangle(session) {BorderWidth = a.Count * 2}});
          Assert.That(bar.Rectangle.BorderWidth, Is.EqualTo(10));
          bar.Rectangle = new Rectangle(session);

          bar.Rectangle = new Rectangle(session) {First = new Point(session) {X = 1, Y = 2}, Second = new Point(session) {X = 3, Y = 4}};
          session.Query.All<Bar>().Update(
            a => new Bar(null) {Rectangle = new Rectangle(session) {BorderWidth = 1, First = new Point(session) {X = 2}}});
          Assert.That(
            bar.Rectangle,
            Is.EqualTo(
              new Rectangle(session) {
                BorderWidth = 1,
                First = new Point(session) {X = 2, Y = 2},
                Second = new Point(session) {X = 3, Y = 4}
              }));
          bar.Rectangle = new Rectangle(session);

          var rectangle = new Rectangle(session) {BorderWidth = 1, First = new Point(session) {X = 2}};
          bar.Rectangle = new Rectangle(session) {First = new Point(session) {X = 1, Y = 2}, Second = new Point(session) {X = 3, Y = 4}};
          session.Query.All<Bar>().Update(a => new Bar(null) {Rectangle = rectangle});
          Assert.That(bar.Rectangle, Is.EqualTo(rectangle));
          bar.Rectangle = new Rectangle(session);
          trx.Complete();
        }
      }
    }
  }
}