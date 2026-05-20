using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClientManager.Models;
using ClientManager.Services;

namespace ClientManager
{
    public partial class MainWindow : Window
    {
        private ClientService service;
        private Client selectedClient;

        public MainWindow()
        {
            InitializeComponent();

            // Создаём сервис сразу в конструкторе
            try
            {
                service = new ClientService();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания сервиса: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                service = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Проверяем, что сервис создан
            if (service == null)
            {
                MessageBox.Show("Сервис не создан. Приложение будет закрыто.", "Ошибка");
                this.Close();
                return;
            }

            // Часы
            try
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += (s, args) => ClockText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                timer.Start();
            }
            catch { }

            LoadCategories();
            RefreshGrid();
            ClearForm();
        }

        private void LoadCategories()
        {
            if (service == null) return;

            try
            {
                var cats = service.GetCategories();
                CategoryComboBox.Items.Clear();
                FilterCategory.Items.Clear();
                FilterCategory.Items.Add("Все");
                foreach (var c in cats)
                {
                    CategoryComboBox.Items.Add(c);
                    FilterCategory.Items.Add(c);
                }
                FilterCategory.SelectedIndex = 0;
                if (CategoryComboBox.Items.Count > 0)
                    CategoryComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void RefreshGrid()
        {
            if (service == null) return;

            try
            {
                var list = service.GetAll();
                ClientsDataGrid.ItemsSource = null;
                ClientsDataGrid.ItemsSource = list;
                TotalCountText.Text = "Клиентов: " + list.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления таблицы: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            FullNameTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            CategoryComboBox.Text = "Обычный";
            RegistrationDatePicker.SelectedDate = DateTime.Now;
            IsActiveCheckBox.IsChecked = true;
            NotesTextBox.Text = "";
            selectedClient = null;
        }

        private void ClientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client client)
            {
                selectedClient = client;
                FullNameTextBox.Text = client.FullName ?? "";
                PhoneTextBox.Text = client.Phone ?? "";
                EmailTextBox.Text = client.Email ?? "";
                CategoryComboBox.Text = client.Category ?? "Обычный";
                RegistrationDatePicker.SelectedDate = client.RegistrationDate;
                IsActiveCheckBox.IsChecked = client.IsActive;
                NotesTextBox.Text = client.Notes ?? "";
            }
        }

        private Client GetClientFromForm()
        {
            return new Client
            {
                FullName = FullNameTextBox.Text ?? "",
                Phone = PhoneTextBox.Text ?? "",
                Email = EmailTextBox.Text ?? "",
                Category = CategoryComboBox.Text ?? "Обычный",
                RegistrationDate = RegistrationDatePicker.SelectedDate ?? DateTime.Now,
                IsActive = IsActiveCheckBox.IsChecked ?? true,
                Notes = NotesTextBox.Text ?? ""
            };
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                service.Add(GetClientFromForm());
                LoadCategories();
                RefreshGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления: " + ex.Message);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента в таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                MessageBox.Show("Введите ФИО!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var client = GetClientFromForm();
                client.Id = selectedClient.Id;
                service.Update(client);
                LoadCategories();
                RefreshGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления: " + ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента в таблице!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить клиента " + selectedClient.FullName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    service.Delete(selectedClient.Id);
                    LoadCategories();
                    RefreshGrid();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ClientsDataGrid.SelectedItem = null;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshGrid();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            try
            {
                var results = service.Search(SearchTextBox.Text);
                ClientsDataGrid.ItemsSource = null;
                ClientsDataGrid.ItemsSource = results;
                TotalCountText.Text = "Найдено: " + results.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
        }

        private void ResetSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            FilterCategory.SelectedIndex = 0;
            FilterActive.SelectedIndex = 0;
            RefreshGrid();
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (service == null) return;

            try
            {
                string cat = null;
                if (FilterCategory.SelectedItem != null)
                {
                    cat = FilterCategory.SelectedItem.ToString();
                    if (cat == "Все") cat = null;
                }

                bool? isActive = null;
                if (FilterActive.SelectedIndex == 1) isActive = true;
                else if (FilterActive.SelectedIndex == 2) isActive = false;

                var results = service.Filter(cat, isActive);
                ClientsDataGrid.ItemsSource = null;
                ClientsDataGrid.ItemsSource = results;
                TotalCountText.Text = "Отфильтровано: " + results.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (service == null) return;
            if (SortComboBox.SelectedItem is ComboBoxItem item)
            {
                try
                {
                    var sorted = service.Sort(item.Content.ToString());
                    ClientsDataGrid.ItemsSource = null;
                    ClientsDataGrid.ItemsSource = sorted;
                }
                catch { }
            }
        }
    }
}