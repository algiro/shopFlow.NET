using System;
using shopFlow.Config;

namespace shopFlow.Persistency;

public class ExpensesConfigPersistency
{
    public const string SUB_FOLDER = "Config";
    public const string EXPENSES_TYPES_FILE = "ExpensesTypes.dat";
    public const string SUPPLIES_FILE = "Supplies.dat";
    public static bool SaveExpensesTypes(IEnumerable<string> expTypes)
    {
        try
        {
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            File.WriteAllText(Path.Combine(baseFolder, EXPENSES_TYPES_FILE), string.Join(Environment.NewLine, expTypes));
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return false;
        }

    }
    public static IEnumerable<string> LoadExpTypes()
    {
        try
        {

            var expTypesFile = Path.Combine(baseFolder, EXPENSES_TYPES_FILE);
            Console.Error.WriteLine("LoadExpTypes... Checking :" + expTypesFile);
            if (File.Exists(expTypesFile))
            {
                return File.ReadAllLines(expTypesFile);
            }
            else
            {
                Console.Error.WriteLine("No expTypes found as: " + expTypesFile);
                return Enumerable.Empty<string>();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
        return Enumerable.Empty<string>();
    }

    public static bool SaveSupplies(IEnumerable<string> supplies)
    {
        try
        {
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            File.WriteAllText(Path.Combine(baseFolder, SUPPLIES_FILE), string.Join(Environment.NewLine, supplies));
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return false;
        }

    }
    public static IEnumerable<string> LoadSupplies()
    {
        try
        {
            var suppliesFile = Path.Combine(baseFolder, SUPPLIES_FILE);
            if (File.Exists(suppliesFile))
            {
                return File.ReadAllLines(suppliesFile);
            }
            else
            {
                Console.Error.WriteLine("No supplies found as: " + suppliesFile);
                return Enumerable.Empty<string>();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
        return Enumerable.Empty<string>();
    }


    private static string mainPersistencyFolder = ShopFlowConfig.Instance.PersistencyMainFolder ?? Directory.GetCurrentDirectory();
    private static string baseFolder = Path.Combine(mainPersistencyFolder, SUB_FOLDER);
}

