// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [HierarchyRoot("Id1A", "Id2A", "Id3A")]
  public class ABase: Entity
  {
    [Field]
    public int Id1A { get; private set; }

    [Field]
    public double Id2A { get; private set; }

    [Field]
    public Guid Id3A { get; private set; }
  }

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

    [Field(PairTo = "OneToOne")]
    public B OneToOne { get; set; }

    [Field]
    public EntitySet<B> ZeroToMany { get; private set; }

    [Field(PairTo = "ManyToOne")]
    public EntitySet<B> OneToMany { get; private set; }

    [Field(PairTo = "ManyToMany")]
    public EntitySet<B> ManyToMany { get; private set; }

    [Field(PairTo = "OneToMany")]
    public B ManyToOne { get; set; }
  }

  [HierarchyRoot("Id1", "Id2")]
  public class BBase:Entity
  {
    [Field]
    public float Id1 { get; private set; }

    [Field(Length = 20)]
    public byte Id2 { get; private set; }
  }

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
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class C : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public C OneToZero { get; set; }

    [Field(PairTo = "OneToOne2")]
    public C OneToOne1 { get; set; }

    [Field]
    public C OneToOne2 { get; set; }

    [Field]
    public EntitySet<C> ZeroToMany { get; private set; }

    [Field]
    public EntitySet<C> OneToMany { get; private set; }

    [Field(PairTo = "ManyToMany2")]
    public EntitySet<C> ManyToMany1 { get; private set; }

    [Field]
    public EntitySet<C> ManyToMany2 { get; private set; }

    [Field(PairTo = "OneToMany")]
    public C ManyToOne { get; set; }
  }

  [TestFixture]
  public class TableReferenceTest : AutoBuildTest
  {
    private Schema existingSchema;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (A).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      // Get current SQL model
      var domainHandler = domain.Handlers.DomainHandler;
      using (var connection = (SqlConnection) ((DomainHandler) domainHandler).ConnectionProvider.CreateConnection(configuration.ConnectionInfo.ToString())) {
        var modelProvider = new SqlModelProvider(connection);
        var sqlModel = Xtensive.Sql.Dom.Database.Model.Build(modelProvider);
        existingSchema = sqlModel.DefaultServer.DefaultCatalog.DefaultSchema;
        Assert.IsNotNull(existingSchema);
        return domain;
      }
    }

    [Test]
    public void CheckAssociations()
    {
      TypeInfo typeA = Domain.Model.Types[typeof (A)];
      Assert.IsNotNull(typeA);
      CheckAssociations(typeA.GetAssociations(), 6, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);

      TypeInfo typeB = Domain.Model.Types[typeof (B)];
      Assert.IsNotNull(typeB);
      CheckAssociations(typeB.GetAssociations(), 6, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);

      TypeInfo typeC = Domain.Model.Types[typeof (C)];
      Assert.IsNotNull(typeC);
      CheckAssociations(typeC.GetAssociations(), 8, Multiplicity.ManyToMany, Multiplicity.ManyToOne, Multiplicity.OneToMany, Multiplicity.OneToOne, Multiplicity.ZeroToMany, Multiplicity.ZeroToOne);
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