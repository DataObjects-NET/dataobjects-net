// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql.Tests;

namespace Xtensive.Sql.Tests.MySQL.v5_0
{
    [TestFixture, Explicit]
    public class TypeMappingTest : Xtensive.Sql.Tests.TypeMappingTest
    {
        protected override string Url { get { return TestUrl.MySql50; } }
    }
}
