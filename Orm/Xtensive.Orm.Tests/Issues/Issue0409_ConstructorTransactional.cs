// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2009.09.17

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0409_ConstructorTransactional_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0409_ConstructorTransactional_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0409_ConstructorTransactional : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Document).Assembly, typeof (Document).Namespace);
      config.Sessions.Default.Options |= SessionOptions.AutoTransactionOpenMode;
      return config;
    }

    [Test]
    public void DocumentCreatedInSeparateTransactionTest()
    {
      Key key;
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          var document = new Document();
          key = document.Key;
          t.Complete();
        }
      }
      using (var s = Domain.OpenSession()) {
        var document = s.Query.Single<Document>(key);
      }
    }

    [Test]
    public void DocumentCreatedInAutoTransactionTest()
    {
      using (var s = Domain.OpenSession()) {
        Assert.Throws<InvalidOperationException>(() => { var document = new Document(); });
      }
    }
  }
}