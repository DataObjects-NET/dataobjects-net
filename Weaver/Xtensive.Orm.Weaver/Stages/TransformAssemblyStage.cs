using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class TransformAssemblyStage : ProcessorStage
  {
    public override bool CanExecute(ProcessorContext context)
    {
      return true;
    }

    public override ActionResult Execute(ProcessorContext context)
    {
      var configuration = context.Configuration;
      var inputFile = context.InputFile;
      var outputFile = inputFile;

      if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile)) {
        context.Logger.Write(MessageCode.ErrorInputFileIsNotFound, inputFile);
        return ActionResult.Failure;
      }

      if (configuration.ProcessDebugSymbols) {
        var debugSymbolsFile = FileHelper.GetDebugSymbolsFile(inputFile);
        if (!File.Exists(debugSymbolsFile)) {
          configuration.ProcessDebugSymbols = false;
          context.Logger.Write(MessageCode.WarningDebugSymbolsFileIsNotFound, debugSymbolsFile);
        }
      }

      var readerParameters = new ReaderParameters {
        ReadingMode = ReadingMode.Deferred,
        AssemblyResolver = context.AssemblyResolver,
        MetadataResolver = context.MetadataResolver,
        InMemory = true,
        ReadWrite = true,
        // will be used DefaultSymbolReaderProvider
        // it can identify pdb file by module
        // so there is no need to open stream and set SymbolReaderProvider
        ReadSymbols = configuration.ProcessDebugSymbols
      };

      var targetModule = ModuleDefinition.ReadModule(inputFile, readerParameters);

      if (context.Configuration.MakeBackup)
        FileHelper.CopyWithPdb(context, inputFile, FileHelper.GetBackupFile(inputFile));

      var writerParameters = new WriterParameters(){
        WriteSymbols = configuration.ProcessDebugSymbols
      };

      var strongNameKey = configuration.StrongNameKey;
      if (!string.IsNullOrEmpty(strongNameKey)) {
        if (File.Exists(strongNameKey))
          writerParameters.StrongNameKeyPair = LoadStrongNameKey(strongNameKey);
        else {
          context.Logger.Write(MessageCode.ErrorStrongNameKeyIsNotFound);
          return ActionResult.Failure;
        }
      }
      targetModule.Write(outputFile, writerParameters);

      context.TranformationPerformed = true;
      return ActionResult.Success;
    }

    private StrongNameKeyPair LoadStrongNameKey(string fileName)
    {
      using (var file = File.OpenRead(fileName))
        return new StrongNameKeyPair(file);
    }
  }
}