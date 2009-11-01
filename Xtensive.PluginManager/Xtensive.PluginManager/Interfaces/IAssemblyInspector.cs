using System;
using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Defines interface for assembly inspection in ternms of finding types by specific criteria.
  /// </summary>
  public interface IAssemblyInspector
  {
    /// <summary>
    /// Occurs when assembly inspection process starts.
    /// </summary>
    event EventHandler<AssemblyInspectorEventArgs> InspectionStart;

    /// <summary>
    /// Occurs when assembly inspection process completed.
    /// </summary>
    event EventHandler<AssemblyInspectorEventArgs> InspectionComplete;

    /// <summary>
    /// Occurs when requested type is found in assembly that is being inspected.
    /// </summary>
    event EventHandler<TypeFoundEventArgs> TypeFound;

    /// <summary>
    /// Searches for types accepted by the given filter and filter criteria. 
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="filterCriteria">The filter criteria.</param>
    void FindTypes(string file, TypeFilter filter, object filterCriteria);

    /// <summary>
    /// Searches for types inherited from baseType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    void FindTypes(string file, Type baseType);

    /// <summary>
    /// Searches for types inherited from baseType and optionally marked with one or more attributes of attributeType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    /// <param name="attributeType">The attribute type.</param>
    void FindTypes(string file, Type baseType, Type attributeType);
  }
}