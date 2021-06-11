using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel;

namespace Xtensive.Orm.Tests.Storage.InlineQueriesCachingTestModel.TestRunners
{
  public class ParameterTestRunner : TestRunnerBase
  {
    private static readonly string thisClassName = nameof(ParameterTestRunner);

    protected override string ThisClassName => thisClassName;

    public void GeneralAccessScalar(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.GeneralAccessScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      GeneralAccessScalarInternal(session, thisMethodName, local1, local2, local3);
    }

    public void GeneralAccessSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.GeneralAccessSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      GeneralAccessScalarInternal(session, thisMethodName, local1, local2, local3);
    }

    public void GeneralAccessOrderedSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.GeneralAccessOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      GeneralAccessScalarInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromNestedMethodScalar(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromNestedMethodScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromNestedMethodScalarInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromNestedMethodSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromNestedMethodSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromNestedMethodSequenceInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromNestedMethodOrderedSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromNestedMethodOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromNestedMethodOrderedSequenceInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromLocalFuncScalar(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromLocalFuncScalar);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromLocalFuncScalarInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromLocalFuncSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromLocalFuncSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromLocalFuncSequenceInternal(session, thisMethodName, local1, local2, local3);
    }

    public void AccessFromLocalFuncOrderedSequence(Session session)
    {
      var thisMethodName = nameof(ParameterTestRunner.AccessFromLocalFuncOrderedSequence);

#pragma warning disable IDE0018 // Inline variable declaration
      ParameterContainer local1, local2, local3;
      InitVariables(thisMethodName, out local1, out local2, out local3);
#pragma warning restore IDE0018 // Inline variable declaration

      AccessFromLocalFuncOrderedSequenceInternal(session, thisMethodName, local1, local2, local3);
    }

    private void GeneralAccessScalarInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      Assert.That(result, Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void GeneralAccessSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query, parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query, parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query, parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query, parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query, parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void GeneralAccessOrderedSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      Assert.That(result.Count(), Is.EqualTo(1));
      result = session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void AccessFromNestedMethodScalarInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = Execute1();
      Assert.That(result, Is.EqualTo(1));
      result = Execute2();
      Assert.That(result, Is.EqualTo(1));
      result = Execute3();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      int Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      }
      int Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      }
      int Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
      }
    }

    private void AccessFromNestedMethodSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      IEnumerable<Enterprise> Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query, parameter1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query, parameter2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query, parameter3);
      }
    }

    private void AccessFromNestedMethodOrderedSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

      var result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = Execute1();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute2();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = Execute3();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));

      IEnumerable<Enterprise> Execute1()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      }
      IEnumerable<Enterprise> Execute2()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      }
      IEnumerable<Enterprise> Execute3()
      {
        return session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
      }
    }

    private void AccessFromLocalFuncScalarInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<int> execute1 = () => session.Query.Execute(cachingKey, (q) => query.Count(), parameter1);
      Func<int> execute2 = () => session.Query.Execute(cachingKey, (q) => query.Count(), parameter2);
      Func<int> execute3 = () => session.Query.Execute(cachingKey, (q) => query.Count(), parameter3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result, Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result, Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void AccessFromLocalFuncSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query, parameter1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query, parameter2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query, parameter3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
    }

    private void AccessFromLocalFuncOrderedSequenceInternal(Session session, string methodName, ParameterContainer parameter1, ParameterContainer parameter2, ParameterContainer parameter3)
    {
      var currentQueryCountInCache = session.Domain.QueryCache.Count;
      var query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameField);
      var cachingKey = ComposeKey(ThisClassName, methodName, "Field");

#pragma warning disable IDE0039 // Use local function
      Func<IEnumerable<Enterprise>> execute1 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter1);
      Func<IEnumerable<Enterprise>> execute2 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter2);
      Func<IEnumerable<Enterprise>> execute3 = () => session.Query.Execute(cachingKey, (q) => query.OrderBy(e => e.Id), parameter3);
#pragma warning restore IDE0039 // Use local function

      var result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.NameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "Prop");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameField);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseField");

      result = execute1.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute2.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      result = execute3.Invoke();
      Assert.That(result.Count(), Is.EqualTo(1));
      Assert.That(session.Domain.QueryCache.Count, Is.EqualTo(currentQueryCountInCache + 1));
      currentQueryCountInCache++;

      query = session.Query.All<Enterprise>().Where(q => q.Name == parameter1.BaseNameProp);
      cachingKey = ComposeKey(ThisClassName, methodName, "BaseProp");

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
