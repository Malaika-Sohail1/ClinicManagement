using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfDashboardApp
{
    public partial class MedicineWindow : Window
    {
        private string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";
        private DataTable medicineTable;

        public MedicineWindow()
        {
            InitializeComponent();
            LoadMedicineData(); // Load medicine data when the window is initialized
        }
      
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var medicineId = (button.DataContext as DataRowView)["medicine_id"]; // Get medicine ID

            if (MessageBox.Show("Are you sure you want to delete this medicine?", "Confirm Deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        await connection.OpenAsync();

                        SqlCommand command = new SqlCommand("proc_archiveDeleteMedicine", connection) 
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@MedicineID", medicineId);

                        await command.ExecuteNonQueryAsync();

                        MessageBox.Show("Medicine archived and deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadMedicineData(); // Reload active medicines
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void ViewArchivedMedicinesButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM tbl_medicine_archive";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable archiveTable = new DataTable();
                    adapter.Fill(archiveTable);

                    if (archiveTable.Rows.Count > 0)
                    {
                        MedicineDataGrid.ItemsSource = archiveTable.DefaultView;
                        ToggleActionsColumnVisibility(false); // Hide Actions column for archived medicines
                        MessageBox.Show("Archived medicines loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No archived medicines found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        ToggleActionsColumnVisibility(false); // Ensure Actions column is hidden
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //load medicine from database
        private void LoadMedicineData()
        {
            string query = "SELECT * FROM tbl_medicine";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    medicineTable = new DataTable();
                    adapter.Fill(medicineTable);

                    if (medicineTable.Rows.Count > 0)
                    {
                        MedicineDataGrid.ItemsSource = medicineTable.DefaultView;
                        ToggleActionsColumnVisibility(true); // Show Actions column for active medicines
                    }
                    else
                    {
                        MessageBox.Show("No records found in the database.", "Data Load", MessageBoxButton.OK, MessageBoxImage.Information);
                        ToggleActionsColumnVisibility(false); // Hide Actions column if no data
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // Handle Search Button Click
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchValue = SearchTextBox.Text.Trim(); // Trim to remove leading/trailing spaces

            if (string.IsNullOrWhiteSpace(searchValue))
            {
                MessageBox.Show("Please enter a search term to perform the search.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Exit the method without executing the query
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Use proc_search_medicine to perform the search
                    SqlCommand command = new SqlCommand("proc_search_medicine", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@SearchTerm", searchValue);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable searchResultsTable = new DataTable();
                    adapter.Fill(searchResultsTable);

                    MedicineDataGrid.ItemsSource = searchResultsTable.DefaultView;
                    SearchTextBox.Clear();

                    if (searchResultsTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No records found matching your search.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // Handle Sort Selection Change
        private void SortComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (medicineTable == null) return;

            DataView view = medicineTable.DefaultView;

            switch (SortComboBox.SelectedIndex)
            {
                case 0: // Alphabetically (A-Z)
                    view.Sort = "medicine_name ASC";
                    break;
                case 1: // Alphabetically (Z-A)
                    view.Sort = "medicine_name DESC";
                    break;
                case 2: // Price (Ascending)
                    view.Sort = "price ASC";
                    break;
                case 3: // Price (Descending)
                    view.Sort = "price DESC";
                    break;
            }

            MedicineDataGrid.ItemsSource = view;
        }

        // Handle Add Medicine Button Click
        private void AddMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddMedicineFormWindow addMedicineForm = new AddMedicineFormWindow();
                addMedicineForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Handle Submit Button to Add New Medicine
        private void SubmitAddMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            string medicineName = MedicineNameTextBox.Text;
            string brand = BrandTextBox.Text;
            string supplierId = SupplierTextBox.Text;
            string price = PriceTextBox.Text;
            string expiryDate = ExpDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            string quantity = QuantityTextBox.Text;

            if (string.IsNullOrWhiteSpace(medicineName) || string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(supplierId)
                || string.IsNullOrWhiteSpace(price) || string.IsNullOrWhiteSpace(expiryDate) || string.IsNullOrWhiteSpace(quantity))
            {
                MessageBox.Show("Please fill in all the fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string query = "INSERT INTO tbl_medicine (medicine_name, brand, supplier_id, price, exp_date, quantity) VALUES (@medicineName, @brand, @supplierId, @price, @expDate, @quantity)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@medicineName", medicineName);
                    command.Parameters.AddWithValue("@brand", brand);
                    command.Parameters.AddWithValue("@supplierId", supplierId);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@expDate", expiryDate);
                    command.Parameters.AddWithValue("@quantity", quantity);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Medicine added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        AddMedicineForm.Visibility = Visibility.Collapsed;
                        SearchAndFiltersPanel.Visibility = Visibility.Visible;
                        MedicineDataGrid.Visibility = Visibility.Visible;
                        LoadMedicineData(); // Refresh data grid
                    }
                    else
                    {
                        MessageBox.Show("Error: Medicine not added.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LowStockButton_Click(object sender, RoutedEventArgs e)
        {
            int stockThreshold = 10; // Threshold for low stock

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Use proc_low_stock_report for low stock retrieval
                    SqlCommand command = new SqlCommand("proc_low_stock_report", connection); // Correct procedure name
                    command.CommandType = CommandType.StoredProcedure;

                    // Add the stock threshold parameter
                    command.Parameters.AddWithValue("@StockThreshold", stockThreshold);

                    // Execute the stored procedure and fill the results into a DataTable
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable lowStockTable = new DataTable();
                    adapter.Fill(lowStockTable);

                    // Bind the low stock results to the DataGrid
                    MedicineDataGrid.ItemsSource = lowStockTable.DefaultView;

                    if (lowStockTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No medicines found with low stock.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ExpiryDateNearButton_Click(object sender, RoutedEventArgs e)
        {
            int expirationThreshold = 30; // Threshold for days to expiry

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Use proc_expiry_date_report for near expiry retrieval
                    SqlCommand command = new SqlCommand("proc_expiry_date_report", connection); // Correct procedure name
                    command.CommandType = CommandType.StoredProcedure;

                    // Add the expiration threshold parameter
                    command.Parameters.AddWithValue("@ExpirationThreshold", expirationThreshold);

                    // Execute the stored procedure and fill the results into a DataTable
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable nearExpiryTable = new DataTable();
                    adapter.Fill(nearExpiryTable);

                    // Bind the near expiry results to the DataGrid
                    MedicineDataGrid.ItemsSource = nearExpiryTable.DefaultView;

                    if (nearExpiryTable.Rows.Count == 0)
                    {
                        MessageBox.Show("No medicines found near expiration.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ToggleActionsColumnVisibility(bool showActions)
        {
            foreach (var column in MedicineDataGrid.Columns)
            {
                if (column.Header.ToString() == "Actions") // Match the column header name
                {
                    column.Visibility = showActions ? Visibility.Visible : Visibility.Collapsed;
                    break;
                }
            }
        }




        public class MedicineStock
        {
            public int MedicineID { get; set; }       
            public string MedicineName { get; set; }
            public int Quantity { get; set; }
            public DateTime ExpiryDate { get; set; }
        }


        private async void ViewPriceHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the PriceHistoryPanel
            PriceHistoryPanel.Visibility = Visibility.Visible;

            // Optionally, hide other panels or adjust visibility as needed
            AddMedicineForm.Visibility = Visibility.Collapsed;

            // SQL query to fetch price history where old price is different from new price
            string query = "SELECT audit_id, medicine_id, action_date, old_price, new_price, changed_by " +
                           "FROM tbl_medicine_audit WHERE old_price <> new_price ORDER BY action_date DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable priceHistoryTable = new DataTable();

                        // Asynchronously fill the data table
                        await Task.Run(() => adapter.Fill(priceHistoryTable));

                        if (priceHistoryTable.Rows.Count > 0)
                        {
                            // Set the data table to the DataGrid for viewing
                            PriceHistoryDataGrid.ItemsSource = priceHistoryTable.DefaultView;
                            ToggleActionsColumnVisibility(false); // Hide Actions column for price history
                            MessageBox.Show("Price history loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No price history found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            ToggleActionsColumnVisibility(false); // Ensure Actions column is hidden
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Commit any pending edits in the DataGrid
            if (MedicineDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                try
                {
                    // Check if a row is selected
                    if (MedicineDataGrid.SelectedItem is DataRowView selectedRow)
                    {
                        // Confirm the update action
                        if (MessageBox.Show("Do you want to save the changes to this medicine?", "Confirm Update",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            int medicineId = Convert.ToInt32(selectedRow["medicine_id"]);
                            string medicineName = selectedRow["medicine_name"].ToString();
                            string brand = selectedRow["brand"].ToString();
                            int supplierId = Convert.ToInt32(selectedRow["supplier_id"]);
                            decimal price = Convert.ToDecimal(selectedRow["price"]);
                            DateTime expDate = Convert.ToDateTime(selectedRow["exp_date"]);
                            int quantity = Convert.ToInt32(selectedRow["quantity"]);

                            // Parameters for the stored procedure
                            var parameters = new[]
                            {
                        new SqlParameter("@MedicineID", medicineId),
                        new SqlParameter("@MedicineName", medicineName),
                        new SqlParameter("@Brand", brand),
                        new SqlParameter("@SupplierID", supplierId),
                        new SqlParameter("@Price", price),
                        new SqlParameter("@ExpDate", expDate),
                        new SqlParameter("@Quantity", quantity)
                    };

                            // Update medicine details asynchronously
                            await Task.Run(() => ExecuteNonQuery("UpdateMedicine", parameters));

                            MessageBox.Show("Medicine details updated successfully!", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Refresh the DataGrid
                            LoadMedicineData();
                            MedicineDataGrid.Items.Refresh();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a medicine to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void RefreshMedcineButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMedicineData();
            // Show the PriceHistoryPanel
            PriceHistoryPanel.Visibility = Visibility.Collapsed;

            // Optionally, hide other panels or adjust visibility as needed
            AddMedicineForm.Visibility = Visibility.Visible;

        }


        // Handle Exit Button Click
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


       
        }


 }
