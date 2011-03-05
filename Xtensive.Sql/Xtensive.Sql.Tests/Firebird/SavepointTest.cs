// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.21

using NUnit.Framework;
using System;

namespace Xtensive.Sql.Tests.Firebird
{
    public abstract class SavepointTest : Tests.SavepointsTest
    {
        public override void SetUp()
        {
            TestHelpers.StartTraceToLogFile(this);
            base.SetUp();
            // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
            RealTestFixtureSetUp();
        }

        public override void TearDown()
        {
            base.TearDown();
            // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
            RealTestFixtureTearDown();
            TestHelpers.StopTraceToLogFile(this);
        }

    }
}
