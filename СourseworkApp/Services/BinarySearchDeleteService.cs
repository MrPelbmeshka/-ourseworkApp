using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СourseworkApp.Models;

namespace СourseworkApp.Services
{
    public class BinarySearchDeleteService : IAlgorithm
    {
        private readonly ILogger _logger;

        public BinarySearchDeleteService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<AlgorithmResult> RunAsync(SortedArrayAlgorithmSettings settings, Action<AlgorithmStep> onStepUpdate)
        {
            var result = new AlgorithmResult();

            try
            {
                _logger.Info($"Запуск алгоритма. Поиск: {settings.SearchKey}, Удаление: {settings.DeleteValue}");

                // Проверка входных данных
                if (settings.DataArray == null || settings.DataArray.Length == 0)
                {
                    result.Success = false;
                    result.Message = "Массив пуст";
                    _logger.Error("Массив пуст");
                    return result;
                }

                // Выполняем поиск, если указан SearchKey
                if (settings.SearchKey.HasValue)
                {
                    var searchResult = await PerformBinarySearch(settings, onStepUpdate);
                    if (searchResult.IsFound)
                    {
                        result.FoundIndex = searchResult.Index;
                        _logger.Info($"Элемент {settings.SearchKey} найден на индексе {searchResult.Index}");
                    }
                    else
                    {
                        _logger.Warning($"Элемент {settings.SearchKey} не найден в массиве");
                    }
                }

                // Выполняем удаление, если указан DeleteValue
                if (settings.DeleteValue.HasValue)
                {
                    var deleteResult = await PerformDelete(settings, onStepUpdate);
                    if (deleteResult.Deleted)
                    {
                        result.DeletedValue = settings.DeleteValue;
                        _logger.Info($"Удаление значения {settings.DeleteValue} выполнено успешно");
                    }
                    else
                    {
                        _logger.Warning($"Значение {settings.DeleteValue} не найдено для удаления");
                    }
                }

                result.Success = true;
                result.Message = "Операция выполнена успешно";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Ошибка: {ex.Message}";
                _logger.Error($"Ошибка выполнения: {ex.Message}");
            }

            return result;
        }

        private async Task<(bool IsFound, int Index)> PerformBinarySearch(SortedArrayAlgorithmSettings settings, Action<AlgorithmStep> onStepUpdate)
        {
            int left = 0;
            int right = settings.EffectiveSize - 1;
            int searchValue = settings.SearchKey.Value;
            var array = settings.DataArray;

            while (left <= right)
            {
                int middle = (left + right) / 2;

                // Отправляем шаг для визуализации(ui)
                onStepUpdate(new AlgorithmStep
                {
                    StepName = "Бинарный поиск",
                    Description = $"Проверка middle = {middle}, значение = {array[middle]}",
                    ArrayState = (int[])array.Clone(),
                    EffectiveSize = settings.EffectiveSize,
                    Left = left,
                    Right = right,
                    Middle = middle,
                    IsSearchComplete = false
                });

                await Task.Delay(500); // Задержка для визуализации

                if (array[middle] == searchValue)
                {
                    onStepUpdate(new AlgorithmStep
                    {
                        StepName = "Бинарный поиск",
                        Description = $"Элемент {searchValue} найден на индексе {middle}!",
                        ArrayState = (int[])array.Clone(),
                        EffectiveSize = settings.EffectiveSize,
                        Left = left,
                        Right = right,
                        Middle = middle,
                        IsSearchComplete = true,
                        IsFound = true
                    });
                    return (true, middle);
                }

                if (searchValue < array[middle])
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            onStepUpdate(new AlgorithmStep
            {
                StepName = "Бинарный поиск",
                Description = $"Элемент {searchValue} не найден в массиве",
                ArrayState = (int[])array.Clone(),
                EffectiveSize = settings.EffectiveSize,
                IsSearchComplete = true,
                IsFound = false
            });

            return (false, -1);
        }

        private async Task<(bool Deleted, int Index)> PerformDelete(SortedArrayAlgorithmSettings settings, Action<AlgorithmStep> onStepUpdate)
        {
            int deleteValue = settings.DeleteValue.Value;
            var array = settings.DataArray;

            // Сначала ищем элемент для удаления
            int deleteIndex = -1;
            for (int i = 0; i < settings.EffectiveSize; i++)
            {
                if (array[i] == deleteValue)
                {
                    deleteIndex = i;
                    break;
                }
            }

            if (deleteIndex == -1)
            {
                return (false, -1);
            }

            // Отмечаем элемент для удаления
            onStepUpdate(new AlgorithmStep
            {
                StepName = "Удаление",
                Description = $"Элемент {deleteValue} найден на индексе {deleteIndex}. Удаляем...",
                ArrayState = (int[])array.Clone(),
                EffectiveSize = settings.EffectiveSize,
                DeleteIndex = deleteIndex,
                IsDeleteComplete = false
            });

            await Task.Delay(500);

            // Сдвигаем элементы влево
            for (int i = deleteIndex; i < settings.EffectiveSize - 1; i++)
            {
                array[i] = array[i + 1];

                onStepUpdate(new AlgorithmStep
                {
                    StepName = "Удаление",
                    Description = $"Сдвиг: элемент с индекса {i + 1} перемещён на индекс {i}",
                    ArrayState = (int[])array.Clone(),
                    EffectiveSize = settings.EffectiveSize,
                    DeleteIndex = deleteIndex,
                    ShiftIndex = i,
                    IsDeleteComplete = false
                });

                await Task.Delay(300);
            }

            // Уменьшаем эффективный размер
            settings.EffectiveSize--;

            onStepUpdate(new AlgorithmStep
            {
                StepName = "Удаление",
                Description = $"Удаление завершено. Эффективный размер массива: {settings.EffectiveSize}",
                ArrayState = (int[])array.Clone(),
                EffectiveSize = settings.EffectiveSize,
                IsDeleteComplete = true
            });

            return (true, deleteIndex);
        }

        public void SaveToFile(string filePath, SortedArrayAlgorithmSettings settings)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"#SavedArray");
                    writer.WriteLine($"EffectiveSize:{settings.EffectiveSize}");
                    writer.WriteLine($"TotalSize:{settings.CollectionSize}");
                    writer.WriteLine($"ArrayData:{string.Join(",", settings.DataArray)}");
                }
                _logger.Info($"Сохранение в файл: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка сохранения: {ex.Message}");
                throw;
            }
        }

        public SortedArrayAlgorithmSettings LoadFromFile(string filePath)
        {
            try
            {
                var settings = new SortedArrayAlgorithmSettings();
                using (var reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("EffectiveSize:"))
                            settings.EffectiveSize = int.Parse(line.Substring("EffectiveSize:".Length));
                        else if (line.StartsWith("TotalSize:"))
                            settings.CollectionSize = int.Parse(line.Substring("TotalSize:".Length));
                        else if (line.StartsWith("ArrayData:"))
                        {
                            var dataStr = line.Substring("ArrayData:".Length);
                            var data = dataStr.Split(',');
                            settings.DataArray = new int[settings.CollectionSize];
                            for (int i = 0; i < data.Length && i < settings.CollectionSize; i++)
                            {
                                settings.DataArray[i] = int.Parse(data[i]);
                            }
                        }
                    }
                }
                _logger.Info($"Загрузка из файла: {filePath}");
                return settings;
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка загрузки: {ex.Message}");
                throw;
            }
        }
    }

    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
}
