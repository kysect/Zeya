using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.IntegrationManager;
using Spectre.Console;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tui.Commands;

public class AddPolicyCommand(
    ValidationPolicyService validationPolicyService,
    IFileSystem fileSystem) : ITuiCommand
{
    public string Name => "Add policy";

    public void Execute()
    {
        string name = AnsiConsole.Ask<string>("Enter policy name");
        string filePath = AnsiConsole.Ask<string>("Enter the path to the policy file");

        if (!fileSystem.File.Exists(filePath))
        {
            AnsiConsole.WriteLine("File does not exist.");
            return;
        }

        string content = fileSystem.File.ReadAllText(filePath);

        validationPolicyService.Create(name, content);
    }
}