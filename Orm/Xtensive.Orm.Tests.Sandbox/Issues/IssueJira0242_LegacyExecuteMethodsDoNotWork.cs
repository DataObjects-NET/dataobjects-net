// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.27

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MetadataType = Xtensive.Orm.Metadata.Type;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0242_LegacyExecuteMethodsDoNotWork : AutoBuildTest
  {
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
    }

    #region Compiled queries

    [Test]
    public void CompiledSequenceTest()
    {
      var id = GetId();
      var r = Query.Execute(() => Query.All<MetadataType>().Where(m => m.Id==id));
      VerifyResult(r);
    }

    [Test]
    public void CompiledSequenceWithKeyTest()
    {
      var id = GetId();
      var r = Query.Execute(GetKey(), () => Query.All<MetadataType>().Where(m => m.Id==id));
      VerifyResult(r);
    }

    [Test]
    public void CompiledScalarTest()
    {
      var id = GetId();
      var r = Query.Execute(() => Query.All<MetadataType>().Single(m => m.Id==id));
      VerifyResult(r);
    }

    [Test]
    public void CompiledScalarWithKeyTest()
    {
      var id = GetId();
      var r = Query.Execute(GetKey(), () => Query.All<MetadataType>().Single(m => m.Id==id));
      VerifyResult(r);
    }

    #endregion

    #region Future queries

    [Test]
    public void FutureSequenceTest()
    {
      var id = GetId();
      var r = Query.ExecuteFuture(() => Query.All<MetadataType>().Where(m => m.Id==id));
      VerifyResult(r);
    }

    [Test]
    public void FutureSequenceWithKeyTest()
    {
      var id = GetId();
      var r = Query.ExecuteFuture(GetKey(), () => Query.All<MetadataType>().Where(m => m.Id==id));
      VerifyResult(r);
    }

    [Test]
    public void FutureScalarTest()
    {
      var id = GetId();
      var r = Query.ExecuteFutureScalar(() => Query.All<MetadataType>().Single(m => m.Id==id));
      VerifyResult(r.Value);
    }

    [Test]
    public void FutureScalarWithKeyTest()
    {
      var id = GetId();
      var r = Query.ExecuteFutureScalar(GetKey(), () => Query.All<MetadataType>().Single(m => m.Id==id));
      VerifyResult(r.Value);
    }

    #endregion

    private void VerifyResult(MetadataType result)
    {
      Assert.That(result.Id, Is.EqualTo(GetId()));
    }

    private void VerifyResult(IEnumerable<MetadataType> result)
    {
      var items = result.ToList();
      Assert.That(items.Count, Is.EqualTo(1));
      VerifyResult(items[0]);
    }

    private int GetId()
    {
      return Domain.Model.Types[typeof (MetadataType)].TypeId;
    }

    private object GetKey()
    {
      return new object();
    }
  }
}