// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0647_WrongLinqQuery_Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0647_WrongLinqQuery_Model
  {
    [Serializable]
    public abstract class EntityBase : Entity
    {
      protected EntityBase(Guid id)
        : base(id)
      {
      }

      [Field]
      [Key]
      public Guid Id { get; private set; }
    }


    [HierarchyRoot]
    [Index("SysName", Unique = true)]
    [Index("Name", Unique = true)]
    public class Document : EntityBase
    {
      public Document(Guid id)
        : base(id)
      {
      }

      [Field(Length = 64)]
      public string SysName { get; set; }

      [Field(Length = 400)]
      public string Name { get; set; }

      [Field(Length = 400)]
      public string Description { get; set; }

      [Field]
      public Document OwnerEntity { get; set; }

      [Field]
      public Document LinkedEntity { get; set; }

      [Field]
      public bool IsOrdered { get; set; }

      [Field]
      public bool IsHierarchical { get; set; }

      public override string ToString()
      {
        return string.Format("SysName: {1}, Name: {2}", base.ToString(), SysName, Name);
      }
    }


    [HierarchyRoot]
    [Index("Name", Unique = true)]
    public class Role : EntityBase
    {
      public Role(Guid id)
        : base(id)
      {}

      [Field(Length = 100)]
      public string Name { get; set; }

      [Field(Length = 255)]
      public string DisplayName { get; set; }
    }

    [HierarchyRoot]
    public class RowLevelPermission : EntityBase
    {
      public RowLevelPermission(Guid id)
        : base(id)
      {
      }

      [Field(Length = 400, Nullable = false)]
      public string Name { get; set; }

      [Field(Nullable = false)]
      public Role Role { get; set; }

      [Field]
      [Association(PairTo = "Owner")]
      public EntitySet<MlRowlevelPermissionDocEntityField> Fields { get; private set; }

      [HierarchyRoot]
      public class MlRowlevelPermissionDocEntityField : Multilink<RowLevelPermission, Document>
      {
        /// <summary>Initializes a new instance of the <see cref="Multilink{TOwner,TLinked}"/> class.</summary>
        /// <param name="id">Идентификатор создаваемого элемента.</param>
        public MlRowlevelPermissionDocEntityField(Guid id)
          : base(id)
        {
        }
      }
    }

    [Serializable]
    public abstract class Multilink<TOwner, TLinked> : EntityBase
      where TOwner : EntityBase
      where TLinked : EntityBase
    {
      protected Multilink(Guid id)
        : base(id)
      {
      }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public TOwner Owner { get; set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Deny)]
      public TLinked Linked { get; set; }

      public override string ToString()
      {
        return string.Format("{0}, Owner: {1}, Linked: {2}", base.ToString(), Owner, Linked);
      }
    }


    [HierarchyRoot]
    [Index("Role", "Entity", Unique = true)]
    public class FunctionalPermission : EntityBase
    {
      public FunctionalPermission(Guid id)
        : base(id)
      {
      }

      [Field(Length = 400, Nullable = false)]
      public string Name { get; set; }

      [Field]
      public Role Role { get; set; }

      [Field]
      public Document Entity { get; set; }

      [Field]
      public bool? FullControl { get; set; }

      [Field]
      public bool? Read { get; set; }

      [Field]
      public bool? Create { get; set; }

      [Field]
      public bool? Delete { get; set; }

      public static bool CanRead(string entity, string[] roles)
      {
        return HasPermission(entity, roles, e => e.Read);
      }
      
      public static bool CanCreate(string entity, string[] roles)
      {
        return HasPermission(entity, roles, e => e.Create);
      }

      public static bool CanDelete(string entity, string[] roles)
      {
        return HasPermission(entity, roles, e => e.Delete);
      }

      public static bool CanControl(string entity, string[] roles)
      {
        return HasPermission(entity, roles, e => e.FullControl);
      }

      private static bool HasPermission(string entity, string[] roles, Expression<Func<FunctionalPermission, bool?>> permission)
      {
        var effectiveEntity = entity;

        var q = from perm in Session.Demand().Query.All<FunctionalPermission>()
                where perm.Role.Name.In(roles) && perm.Entity.SysName == effectiveEntity
                select perm;

        var total = q.Select(permission).ToArray();
        return total.Contains(true) && !total.Contains(false);
      }
    }

  }

  [Serializable]
  public class Issue0647_WrongLinqQuery : AutoBuildTest
  {
    private Guid globalId1 = new Guid("A3A01D2F-41A9-416E-8513-6C2F55224B56");
    private Guid globalId2 = new Guid("FDBDBCF1-0117-4508-89A3-F47A9A098478");
    private Guid globalId3 = new Guid("EC2F180E-48BB-445C-BE95-C384B069D60B");
    private Guid globalId4 = new Guid("07F0DF2A-B55F-44BE-BB9B-034A875BF0DE");

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderIsNot(StorageProvider.Oracle);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(Role).Assembly, typeof(Role).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var r1 = new Role(globalId1) { Name = "1" };
        var r2 = new Role(globalId2) { Name = "2" };

        var de = new Document(Guid.NewGuid()) { SysName = "Control" };

        var p1 = new FunctionalPermission(globalId3) { FullControl = false, Read = true, Name = "q", Entity = de, Role = r1 };
        var p2 = new FunctionalPermission(globalId4) { FullControl = true, Read = false, Delete = true, Name = "q", Entity = de, Role = r2 };
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Assert.IsFalse(FunctionalPermission.CanRead("Control", new string[] { }));
        Assert.IsTrue(FunctionalPermission.CanRead("Control", new[] { "1" }));
        Assert.IsFalse(FunctionalPermission.CanControl("Control", new[] { "1" }));
        Assert.IsFalse(FunctionalPermission.CanCreate("Control", new[] { "1", "2" }));
        Assert.IsFalse(FunctionalPermission.CanRead("Control", new[] { "1", "2" }));
        Assert.IsTrue(FunctionalPermission.CanDelete("Control", new[] { "1", "2" }));

        var roles = new[] { "1", "2" };
        var q = from perm in session.Query.All<RowLevelPermission>()
                where perm.Role.Name.In(roles) && perm.Fields.Any(m => m.Linked.SysName == "Control")
                select perm;
        var result = q.ToList();
      }
    }
  }
}