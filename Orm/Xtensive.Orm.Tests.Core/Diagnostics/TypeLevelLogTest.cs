// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.24

using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.Diagnostics
{
  public class TypeLevelLogTestTest<T1, T2>
  {
    protected static ILog Log = LogProvider.GetLog(typeof (TypeLevelLogTestTest<,>));
  }

  [TestFixture]
  public class TypeLevelLogTestTest : TypeLevelLogTestTest<int, string>
  {
    [Test]
    public void CombinedTest()
    {
      TestLog.Info("Type-level logging works!");
    }
  }
}