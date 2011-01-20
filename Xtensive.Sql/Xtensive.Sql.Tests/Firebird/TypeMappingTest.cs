// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.19

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Firebird
{
    public abstract class TypeMappingTest : Tests.TypeMappingTest
    {
        public override void SetUp()
        {
            base.SetUp();
            // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
            RealTestFixtureSetUp();
        }

        public override void TearDown()
        {
            base.TearDown();
            // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
            RealTestFixtureTearDown();
        }

        protected override void CheckEquality(object expected, object actual)
        {
            var arrayValue = expected as byte[];
            var stringValue = expected as string;
            var charValue = expected as char?;

            bool nullExpected =
              arrayValue != null && arrayValue.Length == 0 ||
              stringValue != null && stringValue.Length == 0 ||
              charValue != null && charValue == default(char);

            if (nullExpected)
                Assert.IsNull(actual);
            else
                base.CheckEquality(expected, actual);
        }
    }
}