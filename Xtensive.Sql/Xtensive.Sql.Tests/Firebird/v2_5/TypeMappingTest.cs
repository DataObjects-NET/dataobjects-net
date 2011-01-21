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
        private const string logfile = "c:\\TypeMappingTestLog.txt";
        private TraceListener tl = null;

        public override void SetUp()
        {
            base.SetUp();
            System.IO.File.Delete(logfile);
            tl = new TextWriterTraceListener(logfile);
            Debug.Listeners.Add(tl);
            Debug.WriteLine("Start...");
            Debug.Flush();
        }

        public override void TearDown()
        {
            base.TearDown();
            tl.Flush();
            tl.Close();
            Debug.Listeners.Remove(tl);
            tl.Dispose();
            tl = null;
        }

        public TypeMappingTest()
        {
        }
    }
}
