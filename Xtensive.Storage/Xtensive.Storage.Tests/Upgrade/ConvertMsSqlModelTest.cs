// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.23

using NUnit.Framework;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Tests.Indexing;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class ConvertMsSqlModelTest : ConvertDomainModelTest
  {
    [SetUp]
    public override void SetUp()
    {
      BuildDomain("mssql2005");
    }
  }
}