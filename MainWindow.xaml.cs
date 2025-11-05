using ElectronicsInventoryApp.Models;
using ElectronicsInventoryApp.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ElectronicsInventoryApp
{
    public partial class MainWindow : Window
    {
        private readonly ComponentService _service;
        private readonly CategoryService _categoryService;

        public MainWindow()
        {
            InitializeComponent();

            string dataDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data");
            _service = new ComponentService(System.IO.Path.Combine(dataDir, "electronicParts.json"));
            _categoryService = new CategoryService(System.IO.Path.Combine(dataDir, "categories.json"));

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
                    Category = _categoryService.GetById(p.CategoryId)?.Name ?? string.Empty,
                    p.Quantity,
                    Original = p
                })
                .ToList();

            DataGridComponents.ItemsSource = items;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var q = SearchBox.Text ?? string.Empty;
            DataGridComponents.ItemsSource = _service.Search(q).ToList();
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
                $"Ilość: {part.Quantity}\n",
                "Szczegóły elementu");
        }

        private void DataGridComponents_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MenuItem_Edit_Click(sender, e);
        }
  }
}
