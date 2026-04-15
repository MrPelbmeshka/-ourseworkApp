namespace СourseworkApp.Models
{
    public class SortedArrayAlgorithmSettings : AlgorithmSettings
    {
        public int[] DataArray { get; set; }
        public int EffectiveSize { get; set; }
        public int? SearchKey { get; set; }
        public int? DeleteValue { get; set; }

        public SortedArrayAlgorithmSettings()
        {
            DataArray = new int[CollectionSize];
            EffectiveSize = CollectionSize;
        }

        // Создание копии
        public SortedArrayAlgorithmSettings Clone()
        {
            return new SortedArrayAlgorithmSettings
            {
                Name = this.Name,
                Description = this.Description,
                CollectionSize = this.CollectionSize,
                DataArray = (int[])this.DataArray.Clone(),
                EffectiveSize = this.EffectiveSize,
                SearchKey = this.SearchKey,
                DeleteValue = this.DeleteValue
            };
        }
    }
}
