// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.11.16

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0801_ReduntandColumnRemoverRemovesPartOfTempTableColumnsModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0801_ReduntandColumnRemoverRemovesPartOfTempTableColumnsModel
{
  [HierarchyRoot]
  public class EquipmentStateRecord : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime StartDate { get; set; }

    [Field]
    public DateTime? EndDate { get; set; }

    [Field]
    public Equipment Equipment { get; set; }

    [Field]
    public MachineStateType Type { get; set; }
  }

  [HierarchyRoot]
  public class Equipment : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }

  public enum MachineStateType
  {
    Undefined = 0,
    Production = 1,
    EmergencyStop = 2,
    Adjustment = 3,
    SwitchedOn = 4,
    SwitchedOff = 5
  }

  public class Segment
  {
    public long EquipmentId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0801_ReduntandColumnRemoverRemovesPartOfTempTableColumns : AutoBuildTest
  {
    private long equipmentId;

    protected override void CheckRequirements() =>
      Require.AllFeaturesSupported(Providers.ProviderFeatures.TemporaryTables);

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(EquipmentStateRecord).Assembly, typeof(EquipmentStateRecord).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var equipment = new Equipment();
        equipmentId = equipment.Id;

        foreach (var item in Enum.GetValues(typeof(MachineStateType)).Cast<MachineStateType>()) {
          _ = new EquipmentStateRecord() {
            Equipment = equipment,
            Type = item,
            StartDate = DateTime.Now.AddDays(-1),
            EndDate = DateTime.Now.AddHours(-1)
          };

          _ = new EquipmentStateRecord() {
            Equipment = equipment,
            Type = item,
            StartDate = DateTime.Now.AddDays(-2),
            EndDate = DateTime.Now.AddHours(-2)
          };

          _ = new EquipmentStateRecord() {
            Equipment = equipment,
            Type = item,
            StartDate = DateTime.Now.AddDays(-3),
            EndDate = DateTime.Now.AddHours(-3)
          };
        }

        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var segments = GetSegments().Select(x => new { x.EquipmentId, x.StartDate, x.EndDate }).ToArray();
        var remoteSegments = session.Query.Store(segments);

        var testQuery = session.Query.All<EquipmentStateRecord>()
          .Where(x => x.EndDate.HasValue)
          .Join(remoteSegments,
                record => record.Equipment.Id,
                segment => segment.EquipmentId,
                (record, segment) => new {
                  EquipmentId = record.Equipment.Id,
                  Type = record.Type,
                  StateStartDate = record.StartDate,
                  StateEndDate = record.EndDate.Value,
                  SegmentStartDate = segment.StartDate,
                  SegmentEndDate = segment.EndDate,
                  StartTime = record.StartDate < segment.StartDate ? segment.StartDate : record.StartDate,
                  EndTime = record.EndDate.Value > segment.EndDate ? segment.EndDate : record.EndDate.Value
                })
          .Where(x => x.StateStartDate < x.SegmentEndDate)
          .GroupBy(x => new { x.EquipmentId, x.Type }, x => x.EndTime - x.StartTime)
          .ToArray();
      }
    }

    private IEnumerable<Segment> GetSegments()
    {
      yield return new Segment { EquipmentId = equipmentId, StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now };
      yield return new Segment { EquipmentId = equipmentId, StartDate = DateTime.Now.AddDays(-2), EndDate = DateTime.Now };
      yield return new Segment { EquipmentId = equipmentId, StartDate = DateTime.Now.AddDays(-3), EndDate = DateTime.Now };
    }
  }
}
