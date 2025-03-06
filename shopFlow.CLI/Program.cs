using shopFlow.Persistency.Impl;
using shopFlow.Persistency;
using shopFlow.CLI;
using shopFlow.CLI.Actions;

var command = args != null ? args[0] : null;
if (command == null)
{
    Console.WriteLine("shopFlow.CLI..available commands are:\n" + string.Join(',', Enum.GetValues<Commands>()));
    return;
}
if (Enum.TryParse<Commands>(command, true, out var cmd))
{
    Console.WriteLine($"Executing : {cmd}");
    switch (cmd)
    {
        case Commands.MIGRATE:
            new MigrateJsonToLiteDBAction().Execute();
            break;
    }
    return;
}
else
{
    Console.WriteLine("shopFlow.CLI..unknown command:" + command + " available commands are:\n" + string.Join(',', Enum.GetValues<Commands>()));
}
