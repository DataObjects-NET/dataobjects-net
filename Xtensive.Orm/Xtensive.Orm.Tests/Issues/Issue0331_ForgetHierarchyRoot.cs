// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.03

using System;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0331_ForgetHierarchyRoot_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0331_ForgetHierarchyRoot_Model
{
    [Serializable]
    [HierarchyRoot]
    public class Cell : Entity
    {
        [Key, Field]
        public int Id { get; private set; }

        [Field]
        public int X { get; set; }

        [Field]
        public int Y { get; set; }

        [Field, Association(PairTo = "Cell", OnTargetRemove = OnRemoveAction.Clear)]
        public Creature Creature { get; set; }
    }

    [Serializable]
    public class Creature : Entity
    {
        [Key, Field]
        public int ID { get; private set; }

        [Field]
        public Cell Cell { get; set; }
    }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0331_ForgetHierarchyRoot : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Cell).Assembly, typeof (Cell).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = null;
      AssertEx.Throws<DomainBuilderException>(() => domain = base.BuildDomain(configuration));
      return domain;
    }
  }
}