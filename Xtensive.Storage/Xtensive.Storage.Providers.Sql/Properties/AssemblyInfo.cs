// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xtensive.Core.Aspects;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xtensive.Storage.Providers.Sql")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Xtensive.Storage.Providers.Sql")]
[assembly: AssemblyCopyright("Copyright ©  2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("bee63806-bd6e-44e0-ab92-03275cbde45d")]

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

// This ensures the RecordSetProvider & its ancestors will be "initializable"
[assembly:Initializable(AttributeTargetTypes = "*")]

#if XTENSIVEBUILD
[assembly: AssemblyKeyFile(@"..\..\..\Lib\Key.snk")]
[assembly: InternalsVisibleTo("Xtensive.Storage.Tests, PublicKey=" + 
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
#else
[assembly: InternalsVisibleTo("Xtensive.Storage.Tests")]
#endif
