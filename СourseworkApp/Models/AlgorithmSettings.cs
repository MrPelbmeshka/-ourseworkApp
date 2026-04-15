namespace СourseworkApp.Models
{
    public class AlgorithmSettings
    {
        public string Name { get; set; } = "Бинарный поиск и удаление в отсортированном массиве";
        public string Description { get; set; } = "Визуализация операций бинарного поиска и удаления по значению";
        public int CollectionSize { get; set; }

        public AlgorithmSettings()
        {
            CollectionSize = 8;
        }
    }
}
