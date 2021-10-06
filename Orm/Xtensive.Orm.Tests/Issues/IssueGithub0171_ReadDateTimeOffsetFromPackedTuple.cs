// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0171_ReadDateTimeOffsetFromPackedTupleModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0171_ReadDateTimeOffsetFromPackedTupleModel
{
  [HierarchyRoot]
  public class Cargo : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTimeOffset DateTimeOffsetField { get; set; }

    [Field]
    public DateTime DateTimeField { get; set; }

    [Field, Association(PairTo = nameof(CargoLoad.Cargo))]
    public EntitySet<CargoLoad> Loads { get; private set; }

    public Cargo(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class CargoLoad: Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Cargo Cargo { get; private set; }

    public CargoLoad(Session session, Cargo cargo)
      : base(session)
    {
      Cargo = cargo;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueGithub0171_ReadDateTimeOffsetFromPackedTuple : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Cargo));
      config.Types.Register(typeof(CargoLoad));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    [Test]
    public void DateTimeOffsetCase()
    {
      // NRE on within PackedFieldAccessor.GetValue<T>
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var cargo3 = new Cargo(session);
        var cargoLoad = new CargoLoad(session, null);

        var query = session.Query.All<CargoLoad>()
          .LeftJoin(session.Query.All<Cargo>(),
            cl => cl.Cargo,
            c => c,
            (cl, c) => new { CargoLoad = cl, Cargo = c })
          .Select(t => t.Cargo.DateTimeOffsetField)
          .ToArray();
      }
    }

    [Test]
    public void DateTimeCase()
    {
      //Works fine.
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var cargo3 = new Cargo(session);
        var cargoLoad = new CargoLoad(session, null);

        var query = session.Query.All<CargoLoad>()
          .LeftJoin(session.Query.All<Cargo>(),
            cl => cl.Cargo,
            c => c,
            (cl, c) => new { CargoLoad = cl, Cargo = c })
          .Select(t => t.Cargo.DateTimeField)
          .ToArray();
      }
    }
  }
}