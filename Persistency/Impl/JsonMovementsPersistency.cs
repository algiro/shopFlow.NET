using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using shopFlow.Config;
using shopFlow.Services;
using shopFlow.Utils;

namespace shopFlow.Persistency
{

    public class JsonMovementPersistency : IMovementPersistency
    {
        private readonly static ILogger _logger = LoggerUtils.CreateLogger<JsonMovementPersistency>();

        public const string SUB_FOLDER = "Movements";
        public bool Save(IMovement movement)
        {
            try
            {
                _logger.LogInformation("Save movement: {Movement}", movement);
                var periodFolder = $"{movement.Date.Year}-{movement.Date.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                File.WriteAllText(Path.Combine(fullPath, movement.GetFileName()), JsonConvert.SerializeObject(movement));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving movement: {Movement}", movement);
                return false;
            }

        }
        public bool Delete(IMovement movement)
        {
            try
            {
                _logger.LogInformation("Delete movement: {Movement}", movement);
                var periodFolder = $"{movement.Date.Year}-{movement.Date.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                var filePath = Path.Combine(fullPath, movement.GetFileName());
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movement: {Movement}", movement);
                return false;
            }

        }

        private IEnumerable<IMovement> LoadMovements((int Year, int Month) period)
        {
            try
            {
                _logger.LogInformation("LoadMovements with period: {Period}", period);
                var periodFolder = $"{period.Year}-{period.Month.ToString("00")}";
                var fullPath = Path.Combine(baseFolder, periodFolder);
                if (Directory.Exists(fullPath))
                {
                    DirectoryInfo d = new DirectoryInfo(fullPath);
                    FileInfo[] movementFiles = d.GetFiles("*_MOV.json"); //Getting Text files
                    List<IMovement> movements = new();
                    foreach (FileInfo file in movementFiles)
                    {
                        try
                        {
                            var movement = JsonConvert.DeserializeObject<DefaultMovement>(File.ReadAllText(file.FullName));
                            if (movement != null)
                                movements.Add(movement);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error loading movement from file: {File}", file.FullName);
                        }
                    }
                    _logger.LogInformation("Movements loaded: {Movements}", movements.Count);

                    return movements;
                }
                else
                {
                    _logger.LogInformation("No movements found for period: {Period}", period);
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movements for period: {Period}", period);
            }
            return [];
        }

        public IEnumerable<IMovement> LoadMovements(DateOnly fromDate, DateOnly toDate)
        {
            try
            {
                var periods = GetPeriods(fromDate, toDate);
                _logger.LogInformation("LoadMovements from {FromDate} to {ToDate} with periods: {Periods}", fromDate, toDate, periods);
                List<IMovement> movements = new();
                foreach (var period in periods)
                {
                    var periodMovs = LoadMovements(period);
                    movements.AddRange(periodMovs);
                }
                _logger.LogInformation("Movements loaded: {Movements}", movements.Count);

                return movements.Where(m => m.Date >= fromDate.ToDateTime(TimeOnly.Parse("00:00 AM")) && m.Date <= toDate.ToDateTime(TimeOnly.Parse("11:59 PM")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movements from {FromDate} to {ToDate}", fromDate, toDate);
            }
            return [];
        }

        private static (int Year, int Month)[] GetPeriods(DateOnly fromDate, DateOnly toDate)
        {
            var currDate = fromDate;
            List<(int Year, int Month)> periodFolders = new();
            while (currDate <= toDate)
            {
                var periodFolder = (currDate.Year, currDate.Month);
                periodFolders.Add(periodFolder);
                _logger.LogInformation("Period folder: {PeriodFolder}", periodFolder);
                currDate = currDate.AddMonths(1);
            }
            return periodFolders.ToArray();
        }
        private static string mainPersistencyFolder = ShopFlowConfig.Instance.PersistencyMainFolder ?? Directory.GetCurrentDirectory();
        private static string baseFolder = Path.Combine(mainPersistencyFolder, SUB_FOLDER);
    }
}