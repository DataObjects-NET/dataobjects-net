// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.Interfaces.Alphabet;

namespace Xtensive.Storage.Tests.Linq.Interfaces
{
  [Serializable]
  public class AlphabetTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(INamed).Assembly, typeof(INamed).Namespace);
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