// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System.IO;

namespace Xtensive.Orm.Logging
{
  internal sealed class FileWriter : LogWriter
  {
    private readonly string fileName;
    private readonly object syncRoot = new object();

    /// <inheritdoc/>
    public override void Write(in LogEventInfo logEvent)
    {
      lock (syncRoot) {
        using (var streamWriter = new StreamWriter(fileName, true)) {
          streamWriter.WriteLine(logEvent);
          streamWriter.Close();
        }
      }
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="filename">Name of file to write to.</param>
    public FileWriter(string filename)
    {
      fileName = filename;
    }
  }
}
