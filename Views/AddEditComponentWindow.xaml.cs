using ElectronicsInventoryApp.Models;
using System.Linq;
using System.Windows;

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


            CategoryBox.ItemsSource = _categoryService.GetAll();
            CategoryBox.DisplayMemberPath = "Name";
            CategoryBox.SelectedValuePath = "Id";

            if (_isEdit)
            {
                NameBox.Text = _part.Name;
                ManufacturerBox.Text = _part.Manufacturer;
                PartNumberBox.Text = _part.PartNumber;
                DescriptionBox.Text = _part.Description;
                QuantityBox.Text = _part.Quantity.ToString();

                CategoryBox.SelectedValue = _part.CategoryId;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _part.Name = NameBox.Text;
            _part.Manufacturer = ManufacturerBox.Text;
            _part.PartNumber = PartNumberBox.Text;
            _part.Description = DescriptionBox.Text;
            int.TryParse(QuantityBox.Text, out int qty);
            _part.Quantity = qty;

            if (CategoryBox.SelectedValue is int catId)
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
