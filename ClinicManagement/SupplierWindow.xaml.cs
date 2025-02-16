using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfDashboardApp
{
    public partial class SupplierWindow : Window
    {
        private string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";
        private DataTable supplierTable;

        public SupplierWindow()
        {
            InitializeComponent();
            LoadSupplierData(); // Load supplier data when the window is initialized
        }

        // Load supplier data from the database
        private void LoadSupplierData()
        {
            string query = "SELECT supplier_id, supplier_name, supplier_type, payment_due, contact_no, supplier_street, supplier_area, supplier_city, supplier_postal_code, supplier_email, last_supply_date FROM tbl_suppliers";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    supplierTable = new DataTable();
                    adapter.Fill(supplierTable);
                    SupplierDataGrid.ItemsSource = supplierTable.DefaultView; // Bind data to the DataGrid
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Commit any pending edits in the DataGrid
            if (SupplierDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                try
                {
                    // Check if a row is selected
                    if (SupplierDataGrid.SelectedItem is DataRowView selectedRow)
                    {
                        // Confirm the update action
                        if (MessageBox.Show("Do you want to save the changes to this supplier?", "Confirm Update",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            int supplierId = Convert.ToInt32(selectedRow["supplier_id"]);
                            string supplierName = selectedRow["supplier_name"].ToString();
                            string supplierType = selectedRow["supplier_type"].ToString();
                            decimal paymentDue = Convert.ToDecimal(selectedRow["payment_due"]);
                            string contactNo = selectedRow["contact_no"].ToString();
                            string street = selectedRow["supplier_street"].ToString();
                            string area = selectedRow["supplier_area"].ToString();
                            string city = selectedRow["supplier_city"].ToString();
                            string postalCode = selectedRow["supplier_postal_code"].ToString();
                            string email = selectedRow["supplier_email"].ToString();
                            DateTime lastSupplyDate = Convert.ToDateTime(selectedRow["last_supply_date"]);

                            // Parameters for the stored procedure
                            var parameters = new[]
                            {
                        new SqlParameter("@SupplierID", supplierId),
                        new SqlParameter("@SupplierName", supplierName),
                        new SqlParameter("@SupplierType", supplierType),
                        new SqlParameter("@PaymentDue", paymentDue),
                        new SqlParameter("@ContactNo", contactNo),
                        new SqlParameter("@Street", street),
                        new SqlParameter("@Area", area),
                        new SqlParameter("@City", city),
                        new SqlParameter("@PostalCode", postalCode),
                        new SqlParameter("@Email", email),
                        new SqlParameter("@LastSupplyDate", lastSupplyDate)
                    };

                            // Update supplier details asynchronously
                            await Task.Run(() => ExecuteNonQuery("UpdateSupplier", parameters));

                            MessageBox.Show("Supplier details updated successfully!", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Refresh the DataGrid
                            LoadSupplierData();
                            SupplierDataGrid.Items.Refresh();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a supplier to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExecuteNonQuery(string storedProcedure, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(storedProcedure, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the Supplier ID from the DataContext of the button
            var button = sender as Button;
            var supplierID = (button.DataContext as DataRowView)["supplier_id"]; // Extract supplier_id from the bound data context

            // Show confirmation dialog
            var result = MessageBox.Show("Are you sure you want to delete this supplier?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Define the SQL command for the stored procedure
                        using (SqlCommand command = new SqlCommand("proc_deleteSupplier", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            // Add parameter
                            command.Parameters.AddWithValue("@SupplierID", supplierID);

                            // Execute the stored procedure
                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Supplier deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                RefreshSupplierDataGrid(); // Refresh the data grid to reflect the deletion
                            }
                            else
                            {
                                MessageBox.Show("No supplier was deleted. Please check the supplier ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshSupplierDataGrid()
        {
            LoadSupplierData();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSupplierData();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchValue = SearchTextBox.Text.Trim(); // Get the value from the search box and remove leading/trailing spaces

            // Check if the search field is empty
            if (string.IsNullOrEmpty(searchValue))
            {
                MessageBox.Show("Please enter a name to search.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Exit the method if search field is empty
            }

            DataTable dt = new DataTable();

            try
            {
                // Update the connection string with your database credentials
                string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Use the stored procedure
                    using (SqlCommand cmd = new SqlCommand("SearchSupplierByName", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SearchValue", searchValue);

                        // Fill the DataTable with the search results
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                        SearchTextBox.Clear();
                    }
                }

                // Bind the results to the DataGrid
                if (dt.Rows.Count > 0)
                {
                    SupplierDataGrid.ItemsSource = dt.DefaultView;
                }
                else
                {
                    SupplierDataGrid.ItemsSource = null;
                    MessageBox.Show("No records found matching your search.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Handle Sort Selection Change
        private void SortComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (supplierTable == null) return;

            DataView view = supplierTable.DefaultView;

            switch (SortComboBox.SelectedIndex)
            {
                case 0: // Alphabetically (A-Z)
                    view.Sort = "supplier_name ASC";
                    break;
                case 1: // Alphabetically (Z-A)
                    view.Sort = "supplier_name DESC";
                    break;
                case 2: // Payment Due (Ascending)
                    view.Sort = "payment_due ASC";
                    break;
                case 3: // Payment Due (Descending)
                    view.Sort = "payment_due DESC";
                    break;
            }

            SupplierDataGrid.ItemsSource = view;
        }

        // Handle Add Supplier Button Click
        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            SupplierDataGrid.Visibility = Visibility.Collapsed;
            SearchAndFiltersPanel.Visibility = Visibility.Collapsed;

            AddSupplierForm.Visibility = Visibility.Visible;
            ToggleExitButtonVisibility(false);
        }

        public enum SupplierType
        {
            Medicinal,
            Equipment,
            Services
        }

        private void SubmitAddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            string supplierName = SupplierNameTextBox.Text;
            string supplierTypeText = SupplierTypeTextBox.Text; // Get input from user
            string paymentDue = PaymentDueTextBox.Text;
            string contactNo = ContactNoTextBox.Text;
            string street = StreetTextBox.Text;
            string area = AreaTextBox.Text;
            string city = CityTextBox.Text;
            string postalCode = PostalCodeTextBox.Text;
            string email = EmailTextBox.Text;
            string lastSupplyDate = LastSupplyDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            // Validate input fields
            if (string.IsNullOrWhiteSpace(supplierName) || string.IsNullOrWhiteSpace(supplierTypeText) || string.IsNullOrWhiteSpace(paymentDue)
                || string.IsNullOrWhiteSpace(contactNo) || string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(area)
                || string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(lastSupplyDate))
            {
                MessageBox.Show("Please fill in all the fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate and parse supplier type
            if (!Enum.TryParse(supplierTypeText, true, out SupplierType supplierType))
            {
                MessageBox.Show("Invalid supplier type entered. Valid options are: Medicinal, Equipment, Services.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string query = "AddNewSupplier";  // Name of the stored procedure

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@supplierName", supplierName);
                    command.Parameters.AddWithValue("@supplierType", supplierType.ToString()); // Pass enum as string
                    command.Parameters.AddWithValue("@paymentDue", paymentDue);
                    command.Parameters.AddWithValue("@contactNo", contactNo);
                    command.Parameters.AddWithValue("@street", street);
                    command.Parameters.AddWithValue("@area", area);
                    command.Parameters.AddWithValue("@city", city);
                    command.Parameters.AddWithValue("@postalCode", postalCode);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@lastSupplyDate", lastSupplyDate);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Supplier added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        AddSupplierForm.Visibility = Visibility.Collapsed;
                        SearchAndFiltersPanel.Visibility = Visibility.Visible;
                        SupplierDataGrid.Visibility = Visibility.Visible;
                        LoadSupplierData(); // Refresh data grid
                    }
                    else
                    {
                        MessageBox.Show("Error: Supplier not added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender,RoutedEventArgs e)
        {
            SearchAndFiltersPanel.Visibility = Visibility.Visible;
            SupplierDataGrid.Visibility = Visibility.Visible;
            AddSupplierForm.Visibility = Visibility.Collapsed;
            ToggleExitButtonVisibility(true);
        }
        private void ToggleExitButtonVisibility(bool isVisible)
        {
            ExitButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }


        // Handle Exit Button Click
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ToggleExitButtonVisibility(true);
        }
        public class Supplier
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; }
            public string SupplierType { get; set; }
            public decimal PaymentDue { get; set; }
            public string ContactNo { get; set; }
            public string SupplierStreet { get; set; }
            public string SupplierArea { get; set; }
            public string SupplierCity { get; set; }
            public string SupplierPostalCode { get; set; }
            public string SupplierEmail { get; set; }
            public DateTime LastSupplyDate { get; set; }
        }

    }


}