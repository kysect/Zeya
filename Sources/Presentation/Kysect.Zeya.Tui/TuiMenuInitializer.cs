using Kysect.TerminalUserInterface.DependencyInjection;
using Kysect.TerminalUserInterface.Navigation;

namespace Kysect.Zeya.Tui;

public class TuiMenuInitializer
{
    private readonly TuiMenuProvider _menuProvider;

    public TuiMenuInitializer(TuiMenuProvider menuProvider)
    {
        _menuProvider = menuProvider;
    }

    public TuiMenuNavigationItem CreateMenu()
    {
        var builder = new TuiMenuNavigationBuilder(_menuProvider);

        builder.WithMainMenu<RootMenu>();

        return builder.Build();
    }
}