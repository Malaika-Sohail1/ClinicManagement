using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfDashboardApp;

namespace WpfDashboardApp
{
    public partial class FinanceWindow : Window
    {
        private readonly List<MedicineEntry> Medicines = new List<MedicineEntry>();
        private readonly List<KeyValuePair<int, string>> MedicinesList = new List<KeyValuePair<int, string>>();

        string _connectionString = "Server=DESKTOP-52UOL6L\\SQLEXPRESS;Database=db_clinic;Integrated Security=True;TrustServerCertificate=True;";

        public FinanceWindow()
        {
            InitializeComponent();
            MedicinesDataGrid.ItemsSource = Medicines;
            LoadMedicineNames();
        }

        private void LoadMedicineNames()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetMedicineNames", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            MedicinesList.Clear();
                            while (reader.Read())
                            {
                                MedicinesList.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                            }

                            MedicineNameComboBox.ItemsSource = MedicinesList;
                            MedicineNameComboBox.DisplayMemberPath = "Value";
                            MedicineNameComboBox.SelectedValuePath = "Key";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading medicine names: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBillButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientIDTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Patient ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Medicines.Count == 0)
            {
                MessageBox.Show("Please add at least one medicine to generate a bill.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string billStatus = BillStatusComboBox.SelectedItem == null ? "Unpaid" : ((ComboBoxItem)BillStatusComboBox.SelectedItem).Content.ToString();

            BillWindow billWindow = new BillWindow(PatientIDTextBox.Text.Trim(), Medicines, billStatus);
            billWindow.ShowDialog();

        }

        private void ShowFinanceTableButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable financeTable = new DataTable();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetFinanceRecords", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(financeTable);
                        }
                    }
                }

                FinanceDataGrid.ItemsSource = financeTable.DefaultView;
                FinanceDataGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching finance data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MedicineNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MedicineNameComboBox.SelectedValue != null)
            {
                MedicineIDTextBox.Text = MedicineNameComboBox.SelectedValue.ToString();
            }

            MedicineNameComboBox.ItemsSource = MedicinesList;
        }

        private void MedicineNameComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (MedicineNameComboBox.Text is not { } searchText || string.IsNullOrWhiteSpace(searchText)) return;

            var filteredList = MedicinesList
                .Where(m => m.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            MedicineNameComboBox.ItemsSource = filteredList;
            MedicineNameComboBox.IsDropDownOpen = true;
        }

        private void MedicineNameComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            MedicineNameComboBox.ItemsSource = MedicinesList;
        }

        private void AddMedicineButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PatientIDTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Patient ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DoesPatientExist(PatientIDTextBox.Text.Trim()))
            {
                MessageBox.Show("Patient ID does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MedicineNameComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(QuantityTextBox.Text) ||
                string.IsNullOrWhiteSpace(DurationTextBox.Text))
            {
                MessageBox.Show("Please fill all fields before adding a medicine.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                MessageBox.Show("Invalid quantity. Please enter a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!UpdateMedicineStock(MedicineIDTextBox.Text.Trim(), quantity))
            {
                return;
            }

            Medicines.Add(new MedicineEntry
            {
                MedicineID = MedicineIDTextBox.Text,
                MedicineName = MedicineNameComboBox.SelectedItem.ToString(),  
                Quantity = quantity,
                Duration = DurationTextBox.Text,
                Instructions = InstructionsTextBox.Text
            });


            MedicinesDataGrid.Items.Refresh();
            ClearMedicineFields(null, null);
        }

        private bool UpdateMedicineStock(string medicineId, int quantityPurchased)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateMedicineStock", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@medicine_id", int.Parse(medicineId));
                        cmd.Parameters.AddWithValue("@quantity", quantityPurchased);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Medicine stock updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating medicine stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ClearMedicineFields(object sender, RoutedEventArgs e)
        {
            MedicineNameComboBox.SelectedIndex = -1;
            MedicineIDTextBox.Clear();
            QuantityTextBox.Clear();
            DurationTextBox.Clear();
            InstructionsTextBox.Clear();
        }

        private bool DoesPatientExist(string patientId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_CheckPatientExists", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@patient_id", int.Parse(patientId));
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking patient ID: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void GetDailySales(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetDailySales", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var result = cmd.ExecuteScalar();
                        decimal dailySales = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                        MessageBox.Show($"Daily Sales: {dailySales:C}", "Daily Finance", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching daily sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void GetMonthlySales(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_GetMonthlySales", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        var result = cmd.ExecuteScalar();
                        decimal monthlySales = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                        MessageBox.Show($"Monthly Sales: {monthlySales:C}", "Monthly Finance", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching monthly sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void DisplayYearlyFinance(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal totalSales = 0;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Fetch Yearly Sales
                    using (SqlCommand salesCmd = new SqlCommand("sp_GetYearlySales", conn))
                    {
                        salesCmd.CommandType = CommandType.StoredProcedure;
                        var result = salesCmd.ExecuteScalar();
                        totalSales = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }
                }

                // Display Yearly Sales
                MessageBox.Show(
                    $"Yearly Sales: {totalSales:C}",
                    "Yearly Finance",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching yearly sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
    public class MedicineEntry
    {
        public string MedicineID { get; set; }
        public string MedicineName { get; set; }  // Add this property
        public int Quantity { get; set; }
        public string Duration { get; set; }
        public string Instructions { get; set; }
    }

}