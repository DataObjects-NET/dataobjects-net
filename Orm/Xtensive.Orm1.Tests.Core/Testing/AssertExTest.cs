// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.20

using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Testing
{
  [TestFixture]
  public class AssertExTest
  {
    [Test]
    public void IsRegexMatchTest()
    {
      AssertEx.IsRegexMatch("A B C D", "^.*B.*D.*$");
      AssertEx.IsRegexMatch("A B C D", "^.*B C.*$");
      AssertEx.IsRegexMatch("A B C D", "^A B C.*$");
      AssertEx.IsRegexMatch("A B C D", "^.*B C.*D$");
      AssertEx.IsRegexMatch("A B C D", "^A B C D$");
      AssertEx.IsRegexMatch("A B C D", "^.*A B C D.*$");
      AssertEx.IsRegexMatch("A B C D", "B.*C.*");
      AssertEx.IsNotRegexMatch("A B C D", "^B.*C.*$");
      AssertEx.IsNotRegexMatch("A B C D", "^A B.*C$");
      AssertEx.IsNotRegexMatch("A B C D", "^A.*C.*B.*D$");
    }

    [Test]
    public void IsPatternMatchTest()
    {
      AssertEx.IsPatternMatch("*A*B?C*D*", "*B*D*");
      AssertEx.IsPatternMatch("*A*B?C*D*", "*B?C*");
      AssertEx.IsPatternMatch("*A*B?C*D*", "*A???C*");
      AssertEx.IsNotPatternMatch("*A*B?C*D*", "A*C");
      AssertEx.IsNotPatternMatch("*A*B?C*D*", "A?B");
    }
  }
}