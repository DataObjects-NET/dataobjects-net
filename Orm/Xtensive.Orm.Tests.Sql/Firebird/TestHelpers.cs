// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.21

using System.Data.Common;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
    public static class TestHelpers
    {

        private static Dictionary<object, TraceListener> testListeners;
        private static Dictionary<object, TraceListener> TestListeners
        {
            get
            {
                if (testListeners == null)
                    testListeners = new Dictionary<object, TraceListener>();
                return testListeners;
            }
        }

        public static void StartTraceToLogFile(object test, string filename = null)
        {
            //if (string.IsNullOrWhiteSpace(filename))
            //    filename = "c:\\" + test.GetType().FullName + ".Log.txt";
            //System.IO.File.Delete(filename);
            //TraceListener tl = new TextWriterTraceListener(filename);
            //TestListeners.Add(test, tl);
            //Debug.Listeners.Add(tl);
            //Debug.WriteLine(test.GetType().FullName + " started at " + DateTime.Now.ToLocalTime());
            //Debug.Flush();
        }

        public static void StopTraceToLogFile(object test)
        {
            //TraceListener tl = TestListeners[test];
            //Debug.WriteLine(test.GetType().FullName + " finished at " + DateTime.Now.ToLocalTime());
            //tl.Flush();
            //tl.Close();
            //Debug.Listeners.Remove(tl);
            //TestListeners.Remove(test);
            //tl.Dispose();
            //tl = null;
        }

        public static void dump(DbCommand command)
        {
            //System.Diagnostics.Debug.WriteLine("Command Dump");
            //System.Diagnostics.Debug.WriteLine(command.CommandText);
            //System.Diagnostics.Debug.Indent();
            //foreach (DbParameter p in command.Parameters)
            //    System.Diagnostics.Debug.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}", p.ParameterName, p.DbType, p.Direction, p.Size, p.Value == null ? "<dbnull>" : p.Value));
            //System.Diagnostics.Debug.Unindent();
            //System.Diagnostics.Debug.Flush();
        }

    }
}
