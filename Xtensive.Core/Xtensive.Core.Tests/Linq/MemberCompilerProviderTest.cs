// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public partial class MemberCompilerProviderTest
  {
    private readonly string[] dummy = new string[10];
    
    private static Func<string, string[],string> GetCompilerForMethod(
      IMemberCompilerProvider<string> provider, Type source, string methodName)
    {
      if (source.IsGenericTypeDefinition)
        source = source.MakeGenericType(typeof (object));

      var mi = source.GetMethod(methodName);
      Assert.IsNotNull(mi);
      if (mi.IsGenericMethodDefinition)
        mi = mi.MakeGenericMethod(typeof(object));
      var result = provider.GetCompiler(mi);
      Assert.IsNotNull(result);
      return result;
    }

    private static Func<string, string[], string> GetCompilerForCtor(
      IMemberCompilerProvider<string> provider, Type source)
    {
      if (source.IsGenericTypeDefinition)
        source = source.MakeGenericType(typeof(object));

      var ci = source.GetConstructors().First();
      var result = provider.GetCompiler(ci);
      Assert.IsNotNull(result);
      return result;
    }

    private static Func<string, string[], string> GetCompilerForField(
      IMemberCompilerProvider<string> provider, Type source, string fieldName)
    {
      if (source.IsGenericTypeDefinition)
        source = source.MakeGenericType(typeof(object));

      var fi = source.GetField(fieldName);
      Assert.IsNotNull(fi);
      var result = provider.GetCompiler(fi);
      Assert.IsNotNull(result);
      return result;
    }

    [Test]
    public void MethodsTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(MethodCompiler));
      
      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s1 in new[]{"Instance", "Static"})
          foreach (string s2 in new[]{"Generic", "NonGeneric"}) {
            string method = s1 + s2 + "Method";
            var d = GetCompilerForMethod(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(null, dummy));
          }
    }

    [Test]
    public void PropertiesTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(PropertyCompiler));

      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s1 in new[]{WellKnown.GetterPrefix, WellKnown.SetterPrefix})
          foreach (string s2 in new[] { "InstanceProperty", "StaticProperty", "Item" }) {
            string method = s1 + s2;
            var d = GetCompilerForMethod(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(null, dummy));
          }
    }

    [Test]
    public void FieldsTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(FieldCompiler));

      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s in new[] {"InstanceField", "StaticField"}) {
          var d = GetCompilerForField(provider, t, s);
          Assert.AreEqual(t.Name + "." + s, d(null, dummy));
        }
    }

    [Test]
    public void CtorsTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(CtorCompiler));
      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)}) {
        var d = GetCompilerForCtor(provider, t);
        Assert.AreEqual(t.Name + WellKnown.CtorName, d(null, dummy));
      }
    }

    [Test]
    public void GenericFindTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(SuperGenericCompiler));

      var mi = typeof (SuperGenericTarget<string>)
        .GetMethod("Method",
          BindingFlags.Static | BindingFlags.Public,
          new[] {"T1"},
          new[] {typeof (int), typeof (string), null})
        .MakeGenericMethod(typeof(string));

      var d = provider.GetCompiler(mi);
      Assert.IsNotNull(d);
      Assert.AreEqual("OK", d(null, dummy));
    }

    [Test]
    public void ArrayAndEnumerableOverloadTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof (EachCompilers));
      var eachArray = typeof (EachExtensions)
        .GetMethods()
        .Single(method => method.GetParameterTypes().Any(type => type.IsArray))
        .MakeGenericMethod(typeof (int));
      var eachArrayCompiler = provider.GetCompiler(eachArray);
      Assert.AreEqual("EachInArray", eachArrayCompiler.Invoke(null, dummy));

      var eachEnumerable = typeof (EachExtensions)
        .GetMethods()
        .Single(method => method.GetParameterTypes()
          .Any(type => type.IsGenericType && type.GetGenericTypeDefinition()==typeof(IEnumerable<>)))
        .MakeGenericMethod(typeof (int));
      var eachEnumerableCompiler = provider.GetCompiler(eachEnumerable);
      Assert.AreEqual("EachInEnumerable", eachEnumerableCompiler.Invoke(null, dummy));
    }

    [Test]
    public void ConflictKeepOldTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.KeepOld);
      var d = GetCompilerForMethod(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler1", d(null, dummy));
    }

    [Test]
    public void ConflictOverwriteTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.Overwrite);
      var d = GetCompilerForMethod(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler2", d(null, dummy));
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConflictReportErrorTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.ReportError);
    }

    [Test]
    public void NonPublicGetterTest()
    {
      var provider = MemberCompilerProviderFactory.Create<string>();
      var property = typeof (NonGenericTarget)
        .GetProperty("InternalProperty", BindingFlags.Instance | BindingFlags.NonPublic);
      Assert.IsNotNull(property);
      var result = provider.GetCompiler(property);
      Assert.IsNull(result);
    }
  }
}
