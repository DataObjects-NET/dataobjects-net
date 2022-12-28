using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TypeIdAsParameterTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class TypeIdAsParameterTest : AutoBuildTest
  {
    private const string AdditionalNode1Name = "Additional1";
    private const string AdditionalNode2Name = "Additional2";

    private readonly Dictionary<Type, int> defaultNodeTypeIds = new();
    private readonly Dictionary<Type, int> additionalNode1TypeIds = new();
    private readonly Dictionary<Type, int> additionalNode2TypeIds = new();

    public static IEnumerable<string> NodeIdentifiers
    {
      get => StorageProviderInfo.Instance.CheckAllFeaturesSupported(Orm.Providers.ProviderFeatures.Multischema)
        ? new[] { WellKnown.DefaultNodeId, AdditionalNode1Name, AdditionalNode2Name }
        : new[] { WellKnown.DefaultNodeId };
    }

    private bool IsMultischema { get; } = StorageProviderInfo.Instance.CheckAllFeaturesSupported(Orm.Providers.ProviderFeatures.Multischema);

    [OneTimeSetUp]
    public void OneTimeSetup() => Require.ProviderIsNot(StorageProvider.Firebird);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);

      if(IsMultischema) {
        configuration.DefaultSchema = GetDefaultSchema(StorageProviderInfo.Instance);
      }

      return configuration;

      static string GetDefaultSchema(StorageProviderInfo instance)
      {
        return instance.Provider switch {
          StorageProvider.SqlServer => WellKnownSchemas.SqlServerDefaultSchema,
          StorageProvider.PostgreSql => WellKnownSchemas.PgSqlDefalutSchema,
          _ => throw new NotSupportedException()
        };
      }
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);

      if (IsMultischema) {
        _ = domain.StorageNodeManager
          .AddNode(BuildNodeConfiguration(AdditionalNode1Name, configuration.DefaultSchema, WellKnownSchemas.Schema1));
        _ = domain.StorageNodeManager
          .AddNode(BuildNodeConfiguration(AdditionalNode2Name, configuration.DefaultSchema, WellKnownSchemas.Schema2));
      }

      return domain;

      static NodeConfiguration BuildNodeConfiguration(string name, string defaultSchemaName, string schemaName)
      {
        var nodeConfig = new NodeConfiguration(name);
        nodeConfig.SchemaMapping.Add(defaultSchemaName, schemaName);
        nodeConfig.UpgradeMode = DomainUpgradeMode.Recreate;

        return nodeConfig;
      }
    }

    protected override void PopulateData() 
    {
      var defaultNode = Domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);

      foreach (var typeInfo in Domain.Model.Types.Entities) {
        defaultNodeTypeIds.Add(typeInfo.UnderlyingType, defaultNode.TypeIdRegistry[typeInfo]);
      }

      if (IsMultischema) {
        var additional1 = Domain.StorageNodeManager.GetNode(AdditionalNode1Name);
        var additional2 = Domain.StorageNodeManager.GetNode(AdditionalNode2Name);
        foreach (var typeInfo in Domain.Model.Types.Entities) {
          additionalNode1TypeIds.Add(typeInfo.UnderlyingType, additional1.TypeIdRegistry[typeInfo]);
          additionalNode2TypeIds.Add(typeInfo.UnderlyingType, additional2.TypeIdRegistry[typeInfo]);
        }
      }
    }

    [Test]
    [TestCaseSource(nameof(NodeIdentifiers))]
    public void SimpleQueryTest(string nodeId)
    {
      var typeId = GetTypeIdMapForNode(nodeId)[typeof(SimpleEntity)];
      var node = Domain.StorageNodeManager.GetNode(nodeId);

      using (var session = node.OpenSession())
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
        var command = args.Command;

        if (command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }

        var currentTypeIdParam = command.Parameters[0];
        Assert.That(currentTypeIdParam.Value, Is.EqualTo(typeId));
      }
    }

    [Test]
    [TestCaseSource(nameof(NodeIdentifiers))]
    public void UnionOfQuerues(string nodeId)
    {
      var typeId = GetTypeIdMapForNode(nodeId)[typeof(SimpleEntity)];
      var node = Domain.StorageNodeManager.GetNode(nodeId);

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<SimpleEntity>().Where(e => e.Id > 10)
          .Union(session.Query.All<SimpleEntity>().Where(e => e.Id < 10))
          .ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var command = args.Command;

        if (command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }

        var parameters = command.Parameters;
        foreach (var parameter in parameters) {
          var value = (int) ((DbParameter)parameter).Value;
          if (value == 10) {
            continue;
          }
          Assert.That(value, Is.EqualTo(typeId));
        }
      }
    }

    [Test]
    [TestCaseSource(nameof(NodeIdentifiers))]
    public void InterfaceTest(string nodeId)
    {
      var typeIds = GetTypeIdMapForNode(nodeId);
      var node = Domain.StorageNodeManager.GetNode(nodeId);

      var rootTypeId = typeIds[typeof(InterfaceHierarchyRoot)];
      var midTypeId = typeIds[typeof(InterfaceHierarchyMid)];
      var leafTypeId = typeIds[typeof(InterfaceHierarchyLeaf)];

      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<IBaseEnity>().ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var command = args.Command;
        var commandText = (ReadOnlySpan<char>) command.CommandText;
        if (commandText.Contains((ReadOnlySpan<char>) rootTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) midTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) leafTypeId.ToString(), StringComparison.Ordinal)) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }

        Assert.That(command.Parameters.Count, Is.EqualTo(4));
        Assert.That(
          command.Parameters.Cast<DbParameter>()
            .Select(p => (int) p.Value)
            .All(pV => pV == rootTypeId || pV == midTypeId || pV == leafTypeId),
          Is.True);
        Assert.That(command.Parameters.Cast<DbParameter>().Any(p => ((int)p.Value) == rootTypeId), Is.True);
        Assert.That(command.Parameters.Cast<DbParameter>().Any(p => ((int)p.Value) == midTypeId), Is.True);
        Assert.That(command.Parameters.Cast<DbParameter>().Any(p => ((int)p.Value) == leafTypeId), Is.True);
      }
    }

    [Test]
    [TestCaseSource(nameof(NodeIdentifiers))]
    public void ConcreteTableHierarchyTest(string nodeId)
    {
      var typeIds = GetTypeIdMapForNode(nodeId);
      var node = Domain.StorageNodeManager.GetNode(nodeId);

      var rootTypeId = typeIds[typeof(BasicConcreteHierarchyRoot)];
      var midTypeId = typeIds[typeof(BasicConcreteHierarchyMid)];
      var leafTypeId = typeIds[typeof(BasicConcreteHierarchyLeaf)];

      using (var session = node.OpenSession())
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
        var command = args.Command;

        var commandText = (ReadOnlySpan<char>) command.CommandText;
        if (commandText.Contains((ReadOnlySpan<char>) rootTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) midTypeId.ToString(), StringComparison.Ordinal)
          || commandText.Contains((ReadOnlySpan<char>) leafTypeId.ToString(), StringComparison.Ordinal)) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }

        Assert.That(
          command.Parameters.Cast<DbParameter>()
            .Select(p => (int) p.Value)
            .All(pV => pV == rootTypeId || pV == midTypeId || pV == leafTypeId),
          Is.True);
      }
    }

    [Test]
    [TestCaseSource(nameof(NodeIdentifiers))]
    public void CachingTest(string nodeId)
    {
      var typeId = GetTypeIdMapForNode(nodeId)[typeof(SimpleEntity)];

      var stringFilter = "a";

      var node = Domain.StorageNodeManager.GetNode(nodeId);
      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.Execute("q1", q => q.All<SimpleEntity>().Where(s => s.Name == stringFilter)).ToList();
        result = session.Query.Execute("q1", q => q.All<SimpleEntity>().Where(s => s.Name == stringFilter)).ToList();
        result = session.Query.Execute("q1", q => q.All<SimpleEntity>().Where(s => s.Name == stringFilter)).ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var command = args.Command;

        if (command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }


        if (command.Parameters.Count != 2) {
          throw new AssertionException("Wrong number of paramters");
        }

        var parameter = command.Parameters[1];

        if (((int) parameter.Value) != typeId)
          throw new AssertionException("Wrong value of paramters");
      }
    }

    private Dictionary<Type, int> GetTypeIdMapForNode(string name)
    {
      return name switch {
        WellKnown.DefaultNodeId => defaultNodeTypeIds,
        AdditionalNode1Name => additionalNode1TypeIds,
        AdditionalNode2Name => additionalNode2TypeIds,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
      };
    }
  }

  [TestFixture]
  public class MultinodeTypeIdAsParameterTest: AutoBuildTest
  {
    private const string AdditionalNode1Name = "Additional1";
    private const string AdditionalNode2Name = "Additional2";

    private readonly Dictionary<Type, int> defaultNodeTypeIds = new();
    private readonly Dictionary<Type, int> additionalNode1TypeIds = new();
    private readonly Dictionary<Type, int> additionalNode2TypeIds = new();

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);

      return configuration;
    }

    private NodeConfiguration BuildNodeConfiguration(string name, string schemaName)
    {
      var config = new NodeConfiguration(name);
      config.SchemaMapping.Add(WellKnownSchemas.SqlServerDefaultSchema, schemaName);
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);

      _ = domain.StorageNodeManager.AddNode(BuildNodeConfiguration(AdditionalNode1Name, WellKnownSchemas.Schema1));
      _ = domain.StorageNodeManager.AddNode(BuildNodeConfiguration(AdditionalNode2Name, WellKnownSchemas.Schema2));

      return domain;
    }

    protected override void PopulateData()
    {
      var defaultNode = Domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
      var additional1 = Domain.StorageNodeManager.GetNode(AdditionalNode1Name);
      var additional2 = Domain.StorageNodeManager.GetNode(AdditionalNode2Name);

      foreach (var typeInfo in Domain.Model.Types.Entities) {
        defaultNodeTypeIds.Add(typeInfo.UnderlyingType, defaultNode.TypeIdRegistry[typeInfo]);
        additionalNode1TypeIds.Add(typeInfo.UnderlyingType, additional1.TypeIdRegistry[typeInfo]);
        additionalNode2TypeIds.Add(typeInfo.UnderlyingType, additional2.TypeIdRegistry[typeInfo]);
      }
    }


    [Test]
    [TestCase(WellKnown.DefaultNodeId, TestName = "DefaultNodeSimpleQueryTest")]
    [TestCase(AdditionalNode1Name, TestName = "AdditionalNode1SimpleQueryTest")]
    [TestCase(AdditionalNode2Name, TestName = "AdditionalNode2SimpleQueryTest")]
    public void SimpleQueryTest(string nodeName)
    {
      var typeId = GetTypeIdMapForNode(nodeName)[typeof(SimpleEntity)];

      var node = Domain.StorageNodeManager.GetNode(nodeName);
      using (var session = node.OpenSession())
      using (var tx = session.OpenTransaction()) {

        session.Events.DbCommandExecuting += OnDbCommandExecuting;

        var result = session.Query.All<SimpleEntity>().ToList();

        session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      }

      void OnDbCommandExecuting(object sender, DbCommandEventArgs args)
      {
        var command = args.Command;

        if (args.Command.CommandText.Contains(typeId.ToString())) {
          throw new AssertionException("Not all TypeId mentions were replaced");
        }
      }
    }

    private Dictionary<Type, int> GetTypeIdMapForNode(string name)
    {
      return name switch {
        WellKnown.DefaultNodeId => defaultNodeTypeIds,
        AdditionalNode1Name => additionalNode1TypeIds,
        AdditionalNode2Name => additionalNode2TypeIds,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
      };
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.TypeIdAsParameterTestModel
{
  namespace Upgraders
  {

    public class DefaultNodeTypeIdsAssigner : Xtensive.Orm.Upgrade.UpgradeHandler
    {
      public override bool IsEnabled {
        get { return this.UpgradeContext.NodeConfiguration.NodeId == WellKnown.DefaultNodeId; }
      }


      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntity).FullName, 100);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntityWithIncludedId).FullName, 101);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyRoot).FullName, 102);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyMid).FullName, 103);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyLeaf).FullName, 104);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyRoot).FullName, 105);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyMid).FullName, 106);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyLeaf).FullName, 107);
      }
    }

    public class AdditionalNode1TypeIdsAssigner : Xtensive.Orm.Upgrade.UpgradeHandler
    {
      public override bool IsEnabled {
        get { return this.UpgradeContext.NodeConfiguration.NodeId == "Additional1"; }
      }


      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntity).FullName, 200);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntityWithIncludedId).FullName, 201);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyRoot).FullName, 202);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyMid).FullName, 203);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyLeaf).FullName, 204);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyRoot).FullName, 205);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyMid).FullName, 206);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyLeaf).FullName, 207);
      }
    }

    public class AdditionalNode2TypeIdsAssigner : Xtensive.Orm.Upgrade.UpgradeHandler
    {
      public override bool IsEnabled {
        get { return this.UpgradeContext.NodeConfiguration.NodeId == "Additional2"; }
      }


      public override bool CanUpgradeFrom(string oldVersion) => true;

      public override void OnPrepare()
      {
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntity).FullName, 300);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(SimpleEntityWithIncludedId).FullName, 301);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyRoot).FullName, 302);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyMid).FullName, 303);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(BasicConcreteHierarchyLeaf).FullName, 304);

        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyRoot).FullName, 305);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyMid).FullName, 306);
        UpgradeContext.UserDefinedTypeMap.Add(typeof(InterfaceHierarchyLeaf).FullName, 307);
      }
    }
  }


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
