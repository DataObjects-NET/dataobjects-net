using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xtensive.Adapters.log4net")]
[assembly: AssemblyDescription("log4net adapter for Xtensive solutions")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Xtensive LLC")]
[assembly: AssemblyProduct("Xtensive.Adapters.log4net")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3b1f997a-58dd-402c-864d-9dbd6d8bfde2")]

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
[assembly: AllowPartiallyTrustedCallers]
#if !NET40
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#endif
