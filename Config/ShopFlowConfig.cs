using System.Configuration;
using shopFlow.Persistency;

namespace shopFlow.Config
{
    public interface IShopFlowConfig
    {
        string PersistencyMainFolder { get; }
        string[] GetSupplies();
        string[] GetExpensesTypes();
    }
    public class ShopFlowConfig
    {
        public static readonly IShopFlowConfig Instance = new DefaultShopFlowConfig();
        public static void SetConfigurationManger(ConfigurationManager configuration)
        {
            _configurationManager = configuration;
        }
        private static ConfigurationManager? _configurationManager;
        private sealed class DefaultShopFlowConfig : IShopFlowConfig
        {
            public string PersistencyMainFolder { get; } = _configurationManager?.GetValue<string>("PersistencyMainFolder") ?? "/data/shopFlowMovs";
            public string[] GetSupplies() => ExpensesConfigPersistency.LoadSupplies().ToArray();
            public string[] GetExpensesTypes() => ExpensesConfigPersistency.LoadExpTypes().ToArray();
        }
    }
}