using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public class LocalVariableTestRunner : TestRunnerBase
  {
    private readonly static string thisClassName = nameof(LocalVariableTestRunner);

    protected override string ThisClassName => thisClassName;

    public void GeneralAccessScalar(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.GeneralAccessScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessSequence(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.GeneralAccessSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query, local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query, local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query, local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query, local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void GeneralAccessOrderedSequence(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.GeneralAccessOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    public void AccessFromNestedMethodScalar(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromNestedMethodScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      }
      int Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      }
      int Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), local3);
      }
    }

    public void AccessFromNestedMethodSequence(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromNestedMethodSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query, local1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query, local2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query, local3);
      }
    }

    public void AccessFromNestedMethodOrderedSequence(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromNestedMethodOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
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
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local3);
      }
    }

    public void AccessFromLocalFuncScalar(Session session)
    {
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromLocalFuncScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<int> execute1 = () => session.Query.Execute(cachingKey, (q) => query.Count(), local1);
      Func<int> execute2 = () => session.Query.Execute(cachingKey, (q) => query.Count(), local2);
      Func<int> execute3 = () => session.Query.Execute(cachingKey, (q) => query.Count(), local3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
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
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromLocalFuncSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query, local1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query, local2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query, local3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
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
      var thisMethodName = nameof(LocalVariableTestRunner.AccessFromLocalFuncOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameField);
      var cachingKey = ComposeKey(ThisClassName, thisMethodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), local3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.NameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == local1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, thisMethodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }
  }
}
