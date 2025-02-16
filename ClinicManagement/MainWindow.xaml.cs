using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace WpfDashboardApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

        private bool isPatientAdded = false; 

        public MainWindow()
        {
            InitializeComponent();
        }

        // Event handler for Login button click
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Simple login validation for demonstration purposes
            if (username == "a" && password == "1")
            {
                // Show the Dashboard and hide the Login
                LoginGrid.Visibility = Visibility.Collapsed;
                DashboardView.Visibility = Visibility.Visible;
            }
            else
            {
                // Show error message if login fails
                MessageBox.Show("Invalid Username or Password. Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler for Log Out button click
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the Login view and hide the Dashboard
            DashboardView.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;

            // Clear the login fields for the next login attempt
            UsernameTextBox.Clear();
            PasswordBox.Clear();
        }

        private void ShowAddPatientForm_Click(object sender, RoutedEventArgs e)
        {
            AddPatientWindow addPatientWindow = new AddPatientWindow();
            addPatientWindow.ShowDialog(); // Opens the window as a modal dialog
        }

        private void ShowHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            PatientHistoryWindow patientHistoryWindow = new PatientHistoryWindow();
            patientHistoryWindow.Show();  // Open the new window with patient data
        }
        private void StaffButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the StaffWindow
                StaffWindow staffWindow = new StaffWindow();
                staffWindow.ShowDialog();  // Show the window as a modal dialog
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Staff Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MedicinesButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the MedicineWindow and show it
            MedicineWindow medicineWindow = new MedicineWindow();
            medicineWindow.ShowDialog(); // This will open the Medicine window as a modal
        }
        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of the supplierWindow and show it
            SupplierWindow supplierWindow = new SupplierWindow();
            supplierWindow.ShowDialog(); // This will open the Medicine window as a modal
        }
        private void FinanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the FinanceWindow
                FinanceWindow financeWindow = new FinanceWindow();
                financeWindow.ShowDialog(); // Show the Finance window as a modal dialog
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Finance Window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}