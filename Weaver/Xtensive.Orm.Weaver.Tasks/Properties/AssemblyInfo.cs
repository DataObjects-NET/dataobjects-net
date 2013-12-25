// Copyright (C) 2003-2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xtensive.Orm.Weaver;
using Xtensive.Orm.Weaver.Stages;

[assembly: AssemblyTitle(ThisAssembly.ProductName + " - assembly transformation tasks")]

[assembly: ProcessorStage(typeof (ValidateLicenseStage), 0)]
[assembly: ProcessorStage(typeof (ModifyPersistentTypesStage), 1)]
[assembly: ProcessorStage(typeof (MarkAssemblyStage), 2)]
