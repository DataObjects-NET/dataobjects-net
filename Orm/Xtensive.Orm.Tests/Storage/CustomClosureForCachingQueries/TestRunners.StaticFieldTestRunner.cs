using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public class StaticFieldTestRunner : TestRunnerBase
  {
    private readonly static string thisClassName = nameof(StaticFieldTestRunner);

    private static ParameterContainer field1;
    private static ParameterContainer field2;
    private static ParameterContainer field3;

    protected override string ThisClassName => thisClassName;

    public void GeneralAccessScalar(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.GeneralAccessScalar);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), field3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.GeneralAccessSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query, field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query, field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query, field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query, field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.GeneralAccessOrderedSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void AccessFromNestedMethodScalar(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromNestedMethodScalar);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      int Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      }
      int Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      }
      int Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), field3);
      }
    }

    public void AccessFromNestedMethodSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromNestedMethodSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      IEnumerable<Enterprise> Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query, field1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query, field2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query, field3);
      }
    }

    public void AccessFromNestedMethodOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromNestedMethodOrderedSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      IEnumerable<Enterprise> Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
      }
    }

    public void AccessFromLocalFuncScalar(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromLocalFuncScalar);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<int> execute1 = () => session.Query.Execute(cachingKey, (q) => query.Count(), field1);
      Func<int> execute2 = () => session.Query.Execute(cachingKey, (q) => query.Count(), field2);
      Func<int> execute3 = () => session.Query.Execute(cachingKey, (q) => query.Count(), field3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void AccessFromLocalFuncSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromLocalFuncSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query, field1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query, field2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query, field3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void AccessFromLocalFuncOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticFieldTestRunner.AccessFromLocalFuncOrderedSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void CacheAcrossMethodsScalar(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.CacheAcrossMethodsScalar);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = ScalarMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);

      result = ScalarMethod1(session, cachingKey, queryable1);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod1(session, cachingKey, queryable2);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod1(session, cachingKey, queryable3);
      Assert.That(result, Is.EqualTo(1));

      result = ScalarMethod2(session, cachingKey, queryable1);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod2(session, cachingKey, queryable2);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod2(session, cachingKey, queryable3);
      Assert.That(result, Is.EqualTo(1));

      result = ScalarMethod3(session, cachingKey, queryable1);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod3(session, cachingKey, queryable2);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod3(session, cachingKey, queryable3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void CacheAcrossMethodsSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.CacheAcrossMethodsSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = SequenceMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);

      result = SequenceMethod1(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod1(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod1(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = SequenceMethod2(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod2(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod2(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = SequenceMethod3(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod3(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod3(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void CacheAcrossMethodsOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.CacheAcrossMethodsOrderedSequence);

      InitVariables(thisMethodName, out field1, out field2, out field3);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = OrderedSequenceMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == field1.BaseNameProp);

      result = OrderedSequenceMethod1(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod1(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod1(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = OrderedSequenceMethod2(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod2(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod2(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));

      result = OrderedSequenceMethod3(session, cachingKey, queryable1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod3(session, cachingKey, queryable2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod3(session, cachingKey, queryable3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private int ScalarMethod1(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.Count(), field1);

    private int ScalarMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.Count(), field2);

    private int ScalarMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.Count(), field3);

    private IEnumerable<Enterprise> SequenceMethod1(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, field1);

    private IEnumerable<Enterprise> SequenceMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, field2);

    private IEnumerable<Enterprise> SequenceMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, field3);

    private IEnumerable<Enterprise> OrderedSequenceMethod1(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field1);

    private IEnumerable<Enterprise> OrderedSequenceMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field2);

    private IEnumerable<Enterprise> OrderedSequenceMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), field3);
  }
}
