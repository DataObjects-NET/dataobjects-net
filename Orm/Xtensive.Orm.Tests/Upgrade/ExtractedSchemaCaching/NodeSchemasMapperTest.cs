using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms.VisualStyles;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Tests.Upgrade.Multinode
{
  public class NodeSchemasMapperTest : AutoBuildTest
  {
    [Test]
    public void SingleSchemaNodesTest()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.Not.True);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "model1");
      nodeConfiguration1.Lock();

      var nodeConfiguration2 = new NodeConfiguration("TestNode2");
      nodeConfiguration2.SchemaMapping.Add("dbo", "model2");
      nodeConfiguration2.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var mapper = new NodeSchemasMapper(domainConfiguration, defaultSchemaInfo);
      var mapping = mapper.Map(catalogName, defaultNodeConfiguration, nodeConfiguration1);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(1));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model1"));

      mapping = mapper.Map(catalogName, defaultNodeConfiguration, nodeConfiguration2);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(1));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model2"));

      mapping = mapper.Map(catalogName, nodeConfiguration1, nodeConfiguration2);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(1));
      Assert.That(mapping.Apply("model1"), Is.EqualTo("model2"));

      mapping = mapper.Map(catalogName, nodeConfiguration2, nodeConfiguration1);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(1));
      Assert.That(mapping.Apply("model2"), Is.EqualTo("model1"));
    }

    [Test]
    public void MultischemaNodesTest()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).ToSchema("dbo");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace + "1").ToSchema("second");


      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.Not.True);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "model1");
      nodeConfiguration1.SchemaMapping.Add("second", "model3");
      nodeConfiguration1.Lock();

      var nodeConfiguration2 = new NodeConfiguration("TestNode2");
      nodeConfiguration2.SchemaMapping.Add("dbo", "model2");
      nodeConfiguration2.SchemaMapping.Add("second", "model4");
      nodeConfiguration2.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var mapper = new NodeSchemasMapper(domainConfiguration, defaultSchemaInfo);
      var mapping = mapper.Map(catalogName, defaultNodeConfiguration, nodeConfiguration1);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model1"));
      Assert.That(mapping.Apply("second"), Is.EqualTo("model3"));

      mapping = mapper.Map(catalogName, defaultNodeConfiguration, nodeConfiguration2);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model2"));
      Assert.That(mapping.Apply("second"), Is.EqualTo("model4"));


      mapping = mapper.Map(catalogName, nodeConfiguration1, nodeConfiguration2);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("model1"), Is.EqualTo("model2"));
      Assert.That(mapping.Apply("model3"), Is.EqualTo("model4"));

      mapping = mapper.Map(catalogName, nodeConfiguration2, nodeConfiguration1);
      Assert.That(mapping, Is.Not.Null);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("model2"), Is.EqualTo("model1"));
      Assert.That(mapping.Apply("model4"), Is.EqualTo("model3"));
    }

    [Test]
    public void MultidatabaseAndSingleSchemaNodeTest()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultDatabase = "DO40-Tests";
      domainConfiguration.DefaultSchema = "dbo";
      domainConfiguration.Databases.Add("DO40-Tests");
      domainConfiguration.Databases.Add("DO-Tests-1");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).To("DO40-Tests", "dbo");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace + "1").To("DO40-Tests", "second");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace + "2").To("DO-Tests-1", "dbo");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace + "3").To("DO-Tests-1", "third");


      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.Not.True);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.DatabaseMapping.Add("DO40-Tests", "DO40-Tests");
      nodeConfiguration1.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "model1");
      nodeConfiguration1.SchemaMapping.Add("second", "model3");
      nodeConfiguration1.SchemaMapping.Add("third", "model5");

      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");
      var mapper = new NodeSchemasMapper(domainConfiguration, defaultSchemaInfo);

      var mapping = mapper.Map("DO40-Tests", defaultNodeConfiguration, nodeConfiguration1);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model1"));
      Assert.That(mapping.Apply("second"), Is.EqualTo("model3"));

      mapping = mapper.Map("DO-Tests-1", defaultNodeConfiguration, nodeConfiguration1);
      Assert.That(mapping.Count, Is.EqualTo(2));
      Assert.That(mapping.Apply("dbo"), Is.EqualTo("model1"));
      Assert.That(mapping.Apply("third"), Is.EqualTo("model5"));
    }
  }
}
