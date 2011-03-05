// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.03

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Model
{
  [HierarchyRoot]
  public class Source<TTag1, TTag2> : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public TTag1 Tag1 { get; set; }

    [Field]
    public TTag1 Tag2 { get; set; }

    [Field, Association(PairTo = "Sources")]
    public EntitySet<Target<TTag1, TTag2>> Targets { get; private set; }
  }

  [HierarchyRoot]
  public class Target<TTag1, TTag2> : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public TTag1 Tag1 { get; set; }

    [Field]
    public TTag2 Tag2 { get; set; }

    [Field]
    public EntitySet<Source<TTag1, TTag2>> Sources { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class GenericManyToManyTest : AutoBuildTest
  {

    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Target<string, int>));
      configuration.Types.Register(typeof (Source<string, int>));
      return configuration;
    }

    [Test]
    public void CombinedTest()
    {
      
    }
  }
}