// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public partial class MemberCompilerProviderTest
  {
    private string[] dummyStringArray = new string[10];
    
    private Func<string[],string> GetCompiler(IMemberCompilerProvider<string> provider,
      Type type, string method)
    {
      var mi = type.GetMethod(method);
      Assert.IsNotNull(mi);
      if (mi.IsGenericMethodDefinition)
        mi = mi.MakeGenericMethod(typeof(object));
      var result = provider.GetCompiler(mi);
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
            var d = GetCompiler(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(dummyStringArray));
          }
    }

    [Test]
    public void PropertiesTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(PropertyCompiler));

      foreach (var t in new[]{typeof(NonGenericTarget), typeof(GenericTarget<>)})
        foreach (string s1 in new[]{WellKnown.GetterPrefix, WellKnown.SetterPrefix})
          foreach (string s2 in new[]{"Instance", "Static"}) {
            string method = s1 + s2 + "Property";
            var d = GetCompiler(provider, t, method);
            Assert.AreEqual(t.Name + "." + method, d(dummyStringArray));
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
      Assert.AreEqual("OK", d(dummyStringArray));
    }

    [Test]
    public void ConflictKeepOldTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.KeepOld);
      var d = GetCompiler(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler1", d(dummyStringArray));
    }

    [Test]
    public void ConflictOverwriteTest()
    {
      var provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(ConflictCompiler1));
      provider.RegisterCompilers(typeof(ConflictCompiler2), ConflictHandlingMethod.Overwrite);
      var d = GetCompiler(provider, typeof(ConflictTarget), "ConflictMethod");
      Assert.AreEqual("Compiler2", d(dummyStringArray));      
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
