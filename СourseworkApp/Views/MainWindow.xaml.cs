using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using СourseworkApp.Helpers;
using СourseworkApp.Models;
using СourseworkApp.Services;

namespace СourseworkApp
{
    public partial class MainWindow : Window
    {
        private SortedArrayAlgorithmSettings _settings;
        private BinarySearchDeleteService _algorithmService;
        private Logger _logger;
        private bool _isRunning = false;

        public ObservableCollection<ArrayItem> ArrayItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            ArrayItems = new ObservableCollection<ArrayItem>();
            ArrayItemsControl.ItemsSource = ArrayItems;

            _logger = new Logger();
            _algorithmService = new BinarySearchDeleteService(_logger);
            _settings = new SortedArrayAlgorithmSettings();

            SpeedSlider.ValueChanged += (s, e) => SpeedValueText.Text = $"{e.NewValue:F0} мс";

            GenerateRandomArray();
        }

        private void GenerateRandomArray()
        {
            int size = int.Parse(SizeTextBox.Text);
            if (size < 3 || size > 15)
            {
                MessageBox.Show("Размер массива должен быть от 3 до 15", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settings.CollectionSize = size;
            _settings.DataArray = new int[size];
            _settings.EffectiveSize = size;

            var rand = new Random();
            for (int i = 0; i < size; i++)
            {
                _settings.DataArray[i] = rand.Next(1, 100);
            }

            //SortArray();
            UpdateArrayDisplay();
            _logger.Info($"Сгенерирован массив размером {size}");
            StatusText.Text = "Сгенерирован новый массив";
        }

        private void SortArray()
        {
            Array.Sort(_settings.DataArray, 0, _settings.EffectiveSize);
            _logger.Info("Массив отсортирован");
        }

        private void UpdateArrayDisplay(int? left = null, int? right = null, int? middle = null,
                                         int? deleteIndex = null, int? shiftIndex = null,
                                         bool lastCellInactive = false)
        {
            ArrayItems.Clear();

            for (int i = 0; i < _settings.CollectionSize; i++)
            {
                bool isEffective = i < _settings.EffectiveSize;
                var item = new ArrayItem
                {
                    Index = i,
                    Value = _settings.DataArray[i],
                    IsEffective = isEffective,
                    BackgroundColor = GetCellColor(i, left, right, middle, deleteIndex, shiftIndex, !isEffective)
                };
                ArrayItems.Add(item);
            }
        }

        private Brush GetCellColor(int index, int? left, int? right, int? middle, int? deleteIndex, int? shiftIndex, bool isInactive)
        {
            if (isInactive)
                return new SolidColorBrush(Colors.LightGray);
            if (deleteIndex.HasValue && index == deleteIndex.Value)
                return new SolidColorBrush(Colors.OrangeRed);
            if (shiftIndex.HasValue && index >= deleteIndex && index <= shiftIndex)
                return new SolidColorBrush(Colors.LightSalmon);
            if (middle.HasValue && index == middle.Value)
                return new SolidColorBrush(Colors.LightGreen);
            if (left.HasValue && index == left.Value)
                return new SolidColorBrush(Colors.LightBlue);
            if (right.HasValue && index == right.Value)
                return new SolidColorBrush(Colors.LightCoral);

            return new SolidColorBrush(Colors.White);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;

            if (!int.TryParse(SearchTextBox.Text, out int searchKey))
            {
                MessageBox.Show("Введите корректное число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            } 
            _isRunning = true;
            SetButtonsEnabled(false);

            var tempSettings = _settings.Clone();
            tempSettings.SearchKey = searchKey;



            UpdateArrayDisplay();
            StepInfoText.Text = $"Поиск значения {searchKey}...";
            StatusText.Text = $"Идёт поиск {searchKey}";

            var result = await _algorithmService.RunAsync(tempSettings, step =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateArrayDisplay(step.Left, step.Right, step.Middle, step.DeleteIndex, step.ShiftIndex);
                    StepInfoText.Text = step.Description;

                    if (step.IsSearchComplete)
                    {
                        if (step.IsFound)
                            StepInfoText.Text = $"Элемент {searchKey} найден!";
                        else
                            StepInfoText.Text = $"Элемент {searchKey} не найден";
                    }
                });
            });

            if (result.FoundIndex >= 0)
                StatusText.Text = $"Найден на индексе {result.FoundIndex}";
            else
                StatusText.Text = $"{searchKey} не найден";

            _isRunning = false;
            SetButtonsEnabled(true);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;

            if (!int.TryParse(DeleteTextBox.Text, out int deleteValue))
            {
                MessageBox.Show("Введите корректное число", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isRunning = true;
            SetButtonsEnabled(false);

            var tempSettings = _settings.Clone();
            tempSettings.DeleteValue = deleteValue;

            UpdateArrayDisplay();
            StepInfoText.Text = $"Удаление значенияи {deleteValue}...";
            StatusText.Text = $"Идёт удаление {deleteValue}";

            var result = await _algorithmService.RunAsync(tempSettings, step =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateArrayDisplay(step.Left, step.Right, step.Middle, step.DeleteIndex, step.ShiftIndex);
                    StepInfoText.Text = step.Description;

                    if (step.IsDeleteComplete)
                    {
                        // Обновляем основной массив
                        _settings.DataArray = step.ArrayState;
                        _settings.EffectiveSize = step.EffectiveSize;
                        UpdateArrayDisplay();
                        StepInfoText.Text = $"Удаление завершено! Эффективный размер: {step.EffectiveSize}";
                    }
                });
            });

            if (result.DeletedValue.HasValue)
            {
                StatusText.Text = $"Значение {deleteValue} удалено. Новый размер: {_settings.EffectiveSize}";
                _logger.Info($"Удалено значение {deleteValue}");
            }
            else
            {
                StatusText.Text = $"Значение {deleteValue} не найдено";
            }

            _isRunning = false;
            SetButtonsEnabled(true);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;
            GenerateRandomArray();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;
            SortArray();
            UpdateArrayDisplay();
            StatusText.Text = "Массив отсортирован";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;
            GenerateRandomArray();
            StepInfoText.Text = "Готов к работе";
            StatusText.Text = "Готов";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Array files (*.arr)|*.arr|All files (*.*)|*.*",
                DefaultExt = ".arr",
                FileName = "array_save"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _algorithmService.SaveToFile(dialog.FileName, _settings);
                    MessageBox.Show($"Сохранено в {dialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = $"Сохранено: {System.IO.Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Array files (*.arr)|*.arr|All files (*.*)|*.*",
                DefaultExt = ".arr"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _settings = _algorithmService.LoadFromFile(dialog.FileName);
                    UpdateArrayDisplay();
                    MessageBox.Show($"Загружено из {dialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText.Text = $"Загружено: {System.IO.Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            GenerateButton.IsEnabled = enabled;
            SortButton.IsEnabled = enabled;
            SearchButton.IsEnabled = enabled;
            DeleteButton.IsEnabled = enabled;
            ResetButton.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;
            LoadButton.IsEnabled = enabled;

        }
    }

    public class ArrayItem : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public int Value { get; set; }
        public bool IsEffective { get; set; }

        private Brush _backgroundColor;
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}