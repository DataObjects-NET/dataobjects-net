// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xtensive.Storage.Rse")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Xtensive.Storage.Rse")]
[assembly: AssemblyCopyright("Copyright © Xtensive LLC 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e746f054-26d8-47f1-b8c6-26726a53f132")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: CLSCompliant(true)]    
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]

// This ensures the RecordSetProvider & its ancestors will be "initializable"
[assembly:Initializable(AttributeTargetTypes = "*")]

[assembly:InternalsVisibleTo("Xtensive.Storage.Tests")]
[assembly: InternalsVisibleTo("Mocks")]
[assembly: InternalsVisibleTo("MockObjects")]