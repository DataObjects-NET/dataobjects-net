// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.11

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0736_AutoValidationCloning
  {
    [Test]
    public void Test()
    {
      var dc = new DomainConfiguration();
      dc.AutoValidation = true;
      Assert.AreEqual(true, dc.Clone().AutoValidation);
      dc.AutoValidation = false;
      Assert.AreEqual(false, dc.Clone().AutoValidation);
    }
  }
}