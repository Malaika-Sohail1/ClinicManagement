using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace WpfDashboardApp
{
    public partial class PatientHistoryWindow : Window
    {
        public PatientHistoryWindow()
        {
            InitializeComponent();
            FetchPatients(); // Fetch all patients initially
        }
        // Method to execute stored procedures and fetch data
        private DataTable ExecuteStoredProcedure(string storedProcedure, SqlParameter[] parameters = null)
        {
            string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(storedProcedure, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"SQL error: {sqlEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching data: {ex.Message}");
                return null;
            }
        }

        // Method to fetch patients from the database
        private void FetchPatients(string storedProcedure = "Proc_GetAllPatients", SqlParameter[] parameters = null)
        {
            DataTable patientsData = ExecuteStoredProcedure(storedProcedure, parameters);
            if (patientsData != null && patientsData.Rows.Count > 0)
            {
                PatientsDataGrid.ItemsSource = null;  
                PatientsDataGrid.Items.Refresh();     
                PatientsDataGrid.ItemsSource = patientsData.DefaultView;
            }
            else
            {
                PatientsDataGrid.ItemsSource = null; 
                MessageBox.Show("No patient data found.");
            }

        }

     

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var parameters = new[] { new SqlParameter("@SearchTerm", searchTerm) };
                FetchPatients("proc_SearchPatients", parameters); // Fetch filtered data based on search term
                SearchTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a name to search.");
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Commit any pending edits in the DataGrid
            if (PatientsDataGrid.CommitEdit(DataGridEditingUnit.Row, true))
            {
                if (PatientsDataGrid.SelectedItem is DataRowView selectedRow)
                {
                    int patientId = Convert.ToInt32(selectedRow["patient_id"]);
                    string name = selectedRow["name"].ToString();
                    int age = Convert.ToInt32(selectedRow["age"]);
                    string gender = selectedRow["gender"].ToString();
                    string contactNo = selectedRow["contact_no"].ToString();
                    string type = selectedRow["type"].ToString();
                    DateTime dateOfVisit = Convert.ToDateTime(selectedRow["dateofVisit"]);

                    var parameters = new[]
                    {
                new SqlParameter("@PatientId", patientId),
                new SqlParameter("@Name", name),
                new SqlParameter("@Age", age),
                new SqlParameter("@Gender", gender),
                new SqlParameter("@ContactNo", contactNo),
                new SqlParameter("@Type", type),
                new SqlParameter("@DateOfVisit", dateOfVisit)
            };

                    try
                    {
                        // Perform the database update asynchronously
                        await Task.Run(() => ExecuteNonQuery("proc_UpdatePatient", parameters));

                        MessageBox.Show("Patient record updated successfully.");

                        // Refresh the grid asynchronously
                        Dispatcher.Invoke(() =>
                        {
                            FetchPatients();
                            PatientsDataGrid.Items.Refresh(); // Explicitly refresh the DataGrid
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating patient record: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a patient to update.");
                }
            }
        }



        private void ExecuteNonQuery(string procedureName, SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection("Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;"))
            {
                using (SqlCommand command = new SqlCommand(procedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void PatientsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                PatientsDataGrid.CommitEdit();
            }
        }


        private void RefreshButton_Click(object sender, EventArgs e)
        {
            FetchPatients();
        }

        // Event handler for sorting the data based on selected option
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string sortBy = selectedItem.Content.ToString();
                string storedProcedure = "Proc_GetAllPatients";
                SqlParameter[] parameters;

                // Add sorting logic based on selection
                if (sortBy == "Alphabetical")
                {
                    parameters = new[] { new SqlParameter("@SortBy", "name") };
                }
                else if (sortBy == "Date")
                {
                    parameters = new[] { new SqlParameter("@SortBy", "dateofVisit") };
                }
                else
                {
                    parameters = null; // No specific sorting
                }

                // Fetch sorted data
                FetchPatients(storedProcedure, parameters);
            }
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the current window (PatientHistoryWindow)
            this.Close();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();  // Open the Main Window (login screen)
        }

    }
}
