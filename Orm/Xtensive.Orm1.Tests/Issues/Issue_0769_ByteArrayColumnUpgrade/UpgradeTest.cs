// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Upgrade.Model;
using Person = Xtensive.Orm.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade.Model.Version1.Person;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (var session = domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var person = new Person {
            Name = "Person",
            Bytes = new byte[] {1, 2, 3}
          };

          var bytesColumn = GetColumnInfo(Upgrader.TargetStorageModel, person.TypeInfo, "Bytes");
          var driver = TestSqlDriver.Create(domain.Configuration.ConnectionInfo);
          var expected = driver.TypeMappings[typeof (byte[])].MapType().Length;
          Assert.AreEqual(expected, bytesColumn.Type.Length);

          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var person = session.Query.All<Model.Version2.Person>().Single();
          AssertEx.HasSameElements("Person", person.Name);
          AssertEx.HasSameElements(new byte[] {1, 2, 3}, person.Bytes);

          var bytesColumn = GetColumnInfo(Upgrader.TargetStorageModel, person.TypeInfo, "Bytes");
          Assert.AreEqual(null, bytesColumn.Type.Length);
        }
      }
    }

    private StorageColumnInfo GetColumnInfo(StorageModel model, TypeInfo type, string fieldName)
    {
      var resolver = Upgrader.MappingResolver;
      var table = model.Tables[resolver.GetNodeName(type)];
      var field = type.Fields[fieldName];
      return table.Columns[field.MappingName];
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain!=null)
        domain.DisposeSafely();

      string ns = typeof (Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof (Upgrader));

      using (Upgrader.Enable(version))
        domain = Domain.Build(configuration);
    }
  }
}