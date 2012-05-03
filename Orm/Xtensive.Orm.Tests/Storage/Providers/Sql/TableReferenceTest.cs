// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Sql.Tests;
using Xtensive.Orm.Providers.Sql;
using Xtensive.Orm.Tests.Storage.Providers.Sql.TableReferenceTestModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql.TableReferenceTestModel
{
  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class ABase: Entity
  {
    [Field, Key(0)]
    public int Id1A { get; private set; }

    [Field, Key(1)]
    public double Id2A { get; private set; }

    [Field, Key(2)]
    public Guid Id3A { get; private set; }
  }

  [Serializable]
  [Index("JustField1", "JustField2")]
  [Index("JustField3")]
  public class A : ABase
  {

    [Field]
    public int JustField1 { get; set; }

    [Field]
    public Guid JustField2 { get; set; }

    [Field]
    public bool JustField3 { get; set; }


    [Field]
    public B OneToZero { get; set; }

    [Field, Association(PairTo = "OneToOne")]
    public B OneToOne { get; set; }

    [Field]
    public EntitySet<B> ZeroToMany { get; private set; }

    [Field, Association(PairTo = "ManyToOne")]
    public EntitySet<B> OneToMany { get; private set; }

    [Field, Association(PairTo = "ManyToMany")]
    public EntitySet<B> ManyToMany { get; private set; }

    [Field, Association(PairTo = "OneToMany")]
    public B ManyToOne { get; set; }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [HierarchyRoot]
  public class BBase:Entity
  {
    [Field, Key(0)]
    public float Id1 { get; private set; }

    [Field(Length = 20), Key(1)]
    public byte Id2 { get; private set; }
  }

  [Serializable]
  public class B : BBase
  {

    [Field]
    public A OneToZero { get; set; }

    [Field]
    public A OneToOne { get; set; }

    [Field]
    public EntitySet<A> ZeroToMany { get; private set; }

    [Field]
    public EntitySet<A> OneToMany { get; private set; }

    [Field]
    public EntitySet<A> ManyToMany { get; private set; }

    [Field]
    public A ManyToOne { get; set; }
  }

  /// <summary>
  /// Self-references
  /// </summary>
  [Serializable]
  [HierarchyRoot]
  public class C : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public C OneToZero { get; set; }

    [Field, Association(PairTo = "OneToOne2")]
    public C OneToOne1 { get; set; }

    [Field]
    public C OneToOne2 { get; set; }

    [Field]
    public EntitySet<C> ZeroToMany { get; private set; }

    [Field]
    public EntitySet<C> OneToMany { get; private set; }

    [Field, Association(PairTo = "ManyToMany2")]
    public EntitySet<C> ManyToMany1 { get; private set; }

    [Field]
    public EntitySet<C> ManyToMany2 { get; private set; }

    [Field, Association(PairTo = "OneToMany")]
    public C ManyToOne { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.Providers.Sql {

  [TestFixture]
  public class TableReferenceTest : AutoBuildTest
  {
    private Schema existingSchema;

    protected override void CheckRequirements()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (A).Namespace);
      return config;
    }

    protected override Xtensive.Orm.Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      // Get current SQL model

      var driver = TestSqlDriver.Create(domain.Configuration.ConnectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        try {
          connection.BeginTransaction();
          existingSchema = driver.ExtractDefaultSchema(connection);
        }
        finally {
          connection.Rollback();
        }
        Assert.IsNotNull(existingSchema);
        return domain;
      }
    }

    [Test]
    public void CheckAssociations()
    {
      TypeInfo typeA = Domain.Model.Types[typeof (A)];
      Assert.IsNotNull(typeA);
      CheckAssociations(typeA.GetTargetAssociations(), 6, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);

      TypeInfo typeB = Domain.Model.Types[typeof (B)];
      Assert.IsNotNull(typeB);
      CheckAssociations(typeB.GetTargetAssociations(), 6, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);

      TypeInfo typeC = Domain.Model.Types[typeof (C)];
      Assert.IsNotNull(typeC);
      CheckAssociations(typeC.GetTargetAssociations(), 8, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);
    }

    private void CheckAssociations(IEnumerable<AssociationInfo> associations, int count, params Multiplicity[] multiplicities)
    {
      Assert.AreEqual(count, associations.Count());
      foreach (Multiplicity multiplicity in multiplicities)
        Assert.IsTrue(associations.Any(association => association.Multiplicity==multiplicity));
    }

    [Test]
    public void OneToZero()
    {
//      var tableA = existingSchema.Tables[Domain.Model.Types[typeof (A)].Name];
//      Assert.IsNotNull(tableA);
//      var fk = tableA.CreateForeignKey("d");
//      ForeignKey abOneToZero = tableA.TableConstraints.
//      tableA.TableConstraints
    }
  }
}