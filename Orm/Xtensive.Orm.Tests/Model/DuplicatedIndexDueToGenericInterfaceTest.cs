// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.11

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Model.DuplicatedIndexDueToGenericInterfaceTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace DuplicatedIndexDueToGenericInterfaceTestModel
  {
    [HierarchyRoot]
    public class ReferencedEntity : Entity
    {
      [Key, Field]
      public int Id { get; private set; }
    }

    public interface IHasReferenceGeneric<TReference> : IEntity
      where TReference : Entity
    {
      [Field]
      TReference Ref { get; }
    }

    [HierarchyRoot]
    public class ImplementorOfGenericInterface : Entity, IHasReferenceGeneric<ReferencedEntity>
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public ReferencedEntity Ref { get; set; }
    }

    public interface IHasReference : IEntity
    {
      [Field]
      ReferencedEntity Ref { get; }
    }

    [HierarchyRoot]
    public class ImplementorOfNonGenericInterface : Entity, IHasReference
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public ReferencedEntity Ref { get; set; }
    }
  }

  [TestFixture]
  public class DuplicatedIndexDueToGenericInterfaceTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ReferencedEntity).Assembly, typeof (ReferencedEntity).Namespace);
      return configuration;
    }

    [Test]
    public void Test()
    {
      var indexes1 = Domain.Model.Types[typeof(ImplementorOfGenericInterface)].Indexes;
      var refIndexes1 = indexes1.Where(i => i.Columns.Any(c => c.Name=="Ref.Id") && i.IsSecondary && !i.IsVirtual).ToList();

      var indexes2 = Domain.Model.Types[typeof(ImplementorOfNonGenericInterface)].Indexes;
      var refIndexes2 = indexes2.Where(i => i.Columns.Any(c => c.Name == "Ref.Id") && i.IsSecondary && !i.IsVirtual).ToList();
      
      Assert.That(refIndexes1.Count, Is.EqualTo(1));
      Assert.That(refIndexes2.Count, Is.EqualTo(1));
    }
  }
}