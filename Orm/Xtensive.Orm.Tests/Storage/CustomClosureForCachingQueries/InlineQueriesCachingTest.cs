using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel
{
  [HierarchyRoot]
  public class Enterprise : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public DateTime CreationDate { get; set; }
  }

  public class BaseParameterContainer
  {
    public string BaseNameField;

    public string BaseNameProp { get; set; }
  }

  public class ParameterContainer : BaseParameterContainer
  {
    public string NameField;

    public string NameProp { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class InlineQueriesCachingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Enterprise).Assembly, typeof(Enterprise).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }

    protected override void PopulateData()
    {
      var testNames = GetTestNames();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach (var name in testNames) {
          _ = new Enterprise() { Name = name };
        }
        tx.Complete();
      }
    }

    [Test]
    public void LocalVariableTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new LocalVariableTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void ParameterTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new ParameterTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void StaticFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new StaticFieldTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void StaticPropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new StaticPropertyTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void InstanceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InstanceFieldTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void InstancePropertyTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new InstancePropertyTestRunner();
        tester.GeneralAccessScalar(session);
        tester.GeneralAccessSequence(session);
        tester.GeneralAccessOrderedSequence(session);

        tester.AccessFromNestedMethodScalar(session);
        tester.AccessFromNestedMethodSequence(session);
        tester.AccessFromNestedMethodOrderedSequence(session);

        tester.AccessFromLocalFuncScalar(session);
        tester.AccessFromLocalFuncSequence(session);
        tester.AccessFromLocalFuncOrderedSequence(session);
      }
    }

    [Test]
    public void NestedClassTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var tester = new NestedClassTestRunner();
        tester.AccessInsideNestedClassTest(session);
        tester.AccessOutideNestedClassTest(session);
      }
    }

    private IEnumerable<string> GetTestNames()
    {
      var classNames = new string[] {
        nameof(LocalVariableTestRunner),
        nameof(ParameterTestRunner),
        nameof(StaticFieldTestRunner),
        nameof(StaticPropertyTestRunner),
        nameof(InstanceFieldTestRunner),
        nameof(InstancePropertyTestRunner)
      };
      var tests = new string[] {
        nameof(StaticFieldTestRunner.GeneralAccessScalar),
        nameof(StaticFieldTestRunner.GeneralAccessSequence),
        nameof(StaticFieldTestRunner.GeneralAccessOrderedSequence),
        nameof(StaticFieldTestRunner.AccessFromNestedMethodScalar),
        nameof(StaticFieldTestRunner.AccessFromNestedMethodSequence),
        nameof(StaticFieldTestRunner.AccessFromNestedMethodOrderedSequence),
        nameof(StaticFieldTestRunner.AccessFromLocalFuncScalar),
        nameof(StaticFieldTestRunner.AccessFromLocalFuncSequence),
        nameof(StaticFieldTestRunner.AccessFromLocalFuncOrderedSequence),
        nameof(StaticFieldTestRunner.CacheAcrossMethodsScalar),
        nameof(StaticFieldTestRunner.CacheAcrossMethodsSequence),
        nameof(StaticFieldTestRunner.CacheAcrossMethodsOrderedSequence)
      };

      foreach (var className1 in classNames) {
        foreach (var methodName in tests) {
          yield return className1 + methodName + "BaseField1";
          yield return className1 + methodName + "BaseProp1";
          yield return className1 + methodName + "Field1";
          yield return className1 + methodName + "Prop1";
          yield return className1 + methodName + "BaseField2";
          yield return className1 + methodName + "BaseProp2";
          yield return className1 + methodName + "Field2";
          yield return className1 + methodName + "Prop2";
          yield return className1 + methodName + "BaseField3";
          yield return className1 + methodName + "BaseProp3";
          yield return className1 + methodName + "Field3";
          yield return className1 + methodName + "Prop3";
        }
      }

      var className2 = nameof(NestedClassTestRunner);
      tests = new string[] {
        nameof(NestedClassTestRunner.AccessInsideNestedClassTest),
        nameof(NestedClassTestRunner.AccessOutideNestedClassTest),
      };

      foreach (var methodName in tests) {
        foreach (var index in Enumerable.Range(1, 8)) {
          yield return className2 + methodName + "BaseField" + index;
          yield return className2 + methodName + "BaseProp" + index;
          yield return className2 + methodName + "Field" + index;
          yield return className2 + methodName + "Prop" + index;
        }
      }
    }


    //private void TestBody(SomeType parameter)
    //{
    //  var localParameter = new SomeType() { Name = "localParameter" };

    //  using (var session = Domain.OpenSession())
    //  using (var tx = session.OpenTransaction()) {
    //    var entsLocal = session.Query.All<Enterprise>().Where(q => q.Name == localParameter.Name);
    //    var entsParameter = session.Query.All<Enterprise>().Where(q => q.Name == parameter.Name);
    //    var entsstaticField = session.Query.All<Enterprise>().Where(q => q.Name == staticField.Name);
    //    var entsStaticProperty = session.Query.All<Enterprise>().Where(q => q.Name == StaticProperty.Name);
    //    var entsinstanceField = session.Query.All<Enterprise>().Where(q => q.Name == instanceField.Name);
    //    var entsInstanceProperty = session.Query.All<Enterprise>().Where(q => q.Name == InstanceProperty.Name);

    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key1", (q) => entsLocal.Count(), InstanceProperty));

    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key2", (q) => entsParameter.Count(), InstanceProperty));

    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key5", (q) => entsinstanceField.Count(), InstanceProperty));

    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key6", (q) => entsInstanceProperty.Count(), InstanceProperty));


    //    Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => q.All<Enterprise>().Where(q => q.Name == staticField.Name).Count()));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => q.All<Enterprise>().Where(q => q.Name == staticField.Name).Count()));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key3", (q) => entsstaticField.Count(), InstanceProperty));

    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), localParameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), parameter));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), staticField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), StaticProperty));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), instanceField));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key4", (q) => entsStaticProperty.Count(), InstanceProperty));

    //    //Assert.AreEqual(0, session.Query.Execute("the_key", (q) => ents.Count(), someType2));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key", (q) => ents.Count(), someType3));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key", (q) => ents.Count(), someType4));
    //    //Assert.AreEqual(0, session.Query.Execute("the_key", (q) => ents.Count(), someType5));
    //  }
    //}
  }
}
