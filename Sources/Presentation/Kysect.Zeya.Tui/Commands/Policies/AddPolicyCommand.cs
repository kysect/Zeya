using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Spectre.Console;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tui.Commands.Policies;

public class AddPolicyCommand(
    IValidationPolicyApi validationPolicyApi,
    IFileSystem fileSystem,
    IAnsiConsole console) : ITuiCommand
{
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

        validationPolicyApi.CreatePolicy(name, content);
    }
}