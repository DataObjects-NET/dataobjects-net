// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2009.09.17

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0409_ConstructorTransactional_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0409_ConstructorTransactional_Model
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0409_ConstructorTransactional : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Document).Assembly, typeof (Document).Namespace);
      return config;
    }

    [Test]
    public void DocumentCreatedInSeparateTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.None
      };
      Key key;
      using (var s = Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var document = new Document();
          key = document.Key;
          t.Complete();
        }
      }
      using (var s = Session.Open(Domain, sessionConfiguration)) {
        var document = Query<Document>.Single(key);
      }
    }

    [Test]
    public void DocumentCreatedInAutoTransactionTest()
    {
      using (var s = Session.Open(Domain)) {
        var document = new Document();
      }
    }
  }
}