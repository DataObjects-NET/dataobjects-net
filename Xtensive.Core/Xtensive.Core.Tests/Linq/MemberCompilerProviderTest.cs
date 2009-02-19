// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System;
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
    private readonly string[] dummyStringArray = new string[10];
    
    private static Func<string, string[],string> GetCompilerForMethod(
      IMemberCompilerProvider<string> provider, Type type, string methodName)
    {
      if (type.IsGenericTypeDefinition)
        type = type.MakeGenericType(typeof (object));

      var mi = type.GetMethod(methodName);
      Assert.IsNotNull(mi);
      if (mi.IsGenericMethodDefinition)
        mi = mi.MakeGenericMethod(typeof(object));
      var result = provider.GetCompiler(mi);
      Assert.IsNotNull(result);
      return result;
    }

    private static Func<string, string[], string> GetCompilerForCtor(
      IMemberCompilerProvider<string> provider, Type type)
    {
      if (type.IsGenericTypeDefinition)
        type = type.MakeGenericType(typeof(object));

      var ci = type.GetConstructors().First();
      var result = provider.GetCompiler(ci);
      Assert.IsNotNull(result);
      return result;
    }

    private static Func<string, string[], string> GetCompilerForField(
      IMemberCompilerProvider<string> provider, Type type, string fieldName)
    {
      if (type.IsGenericTypeDefinition)
        type = type.MakeGenericType(typeof(object));

      var fi = type.GetField(fieldName);
      Assert.IsNotNull(fi);
      var result = provider.GetCompiler(fi);
      Assert.IsNotNull(result);
      return result;
    }

    [Test]
    public void MethodsTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(MethodCompiler));
      
      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s1 in new[]{"Instance", "Static"})
          foreach (string s2 in new[]{"Generic", "NonGeneric"}) {
            string method = s1 + s2 + "Method";
            var d = GetCompilerForMethod(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(null, dummyStringArray));
          }
    }

    [Test]
    public void PropertiesTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(PropertyCompiler));

      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s1 in new[]{WellKnown.GetterPrefix, WellKnown.SetterPrefix})
          foreach (string s2 in new[] { "InstanceProperty", "StaticProperty", "Item" }) {
            string method = s1 + s2;
            var d = GetCompilerForMethod(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(null, dummyStringArray));
          }
    }

    [Test]
    public void FieldsTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(FieldCompiler));

      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s in new[] {"InstanceField", "StaticField"}) {
          var d = GetCompilerForField(provider, t, s);
          Assert.AreEqual(t.Name + "." + s, d(null, dummyStringArray));
        }
    }

    [Test]
    public void CtorsTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(CtorCompiler));
      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)}) {
        var d = GetCompilerForCtor(provider, t);
        Assert.AreEqual(t.Name + WellKnown.CtorName, d(null, dummyStringArray));
      }
    }

    [Test]
    public void GenericFindTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(SuperGenericCompiler));

      var mi = typeof (SuperGenericTarget<string>)
        .GetMethod("Method",
          BindingFlags.Static | BindingFlags.Public,
          new[] {"T1"},
          new[] {typeof (int), typeof (string), null})
        .MakeGenericMethod(typeof(string));

      var d = provider.GetCompiler(mi);
      Assert.IsNotNull(d);
      Assert.AreEqual("OK", d(null, dummyStringArray));
    }

    [Test]
    public void ConflictKeepOldTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.KeepOld);
      var d = GetCompilerForMethod(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler1", d(null, dummyStringArray));
    }

    [Test]
    public void ConflictOverwriteTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.Overwrite);
      var d = GetCompilerForMethod(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler2", d(null, dummyStringArray));      
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConflictReportErrorTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.ReportError);
    }
  }
}
