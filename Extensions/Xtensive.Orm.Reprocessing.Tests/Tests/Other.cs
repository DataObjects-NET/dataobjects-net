using System.Linq;
using System.Transactions;
using NUnit.Framework;
using TestCommon.Model;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Reprocessing.Tests
{
  public class Other : AutoBuildTest
  {
    private class TestExecuteStrategy : HandleUniqueConstraintViolationStrategy
    {
      #region Non-public methods

      protected override bool OnError(ExecuteErrorEventArgs context) => context.Attempt < 2;

      #endregion
    }

    [Test]
    public void ExecuteStrategy()
    {
      var i = 0;
      try {
        var config = Domain.GetReprocessingConfiguration();
        config.DefaultExecuteStrategy = typeof (TestExecuteStrategy);
        Domain.Execute(
          session => {
            _ = new Foo(session) {Name = "test"};
            i++;
            if (i < 5) {
              _ = new Foo(session) {Name = "test"};
            }
          });
      }
      catch {
        Assert.That(i, Is.EqualTo(2));
      }
    }

    [Test]
    public void NestedNewSession()
    {
      Domain.Execute(
        session => {
          using (Session.Deactivate()) {
            Domain.Execute(session2 => Assert.That(session2, Is.Not.SameAs(session)));
          }
        });
    }

    [Test]
    public void NestedSessionReuse()
    {
      Domain.Execute(session1 =>
        Domain.WithExternalSession(session1)
          .Execute(session2 => Assert.That(session1, Is.SameAs(session2))));
    }

    [Test]
    public void Test()
    {
      Domain.Execute(session => {
        _ = new Foo(session);
      });
      Domain.Execute(session => {
        _ = session.Query.All<Foo>().ToArray();
        Domain.WithIsolationLevel(IsolationLevel.Serializable).Execute(session2 => {
          _ = session2.Query.All<Foo>().ToArray();
        });
      });
    }

    [Test]
    public void ParentIsDisconnectedState()
    {
      Domain.Execute(session => {
        _ = new Bar(session);
      });
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using(session.Activate()) {
        var bar = session.Query.All<Bar>().FirstOrDefault();
        bar = new Bar(session);
        session.SaveChanges();
        Domain.WithExternalSession(session).Execute(session1 => {
          bar = session1.Query.All<Bar>().FirstOrDefault();
          bar = new Bar(session1);
        });
        session.SaveChanges();
      }
      Domain.Execute(session => Assert.That(session.Query.All<Bar>().Count(), Is.EqualTo(3)));
    }
  }
}