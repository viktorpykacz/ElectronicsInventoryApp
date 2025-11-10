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
                .Select(p =>
                {
                    var levels = _categoryService.GetPathLevels(p.CategoryId);
                    return new
                        {
                            p.Id,
                            p.Name,
                            p.Manufacturer,
                            p.PartNumber,
                            p.Description,
                            Category = levels.Category,
                            Subcategory = levels.Subcategory,
                            Type = levels.Type,
                            p.Quantity,
                            p.DatasheetUrl,
                            Original = p
                        };
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
        private void DataGridComponents_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MenuItem_Edit_Click(sender, e);
        }
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
        private void MenuItem_ExportXls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Zapisz plik Excel",
                    Filter = "Excel (*.xlsx)|*.xlsx",
                    FileName = "components.xlsx"
                };

                if (dialog.ShowDialog() != true)
                    return;

                var all = _service.GetAll()
                    .Select(p =>
                    {
                        var levels = _categoryService.GetPathLevels(p.CategoryId);
                        return new
                        {
                            p.Name,
                            p.Manufacturer,
                            p.PartNumber,
                            p.Description,
                            Category = levels.Category,
                            Subcategory = levels.Subcategory,
                            Type = levels.Type,
                            p.Quantity,
                            p.DatasheetUrl
                        };
                    })
                    .ToList();

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Components");

                    // nagłówki
                    ws.Cell(1, 1).Value = "Nazwa";
                    ws.Cell(1, 2).Value = "Producent";
                    ws.Cell(1, 3).Value = "Nr katalogowy";
                    ws.Cell(1, 4).Value = "Opis";
                    ws.Cell(1, 5).Value = "Kategoria";
                    ws.Cell(1, 6).Value = "Podkategoria";
                    ws.Cell(1, 7).Value = "Typ";
                    ws.Cell(1, 8).Value = "Ilość";
                    ws.Cell(1, 9).Value = "Datasheet URL";

                    int row = 2;
                    foreach (var item in all)
                    {
                        ws.Cell(row, 1).Value = item.Name;
                        ws.Cell(row, 2).Value = item.Manufacturer;
                        ws.Cell(row, 3).Value = item.PartNumber;
                        ws.Cell(row, 4).Value = item.Description;

                        ws.Cell(row, 5).Value = item.Category;
                        ws.Cell(row, 6).Value = item.Subcategory;
                        ws.Cell(row, 7).Value = item.Type;

                        ws.Cell(row, 8).Value = item.Quantity;
                        ws.Cell(row, 9).Value = item.DatasheetUrl;
                        row++;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(dialog.FileName);
                }

                MessageBox.Show("Eksport zakończony pomyślnie!", "Sukces",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas eksportu:\n{ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
