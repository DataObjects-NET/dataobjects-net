// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using NUnit.Framework;
using Xtensive.Modelling.Tests.DatabaseModel;

namespace Xtensive.Modelling.Tests
{
  [TestFixture]
  public class DatabaseModelTest
  {
    [Test]
    public void CombinedTest()
    {
      var srv = new Server("srv");
      var db = new Database(srv, "db", srv.Databases.Count);
      var s = new Schema(db, "s", db.Schemas.Count);
      Log.Info("s.Path={0}", s.Path);
    }
  }
}