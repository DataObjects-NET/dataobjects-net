using System.Collections.Generic;
using NUnit.Framework;
using TestCommon.Model;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;

namespace TestCommon
{
  [TestFixture]
  public abstract class CommonModelTest
  {
    private List<Session> notDisposed;

    protected Domain Domain { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
      var config = BuildConfiguration();
      Domain = BuildDomain(config);
      notDisposed = new List<Session>();
      Domain.SessionOpen += (sender, args) => {
        notDisposed.Add(args.Session);
        args.Session.Events.Disposing += (o, eventArgs) => {
          lock (notDisposed) {
            notDisposed.Remove(args.Session);
          }
        };
      };
      PopulateData();
    }

    [TearDown]
    public virtual void TearDown()
    {
      if (notDisposed!=null)
        Assert.That(notDisposed, Is.Empty);
      Assert.That(SessionScope.CurrentSession, Is.Null);
      Domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (Bar).Assembly);
      return configuration;
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    protected virtual void PopulateData()
    {
    }
  }
}