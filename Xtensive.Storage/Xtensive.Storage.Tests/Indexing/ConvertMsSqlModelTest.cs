// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.23

using NUnit.Framework;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Tests.Indexing
{
  [TestFixture]
  public class ConvertMsSqlModelTest : ConvertDomainModelTest
  {
    protected override StorageInfo Schema { get { return StorageSchema; } }

    [SetUp]
    public override void SetUp()
    {
      BuildDomain("mssql2005");
    }
  }
}