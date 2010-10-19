using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Path node reference.
  /// </summary>
  [Serializable]
  public struct PathNodeReference :
    IEquatable<PathNodeReference>
  {
    private string path;

    /// <summary>
    /// Gets the path to the node.
    /// </summary>
    public string Path {
      get { return path; }
    }

    /// <summary>
    /// Gets the <see cref="PathNodeReference"/> to the specified source,
    /// if the source is <see cref="IPathNode"/>;
    /// otherwise, returns <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns><see cref="PathNodeReference"/> to the specified source,
    /// if the source is <see cref="IPathNode"/>;
    /// otherwise, returns <paramref name="source"/>.</returns>
    public static object Get(object source)
    {
      var pathNode = source as IPathNode;
      if (pathNode==null)
        return source;
      return new PathNodeReference(pathNode.Path);
    }

    /// <summary>
    /// Resolves the specified object (possibly <see cref="PathNodeReference"/>).
    /// Reverts the effect of <see cref="Get"/> method.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="source">The object to resolve.</param>
    /// <returns>Either original object, or
    /// resolved <see cref="PathNodeReference"/> (<see cref="IPathNode"/>)</returns>
    public static object Resolve(IModel model, object source)
    {
      if (!(source is PathNodeReference))
        return source;
      var pnr = (PathNodeReference) source;
      return model.Resolve(pnr.Path, true);
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(PathNodeReference obj)
    {
      return obj.Path==Path;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (PathNodeReference))
        return false;
      return Equals((PathNodeReference) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (Path!=null ? Path.GetHashCode() : 0);
    }

    /// <inheritdoc/>
    public static bool operator ==(PathNodeReference left, PathNodeReference right)
    {
      return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(PathNodeReference left, PathNodeReference right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return path;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="path">The <see cref="Path"/> value.</param>
    public PathNodeReference(string path)
    {
      this.path = path;
    }
  }
}