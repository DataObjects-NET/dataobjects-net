// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.10

using System;
using NUnit.Framework;
using Xtensive.Core.Linq;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public partial class MemberCompilerProviderTest
  {
    private IMemberCompilerProvider<string> provider;
    private string[] dummyStringArray;
    
    private Func<string[],string> GetCompiler(Type type, string method)
    {
      var mi = type.GetMethod(method);
      Assert.IsNotNull(mi);
      var result = provider.GetCompiler(mi);
      Assert.IsNotNull(result);
      return result;
    }

    [TestFixtureSetUp]
    public void FixtureSetUp()
    {
      dummyStringArray = new string[10];
      provider = new MemberCompilerProvider<string>();
      provider.RegisterCompilers(typeof(MainCompiler));
    }

    [Test]
    public void CompilerNNTest()
    {
      var d = GetCompiler(typeof (NonGenericTarget), "NonGenericMethod");
      Assert.AreEqual("CompilerNN", d(dummyStringArray));
    }

    [Test]
    public void CompilerNGTest()
    {
      var d = GetCompiler(typeof (NonGenericTarget), "GenericMethod");
      Assert.AreEqual("CompilerNG", d(dummyStringArray));
    }

    [Test]
    public void CompilerGNTest()
    {
      var d = GetCompiler(typeof (GenericTarget<>), "NonGenericMethod");
      Assert.AreEqual("CompilerGN", d(dummyStringArray));
    }

    [Test]
    public void CompilerGGTest()
    {
      var d = GetCompiler(typeof (GenericTarget<>), "GenericMethod");
      Assert.AreEqual("CompilerGG", d(dummyStringArray));
    }

    [Test]
    public void InstanceCompilerTest()
    {
      var d = GetCompiler(typeof (NonGenericTarget), "InstanceMethod");
      Assert.AreEqual("InstanceCompiler", d(dummyStringArray));
    }
  }
}
