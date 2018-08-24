using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.Multinode.TypeIdExtractionTestModel;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  [TestFixture]
  public class TypeIdAllocationTest
  {
    private const string FirstSchema = "Model1";
    private const string SecondSchema = "Model2";

    [Test]
    public void RecreateTest()
    {
      var mode = DomainUpgradeMode.Recreate;
      BuildInitialDomains();
      var domain = BuildMultinodeDomain(mode, mode, FirstSchema, SecondSchema);
      MainTest(domain);

      domain = BuildMultinodeDomain(mode, mode, SecondSchema, FirstSchema);
      MainTest(domain);
    }

    [Test]
    public void ValidateTest()
    {
      var mode = DomainUpgradeMode.Validate;
      BuildInitialDomains();
      var domain = BuildMultinodeDomain(mode, mode, FirstSchema, SecondSchema);
      MainTest(domain);

      domain = BuildMultinodeDomain(mode, mode, SecondSchema, FirstSchema);
      MainTest(domain);
    }

    [Test]
    public void SkipTest()
    {
      var mode = DomainUpgradeMode.Skip;
      BuildInitialDomains();
      var domain = BuildMultinodeDomain(mode, mode, FirstSchema, SecondSchema);
      MainTest(domain);

      domain = BuildMultinodeDomain(mode, mode, SecondSchema, FirstSchema);
      MainTest(domain);
    }

    [Test]
    public void PerformTest()
    {
      var mode = DomainUpgradeMode.Perform;
      BuildInitialDomains();
      var domain = BuildMultinodeDomain(mode, mode, FirstSchema, SecondSchema);
      MainTest(domain);

      domain = BuildMultinodeDomain(mode, mode, SecondSchema, FirstSchema);
      MainTest(domain);
    }

    [Test]
    public void PerformSafelyTest()
    {
      var mode = DomainUpgradeMode.PerformSafely;
      BuildInitialDomains();
      var domain = BuildMultinodeDomain(mode, mode, FirstSchema, SecondSchema);
      MainTest(domain);

      domain = BuildMultinodeDomain(mode, mode, SecondSchema, FirstSchema);
      MainTest(domain);
    }


    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      CheckRequirements();
    }


    public void MainTest(Domain domain)
    {
      var isRecreate = domain.Configuration.UpgradeMode==DomainUpgradeMode.Recreate;
      var domainModel = domain.Model;
      var defaultNode = domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
      var additionalNode = domain.StorageNodeManager.GetNode(FirstSchema) ?? domain.StorageNodeManager.GetNode(SecondSchema);
      var additionalNodeSchema = additionalNode.Id;
      var defaultNodeSchema = additionalNode.Id==FirstSchema ? SecondSchema : FirstSchema;

      if (!isRecreate) {
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => e.TypeId==defaultNode.TypeIdRegistry[e]), Is.True);
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => TypeIdentifierMapper.GetTypeId(e.UnderlyingType, defaultNodeSchema)==defaultNode.TypeIdRegistry[e]));
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => e.TypeId!=additionalNode.TypeIdRegistry[e]), Is.True);
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => TypeIdentifierMapper.GetTypeId(e.UnderlyingType, additionalNodeSchema)==additionalNode.TypeIdRegistry[e]));
      }
      else {
        Assert.That(domainModel.Types.Entities.All(e => e.TypeId==defaultNode.TypeIdRegistry[e]), Is.True);
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => defaultNode.TypeIdRegistry[e] < 300));
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => e.TypeId==additionalNode.TypeIdRegistry[e]), Is.True);
        Assert.That(domainModel.Types.Entities.Where(e => !e.IsSystem).All(e => additionalNode.TypeIdRegistry[e]<300));
      }
    }

    private Domain BuildMultinodeDomain(DomainUpgradeMode domainUpgradeMode, DomainUpgradeMode nodeUpgradeMode, string masterSchema, string slaveSchema)
    {
      var pair = BuildMultitnodeConfiguration(domainUpgradeMode, nodeUpgradeMode, masterSchema, slaveSchema);
      var domain = Domain.Build(pair.First);
      domain.StorageNodeManager.AddNode(pair.Second);
      return domain;
    }

    private void BuildInitialDomains()
    {
      foreach (var buildInitialConfiguration in BuildInitialConfigurations()) {
        using (var domain = Domain.Build(buildInitialConfiguration)) {
          var verificationResult = domain.Model.Types.Entities.Where(e => !e.IsSystem).All(el => el.TypeId==TypeIdentifierMapper.GetTypeId(el.UnderlyingType, buildInitialConfiguration.DefaultSchema));
          Assert.That(verificationResult, Is.True);
        }
      }
    }

    public virtual void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multischema);
    }

    private DomainConfiguration[] BuildInitialConfigurations()
    {
      var configurations = new DomainConfiguration[2];

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Laptop).Assembly, typeof (Laptop).Namespace);
      configuration.Name = "FirstSingleSchemaDomain";
      configuration.DefaultSchema = "Model1";
      configurations[0] = configuration;

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Laptop).Assembly, typeof (Laptop).Namespace);
      configuration.Name = "SecondSingleSchemaDomain";
      configuration.DefaultSchema = "Model2";
      configurations[1] = configuration;

      return configurations;
    }

    private Pair<DomainConfiguration, NodeConfiguration> BuildMultitnodeConfiguration(DomainUpgradeMode domainUpgradeMode, DomainUpgradeMode nodeUpgradeMode, string masterSchema, string slaveSchema)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.DefaultSchema = masterSchema;
      configuration.Types.Register(typeof (Laptop).Assembly, typeof (Laptop).Namespace);
      configuration.UpgradeMode = domainUpgradeMode;
      configuration.Name = "MultiNodeDomain";

      var nodeConfiguration = new NodeConfiguration(slaveSchema);
      nodeConfiguration.SchemaMapping.Add(masterSchema, slaveSchema);
      nodeConfiguration.UpgradeMode = nodeUpgradeMode;

      return new Pair<DomainConfiguration, NodeConfiguration>(configuration, nodeConfiguration);
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.Multinode.TypeIdExtractionTestModel
{
  public class TypeIdentifierMapper
  {
    private static Dictionary<string, int> typeIdOffsetMap = new Dictionary<string, int>();
    private static Dictionary<Type, int> typeIdOffsets = new Dictionary<Type, int>();

    public static int GetTypeIdOffset(Type type)
    {
      return typeIdOffsets[type];
    }

    public static int GetTypeId(Type type, string model)
    {
      var typeIdBase = typeIdOffsetMap[model];
      int typeIdOffset;
        if (!typeIdOffsets.TryGetValue(type, out typeIdOffset))
          throw new InvalidOperationException();
      return typeIdBase + typeIdOffset;
    }

    private static void SetTypeOffsets(IDictionary<Type, int> typeMap)
    {
      typeMap.Add(typeof(Laptop), 1);
      typeMap.Add(typeof(Manufacturer), 2);
      typeMap.Add(typeof(DisplayInfo), 3);
      typeMap.Add(typeof(CPUInfo), 4);
      typeMap.Add(typeof(IOPortsInfo), 5);
      typeMap.Add(typeof(KeyboardInfo), 6);
      typeMap.Add(typeof(StorageInfo), 7);
      typeMap.Add(typeof(GraphicsCardInfo), 8);
    }

    private static void SetBaseTypeIds()
    {
      typeIdOffsetMap["Model1"] = 300;
      typeIdOffsetMap["Model2"] = 400;
    }

    static TypeIdentifierMapper()
    {
      SetTypeOffsets(typeIdOffsets);
      SetBaseTypeIds();
    }
  }

  [HierarchyRoot]
  public class Laptop : Entity
  {
    [Field, Key]
    public int Id { get; set; }
    
    [Field]
    public Manufacturer Manufacturer { get; set; }

    [Field]
    public string SerialNumber { get; set; }

    [Field]
    public DisplayInfo DisplayInfo { get; set; }

    [Field]
    public CPUInfo CpuInfo { get; set; }

    [Field]
    public GraphicsCardInfo GraphicsCardInfo { get; set; }

    [Field]
    public StorageInfo StorageInfo { get; set; }

    [Field]
    public KeyboardInfo KeyboardInfo { get; set; }

    [Field]
    public IOPortsInfo IOPortsInfo { get; set; }
  }

  [HierarchyRoot]
  public class Manufacturer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class IOPortsInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public int UsbPortsCount { get; set; }

    [Field]
    public bool HasHDMIPort { get; set; }

    [Field]
    public int Usb30PortsCount { get; set; }

    [Field]
    public bool HasWiFiModule { get; set; }

    [Field]
    public bool HasEthernetPort { get; set; }

    [Field]
    public bool HasMicroInput { get; set; }

    [Field]
    public bool HasHeadphonesOutput { get; set; }

    [Field]
    [Association(PairTo = "IOPortsInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  [HierarchyRoot]
  public class DisplayInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Manufacturer Manufacturer { get; set; }

    [Field]
    public int Dpi { get; set; }

    [Field]
    public Resolution Resolution { get; set; }

    [Field]
    public Formats Format { get; set; }

    [Field]
    [Association(PairTo = "DisplayInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  [HierarchyRoot]
  public class CPUInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Manufacturer Manufacturer { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public float BaseClockSpeed { get; set; }

    [Field]
    public int Multiplier { get; set; }

    [Field]
    public string ArcitectureName { get; set; }

    [Field]
    [Association(PairTo = "CpuInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  [HierarchyRoot]
  public class GraphicsCardInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public ChipProducers ChipProducer { get; set; }

    [Field]
    public Manufacturer Vendor { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "GraphicsCardInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  [HierarchyRoot]
  public class StorageInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public Manufacturer Manufacturer { get; set; }

    [Field]
    public Capacity Capacity { get; set; }

    [Field]
    public bool IsSsd { get; set; }

    [Field]
    public int MaxWriteSpeed { get; set; }

    [Field]
    public int MaxReadSpeed { get; set; }

    [Field]
    [Association(PairTo = "StorageInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  [HierarchyRoot]
  public class KeyboardInfo : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public int KeysCount { get; set; }

    [Field]
    public KeyboardType Type { get; set; }

    [Field]
    [Association(PairTo = "KeyboardInfo")]
    public EntitySet<Laptop> Laptops { get; set; }
  }

  public class Resolution : Structure
  {
    [Field]
    public int Wide { get; set; }

    [Field]
    public int Hight { get; set; }
  }

  public class Capacity : Structure
  {
    [Field]
    public int Value { get; set; }

    public StorageCapacityMeasure Measure { get; set; }
  }

  public enum Formats
  {
    Normal,
    Wide,
    UltraWide,
  }

  public enum ChipProducers
  {
    Amd,
    Nvidia,
    Intel
  }

  public enum KeyboardType
  {
    First,
    Second,
    Third,
  }

  public enum StorageCapacityMeasure
  {
    Kilobytes,
    Megabytes,
    Gigabytes,
    Terabytes
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override bool IsEnabled
    {
      get{ return UpgradeContext.Configuration.Name=="SecondSingleSchemaDomain" || UpgradeContext.Configuration.Name=="FirstSingleSchemaDomain";}
    }
    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      var isNodeBuilding = UpgradeContext.ParentDomain!=null;
      var schemaName = (isNodeBuilding)
        ? UpgradeContext.NodeConfiguration.SchemaMapping.Apply(UpgradeContext.Configuration.DefaultSchema)
        : UpgradeContext.Configuration.DefaultSchema;

      UpgradeContext.UserDefinedTypeMap.Add(typeof (Laptop).FullName, TypeIdentifierMapper.GetTypeId(typeof (Laptop), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (Manufacturer).FullName, TypeIdentifierMapper.GetTypeId(typeof (Manufacturer), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (DisplayInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (DisplayInfo), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (CPUInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (CPUInfo), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (IOPortsInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (IOPortsInfo), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (KeyboardInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (KeyboardInfo), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (StorageInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (StorageInfo), schemaName));
      UpgradeContext.UserDefinedTypeMap.Add(typeof (GraphicsCardInfo).FullName, TypeIdentifierMapper.GetTypeId(typeof (GraphicsCardInfo), schemaName));
    }
  }
}