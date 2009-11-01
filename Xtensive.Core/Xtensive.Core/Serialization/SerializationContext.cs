// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// The context of the serialization / deserialization process.
  /// </summary>
  public abstract class SerializationContext: Context<SerializationScope>
  {
    private FormatterConfiguration                          configuration;
    private FormatterProcessType                                  processType;
    private Dictionary<IReference, object>                  referenceTable = new Dictionary<IReference, object>();
    private Dictionary<object, IReference>                  objectTable = new Dictionary<object, IReference>();
    private FixupActionQueue                                fixupQueue = new FixupActionQueue();
    private Formatter                                       formatter;
    private Collections.Stack<IReference>                   traversalPath = new Collections.Stack<IReference>(8);
    private Queue<SerializationData>                        deserializationQueue = new Queue<SerializationData>(32);
    private ReferenceManager                                referenceManager = new ReferenceManager();

    /// <summary>
    /// Gets the current <see cref="Formatter"/> processType.
    /// </summary>
    public FormatterProcessType ProcessType
    {
      get { return processType; }
      internal set { processType = value; }
    }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public FormatterConfiguration Configuration
    {
      get { return configuration; }
      internal set { configuration = value; }
    }

    public FixupActionQueue FixupQueue
    {
      get { return fixupQueue; }
    }

    public Formatter Formatter
    {
      get { return formatter; }
    }

    internal Collections.Stack<IReference> TraversalPath
    {
      get { return traversalPath; }
    }

    public Queue<SerializationData> DeserializationQueue
    {
      get { return deserializationQueue; }
    }

    public ReferenceManager ReferenceManager
    {
      get { return referenceManager; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="formatter">The formatter.</param>
    protected SerializationContext(Formatter formatter)
    {
      this.formatter = formatter;
    }
  }
}