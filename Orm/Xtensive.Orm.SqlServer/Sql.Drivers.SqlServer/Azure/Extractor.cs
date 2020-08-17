// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2010.03.02

using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Sql.Drivers.SqlServer.Azure
{
  internal class Extractor : v12.Extractor
  {
    protected override void ExtractFulltextIndexes(ExtractionContext context)
    {
    }

    protected override Task ExtractFulltextIndexesAsync(ExtractionContext context, CancellationToken token) => Task.CompletedTask;


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}