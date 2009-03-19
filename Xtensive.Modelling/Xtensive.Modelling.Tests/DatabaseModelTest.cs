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
    private Server srv;
    private Database db;
    private Schema s;

    [TestFixtureSetUp]
    public void Setup()
    {
      srv = new Server("srv");
      db = new Database(srv, "db", srv.Databases.Count);
      s = new Schema(db, "s", db.Schemas.Count);
    }

    [Test]
    public void PathTest()
    {
      Log.Info("srv.Path={0}", srv.Path);
      Assert.AreEqual(srv.Path, string.Empty);
      Assert.AreEqual(srv.Databases.Path, "Databases");

      Log.Info("db.Path={0}", db.Path);
      Assert.AreEqual(db.Path, "Databases/db");
      Assert.AreEqual(db.Schemas.Path, "Databases/db/Schemas");

      Log.Info("s.Path={0}", s.Path);
      Assert.AreEqual(s.Path, "Databases/db/Schemas/s");
    }
  }
}