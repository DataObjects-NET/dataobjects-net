// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Issues
{
  using IssuesIssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterfaceModels;

  [TestFixture]
  public class IssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterface
  {
    [Test]
    [TestCase(typeof (RecycledWithInterfaceEntity), true)]
    [TestCase(typeof (NonRecycledWithInterfaceEntity), false)]
    [TestCase(typeof (RecycledEntity), true)]
    public void MainTest(Type type, bool isNull)
    {
      using (var domain = Domain.Build(BuildConfiguration(type, typeof (ITestInterface<>)))) {
        TypeInfo typeInfo;
        domain.Model.Types.TryGetValue(type, out typeInfo);
        var expectedTableName = (typeInfo==null ? null : typeInfo.MappingName) ?? type.Name;

        using (var session = domain.OpenSession())
        using (var conn = ((SqlSessionHandler) session.Handler).Connection.UnderlyingConnection)
        using (var cmd = conn.CreateCommand()) {
          cmd.CommandText = "SELECT OBJECT_ID(@table, N'U');";
          var parameter = cmd.CreateParameter();
          parameter.ParameterName = "table";
          parameter.Value = expectedTableName;
          cmd.Parameters.Add(parameter);

          var objectId = cmd.ExecuteScalar();

          var expression = isNull ? Is.EqualTo(DBNull.Value) : Is.Not.EqualTo(DBNull.Value);
          Assert.That(objectId, expression);
        }
      }
    }

    private DomainConfiguration BuildConfiguration(params Type[] types)
    {
      var config = DomainConfigurationFactory.Create();
      types.ForEach(x => config.Types.Register(x));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}

namespace Xtensive.Orm.Tests.IssuesIssueJira0752_RecycledAttributeNotWorkingForPersistentGenericInterfaceModels
{
  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  interface ITestInterface<T> : IEntity
  {
  }

  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class NonRecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }
}