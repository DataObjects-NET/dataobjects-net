// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Indexing.Model.Convert;
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests;
using Xtensive.Storage;
using Xtensive.Sql.Common;
using Domain=Xtensive.Storage.Domain;
using Xtensive.Storage.Model;
using Xtensive.Storage.Model.Convert;

namespace Xtensive.Indexing.Tests.Storage
{
  [Serializable]
  public class ConvertModelTest : AutoBuildTest
  {
    private Model sqlModel;
    private DomainModel domainModel;
    private ServerInfo server;
    private StorageInfo storage;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(A).Namespace);
      return config;
    }

    protected override Xtensive.Storage.Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      // Get current SQL model
      var domainHandler = domain.Handlers.DomainHandler;
      using (var connection = (SqlConnection)((DomainHandler)domainHandler).ConnectionProvider
        .CreateConnection(configuration.ConnectionInfo.ToString()))
      {
        server = connection.Driver.ServerInfo;
        sqlModel = new SqlModelProvider(connection).Build();
        domainModel = domain.Model;

        storage = new SqlModelConverter().Convert(sqlModel.DefaultServer.DefaultCatalog.DefaultSchema, server);
        //storage = new ModelConverter().Convert(domainModel, "dbo");
        
        storage.Dump();

        return domain;
      }
    }

    [Test]
    public void BaseTest()
    {
      Assert.IsNotNull(storage);
      Assert.IsNotNull(storage.Tables["A"]);
      Assert.IsNotNull(storage.Tables["A"].PrimaryIndex);
      Assert.AreEqual(1, storage.Tables["A"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(4, storage.Tables["A"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(1, storage.Tables["A"].SecondaryIndexes.Count);
      Assert.AreEqual(2, storage.Tables["A"].SecondaryIndexes[0].KeyColumns.Count);
      Assert.IsTrue(storage.Tables["A"].SecondaryIndexes[0].IsUnique);
      Assert.AreEqual(new Xtensive.Storage.Indexing.Model.TypeInfo(typeof(string), "Cyrillic_General_CI_AS", 125),
        storage.Tables["A"].Columns["Col3"].ColumnType);
      
      Assert.IsNotNull(storage.Tables["B"]);
      Assert.IsNotNull(storage.Tables["B"].PrimaryIndex);
      Assert.AreEqual(1, storage.Tables["B"].PrimaryIndex.KeyColumns.Count);
      Assert.AreEqual(3, storage.Tables["B"].PrimaryIndex.ValueColumns.Count);
      Assert.AreEqual(2, storage.Tables["B"].SecondaryIndexes.Count);
      Assert.IsFalse(storage.Tables["B"].SecondaryIndexes[0].IsUnique);
    }

    [Test]
    public void IncludedColumnsTest()
    {
      Assert.AreEqual(1, storage.Tables["A"].SecondaryIndexes[0].ValueColumns.Count);
    }

    [Test]
    public void ForeignKeyTest()
    {
      Assert.AreEqual(1, storage.Tables["B"].ForeignKeys.Count);
      Assert.AreEqual(storage.Tables["A"].PrimaryIndex,
        storage.Tables["B"].ForeignKeys[0].ReferencedIndex);
      Assert.AreEqual(storage.Tables["B"].SecondaryIndexes[0],
        storage.Tables["B"].ForeignKeys[0].ReferencingIndex);
    }

    [Test]
    public void TimeSpanColumnTest()
    {
      Assert.AreEqual(new Xtensive.Storage.Indexing.Model.TypeInfo(typeof(TimeSpan)),
        storage.Tables["C"].Columns["Col1"].ColumnType);
    }

  }

  #region Model

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

  #endregion
}