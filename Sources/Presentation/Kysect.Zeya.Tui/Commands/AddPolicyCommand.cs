using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.IntegrationManager;
using Spectre.Console;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tui.Commands;

public class AddPolicyCommand(
    ValidationPolicyService validationPolicyService,
    IFileSystem fileSystem,
    IAnsiConsole console) : ITuiCommand
{
    public string Name => "Add policy";

    public void Execute()
    {
        string name = console.Ask<string>("Enter policy name");
        string filePath = console.Ask<string>("Enter the path to the policy file");

        if (!fileSystem.File.Exists(filePath))
        {
            console.WriteLine("File does not exist.");
            return;
        }

        string content = fileSystem.File.ReadAllText(filePath);

        validationPolicyService.CreatePolicy(name, content);
    }
}