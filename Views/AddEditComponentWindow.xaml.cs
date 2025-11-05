using ElectronicsInventoryApp.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ElectronicsInventoryApp.Views
{
    public partial class AddEditComponentWindow : Window
    {
        private readonly ComponentService _service;
        private readonly CategoryService _categoryService;
        private readonly ElectronicPart _part;
        private readonly bool _isEdit;

        public AddEditComponentWindow(ComponentService service, CategoryService categoryService, ElectronicPart? part = null)
        {
            InitializeComponent();
            _service = service;
            _categoryService = categoryService;
            _part = part ?? new ElectronicPart();
            _isEdit = part != null;

            LoadCategories();

            if (_isEdit)
                LoadPartData();
        }

        private void LoadCategories()
        {
            var mainCategories = _categoryService.GetAll()
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToList();

            CategoryBox.ItemsSource = mainCategories;
            CategoryBox.DisplayMemberPath = "Name";
            CategoryBox.SelectedValuePath = "Id";
        }

        private void CategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCategory = CategoryBox.SelectedItem as Category;

            if (selectedCategory?.Subcategories?.Any() == true)
            {
                SubCategoryBox.ItemsSource = selectedCategory.Subcategories;
                SubCategoryBox.DisplayMemberPath = "Name";
                SubCategoryBox.SelectedValuePath = "Id";
                SubCategoryBox.IsEnabled = true;
            }
            else
            {
                SubCategoryBox.ItemsSource = null;
                SubCategoryBox.IsEnabled = false;
                SubSubCategoryBox.ItemsSource = null;
                SubSubCategoryBox.IsEnabled = false;
            }
        }

        private void SubCategoryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSub = SubCategoryBox.SelectedItem as Category;

            if (selectedSub?.Subcategories?.Any() == true)
            {
                SubSubCategoryBox.ItemsSource = selectedSub.Subcategories;
                SubSubCategoryBox.DisplayMemberPath = "Name";
                SubSubCategoryBox.SelectedValuePath = "Id";
                SubSubCategoryBox.IsEnabled = true;
            }
            else
            {
                SubSubCategoryBox.ItemsSource = null;
                SubSubCategoryBox.IsEnabled = false;
            }
        }

        private void LoadPartData()
        {
            NameBox.Text = _part.Name;
            ManufacturerBox.Text = _part.Manufacturer;
            PartNumberBox.Text = _part.PartNumber;
            DescriptionBox.Text = _part.Description;
            QuantityBox.Text = _part.Quantity.ToString();
            DatasheetUrlBox.Text = _part.DatasheetUrl;

            var currentCategory = _categoryService.GetById(_part.CategoryId);
            if (currentCategory == null)
                return;

            // Sprawdź poziomy hierarchii
            if (currentCategory.ParentId == null)
            {
                // Poziom 1
                CategoryBox.SelectedValue = currentCategory.Id;
            }
            else
            {
                var parent = _categoryService.GetById(currentCategory.ParentId.Value);

                if (parent != null)
                {
                    if (parent.ParentId == null)
                    {
                        // Poziom 2 (podkategoria)
                        CategoryBox.SelectedValue = parent.Id;
                        SubCategoryBox.ItemsSource = parent.Subcategories;
                        SubCategoryBox.DisplayMemberPath = "Name";
                        SubCategoryBox.SelectedValuePath = "Id";
                        SubCategoryBox.SelectedValue = currentCategory.Id;
                        SubCategoryBox.IsEnabled = true;
                    }
                    else
                    {
                        // Poziom 3 (pod-podkategoria)
                        var grandParent = _categoryService.GetById(parent.ParentId.Value);
                        if (grandParent != null)
                        {
                            CategoryBox.SelectedValue = grandParent.Id;
                            SubCategoryBox.ItemsSource = grandParent.Subcategories;
                            SubCategoryBox.DisplayMemberPath = "Name";
                            SubCategoryBox.SelectedValuePath = "Id";
                            SubCategoryBox.SelectedValue = parent.Id;
                            SubCategoryBox.IsEnabled = true;

                            SubSubCategoryBox.ItemsSource = parent.Subcategories;
                            SubSubCategoryBox.DisplayMemberPath = "Name";
                            SubSubCategoryBox.SelectedValuePath = "Id";
                            SubSubCategoryBox.SelectedValue = currentCategory.Id;
                            SubSubCategoryBox.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _part.Name = NameBox.Text;
            _part.Manufacturer = ManufacturerBox.Text;
            _part.PartNumber = PartNumberBox.Text;
            _part.Description = DescriptionBox.Text;
            _part.DatasheetUrl = DatasheetUrlBox.Text;
            int.TryParse(QuantityBox.Text, out int qty);
            _part.Quantity = qty;

            // Hierarchicznie: pod-podkategoria > podkategoria > kategoria
            if (SubSubCategoryBox.SelectedValue is int subSubCatId)
                _part.CategoryId = subSubCatId;
            else if (SubCategoryBox.SelectedValue is int subCatId)
                _part.CategoryId = subCatId;
            else if (CategoryBox.SelectedValue is int catId)
                _part.CategoryId = catId;

            if (_isEdit)
                _service.Update(_part);
            else
                _service.Add(_part);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
