// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.23

using NUnit.Framework;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  [Explicit("Requires MSSQL servers")]
  public class ConvertMsSqlModelTest : ConvertDomainModelTest
  {
    [SetUp]
    public override void SetUp()
    {
      Schema = BuildDomain("mssql2005").ExtractedSchema;
    }
  }
}