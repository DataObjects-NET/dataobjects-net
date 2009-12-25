// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System.ComponentModel;
using System.Diagnostics;
using NUnit.Framework;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class Stuff
  {
    [Test]
    [ExpectedException(typeof (Win32Exception))]
    public void DllStart()
    {
      string fileName = @"C:\Projects\Xtensive\Xtensive.Distributed.Test\Lib\Xtensive.Distributed.Test.Core.dll";
      ProcessStartInfo startInfo = new ProcessStartInfo(fileName);
      Process.Start(startInfo);
    }
  }
}