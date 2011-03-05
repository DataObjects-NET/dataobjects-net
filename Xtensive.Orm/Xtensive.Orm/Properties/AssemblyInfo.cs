// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using PostSharp.Extensibility;
using Xtensive.Aspects;
using Xtensive.Orm;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xtensive.Orm")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Xtensive LLC")]
[assembly: AssemblyProduct("Xtensive.Orm")]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("84eed58e-4fbe-43bf-82d3-769cd23184ea")]

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
#if !NET40
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#endif

#if !CUSTOMKEY
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Core, PublicKey=" + 
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests, PublicKey=" + 
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Sandbox, PublicKey=" + 
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Integrity, PublicKey=" +
"0024000004800000940000000602000000240000525341310004000001000100fbdd689d62e9c6" +
"7bb6356267f95e0b58d478cf56393c4f060fbaff42a9686272e37009ab71bfa2e41046e952f389" +
"f37c6a033d1a2a5354fc97226fc469128e49e6a479ac5d1dd69d7da5607d0dc4ede0765d477745" +
"1034dc3a15f1532d010db3e633e62fc5e67a3ed175457acb9dc6c9d39ccc8ecfdaae62df34d488" +
"c45009b2")]
#else
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Core, PublicKey=" +
// Insert public key of your custom key here
"0024000004800000940000000602000000240000525341310004000001000100" +
"3D3282E279A44BFC96BB65910134B6795E2D126BECD1BFDDB3A14A8746AC8A81" +
"449D831DF08EE869DC24CC769D40740140DE4C8980B57A473D363F8DAB2A9075" +
"BDE8E4970A1E8C5ECEC874E68CE251CFE75A26D3EAB0895A8594A0E2D788E087" +
"756D43EE3286D4661F56EB04F671173B02BE4FC7B8A56CB329F795A059B8F5E7")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests, PublicKey=" +
// Insert public key of your custom key here
"0024000004800000940000000602000000240000525341310004000001000100" +
"3D3282E279A44BFC96BB65910134B6795E2D126BECD1BFDDB3A14A8746AC8A81" +
"449D831DF08EE869DC24CC769D40740140DE4C8980B57A473D363F8DAB2A9075" +
"BDE8E4970A1E8C5ECEC874E68CE251CFE75A26D3EAB0895A8594A0E2D788E087" +
"756D43EE3286D4661F56EB04F671173B02BE4FC7B8A56CB329F795A059B8F5E7")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Sandbox, PublicKey=" +
// Insert public key of your custom key here
"0024000004800000940000000602000000240000525341310004000001000100" +
"3D3282E279A44BFC96BB65910134B6795E2D126BECD1BFDDB3A14A8746AC8A81" +
"449D831DF08EE869DC24CC769D40740140DE4C8980B57A473D363F8DAB2A9075" +
"BDE8E4970A1E8C5ECEC874E68CE251CFE75A26D3EAB0895A8594A0E2D788E087" +
"756D43EE3286D4661F56EB04F671173B02BE4FC7B8A56CB329F795A059B8F5E7")]
[assembly: InternalsVisibleTo("Xtensive.Orm.Tests.Integrity, PublicKey=" +
// Insert public key of your custom key here
"0024000004800000940000000602000000240000525341310004000001000100" +
"3D3282E279A44BFC96BB65910134B6795E2D126BECD1BFDDB3A14A8746AC8A81" +
"449D831DF08EE869DC24CC769D40740140DE4C8980B57A473D363F8DAB2A9075" +
"BDE8E4970A1E8C5ECEC874E68CE251CFE75A26D3EAB0895A8594A0E2D788E087" +
"756D43EE3286D4661F56EB04F671173B02BE4FC7B8A56CB329F795A059B8F5E7")]
#endif
