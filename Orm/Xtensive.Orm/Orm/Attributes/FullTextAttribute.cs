// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Indicates that persistent property must be included into full-text index.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class FullTextAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets a value indicating whether content of the field marked by this
    /// attribute must be analyzed or not. 
    /// "Analyzed" implies the content must be splat into a sequence of words; 
    /// otherwise it will be represented as a single word in index.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool Analyzed { get; set; }

    /// <summary>
    /// Gets the configuration name for word-breaker and stemmer. 
    /// </summary>
    public string Configuration { get; private set; }

    /// <summary>
    /// Gets the field name that contains type of full text field.
    /// </summary>
    public string DataTypeField { get; private set; }

  
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public FullTextAttribute(string configuration)
    {
      Configuration = configuration;
      Analyzed = true;
      DataTypeField = null;
    }

    /// <summary>
    /// Initialized a new instance of this class.
    /// </summary>
    /// <param name="configuration">Configuration name for word-breaker and stemmer</param>
    /// <param name="dataTypeField">Name of field that contains type of the data in full text field</param>
    public FullTextAttribute(string configuration, string dataTypeField)
      :this(configuration)
    {
      DataTypeField = dataTypeField;
    }
  }
}