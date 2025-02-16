using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace WpfDashboardApp
{
    public partial class StaffWindow : Window
    {
        
        private string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";
        private DataTable staffTable; 
        private int selectedStaffId; 

        public StaffWindow()
        {
            InitializeComponent();
            LoadStaffData(); // Load staff data when the window is initialized

        }
        // Load staff data from the database



        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStaffData();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchValue = SearchTextBox.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Use the stored procedure
                    using (SqlCommand command = new SqlCommand("proc_SearchStaff", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add the search value as a parameter
                        command.Parameters.AddWithValue("@SearchValue", searchValue);

                        // Execute the procedure and load results into a DataTable
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            DataTable searchResults = new DataTable();
                            searchResults.Load(reader);

                            // Bind the results to the DataGrid
                            StaffDataGrid.ItemsSource = searchResults.DefaultView;

                            // Show a message if no results are found
                            if (searchResults.Rows.Count == 0)
                            {
                                MessageBox.Show("No records found matching your search.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure staffTable is initialized before sorting
            if (staffTable == null || staffTable.Rows.Count == 0)
                return;

            // Get the selected ComboBox item and its Tag
            var selectedItem = SortComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string tag = selectedItem.Tag.ToString();
                string sortExpression = string.Empty;

                // Determine the sorting logic based on the Tag value
                switch (tag)
                {
                    case "Name_ASC":
                        sortExpression = "staff_name ASC";
                        break;
                    case "Name_DESC":
                        sortExpression = "staff_name DESC";
                        break;
                    case "Salary_ASC":
                        sortExpression = "salary ASC";
                        break;
                    case "Salary_DESC":
                        sortExpression = "salary DESC";
                        break;
                }

                // Apply sorting to the DefaultView of the DataTable
                if (!string.IsNullOrEmpty(sortExpression))
                {
                    staffTable.DefaultView.Sort = sortExpression;
                    StaffDataGrid.ItemsSource = staffTable.DefaultView; // Refresh the DataGrid
                }
            }
        }


        private async Task AddNewStaff()
        {
            string staffName = StaffNameTextBox.Text;
            int age = int.TryParse(AgeTextBox.Text, out age) ? age : 0;
            string designation = DesignationTextBox.Text;
            int salary = int.TryParse(SalaryTextBox.Text, out salary) ? salary : 0;
            DateTime dateOfJoining = DateOfJoiningDatePicker.SelectedDate ?? DateTime.Now;
            string contactNumber = ContactNumberTextBox.Text;
            string street = StreetTextBox.Text;
            string area = AreaTextBox.Text;
            string city = CityTextBox.Text;

            if (string.IsNullOrEmpty(staffName) || age == 0 || salary == 0)
            {
                MessageBox.Show("Please fill out all required fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Specify the stored procedure name
                    SqlCommand command = new SqlCommand("proc_addStaff", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters for the stored procedure
                    command.Parameters.AddWithValue("@StaffName", staffName);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Designation", designation);
                    command.Parameters.AddWithValue("@Salary", salary);
                    command.Parameters.AddWithValue("@DateOfJoining", dateOfJoining);
                    command.Parameters.AddWithValue("@ContactNumber", contactNumber);
                    command.Parameters.AddWithValue("@Street", street);
                    command.Parameters.AddWithValue("@Area", area);
                    command.Parameters.AddWithValue("@City", city);

                    // Execute the stored procedure
                    await command.ExecuteNonQueryAsync();
                    MessageBox.Show("Staff added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStaffData(); // Reload staff data after addition
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void AddStaffButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the form for adding new staff
            AddStaffForm.Visibility = Visibility.Visible;
        }

        private async void SubmitAddStaffButton_Click(object sender, RoutedEventArgs e)
        {
            await AddNewStaff(); // Assuming the AddNewStaff method already exists
        }


        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Commit any pending edits in the DataGrid
            if (StaffDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                try
                {
                    // Check if a row is selected
                    if (StaffDataGrid.SelectedItem is DataRowView selectedRow)
                    {
                        // Confirm the update action
                        if (MessageBox.Show("Do you want to save the changes to this staff member?", "Confirm Update",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            // Extract data from the selected row
                            int staffId = Convert.ToInt32(selectedRow["staff_id"]);
                            string staffName = selectedRow["staff_name"].ToString();
                            int age = Convert.ToInt32(selectedRow["age"]);
                            string designation = selectedRow["designation"].ToString();
                            int salary = Convert.ToInt32(selectedRow["salary"]);
                            string contactNumber = selectedRow["contact_no"].ToString();
                            string street = selectedRow["staff_street"].ToString();
                            string area = selectedRow["staff_area"].ToString();
                            string city = selectedRow["staff_city"].ToString();
                            DateTime dateOfJoining = Convert.ToDateTime(selectedRow["date_of_joining"]);

                            // Prepare parameters for the stored procedure
                            var parameters = new[]
                            {
                        new SqlParameter("@StaffID", staffId),
                        new SqlParameter("@StaffName", staffName),
                        new SqlParameter("@Age", age),
                        new SqlParameter("@Designation", designation),
                        new SqlParameter("@Salary", salary),
                        new SqlParameter("@DateOfJoining", dateOfJoining),
                        new SqlParameter("@ContactNumber", contactNumber),
                        new SqlParameter("@Street", street),
                        new SqlParameter("@Area", area),
                        new SqlParameter("@City", city)
                    };

                            // Update staff details asynchronously
                            await Task.Run(() => ExecuteNonQuery("proc_updateStaff", parameters));

                            MessageBox.Show("Staff details updated successfully!", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Refresh the DataGrid
                            LoadStaffData();
                            StaffDataGrid.Items.Refresh();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a staff member to update.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
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



        // Handle Delete Button Click for deleting a staff member
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var staffId = (button.DataContext as DataRowView)["staff_id"]; // Get staff ID

            if (MessageBox.Show("Are you sure you want to delete this staff member?", "Confirm Deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        await connection.OpenAsync();

                        SqlCommand command = new SqlCommand("proc_deleteStaff", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@StaffID", staffId);

                        // Execute the stored procedure
                        await command.ExecuteNonQueryAsync();

                        MessageBox.Show("Staff deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadStaffData(); // Reload staff data after deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            this.Close();
        }

        // Sample staff data (can be fetched from a database)
        // Load staff data from the database
        private async void LoadStaffData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Create a SqlCommand for the stored procedure
                    using (SqlCommand command = new SqlCommand("proc_displayStaff", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Execute the command and load the result into a DataTable
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            staffTable = new DataTable();
                            staffTable.Load(reader);
                        }

                        // Bind the DataTable to the DataGrid
                        StaffDataGrid.ItemsSource = staffTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public class Staff
        {
            public int StaffId { get; set; }
            public string StaffName { get; set; }
            public int Age { get; set; }
            public string Designation { get; set; }
            public double Salary { get; set; }
            public string ContactNumber { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
        }
    }
}