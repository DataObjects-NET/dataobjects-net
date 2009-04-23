// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model.Conversion;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests.Indexing.Model;

namespace Xtensive.Storage.Tests.Indexing
{
  [TestFixture]
  public class ConvertDomainModelTest
  {
    protected virtual StorageInfo Schema { get { return DomainSchema; } }

    protected StorageInfo DomainSchema { get; set; }

    protected StorageInfo StorageSchema { get; set; }

    protected Domain BuildDomain(string protocol)
    {
      var configuration = DomainConfigurationFactory.Create(protocol);
      configuration.Types.Register(Assembly.GetExecutingAssembly(), typeof (A).Namespace);

      var domain = Domain.Build(configuration);
      var domainHandler = domain.Handlers.DomainHandler;
      using (var connection = (SqlConnection) ((DomainHandler) domainHandler).ConnectionProvider
        .CreateConnection(configuration.ConnectionInfo.ToString())) {
        var server = connection.Driver.ServerInfo;
        var sqlModel = new SqlModelProvider(connection).Build();
        var domainModel = domain.Model;

        StorageSchema = new SqlModelConverter()
          .Convert(sqlModel.DefaultServer.DefaultCatalog.DefaultSchema, server);

        DomainSchema = new DomainModelConverter(
          domain.NameBuilder.BuildForeignKeyName,
          domain.NameBuilder.BuildForeignKeyName)
          .Convert(domainModel, StorageSchema.Name);

        return domain;
      }
    }

    [SetUp]
    public virtual void SetUp()
    {
      BuildDomain("mssql2005");
    }

    [Test]
    public void BaseTest()
    {
      Assert.IsNotNull(Schema);
      Assert.IsNotNull(Schema.Tables["A"]);
      Assert.IsNotNull(Schema.Tables["A"].PrimaryIndex);
      Assert.AreEqual(1, Schema.Tables["A"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(4, Schema.Tables["A"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, Schema.Tables["A"].SecondaryIndexes.Count);
      Assert.AreEqual(2, Schema.Tables["A"].SecondaryIndexes[0].KeyColumns.Count);
      Assert.IsTrue(Schema.Tables["A"].SecondaryIndexes[0].IsUnique);
      Assert.AreEqual(new TypeInfo(typeof(string), 125),
        Schema.Tables["A"].Columns["Col3"].Type);

      Assert.IsNotNull(Schema.Tables["B"]);
      Assert.IsNotNull(Schema.Tables["B"].PrimaryIndex);
      Assert.AreEqual(1, Schema.Tables["B"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(3, Schema.Tables["B"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(2, Schema.Tables["B"].SecondaryIndexes.Count);
      Assert.IsFalse(Schema.Tables["B"].SecondaryIndexes[0].IsUnique);
    }

    [Test]
    public void IncludedColumnsTest()
    {
      Assert.AreEqual(2,
        Schema.Tables["A"].SecondaryIndexes[0].IncludedColumns.Count);
    }

    [Test]
    public void ForeignKeyTest()
    {
      Assert.AreEqual(1, Schema.Tables["B"].ForeignKeys.Count);
      Assert.AreEqual(Schema.Tables["A"].PrimaryIndex,
        Schema.Tables["B"].ForeignKeys[0].PrimaryKey);
    }

    [Test]
    public void TimeSpanColumnTest()
    {
      Assert.AreEqual(new TypeInfo(typeof(TimeSpan)),
        Schema.Tables["C"].Columns["Col1"].Type);
    }

  }
}


#region Model

namespace Xtensive.Storage.Tests.Indexing.Model
{

  [HierarchyRoot("Id")]
  [Index("Col1", "Col2", IsUnique = true, IncludedFields = new[] { "Col3" })]
  public class A : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public Guid Col2 { get; private set; }

    [Field(Length = 125)]
    public string Col3 { get; private set; }
  }

  [HierarchyRoot("Id")]
  [Index("ColA", MappingName = "A_IX")]
  public class B : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public A ColA { get; private set; }
  }

  [HierarchyRoot("Id")]
  public class C : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public TimeSpan Col1 { get; private set; }
  }

  [HierarchyRoot("Id")]
  public class D : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<E> ColE { get; private set; }
  }

  [HierarchyRoot("Id")]
  public class E : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public D ColD { get; private set; }
  }
}

#endregion