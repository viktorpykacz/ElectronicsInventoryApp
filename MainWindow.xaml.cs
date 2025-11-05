using ElectronicsInventoryApp.Models;
using ElectronicsInventoryApp.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace ElectronicsInventoryApp
{
    public partial class MainWindow : Window
    {
        private readonly ComponentService _service;
        private readonly CategoryService _categoryService;
        private readonly string _dataDir;
        private readonly string _componentsPath;
        private readonly string _categoriesPath;

        public MainWindow()
        {
            InitializeComponent();

            _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _componentsPath = Path.Combine(_dataDir, "electronicParts.json");
            _categoriesPath = Path.Combine(_dataDir, "categories.json");

            _service = new ComponentService(_componentsPath);
            _categoryService = new CategoryService(_categoriesPath);

            LoadGrid();
        }

        private void LoadGrid()
        {
            DataGridComponents.ItemsSource = null;

            var items = _service.GetAll()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Manufacturer,
                    p.PartNumber,
                    p.Description,
                    CategoryPath = _categoryService.GetFullPath(p.CategoryId),
                    p.Quantity,
                    p.DatasheetUrl,
                    Original = p
                })
                .ToList();

            DataGridComponents.ItemsSource = items;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditComponentWindow(_service, _categoryService);
            if (window.ShowDialog() == true)
                LoadGrid();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridComponents.SelectedItem;
            ElectronicPart? c = selected?.Original as ElectronicPart;

            if (c != null)
            {
                var window = new AddEditComponentWindow(_service, _categoryService, c);
                if (window.ShowDialog() == true)
                    LoadGrid();
            }
            else
            {
                MessageBox.Show("Wybierz element z listy");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridComponents.SelectedItem;
            ElectronicPart? c = selected?.Original as ElectronicPart;

            if (c != null)
            {
                if (MessageBox.Show($"Usunąć {c.Name}?", "Usuń", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _service.Delete(c.Id);
                    LoadGrid();
                }
            }
            else
            {
                MessageBox.Show("Wybierz element z listy");
            }
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridComponents.SelectedItem;
            ElectronicPart? part = selected?.Original as ElectronicPart;

            if (part == null)
            {
                MessageBox.Show("Wybierz element z listy.");
                return;
            }

            var window = new AddEditComponentWindow(_service, _categoryService, part);
            if (window.ShowDialog() == true)
                LoadGrid();
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridComponents.SelectedItem;
            ElectronicPart? part = selected?.Original as ElectronicPart;

            if (part == null)
            {
                MessageBox.Show("Wybierz element z listy.");
                return;
            }

            if (MessageBox.Show($"Usunąć {part.Name}?", "Usuń", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _service.Delete(part.Id);
                LoadGrid();
            }
        }

        private void MenuItem_Details_Click(object sender, RoutedEventArgs e)
        {
            dynamic selected = DataGridComponents.SelectedItem;
            ElectronicPart? part = selected?.Original as ElectronicPart;

            if (part == null)
            {
                MessageBox.Show("Wybierz element z listy.");
                return;
            }

            string categoryName = _categoryService.GetById(part.CategoryId)?.Name ?? "(brak kategorii)";

            MessageBox.Show(
                $"Nazwa: {part.Name}\n" +
                $"Producent: {part.Manufacturer}\n" +
                $"Numer katalogowy: {part.PartNumber}\n" +
                $"Kategoria: {categoryName}\n" +
                $"Ilość: {part.Quantity}\n" +
                $"URL: {part.DatasheetUrl}\n",
                "Szczegóły elementu");
        }

        private void DataGridComponents_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MenuItem_Edit_Click(sender, e);
        }

        // 📥 Import danych z JSON
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Wybierz plik komponentów",
                    Filter = "Pliki JSON (*.json)|*.json",
                    FileName = "electronicParts.json"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Wczytaj komponenty
                    var componentsJson = File.ReadAllText(dialog.FileName);
                    var components = JsonSerializer.Deserialize<List<ElectronicPart>>(componentsJson) ?? new List<ElectronicPart>();
                    _service.SetAll(components);

                    // Spróbuj znaleźć plik kategorii w tym samym folderze
                    string dir = Path.GetDirectoryName(dialog.FileName)!;
                    string categoriesPath = Path.Combine(dir, "categories.json");

                    if (File.Exists(categoriesPath))
                    {
                        var categoriesJson = File.ReadAllText(categoriesPath);
                        var categories = JsonSerializer.Deserialize<List<Category>>(categoriesJson) ?? new List<Category>();
                        _categoryService.SetAll(categories);
                    }

                    LoadGrid();
                    MessageBox.Show("Import zakończony pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas importu danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 📤 Eksport danych do JSON
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Zapisz dane komponentów",
                    Filter = "Pliki JSON (*.json)|*.json",
                    FileName = "electronicParts.json"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Zapisz komponenty
                    var componentsJson = JsonSerializer.Serialize(_service.GetAll(), new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dialog.FileName, componentsJson);

                    // Zapisz kategorie w tym samym folderze
                    string dir = Path.GetDirectoryName(dialog.FileName)!;
                    string categoriesPath = Path.Combine(dir, "categories.json");
                    var categoriesJson = JsonSerializer.Serialize(_categoryService.GetAll(), new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(categoriesPath, categoriesJson);

                    MessageBox.Show("Dane zostały zapisane pomyślnie!", "Eksport zakończony", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas eksportu danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
