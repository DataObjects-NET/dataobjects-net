// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.19

using NUnit.Framework;
using System.Diagnostics;

namespace Xtensive.Sql.Tests.Firebird.v2_5
{
    [TestFixture, Explicit]
    public class TypeMappingTest : Firebird.TypeMappingTest
    {
        protected override string Url { get { return TestUrl.Firebird25; } }

        public override void SetUp()
        {
            base.SetUp();
            TestHelpers.StartTraceToLogFile(this);
        }

        public override void TearDown()
        {
            base.TearDown();
            TestHelpers.StopTraceToLogFile(this);
        }

        public TypeMappingTest()
        {
        }
    }
}
