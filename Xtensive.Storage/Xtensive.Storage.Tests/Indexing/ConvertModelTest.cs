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
using Xtensive.Storage.Providers.Sql;
using Xtensive.Storage.Tests;
using Xtensive.Storage;
using Xtensive.Sql.Common;

namespace Xtensive.Indexing.Tests.Storage
{
  [Serializable]
  public class ConvertModelTest : AutoBuildTest
  {
    private Model model;
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
        model = new SqlModelProvider(connection).Build();
        return domain;
      }
    }

    [SetUp]
    public void ExtractStorage()
    {
      storage = new SqlModelConverter().Convert(
        model.DefaultServer.DefaultCatalog.DefaultSchema, server);
      storage.Dump();
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
      Assert.AreEqual(new TypeInfo(typeof(string), "Cyrillic_General_CI_AS", 125),
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
      Assert.IsNotNull(storage.Tables["B"].ForeignKeys[0]);
      Assert.AreEqual(storage.Tables["A"].PrimaryIndex, 
        storage.Tables["B"].ForeignKeys[0].ReferencedIndex);
      Assert.AreEqual(storage.Tables["B"].SecondaryIndexes[1],
        storage.Tables["B"].ForeignKeys[0].ReferencingIndex);
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
  [Index("Col1")]
  public class B : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public double Col1 { get; private set; }

    [Field]
    public A ColA { get; private set; }
  }

  #endregion
}