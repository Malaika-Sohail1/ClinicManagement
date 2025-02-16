using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace WpfDashboardApp
{
    public partial class AddMedicineFormWindow : Window
    {
        private Dictionary<int, string> SuppliersList = new Dictionary<int, string>(); // Supplier ID and Name mapping
        private int SelectedSupplierId = 0; // Holds the selected supplier's ID

        public AddMedicineFormWindow()
        {
            InitializeComponent();
            LoadSuppliers(); // Load supplier data when the form opens
        }

        private void LoadSuppliers()
        {
            string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT supplier_id, supplier_name FROM tbl_suppliers";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            SuppliersList.Clear();
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                SuppliersList.Add(id, name);
                            }
                        }
                    }

                    SupplierComboBox.ItemsSource = SuppliersList.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SupplierComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplierComboBox.Text)) return;

            string searchText = SupplierComboBox.Text.ToLower();

            var filteredList = SuppliersList
                .Where(s => s.Value.ToLower().Contains(searchText))
                .ToList();

            SupplierComboBox.ItemsSource = filteredList;
            SupplierComboBox.IsDropDownOpen = true; // Keep the dropdown open to show filtered results
        }

        private void SupplierComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SupplierComboBox.SelectedValue is int supplierId)
            {
                SelectedSupplierId = supplierId; // Store the selected supplier's ID
            }
        }

        private void SubmitAddMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            string medicineName = MedicineNameTextBox.Text.Trim();
            string brand = BrandTextBox.Text.Trim();
            string price = PriceTextBox.Text.Trim();
            DateTime? expiryDate = ExpDatePicker.SelectedDate;
            string quantity = QuantityTextBox.Text.Trim();

            if (string.IsNullOrEmpty(medicineName) ||
                string.IsNullOrEmpty(brand) ||
                SelectedSupplierId == 0 ||
                string.IsNullOrEmpty(price) ||
                !expiryDate.HasValue ||
                string.IsNullOrEmpty(quantity))
            {
                MessageBox.Show("Please fill all fields before submitting.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(price, out int parsedPrice) || parsedPrice <= 0)
            {
                MessageBox.Show("Please enter a valid positive number for Price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(quantity, out int parsedQuantity) || parsedQuantity <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for Quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveMedicineToDatabase(medicineName, brand, SelectedSupplierId, parsedPrice, expiryDate.Value, parsedQuantity);

            MessageBox.Show("Medicine added successfully!");
            this.Close(); // Close the window after submission
        }

        private void SaveMedicineToDatabase(string medicineName, string brand, int supplierId, int price, DateTime expiryDate, int quantity)
        {
            string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO tbl_medicine (medicine_name, brand, supplier_id, price, exp_date, quantity) " +
                                   "VALUES (@MedicineName, @Brand, @SupplierId, @Price, @ExpDate, @Quantity)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MedicineName", medicineName);
                        command.Parameters.AddWithValue("@Brand", brand);
                        command.Parameters.AddWithValue("@SupplierId", supplierId);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@ExpDate", expiryDate);
                        command.Parameters.AddWithValue("@Quantity", quantity);

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving medicine data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelAddMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the window if cancel is clicked
        }
    }
}
