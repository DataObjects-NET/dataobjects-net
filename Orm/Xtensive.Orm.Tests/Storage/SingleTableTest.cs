// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.10.01

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.Storage.SingleTableTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace SingleTableTestModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class Base : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    public class DerivedNode : Base
    {
      [Field]
      public string Description { get; set; }
    }

    public class Derived : Base
    {
      [Field]
      public string Name { get; set; }
    }

    public class Leaf : Derived
    {
      [Field]
      public string Description { get; set; }
    }
  }

  [TestFixture]
  public class SingleTableTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Base).Assembly, typeof (Base).Namespace);
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Base();
        new Derived() {Name = "Derived"};
        new Leaf() {Name = "Leaf", Description = "Green"};
        new DerivedNode() {Description = "Node"};

        var primaryIndex = Domain.Model.Types[typeof (Base)].Indexes.PrimaryIndex;
        var rs = primaryIndex.GetQuery().GetRecordSetReader(session, new ParameterContext());
        var result = rs.ToEntities(0).ToList();
        t.Complete();
      }
    }
  }
}