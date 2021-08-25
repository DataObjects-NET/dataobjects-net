using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public class NestedClassTestRunner : TestRunnerBase
  {
    public class NestedClass
    {
      private string methodName;

      public static ParameterContainer StaticField1;
      public static ParameterContainer StaticField2;

      public ParameterContainer Field1;
      public ParameterContainer Field2;

      public static ParameterContainer StaticProp1 { get; set; }
      public static ParameterContainer StaticProp2 { get; set; }

      public ParameterContainer Prop1 { get; set; }
      public ParameterContainer Prop2 { get; set; }

      public void StaticFieldScalarTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticField2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void StaticFieldSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query, StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query, StaticField1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query, StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query, StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void StaticFieldOrderedSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticField1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticField1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e=>e.Id), StaticField2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void StaticPropertyScalarTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), StaticProp2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;
      }

      public void StaticPropertySequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query, StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;
      }

      public void StaticPropertyOrderedSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == StaticProp1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), StaticProp2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;
      }

      public void FieldTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void FieldScalarTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void FieldSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void FieldOrderedSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Field1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Field2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void PropertyScalarTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
        Assert.That(result, Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void PropertySequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query, Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query, Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query, Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query, Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query, Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void PropertyOrderedSequenceTest(Session session)
      {
        var currentQueryCountInCache = session.Domain.QueryCache.Count;
        var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
        var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField");

        var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
        currentQueryCountInCache++;

        query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
        cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop");

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
        Assert.That(result.Count(), Is.EqualTo(1));

        result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      }

      public void InitContainers(string methodName)
      {
        this.methodName = methodName;

        StaticField1 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField1",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp1",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field1",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop1",
        };
        StaticField2 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField2",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp2",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field2",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop2",
        };

        StaticProp1 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField3",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp3",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field3",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop3",
        };
        StaticField2 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField4",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp4",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field4",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop4",
        };

        Field1 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField5",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp5",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field5",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop5",
        };
        Field2 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField6",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp6",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field6",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop6",
        };

        Prop1 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField7",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp7",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field7",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop7",
        };
        Prop2 = new ParameterContainer() {
          BaseNameField = NestedClassTestRunner.thisClassName + methodName + "BaseField8",
          BaseNameProp = NestedClassTestRunner.thisClassName + methodName + "BaseProp8",
          NameField = NestedClassTestRunner.thisClassName + methodName + "Field8",
          NameProp = NestedClassTestRunner.thisClassName + methodName + "Prop8",
        };
      }
    }

    private static readonly string thisClassName = nameof(NestedClassTestRunner);


    protected override string ThisClassName => thisClassName;

    public void AccessInsideNestedClassTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      nestedClass.StaticFieldScalarTest(session);
      nestedClass.StaticFieldSequenceTest(session);
      nestedClass.StaticFieldOrderedSequenceTest(session);

      nestedClass.StaticPropertyScalarTest(session);
      nestedClass.StaticPropertySequenceTest(session);
      nestedClass.StaticPropertyOrderedSequenceTest(session);

      nestedClass.FieldScalarTest(session);
      nestedClass.FieldSequenceTest(session);
      nestedClass.FieldOrderedSequenceTest(session);

      nestedClass.PropertyScalarTest(session);
      nestedClass.PropertySequenceTest(session);
      nestedClass.PropertyOrderedSequenceTest(session);
    }

    public void AccessOutideNestedClassTest(Session session)
    {
      StaticFieldScalarTest(session);

      StaticPropertyScalarTest(session);
    }

    private void StaticFieldScalarTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //staticFields
      var methodName = "StaticFieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField1");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp1");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field1");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop1");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticField2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void StaticFieldSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //staticFields
      var methodName = "StaticFieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField1");

      var result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp1");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field1");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop1");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void StaticFieldOrderedSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //staticFields
      var methodName = "StaticFieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField1");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp1");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field1");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticField1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop1");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField1);
      Assert.That(result.Count, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticField2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void StaticPropertyScalarTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      // static properties
      var methodName = "StaticPropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField2");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp2");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field2");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop2");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), NestedClass.StaticProp2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void StaticPropertySequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      // static properties
      var methodName = "StaticPropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField2");

      var result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp2");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field2");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop2");

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void StaticPropertyOrderedSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      // static properties
      var methodName = "StaticPropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField2");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp2");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field2");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == NestedClass.StaticProp1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop2");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), NestedClass.StaticProp2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void FieldScalarTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //fields
      var methodName = "FieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField3");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp3");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field3");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop3");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Field2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void FieldSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //fields
      var methodName = "FieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField3");

      var result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp3");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field3");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop3");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void FieldOrderedSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //fields
      var methodName = "FieldTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField3");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp3");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field3");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Field1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop3");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;
    }

    private void PropertyScalarTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //props
      var methodName = "PropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField4");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp4");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field4");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop4");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop1);
      Assert.That(result, Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.Count(), nestedClass.Prop2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void PropertySequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      var methodName = "PropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField4");

      var result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp4");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field4");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop4");

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query, nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void PropertyOrderedSequenceTest(Session session)
    {
      var nestedClass = new NestedClass();
      nestedClass.InitContainers(nameof(NestedClassTestRunner.AccessInsideNestedClassTest));

      //props
      var methodName = "PropertyTest";

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameField);
      var cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseField4");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.BaseNameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "BaseProp4");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameField);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Field4");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == nestedClass.Prop1.NameProp);
      cachingKey = ComposeKey(nameof(NestedClass), methodName, "Prop4");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), nestedClass.Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }
  }
}
