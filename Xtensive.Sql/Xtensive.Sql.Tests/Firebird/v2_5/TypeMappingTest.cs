// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.19

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Firebird.v2_5
{
    [TestFixture, Explicit]
    public class TypeMappingTest : Firebird.TypeMappingTest
    {
        protected override string Url { get { return TestUrl.Firebird25; } }
    }
}
