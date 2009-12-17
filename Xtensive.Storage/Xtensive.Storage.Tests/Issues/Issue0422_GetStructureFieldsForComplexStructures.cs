// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.05

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0422_GetStructureFieldsForComplexStructures_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0422_GetStructureFieldsForComplexStructures_Model
{
  public class EntityStructure : Structure
  {
    [Field]
    public EntityB B { get; set; }

    [Field]
    public string StructureName { get; set; }

    [Field]
    public DateTime StructureAge { get; set; }
  }

  public class ComplexStructure : Structure
  {
    [Field]
    public EntityB B { get; set; }

    [Field]
    public string ComplexStructureName { get; set; }

    [Field]
    public EntityStructure EntityStructure { get; set; }
  }

  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public ComplexStructure AdditionalInfo { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0422_GetStructureFieldsForComplexStructures : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (EntityB).Assembly, typeof (EntityB).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          // Fill
          IEnumerable<EntityB> entitiesB = Enumerable
            .Range(0, 100)
            .Select(i => new EntityB {
              Name = "NameB_" + i,
              AdditionalInfo = new ComplexStructure {
                B = new EntityB {
                  Name = "NameB_1_" + i
                },
                ComplexStructureName = "StructureName_1_" + i,
                EntityStructure = new EntityStructure {
                  B = new EntityB {
                    Name = "NameB_2_" + i
                  },
                  StructureAge = new DateTime(2000 + i, 10, 10),
                  StructureName = "StructureName_2_" + i,
                }
              }
            })
            .ToList();

          // Query
          session.Persist();
          var structures = Query.All<EntityB>().Select(b => b.AdditionalInfo).Skip(83).Take(1);
          var str = structures.Single();
          var testEntities = Query.All<EntityB>().Where(b => b.AdditionalInfo==str).ToArray();
          var actualEntities = Query.All<EntityB>().AsEnumerable().Where(b => b.AdditionalInfo==str).ToArray();
          Assert.AreEqual(0, actualEntities.Except(testEntities).Count());

          // Rollback
        }
      }
    }
  }
}