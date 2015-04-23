using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Aspects.Weaver.ExtensionBase;

namespace Xtensive.Aspects.Weaver
{
  public class MesControlValidatorStage : ValidatorStage
  {
    private const string MesControlAssemblyCulture = "neutral";
    private const  string MesControlAssemblyPublicKeyToken = "67d5889111bf42c8";
    private readonly Regex mescontrolReferencesMatcher = new Regex(@"^MEScontrol\.[A-Za-z\.]*");

    public override bool Validate(PostSharp.Sdk.Extensibility.Project project)
    {
      var mescontrolReferences = project.Module.AssemblyRefs
        .Where(el => mescontrolReferencesMatcher.IsMatch(el.Name)).ToList();
      if (mescontrolReferences.Count != 0 && mescontrolReferences.Any(el => PublicKeyTokenToString(el.GetPublicKeyToken())==MesControlAssemblyPublicKeyToken))
        return true;
      return false;
    }

    private string PublicKeyTokenToString(byte[] bytes)
    {
      string result = string.Empty;
      for (var i = 0; i < bytes.Length; i++)
        result += string.Format("{0:x2}", bytes[i]);
      return result;
    }
  }

  internal class LogToFile
  {
    private string logFilePath = @"C:\Users\a.kulakov\Documents\visual studio 2010\Projects\MEScontrol.AAA.FakeLibrary\log.txt";
    public void Write(string message)
    {
      using (var fileStream = File.Open(logFilePath,FileMode.Append)) {
        using (var textWriter = new StreamWriter(fileStream)) {
          textWriter.WriteLine(string.Format("{0} | {1}", DateTime.Now, message));
        }
      }
    }
  }
}
