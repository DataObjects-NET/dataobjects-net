// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Tests.DatabaseModel;
using Xtensive.Modelling.Comparison;

namespace Xtensive.Modelling.Tests
{
  [TestFixture]
  public class ActionSorterTest
  {
    private Server srv;
    private Security sec1;
    private Security sec2;
    private User u1;
    private User u2;
    private Role r1;
    private Role r2;
    private Database db1;
    private Database db2;
    private Schema s1;
    private Schema s2;
    private Table t1;
    private Table t2;

    [Test]
    public void BaseTest()
    {
      srv = new Server("srv");
      db1 = new Database(srv, "db1");
      sec1 = new Security(srv, "sec1");
      srv.Security = sec1;
      u1 = new User(sec1, "u1");
      db1.Owner = u1;


      var srvx = new Server("srvx");
      var hintSet = new HintSet(srvx, srv);
      hintSet.Add(new RenameHint("", ""));
      Difference diff;
      using (hintSet.Activate()) {
        diff = srvx.GetDifferenceWith(srv, null);
      }
      var actions = new ActionSequence();
      actions.Add(diff.ToActions());
      DumpDependecies(actions);
    }

    public void DumpDependecies(IEnumerable<NodeAction> actions)
    {
      foreach (var action in actions) {
        Log.Info(string.Format("{0}:", action));
        Log.Info("  Created dependencies:");
        foreach (var dependency in action.GetDependencies())
          Log.Info("    {0}", dependency);
        Log.Info("  Required dependencies:");
        foreach (var dependency in action.GetRequiredDependencies())
          Log.Info("    {0}", dependency);
      }
    }
  }
}