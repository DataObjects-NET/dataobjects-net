// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly : AssemblyTitle("Xtensive.Sql.Oracle")]
[assembly : AssemblyDescription("")]
[assembly : AssemblyConfiguration("")]
[assembly : AssemblyCompany("Xtensive LLC")]
[assembly : AssemblyProduct("Xtensive.Sql.Oracle")]
[assembly : AssemblyTrademark("")]
[assembly : AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly : ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly : Guid("8D8385F7-D430-4e2d-8C41-F5D7B82430A7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
#if !NET40
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#endif
