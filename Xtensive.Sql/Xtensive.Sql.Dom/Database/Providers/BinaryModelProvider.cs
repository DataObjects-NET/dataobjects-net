// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Providers
{
  /// <summary>
  /// Represents a database model provider that serializes/deserializes database model from file.
  /// </summary>
  public class BinaryModelProvider : IModelProvider
  {
    private string filename;

    /// <summary>
    /// Gets or sets the filename where database model is stored.
    /// </summary>
    /// <value>The filename.</value>
    public string Filename
    {
      get { return filename; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(filename, "value");
        filename = value;
      }
    }

    #region IModelProvider Members

    /// <summary>
    /// Builds the database model from serialized state.
    /// </summary>
    public Model Build()
    {
      Model model;

      FileInfo fi = new FileInfo(Filename);
      if (!fi.Exists)
        return null;

      using (Stream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        IFormatter formatter = new BinaryFormatter();
        model = (Model)formatter.Deserialize(stream);
        stream.Close();
      }
      return model;
    }

    /// <summary>
    /// Saves the specified model to the file.
    /// </summary>
    /// <param name="model">The model.</param>
    public void Save(Model model)
    {
      ArgumentValidator.EnsureArgumentNotNull(model, "model");
      using (Stream stream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, model);
        stream.Close();
      }
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryModelProvider"/> class.
    /// </summary>
    /// <param name="filename">The filename where serialized database model is stored.</param>
    public BinaryModelProvider(string filename)
    {
      Filename = filename;
    }
  }
}