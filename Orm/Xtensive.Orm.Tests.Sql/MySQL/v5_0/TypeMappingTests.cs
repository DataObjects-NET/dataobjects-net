﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Sql;

namespace Xtensive.Orm.Tests.Sql.MySQL.v5_0
{
    [TestFixture, Explicit, Ignore("Ignored due to Sakila")]
    public class TypeMappingTest : Sql.TypeMappingTest
    {
        protected override string Url { get { return TestUrl.MySql50; } }

        protected override void TestFixtureSetUp()
        {
          
        }
    }
}
