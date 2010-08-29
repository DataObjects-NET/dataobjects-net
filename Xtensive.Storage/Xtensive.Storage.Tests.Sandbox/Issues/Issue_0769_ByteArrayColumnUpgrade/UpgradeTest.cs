// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade.Model.Version2;
using M1 = Xtensive.Storage.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade.Model.Version1;
using M2 = Xtensive.Storage.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade.Model.Version2;
using NUnit.Framework;
using TypeInfo = Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Tests.Issues.Issue_0769_ByteArrayColumnUpgrade
{
  [TestFixture]
  public class UpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      using (Session.Open(domain)) {
        using (var tx = Transaction.Open()) {
          var person = new M1.Person() {
            Name = "Person",
            Bytes = new byte[] {1, 2, 3}
          };

          var bytesColumn = GetColumnInfo(domain.Schema, person.TypeInfo, "Bytes");
          Assert.AreEqual(4000, bytesColumn.Type.Length);

          tx.Complete();
        }
      }
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          var person = Query.All<M2.Person>().Single();
          AssertEx.AreEqual("Person", person.Name);
          AssertEx.AreEqual(new byte[] {1, 2, 3}, person.Bytes);

          var bytesColumn = GetColumnInfo(domain.Schema, person.TypeInfo, "Bytes");
          Assert.AreEqual(null, bytesColumn.Type.Length);
        }
      }
    }

    private ColumnInfo GetColumnInfo(StorageInfo schema, TypeInfo type, string fieldName)
    {
      var table = schema.Tables[type.MappingName];
      var field = type.Fields[fieldName];
      return table.Columns[field.MappingName];
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      string ns = typeof(M1.Person).Namespace;
      string nsPrefix = ns.Substring(0, ns.Length - 1);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nsPrefix + version);
      configuration.Types.Register(typeof(Upgrader));

      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
  }
}