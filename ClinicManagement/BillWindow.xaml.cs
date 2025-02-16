using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using WpfDashboardApp;

namespace WpfDashboardApp
{
    public partial class BillWindow : Window
    {
        string _connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

        private readonly string _patientId;
        private double _totalBill;  // Total bill will now be a variable that can be updated
        private readonly string _billStatus;

        public BillWindow(string patientId, List<MedicineEntry> medicines, string billStatus)
        {
            InitializeComponent();

            _patientId = patientId;
            _billStatus = billStatus;

            // Set the patient ID
            PatientIDTextBlock.Text = $"Patient ID: {patientId}";

            // Set the current date
            DateTextBlock.Text = $"Date: {DateTime.Now:MMMM dd, yyyy}";

            // Set the medicines list in DataGrid with simplified details
            var medicinesWithTotal = medicines.Select(m => new
            {
                m.MedicineName,  // Add this line
                m.Quantity,
                PricePerUnit = 10,  // Assuming 10 per dosage unit
                TotalPrice = m.Quantity * 10  // Total price = Quantity * Price per unit
            }).ToList();

            // Display medicines with total price in DataGrid
            MedicinesDataGrid.ItemsSource = medicinesWithTotal;

            // Calculate the total bill (without adding 1000 yet)
            _totalBill = medicinesWithTotal.Sum(m => m.TotalPrice);

            // Display the initial total bill (before adding 1000 rupees)
            TotalTextBlock.Text = $"₹{_totalBill:F2}";  // Use ₹ symbol for rupees

            // Display the bill status (Paid/Unpaid)
            BillStatusTextBlock.Text = billStatus;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Add 1000 rupees to the total bill just before saving to the database
                _totalBill += 1000;

                // Get selected payment method
                var selectedMethod = (PaymentMethodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Unknown";

                // Insert finance details into database
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO tbl_finance (patient_id, MedicineCost, PaymentMethod, PaymentStatus, PaymentDate) 
                        VALUES (@patientId, @medicineCost, @paymentMethod, @paymentStatus, @paymentDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@patientId", _patientId);
                        cmd.Parameters.AddWithValue("@medicineCost", _totalBill);  // Insert the updated total bill with 1000 added
                        cmd.Parameters.AddWithValue("@paymentMethod", selectedMethod);
                        cmd.Parameters.AddWithValue("@paymentStatus", BillStatusTextBlock.Text);
                        cmd.Parameters.AddWithValue("@paymentDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Finance details successfully inserted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving finance details:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Close the current window
                this.Close();
            }
        }

        // Method to handle the change payment status
        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the payment status between Paid and Unpaid
            if (BillStatusTextBlock.Text == "Unpaid")
            {
                BillStatusTextBlock.Text = "Paid";
                ChangeStatusButton.Content = "Mark as Unpaid";
            }
            else
            {
                BillStatusTextBlock.Text = "Unpaid";
                ChangeStatusButton.Content = "Mark as Paid";
            }
        }
    }
}