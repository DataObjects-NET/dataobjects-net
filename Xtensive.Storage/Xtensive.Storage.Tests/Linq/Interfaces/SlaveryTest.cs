// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.Interfaces.Slavery;

namespace Xtensive.Storage.Tests.Linq.Interfaces
{
  [Serializable]
  public class SlaveryTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ISlave).Assembly, typeof(ISlave).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      {
        using (var t = Transaction.Open())
        {


          // Rollback
        }
      }
    }
  }
}