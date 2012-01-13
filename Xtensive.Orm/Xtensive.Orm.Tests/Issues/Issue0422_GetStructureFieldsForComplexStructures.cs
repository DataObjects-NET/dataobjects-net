// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.05

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.Issue0422_GetStructureFieldsForComplexStructures_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0422_GetStructureFieldsForComplexStructures_Model
{
  [Serializable]
  public class EntityStructure : Structure
  {
    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntityB EntityB { get; set; }

    [Field]
    public string StructureName { get; set; }

    [Field]
    public DateTime StructureAge { get; set; }
  }

  [Serializable]
  public class ComplexStructure : Structure
  {
    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntityB EntityB { get; set; }

    [Field]
    public string ComplexStructureName { get; set; }

    [Field]
    public EntityStructure EntityStructure { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public ComplexStructure ComplexStructure { get; set; }

    [Field]
    public EntityStructure EntityStructure { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0422_GetStructureFieldsForComplexStructures : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (EntityB).Assembly, typeof (EntityB).Namespace);
      config.NamingConvention.NamespacePolicy = NamespacePolicy.Synonymize;
      config.NamingConvention.NamingRules = NamingRules.None;
      return config;
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          // Fill
          IEnumerable<EntityB> entitiesB = Enumerable
            .Range(0, 100)
            .Select(i => new EntityB {
              Name = "NameB_" + i,
              ComplexStructure = new ComplexStructure {
                EntityB = new EntityB {
                  Name = "NameB_1_" + i
                },
                ComplexStructureName = "StructureName_1_" + i,
                EntityStructure = new EntityStructure {
                  EntityB = new EntityB {
                    Name = "NameB_2_" + i
                  },
                  StructureAge = new DateTime(2000 + i, 10, 10),
                  StructureName = "StructureName_2_" + i,
                }
              }
            })
            .ToList();

          // Query
          session.SaveChanges();
          var structures = session.Query.All<EntityB>().Select(b => b.ComplexStructure).Skip(83).Take(1);
          var str = structures.Single();
          var testEntities = session.Query.All<EntityB>().Where(b => b.ComplexStructure==str).ToArray();
          var actualEntities = session.Query.All<EntityB>().AsEnumerable().Where(b => b.ComplexStructure==str).ToArray();
          Assert.AreEqual(0, actualEntities.Except(testEntities).Count());

          // Rollback
        }
      }
    }
  }
}