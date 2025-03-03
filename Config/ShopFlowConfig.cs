using System.Configuration;
using shopFlow.Persistency;

namespace shopFlow.Config
{
    public interface IShopFlowConfig
    {
        string PersistencyMainFolder { get; }
        string[] Supplies { get; }
        string[] ExpensesTypes { get; }
    }
    public class ShopFlowConfig
    {
        public static IShopFlowConfig Instance = new DefaultShopFlowConfig();
        public static void SetConfigurationManger(ConfigurationManager configuration)
        {
            _configurationManager = configuration;
        }
        private static ConfigurationManager? _configurationManager;
        private sealed class DefaultShopFlowConfig : IShopFlowConfig
        {
            public string PersistencyMainFolder { get; } = "/data/shopFlowMovs"; // default volume target
            public string[] Supplies => ExpensesConfigPersistency.LoadSupplies().ToArray();
            public string[] ExpensesTypes => ExpensesConfigPersistency.LoadExpTypes().ToArray();
        }
    }
}