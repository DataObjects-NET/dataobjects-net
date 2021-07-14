using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public class StaticPropertyTestRunner : TestRunnerBase
  {
    private readonly static string thisClassName = nameof(StaticPropertyTestRunner);

    private static ParameterContainer Prop1 { get; set; }
    private static ParameterContainer Prop2 { get; set; }
    private static ParameterContainer Prop3 { get; set; }

    protected override string ThisClassName => thisClassName;

    public void GeneralAccessScalar(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.GeneralAccessScalar);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.GeneralAccessSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query, Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query, Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query, Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query, Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.GeneralAccessOrderedSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void AccessFromNestedMethodScalar(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromNestedMethodScalar);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      }
      int Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      }
      int Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
      }
    }

    public void AccessFromNestedMethodSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromNestedMethodSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query, Prop1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query, Prop2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query, Prop3);
      }
    }

    public void AccessFromNestedMethodOrderedSequence(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromNestedMethodOrderedSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
      }
    }

    public void AccessFromLocalFuncScalar(Session session)
    {
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromLocalFuncScalar);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<int> execute1 = () => session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
      Func<int> execute2 = () => session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
      Func<int> execute3 = () => session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromLocalFuncSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query, Prop1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query, Prop2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query, Prop3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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
      var thisMethodName = nameof(StaticPropertyTestRunner.AccessFromLocalFuncOrderedSequence);

      InitProperties(thisMethodName);

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
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

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = ScalarMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));
      result = ScalarMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result, Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);

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

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = SequenceMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = SequenceMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);

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

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var sharedQueryable = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      var result = OrderedSequenceMethod1(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod2(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = OrderedSequenceMethod3(session, cachingKey, sharedQueryable);
      Assert.That(result.Count(), Is.EqualTo(1));

      var queryable1 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable2 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);
      var queryable3 = session.Query.All<Enterprise>().Where(q => q.Name == Prop1.BaseNameProp);

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
      session.Query.Execute(cachingKey, (q) => query.Count(), Prop1);
    private int ScalarMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.Count(), Prop2);
    private int ScalarMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.Count(), Prop3);
    private IEnumerable<Enterprise> SequenceMethod1(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, Prop1);
    private IEnumerable<Enterprise> SequenceMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, Prop2);
    private IEnumerable<Enterprise> SequenceMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query, Prop3);
    private IEnumerable<Enterprise> OrderedSequenceMethod1(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop1);
    private IEnumerable<Enterprise> OrderedSequenceMethod2(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop2);
    private IEnumerable<Enterprise> OrderedSequenceMethod3(Session session, string cachingKey, IQueryable<Enterprise> query) =>
      session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), Prop3);

    private void InitProperties(string methodName)
    {
      Prop1 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField1",
        BaseNameProp = ThisClassName + methodName + "BaseProp1",
        NameField = ThisClassName + methodName + "Field1",
        NameProp = ThisClassName + methodName + "Prop1",
      };
      Prop2 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField2",
        BaseNameProp = ThisClassName + methodName + "BaseProp2",
        NameField = ThisClassName + methodName + "Field2",
        NameProp = ThisClassName + methodName + "Prop2",
      };
      Prop3 = new ParameterContainer() {
        BaseNameField = ThisClassName + methodName + "BaseField3",
        BaseNameProp = ThisClassName + methodName + "BaseProp3",
        NameField = ThisClassName + methodName + "Field3",
        NameProp = ThisClassName + methodName + "Prop3",
      };
    }
  }
}
