// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Model1 = Xtensive.Orm.Tests.Upgrade.ExtractSuperClassTestModel.Model1;
using Model2 = Xtensive.Orm.Tests.Upgrade.ExtractSuperClassTestModel.Model2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace ExtractSuperClassTestModel
  {
    namespace Model1
    {
      [HierarchyRoot]
      public class Client : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public DateTime RegistrationDate { get; set; }
      }

      public class RefStruct : Structure
      {
        [Field]
        public Client Client { get; set; }
      }

      [HierarchyRoot]
      public class ClientRef : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public RefStruct Ref { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion()
        {
          return "1";
        }
      }
    }

    namespace Model2
    {
      [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
      public class Client : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      public class Contractor : Client
      {
        [Field]
        public DateTime RegistrationDate { get; set; }
      }

      public class RefStruct : Structure
      {
        [Field]
        public Client Client { get; set; }
      }

      [HierarchyRoot]
      public class ClientRef : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public RefStruct Ref { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion()
        {
          return "2";
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          var oldClientType = typeof (Model1.Client).FullName;
          var oldClientRefType = typeof (Model1.ClientRef).FullName;

          hints.Add(RenameTypeHint.Create<Contractor>(oldClientType));
          hints.Add(RenameTypeHint.Create<ClientRef>(oldClientRefType));
        }

        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }
      }
    }
  }

  [TestFixture]
  public class ExtractSuperClassTest
  {
    [Test]
    public void UpgradeTest()
    {
      using (var domain1 = BuildDomain(typeof(Model1.Client), DomainUpgradeMode.Recreate))
      using (var session = domain1.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var client = new Model1.Client { Name = "TheClient", RegistrationDate = DateTime.Today };
        var clientRef = new Model1.ClientRef { Ref = { Client = client } };
        tx.Complete();
      }

      using (var domain2 = BuildDomain(typeof(Model2.Client), DomainUpgradeMode.PerformSafely))
      using (var session = domain2.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var client = Query.All<Model2.Contractor>().Single();
        Assert.That(client.Name, Is.EqualTo("TheClient"));
        Assert.That(client.RegistrationDate, Is.EqualTo(DateTime.Today));

        var clientRef = Query.All<Model2.ClientRef>().Single();
        Assert.That(clientRef.Ref.Client, Is.EqualTo(client));
      }
    }

    [Test]
    public async Task UpgradeAsyncTest()
    {
      using (var domain1 = BuildDomain(typeof(Model1.Client), DomainUpgradeMode.Recreate))
      using (var session = domain1.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var client = new Model1.Client { Name = "TheClient", RegistrationDate = DateTime.Today };
        var clientRef = new Model1.ClientRef { Ref = { Client = client } };
        tx.Complete();
      }

      using (var domain2 = await BuildDomainAsync(typeof(Model2.Client), DomainUpgradeMode.PerformSafely))
      using (var session = domain2.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var client = Query.All<Model2.Contractor>().Single();
        Assert.That(client.Name, Is.EqualTo("TheClient"));
        Assert.That(client.RegistrationDate, Is.EqualTo(DateTime.Today));

        var clientRef = Query.All<Model2.ClientRef>().Single();
        Assert.That(clientRef.Ref.Client, Is.EqualTo(client));
      }
    }

    private Domain BuildDomain(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = BuildDomainConfiguration(sampleType, mode);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = BuildDomainConfiguration(sampleType, mode);
      return Domain.BuildAsync(configuration);
    }

    private DomainConfiguration BuildDomainConfiguration(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}