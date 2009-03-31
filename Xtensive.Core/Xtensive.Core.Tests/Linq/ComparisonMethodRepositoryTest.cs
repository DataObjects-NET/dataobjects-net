// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq.ComparisonExtraction;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class ComparisonMethodRepositoryTest
  {
    [Test]
    public void ComparableImplementationsTest()
    {
      var sampleMethods = new List<MethodInfo>();
      AddCompareToMethods(typeof (Int16), sampleMethods);
      AddCompareToMethods(typeof (Int32), sampleMethods);
      AddCompareToMethods(typeof (Int64), sampleMethods);
      AddCompareToMethods(typeof (UInt16), sampleMethods);
      AddCompareToMethods(typeof (UInt32), sampleMethods);
      AddCompareToMethods(typeof (UInt64), sampleMethods);
      AddCompareToMethods(typeof (Boolean), sampleMethods);
      AddCompareToMethods(typeof (Char), sampleMethods);
      AddCompareToMethods(typeof (Byte), sampleMethods);
      AddCompareToMethods(typeof (SByte), sampleMethods);
      AddCompareToMethods(typeof (DateTime), sampleMethods);
      AddCompareToMethods(typeof (String), sampleMethods);
      var comparisonMethodInfo = (from method in sampleMethods
                                  let info = ComparisonMethodRepository.Get(method)
                                  where info != null
                                  select info).ToList();
      Assert.AreEqual(sampleMethods.Count, comparisonMethodInfo.Count);
      Assert.IsTrue(comparisonMethodInfo.All(m => !m.IsComplex &&
                                                  !m.CorrespondsToLikeOperation));
    }

    [Test]
    public void StringStaticMethodsTest()
    {
      var sampleMethod = (typeof (string).GetMethod("Compare", new[] {typeof (string), typeof (string)}));
      var methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsFalse(methodInfo.IsComplex);
      Assert.IsFalse(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("Compare", new[] { typeof(string),
                                                                  typeof(string),
                                                                  typeof(bool) }));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsTrue(methodInfo.IsComplex);
      Assert.IsFalse(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("CompareOrdinal", new[] { typeof(string), typeof(string) }));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsFalse(methodInfo.IsComplex);
      Assert.IsFalse(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("CompareOrdinal", new[] { typeof(string),
                                                                         typeof(int),
                                                                         typeof(string),
                                                                         typeof(int),
                                                                         typeof(int)}));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsTrue(methodInfo.IsComplex);
      Assert.IsFalse(methodInfo.CorrespondsToLikeOperation);
    }

    [Test]
    public void StringLikeMethodsTest()
    {
      var sampleMethod = (typeof(string).GetMethod("EndsWith", new[] { typeof(string) }));
      var methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsFalse(methodInfo.IsComplex);
      Assert.IsTrue(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("EndsWith", new[] { typeof(string),
                                                                    typeof(StringComparison) }));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsTrue(methodInfo.IsComplex);
      Assert.IsTrue(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("StartsWith", new[] { typeof(string) }));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsFalse(methodInfo.IsComplex);
      Assert.IsTrue(methodInfo.CorrespondsToLikeOperation);

      sampleMethod = (typeof(string).GetMethod("StartsWith", new[] { typeof(string),
                                                                    typeof(StringComparison) }));
      methodInfo = ComparisonMethodRepository.Get(sampleMethod);
      Assert.IsNotNull(methodInfo);
      Assert.AreEqual(sampleMethod, methodInfo.Method);
      Assert.IsTrue(methodInfo.IsComplex);
      Assert.IsTrue(methodInfo.CorrespondsToLikeOperation);
    }

    private void AddCompareToMethods(Type type, List<MethodInfo> methods)
    {
      const string compareTo = "CompareTo";
      methods.Add(type.GetMethod(compareTo, new[] {type}));
      methods.Add(type.GetMethod(compareTo, new[] { typeof(object) }));
    }
  }
}