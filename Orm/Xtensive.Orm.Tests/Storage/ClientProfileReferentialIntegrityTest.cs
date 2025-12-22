// Copyright (C) 2015-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2015.02.12

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ReferentialIntegrityModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class ClientProfileReferentialIntegrityTest : AutoBuildTest
  {
    private readonly SessionConfiguration clientProfileConfiguration = new SessionConfiguration(SessionOptions.ClientProfile);

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
          A a = new A();
          a.B = new B();
          a.C = new C();
          Session.Current.SaveChanges();
        Assert.That(session.Query.All<A>().Count(), Is.EqualTo(1));
        Assert.That(session.Query.All<B>().Count(), Is.EqualTo(1));
        Assert.That(session.Query.All<C>().Count(), Is.EqualTo(1));

          a.B.Remove();
        Assert.That(a.B, Is.EqualTo(null));
          AssertEx.Throws<ReferentialIntegrityException>(a.C.Remove);
          a.Remove();
          Session.Current.SaveChanges();
        Assert.That(session.Query.All<A>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<B>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<C>().Count(), Is.EqualTo(0));

          Master m = new Master();
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
          m.OneToMany.Add(new Slave());
        Assert.That(m.OneToMany.Count, Is.EqualTo(3));
        Assert.That(ReferenceFinder.GetReferencesTo(m, m.TypeInfo.Fields["OneToMany"].Associations.Last().Reversed).Count(), Is.EqualTo(3));
          ((IEnumerable<Slave>)m.OneToMany).First().Remove();
        Assert.That(m.OneToMany.Count, Is.EqualTo(2));

          m.ZeroToMany.Add(new Slave());
          m.ZeroToMany.Add(new Slave());
          m.ZeroToMany.Add(new Slave());
        Assert.That(m.ZeroToMany.Count, Is.EqualTo(3));
          ((IEnumerable<Slave>)m.ZeroToMany).First().Remove();
        Assert.That(m.ZeroToMany.Count, Is.EqualTo(2));

          m.ManyToMany.Add(new Slave());
          m.ManyToMany.Add(new Slave());
          m.ManyToMany.Add(new Slave());
        Assert.That(m.ManyToMany.Count, Is.EqualTo(3));
          ((IEnumerable<Slave>)m.ManyToMany).First().Remove();
        Assert.That(m.ManyToMany.Count, Is.EqualTo(2));

          session.SaveChanges();
      }
    }

    [Test]
    public void RemoveWithException()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        A a;
        C c;
        using (var t = session.OpenTransaction()) {
          a = new A();
          c = new C();
          a.C = c;
          TestLog.Debug(a.Key.ToString());
          TestLog.Debug(c.Key.ToString());
          a.Remove();
          Session.Current.SaveChanges();
        }
      }
    }

    [Test]
    public void DeletedEntityAddToReference()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        A a = new A();
        B b = new B();
        b.Remove();
        AssertEx.ThrowsInvalidOperationException(() => a.B = b);
      }
    }

    [Test]
    public void DeletedEntityChangeFields()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
          A a = new A();
          a.Remove();
          AssertEx.ThrowsInvalidOperationException(() => a.Name = "newName");
      }
    }

    [Test]
    public void DeepCascadeRemoveTest()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        var c = new Container();
        c.Package1 = new Package();
        c.Package2 = new Package();
        var item = new PackageItem();
        c.Package1.Items.Add(item);
        c.Package2.Items.Add(item);

        c.Remove();

        Assert.That(session.Query.All<Container>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<Package>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<PackageItem>().Count(), Is.EqualTo(0));
      }
    }

    [Test]
    public void CascadeRemoveTest()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        var c = new Container();
        c.Package1 = new Package();
        var item = new PackageItem();
        c.Package1.Items.Add(item);

        c.Remove();

        Assert.That(session.Query.All<Container>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<Package>().Count(), Is.EqualTo(0));
        Assert.That(session.Query.All<PackageItem>().Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void RemovePerformanceTest()
    {
      const int containersCount = 100;
      const int itemCount = 100;
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < containersCount; i++) {
            var c = new Container { Package1 = new Package(), Package2 = new Package() };
            for (int j = 0; j < itemCount; j++) {
              c.Package1.Items.Add(new PackageItem());
              c.Package2.Items.Add(new PackageItem());
            }
          }
          session.SaveChanges();
          t.Complete();
        }

        using (var t = session.OpenTransaction()) {
          const int operationCount = containersCount * 3 + containersCount * itemCount * 2;
          using (new Measurement("Remove...", operationCount)) {
            var containers = session.Query.All<Container>();
            foreach (var container in containers)
              container.Remove();
            session.SaveChanges();
          }

          Assert.That(session.Query.All<Container>().Count(), Is.EqualTo(0));
          Assert.That(session.Query.All<Package>().Count(), Is.EqualTo(0));
          Assert.That(session.Query.All<PackageItem>().Count(), Is.EqualTo(0));
          t.Complete();
        }
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void SessionRemovePerformanceTest()
    {
      const int containersCount = 100;
      const int itemCount = 100;
      using (var session = Domain.OpenSession(clientProfileConfiguration))
      using (session.Activate()) {
        for (int i = 0; i < containersCount; i++) {
          var c = new Container { Package1 = new Package(), Package2 = new Package() };
          for (int j = 0; j < itemCount; j++) {
            c.Package1.Items.Add(new PackageItem());
            c.Package2.Items.Add(new PackageItem());
          }
        }
        session.SaveChanges();

        using (var t = session.OpenTransaction()) {
          const int operationCount = containersCount * 3 + containersCount * itemCount * 2;
          using (new Measurement("Remove...", operationCount)) {
            session.Remove(session.Query.All<Container>());
            session.SaveChanges();
          }

          Assert.That(session.Query.All<Container>().Count(), Is.EqualTo(0));
          Assert.That(session.Query.All<Package>().Count(), Is.EqualTo(0));
          Assert.That(session.Query.All<PackageItem>().Count(), Is.EqualTo(0));
          t.Complete();
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.ReferentialIntegrityModel");
      return config;
    }
  }
}
