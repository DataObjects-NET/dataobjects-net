﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xtensive.Sql.Drivers.Oracle.Resources {
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xtensive.Sql.Drivers.Oracle.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ALTER SEQUENCE RESTART WITH is not supported.
        /// </summary>
        internal static string ExAlterSequenceRestartWithIsNotSupported {
            get {
                return ResourceManager.GetString("ExAlterSequenceRestartWithIsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid boolean string &apos;{0}&apos;.
        /// </summary>
        internal static string ExInvalidBooleanStringX {
            get {
                return ResourceManager.GetString("ExInvalidBooleanStringX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oracle below 9i2 is not supported..
        /// </summary>
        internal static string ExOracleBelow9i2IsNotSupported {
            get {
                return ResourceManager.GetString("ExOracleBelow9i2IsNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oracle does not support local temporary tables.
        /// </summary>
        internal static string ExOracleDoesNotSupportLocalTemporaryTables {
            get {
                return ResourceManager.GetString("ExOracleDoesNotSupportLocalTemporaryTables", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oracle does not support trimming more that one character at once.
        /// </summary>
        internal static string ExOracleDoesNotSupportTrimmingMoreThatOneCharacterAtOnce {
            get {
                return ResourceManager.GetString("ExOracleDoesNotSupportTrimmingMoreThatOneCharacterAtOnce", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Oracle does not support UPDATE FROM statements.
        /// </summary>
        internal static string ExOracleDoesNotSupportUpdateFromStatements {
            get {
                return ResourceManager.GetString("ExOracleDoesNotSupportUpdateFromStatements", resourceCulture);
            }
        }
    }
}
