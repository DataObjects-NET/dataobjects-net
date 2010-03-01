// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.31

using NUnit.Framework;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class AspectHelperTest
  {
    [CompileTimeWarning]
    private int aspectedField;
    [CompileTimeWarning]
    public int AspectedProperty1 { get { return 0; } }
    [CompileTimeWarning]
    public int AspectedProperty2 { set { ; } }
    [CompileTimeWarning]
    public int AutoProperty { private get; set; }
    [CompileTimeWarning]
    private void ApectedMethod(int i)
    {
      return;
    }

    [Test]
    public static void CombinedTest()
    {
      Log.Info("This test must pass in compile time.");
    }
  }
}