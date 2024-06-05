// Copyright (C) 2019-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.07.12

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.AsyncSession.TestSessionOpentingModel;

namespace Xtensive.Orm.Tests.Storage.AsyncSession.TestSessionOpentingModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.AsyncSession
{
  public class AsyncSessionOpeningTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public async Task Test01()
    {
      var session = await Domain.OpenSessionAsync();
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);

      await TestSession(session);
    }

    [Test]
    public async Task Test02()
    {
      var canellationTokenSource = new CancellationTokenSource();
      var session = await Domain.OpenSessionAsync(canellationTokenSource.Token);
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public async Task Test03()
    {
      var canellationTokenSource = new CancellationTokenSource();
      canellationTokenSource.Cancel();

      Session session = null;
      try {
        session = await Domain.OpenSessionAsync(canellationTokenSource.Token);
      }
      catch (OperationCanceledException) {
        Assert.That(session, Is.Null);
      }
    }

    [Test]
    public async Task Test04()
    {
      var session = await Domain.OpenSessionAsync(SessionType.Default);
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public async Task Test05()
    {
      var session = await Domain.OpenSessionAsync(SessionType.System);
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public async Task Test06()
    {
      var session = await Domain.OpenSessionAsync(SessionType.KeyGenerator);
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public async Task Test07()
    {
      var session = await Domain.OpenSessionAsync(SessionType.Service);
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public void Test08()
    {
      var ctSource = new CancellationTokenSource();
      ctSource.Cancel();

      Session session = null;
      Assert.ThrowsAsync<OperationCanceledException>(async () => session = await Domain.OpenSessionAsync(SessionType.Default, ctSource.Token));
      Assert.That(session, Is.Null);
    }

    [Test]
    public void Test09()
    {
      var ctSource = new CancellationTokenSource();
      ctSource.Cancel();

      Session session = null;
      Assert.ThrowsAsync<OperationCanceledException>(async () => session = await Domain.OpenSessionAsync(SessionType.KeyGenerator, ctSource.Token));
      Assert.That(session, Is.Null);
    }

    [Test]
    public void Test10()
    {
      var ctSource = new CancellationTokenSource();
      ctSource.Cancel();

      Session session = null;
      Assert.ThrowsAsync<OperationCanceledException>(async () => session = await Domain.OpenSessionAsync(SessionType.System, ctSource.Token));
      Assert.That(session, Is.Null);
    }

    [Test]
    public void Test11()
    {
      var ctSource = new CancellationTokenSource();
      ctSource.Cancel();

      Session session = null;
      Assert.ThrowsAsync<OperationCanceledException>(async () => session = await Domain.OpenSessionAsync(SessionType.Service, ctSource.Token));
      Assert.That(session, Is.Null);
    }

    [Test]
    public async Task Test12()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.Default);
      var session = await Domain.OpenSessionAsync(sessionConfiguration);
      Assert.That(session, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public async Task Test13()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.Default | SessionOptions.AutoActivation);
      var session = await Domain.OpenSessionAsync(sessionConfiguration, CancellationToken.None).ConfigureAwait(false);
      Assert.That(session, Is.Not.Null);
      Assert.That(Session.Current, Is.Not.Null);

      await TestSession(session).ConfigureAwait(false);
    }

    [Test]
    public async Task Test14()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.Default);
      var session = await Domain.OpenSessionAsync(sessionConfiguration);
      Assert.That(session, Is.Not.Null);
      await TestSession(session);
    }

    [Test]
    public void Test15()
    {
      var ctSource = new CancellationTokenSource();
      ctSource.Cancel();
      var sessionConfiguration = new SessionConfiguration(SessionOptions.Default);

      Session session = null;
      Assert.ThrowsAsync<TaskCanceledException>(async () => session = await Domain.OpenSessionAsync(sessionConfiguration, ctSource.Token));
      Assert.That(session, Is.Null);
    }

    [Test]
    public async Task Test28()
    {
      var session = await Domain.OpenSessionAsync();
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public async Task Test29()
    {
      var session = await Domain.OpenSessionAsync();
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
    }

    [Test]
    public async Task Test30()
    {
      var session = await Domain.OpenSessionAsync();
      Assert.That(session, Is.Not.Null);
      var handler = session.Handler as SqlSessionHandler;
      Assert.That(handler, Is.Not.Null);
    }

    private async Task TestSession(Session session)
    {
      await Task.Run(() =>
      {
        using (session.Activate())
        using (var transacion = session.OpenTransaction())
        {
          Assert.That(Session.Current, Is.Not.Null);
          Assert.That(Session.Current, Is.EqualTo(session));
          var entity = new TestEntity(session);
          session.SaveChanges();
          session.Query.All<TestEntity>().First();
          entity.Remove();
          session.SaveChanges();
        }
      });
    }
  }
}
