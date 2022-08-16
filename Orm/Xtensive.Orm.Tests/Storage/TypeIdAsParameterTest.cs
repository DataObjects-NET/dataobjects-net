using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TypeIdAsParameterTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class TypeIdAsParameterTest : AutoBuildTest
  {
    private ConcurrentDictionary<Type, int> typeIds = new();

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);

      return configuration;
    }

    protected override void PopulateData() 
    {
      foreach(var typeInfo in Domain.Model.Types.Entities) {
        _ = typeIds.TryAdd(typeInfo.UnderlyingType, typeInfo.TypeId);
      }
    }

    [Test]
    public void SimpleQueryTest()
    {
      var typeId = typeIds[typeof(SimpleEntity)];

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<SimpleEntity>().ToList();
        var whereResult = session.Query.All<SimpleEntity>().Where(e => e.TypeId == typeId).ToList();

        var orderByResult = session.Query.All<SimpleEntity>().OrderBy(e => e.TypeId).ToList();
        var groupByResult = session.Query.All<SimpleEntity>().GroupBy(e => e.TypeId).ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        if (args.Command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }
      }
    }

    [Test]
    public void UnionOfQuerues()
    {
      var typeId = typeIds[typeof(SimpleEntity)];

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<SimpleEntity>().Where(e => e.Id > 10)
          .Union(session.Query.All<SimpleEntity>().Where(e => e.Id < 10))
          .ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        if (args.Command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }
      }
    }

    [Test]
    public void InterfaceTest()
    {
      var rootTypeId = typeIds[typeof(InterfaceHierarchyRoot)];
      var midTypeId = typeIds[typeof(InterfaceHierarchyMid)];
      var leafTypeId = typeIds[typeof(InterfaceHierarchyLeaf)];

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<IBaseEnity>().ToList();
        var whereResult = session.Query.All<IBaseEnity>().ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var commandText = (ReadOnlySpan<char>) args.Command.CommandText;
        if (commandText.Contains((ReadOnlySpan<char>) rootTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) midTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) leafTypeId.ToString(), StringComparison.Ordinal)) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }
      }
    }

    [Test]
    public void ConcreteTableHierarchyTest()
    {
      var rootTypeId = typeIds[typeof(BasicConcreteHierarchyRoot)];
      var midTypeId = typeIds[typeof(BasicConcreteHierarchyMid)];
      var leafTypeId = typeIds[typeof(BasicConcreteHierarchyLeaf)];

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result1 = session.Query.All<BasicConcreteHierarchyRoot>().ToList();
        var whereResult1 = session.Query.All<BasicConcreteHierarchyRoot>()
          .Where(e => e.TypeId == rootTypeId || e.TypeId == midTypeId || e.TypeId == leafTypeId)
          .ToList();
        var orderByResult1 = session.Query.All<BasicConcreteHierarchyRoot>()
          .OrderBy(e => e.TypeId)
          .ToList();
        var groupByResult1 = session.Query.All<BasicConcreteHierarchyRoot>()
          .GroupBy(e => e.TypeId)
          .ToList();

        var result2 = session.Query.All<BasicConcreteHierarchyMid>().ToList();
        var whereResult2 = session.Query.All<BasicConcreteHierarchyMid>()
          .Where(e => e.TypeId == midTypeId || e.TypeId == leafTypeId)
          .ToList();
        var orderByResult2 = session.Query.All<BasicConcreteHierarchyMid>()
          .OrderBy(e => e.TypeId)
          .ToList();
        var groupByResult2 = session.Query.All<BasicConcreteHierarchyMid>()
          .GroupBy(e => e.TypeId)
          .ToList();

        var result3 = session.Query.All<BasicConcreteHierarchyLeaf>().ToList();
        var whereResult3 = session.Query.All<BasicConcreteHierarchyLeaf>()
          .Where(e => e.TypeId == leafTypeId)
          .ToList();
        var orderByResult3 = session.Query.All<BasicConcreteHierarchyLeaf>()
          .OrderBy(e => e.TypeId)
          .ToList();
        var groupByResult3 = session.Query.All<BasicConcreteHierarchyLeaf>()
          .GroupBy(e => e.TypeId)
          .ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var commandText = (ReadOnlySpan<char>) args.Command.CommandText;
        if (commandText.Contains((ReadOnlySpan<char>) rootTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) midTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) leafTypeId.ToString(), StringComparison.Ordinal)) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.TypeIdAsParameterTestModel
{
  [HierarchyRoot]
  public sealed class SimpleEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public SimpleEntity(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot(IncludeTypeId = true)]
  public sealed class SimpleEntityWithIncludedId : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public SimpleEntityWithIncludedId(Session session)
      : base(session)
    {

    }
  }

  [HierarchyRoot(InheritanceSchema = Orm.Model.InheritanceSchema.ConcreteTable)]
  public class BasicConcreteHierarchyRoot : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public BasicConcreteHierarchyRoot(Session session)
      : base(session)
    {
    }
  }

  public class BasicConcreteHierarchyMid : BasicConcreteHierarchyRoot
  {
    [Field]
    public DateTime CreationDate { get; set; }

    public BasicConcreteHierarchyMid(Session session)
      : base(session)
    {
    }
  }

  public class BasicConcreteHierarchyLeaf : BasicConcreteHierarchyMid
  {
    [Field]
    public int SomeValue { get; set; }

    public BasicConcreteHierarchyLeaf(Session session)
      : base(session)
    {
    }
  }


  public interface IBaseEnity : IEntity
  {
    [Field]
    int Id { get; }

    [Field]
    string Name { get; set; }
  }

  public interface IHasCreationDate : IBaseEnity
  {
    [Field]
    DateTime CreationDate { get; set; }
  }

  public interface IHasSomeValue : IHasCreationDate
  {
    [Field]
    int SomeValue { get; set; }
  }

  [HierarchyRoot(InheritanceSchema = Orm.Model.InheritanceSchema.ConcreteTable)]
  public class InterfaceHierarchyRoot : Entity, IBaseEnity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public InterfaceHierarchyRoot(Session session)
      : base(session)
    {
    }
  }

  public class InterfaceHierarchyMid : InterfaceHierarchyRoot, IHasCreationDate
  {
    [Field]
    public DateTime CreationDate { get; set; }

    public InterfaceHierarchyMid(Session session)
      : base(session)
    {
    }
  }

  public class InterfaceHierarchyLeaf : InterfaceHierarchyMid, IHasSomeValue
  {
    [Field]
    public int SomeValue { get; set; }

    public InterfaceHierarchyLeaf(Session session)
      : base(session)
    {
    }
  }
}
