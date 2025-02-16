using System;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfDashboardApp
{
    public partial class AddPatientWindow : Window
    {
        // Connection string from App.config
        private string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;"
;

        // Flag to track if a patient is added successfully
        private bool isPatientAdded = false;

        // Variables to store the patient details
        private int patientId;
        private string patientName;

        public AddPatientWindow()
        {
            InitializeComponent();

            // Auto-fill the Date of Visit TextBox with today's date
            PatientDateOfVisitTextBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        // Event handler for Save Patient button click
        private async void SavePatientButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate if all required fields are filled
                if (string.IsNullOrWhiteSpace(PatientNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PatientAgeTextBox.Text) ||
                    PatientGenderComboBox.SelectedItem == null || // Gender is selected from ComboBox now
                    string.IsNullOrWhiteSpace(PatientContactTextBox.Text) ||
                    PatientTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parse values from input fields
                patientName = PatientNameTextBox.Text;
                int age = int.Parse(PatientAgeTextBox.Text);
                string gender = (PatientGenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(); // Get gender from ComboBox
                string contact = PatientContactTextBox.Text;
                string type = (PatientTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

             
                DateTime dateOfVisit = DateTime.Parse(PatientDateOfVisitTextBox.Text);

                // SQL query to insert patient data into the database and retrieve the generated patient_id
                string query = @"
            INSERT INTO tbl_patient (name, age, gender, contact_no, type, dateofVisit)
            VALUES (@Name, @Age, @Gender, @ContactNo, @Type, @DateofVisit);
            SELECT SCOPE_IDENTITY();"; // Retrieve the generated patient_id

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", patientName);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Gender", gender);
                    command.Parameters.AddWithValue("@ContactNo", contact);
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@DateofVisit", dateOfVisit); // Add the dateOfVisit parameter

                    // Execute the query and retrieve the generated patient_id
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        patientId = Convert.ToInt32(result); // Set the patientId to the generated value
                        isPatientAdded = true; // Set flag to true if patient is successfully added
                        MessageBox.Show("Patient added successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Failed to add patient.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        // Event handler for Cancel button click
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the window
        }

       
        private void GenerateBillButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the patient was added before generating the bill
            if (!isPatientAdded)
            {
                MessageBox.Show("Please add a patient before generating the bill.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Fixed doctor checkup fee
            int doctorFee = 1000;

            // Get today's date for the receipt
            string dateOfVisit = DateTime.Today.ToString("yyyy-MM-dd");

           
            string receipt = "--------------------------------\n" +
                             "         IQBAL CLINIC           \n" +
                             "--------------------------------\n" +
                             "Patient ID: " + patientId + "               \n" +
                             "Patient Name: " + patientName + "         \n" +
                             "Date of Visit: " + dateOfVisit + "\n" +
                             "--------------------------------\n" +
                             "Consultation Fees " + doctorFee + " Rupees\n" +
                             "--------------------------------\n" +
                             "TOTAL: " + doctorFee + " Rupees\n" +
                             "--------------------------------\n" +
                             "Thank you for visiting!        \n" +
                             "--------------------------------";

            // Show the formatted receipt in the MessageBox
            MessageBox.Show(receipt, "Bill Generated", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

}
