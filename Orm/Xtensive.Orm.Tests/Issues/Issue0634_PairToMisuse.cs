// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.07

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.Issue0634_PairToMisuse_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0634_PairToMisuse_Model
{
  [HierarchyRoot]
  public class Helper : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  #region OneToOne

  [HierarchyRoot]
  public class OneToOneSlave : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Helper Value1 { get; set; }

    [Field]
    public OneToOneMaster Value2 { get; set; }
  }

  [HierarchyRoot]
  public class OneToOneMaster : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field, Association(PairTo = "Value1")]
    public OneToOneSlave Value { get; set; }
  }

  #endregion

  #region ManyToOne

  [HierarchyRoot]
  public class ManyToOneSlave : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Helper Value1 { get; set; }

    [Field]
    public ManyToOneMaster Value2 { get; set; }
  }

  [HierarchyRoot]
  public class ManyToOneMaster : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field, Association(PairTo = "Value1")]
    public EntitySet<ManyToOneSlave> Value { get; set; }
  }

  #endregion

  #region OneToMany

  [HierarchyRoot]
  public class OneToManyEntitySlave : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Helper> Value1 { get; set; }

    [Field]
    public EntitySet<OneToManyEntityMaster> Value2 { get; set; }
  }

  [HierarchyRoot]
  public class OneToManyEntityMaster : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field, Association(PairTo = "Value1")]
    public OneToManyEntitySlave Value { get; set; }
  }

  #endregion

  #region ManyToMany

  [HierarchyRoot]
  public class ManyToManySlave : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Helper> Value1 { get; set; }

    [Field]
    public EntitySet<ManyToManyMaster> Value2 { get; set; }
  }

  [HierarchyRoot]
  public class ManyToManyMaster : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field, Association(PairTo = "Value1")]
    public EntitySet<ManyToManySlave> Value { get; set; }
  }


  #endregion
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0634_PairToMisuse
  {
    [Test]
    public void OneToOneTest()
    {
      Check(typeof (OneToOneSlave), typeof(OneToOneMaster));
    }

    [Test]
    public void ManyToOneTest()
    {
      Check(typeof (ManyToOneSlave), typeof(ManyToOneMaster));
    }

    [Test]
    public void OneToManyTest()
    {
      Check(typeof (OneToManyEntitySlave), typeof(OneToManyEntityMaster));
    }

    [Test]
    public void ManyToManyTest()
    {
      Check(typeof (ManyToManySlave), typeof (ManyToManyMaster));
    }

    private static void Check(params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (var type in types)
        configuration.Types.Register(type);
      configuration.Types.Register(typeof (Helper));
      AssertEx.Throws<DomainBuilderException>(() => Domain.Build(configuration));
    }
  
  }
}