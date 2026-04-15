using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using СourseworkApp.Models;

namespace СourseworkApp.Services
{
    public interface IAlgorithm
    {
        /// <summary>
        /// Запуск алгоритма с визуализацией
        /// </summary>
        /// <param name="settings">Настройки алгоритма</param>
        /// <param name="onStepUpdate">Колбэк для обновления визуализации на каждом шаге</param>
        /// <returns>Результат выполнения</returns>
        Task<AlgorithmResult> RunAsync(SortedArrayAlgorithmSettings settings, Action<AlgorithmStep> onStepUpdate);

        /// <summary>
        /// Сохранение состояния в файл
        /// </summary>
        void SaveToFile(string filePath, SortedArrayAlgorithmSettings settings);

        /// <summary>
        /// Загрузка состояния из файла
        /// </summary>
        SortedArrayAlgorithmSettings LoadFromFile(string filePath);
    }

    public class AlgorithmStep
    {
        public string StepName { get; set; }
        public string Description { get; set; }
        public int[] ArrayState { get; set; }
        public int EffectiveSize { get; set; }
        public int? Left { get; set; }      // Для поиска: левая граница
        public int? Right { get; set; }     // Для поиска: правая граница
        public int? Middle { get; set; }    // Для поиска: середина
        public int? DeleteIndex { get; set; } // Для удаления: индекс удаляемого
        public int? ShiftIndex { get; set; }  // Для удаления: текущий сдвигаемый индекс
        public bool IsSearchComplete { get; set; }
        public bool IsDeleteComplete { get; set; }
        public bool IsFound { get; set; }
    }

    public class AlgorithmResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int FoundIndex { get; set; } = -1;
        public int? DeletedValue { get; set; }
    }

}
