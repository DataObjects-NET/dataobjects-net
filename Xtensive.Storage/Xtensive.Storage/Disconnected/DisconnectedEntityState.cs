using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// The disconnected entity state.
  /// </summary>
  internal sealed class DisconnectedEntityState : ICloneable
  {
    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; private set; }

    /// <summary>
    /// Gets or sets the persistence state.
    /// </summary>
    public PersistenceState PersistenceState { get; set; }

    /// <summary>
    /// Gets or sets the original tuple.
    /// </summary>
    public Tuple OriginalTuple { get; set; }

    /// <summary>
    /// Gets or sets the difference tuple.
    /// </summary>
    public Tuple DifferenceTuple { get; set; }

    /// <summary>
    /// Gets the type of the entity.
    /// </summary>
    public TypeInfo Type { get { return Key.TypeRef.Type; } }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Creates a new <see cref="DisconnectedEntityState"/> instance 
    /// that is a copy of the current instance.
    /// </summary>
    /// <returns>A new <see cref="DisconnectedEntityState"/> instance 
    /// that is a copy of this instance.</returns>
    public DisconnectedEntityState Clone()
    {
      var newEntityState = new DisconnectedEntityState(Key);
      newEntityState.PersistenceState = PersistenceState;
      newEntityState.OriginalTuple = OriginalTuple!=null ? OriginalTuple.Clone() : null;
      newEntityState.DifferenceTuple = DifferenceTuple!=null ? DifferenceTuple.Clone() : null;
      return newEntityState;
    }

    
    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="key">The key.</param>
    public DisconnectedEntityState(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      Key = key;
    }
  }
}