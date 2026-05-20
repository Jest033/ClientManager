using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ClientManager.Models;
// Service with full CRUD operations, search, filter and sort functionality
namespace ClientManager.Services
{
    public class ClientService
    {
        private string dataPath;
        private List<Client> clients;

        public ClientService()
        {
            dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clients.json");
            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(dataPath))
            {
                string json = File.ReadAllText(dataPath);
                clients = JsonConvert.DeserializeObject<List<Client>>(json);
                if (clients == null) clients = new List<Client>();
            }
            else
            {
                clients = new List<Client>();
                AddSampleData();
            }
        }

        private void SaveData()
        {
            string json = JsonConvert.SerializeObject(clients, Formatting.Indented);
            File.WriteAllText(dataPath, json);
        }

        private void AddSampleData()
        {
            clients.Add(new Client { Id = 1, FullName = "Иванов Иван", Phone = "+79001112233", Email = "ivanov@mail.ru", Category = "VIP", RegistrationDate = DateTime.Now.AddDays(-30), IsActive = true, Notes = "Крупный заказчик" });
            clients.Add(new Client { Id = 2, FullName = "Петрова Анна", Phone = "+79002223344", Email = "petrova@mail.ru", Category = "Обычный", RegistrationDate = DateTime.Now.AddDays(-20), IsActive = true, Notes = "" });
            clients.Add(new Client { Id = 3, FullName = "Сидоров Павел", Phone = "+79003334455", Email = "sidorov@mail.ru", Category = "Постоянный", RegistrationDate = DateTime.Now.AddDays(-15), IsActive = false, Notes = "Неактивен" });
            clients.Add(new Client { Id = 4, FullName = "Козлова Елена", Phone = "+79004445566", Email = "kozlova@mail.ru", Category = "VIP", RegistrationDate = DateTime.Now.AddDays(-10), IsActive = true, Notes = "" });
            clients.Add(new Client { Id = 5, FullName = "Морозов Дмитрий", Phone = "+79005556677", Email = "morozov@mail.ru", Category = "Обычный", RegistrationDate = DateTime.Now.AddDays(-5), IsActive = true, Notes = "Новый" });
            SaveData();
        }

        public List<Client> GetAll()
        {
            return clients.OrderBy(c => c.Id).ToList();
        }

        public void Add(Client client)
        {
            if (clients.Count > 0)
                client.Id = clients.Max(c => c.Id) + 1;
            else
                client.Id = 1;
            clients.Add(client);
            SaveData();
        }

        public void Update(Client client)
        {
            var existing = clients.FirstOrDefault(c => c.Id == client.Id);
            if (existing != null)
            {
                existing.FullName = client.FullName;
                existing.Phone = client.Phone;
                existing.Email = client.Email;
                existing.Category = client.Category;
                existing.RegistrationDate = client.RegistrationDate;
                existing.IsActive = client.IsActive;
                existing.Notes = client.Notes;
                SaveData();
            }
        }

        public void Delete(int id)
        {
            var client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
                SaveData();
            }
        }

        public List<Client> Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAll();

            searchText = searchText.ToLower();
            return clients.Where(c =>
                c.FullName.ToLower().Contains(searchText) ||
                c.Phone.ToLower().Contains(searchText) ||
                c.Email.ToLower().Contains(searchText) ||
                c.Notes.ToLower().Contains(searchText)
            ).OrderBy(c => c.Id).ToList();
        }

        public List<Client> Filter(string category, bool? isActive)
        {
            var query = clients.AsEnumerable();

            if (!string.IsNullOrEmpty(category) && category != "Все")
                query = query.Where(c => c.Category == category);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            return query.OrderBy(c => c.Id).ToList();
        }

        public List<Client> Sort(string sortBy)
        {
            if (sortBy == "Имя")
                return clients.OrderBy(c => c.FullName).ToList();
            else if (sortBy == "Дата")
                return clients.OrderByDescending(c => c.RegistrationDate).ToList();
            else if (sortBy == "Категория")
                return clients.OrderBy(c => c.Category).ToList();
            else
                return clients.OrderBy(c => c.Id).ToList();
        }

        public List<string> GetCategories()
        {
            return clients.Select(c => c.Category).Distinct().OrderBy(c => c).ToList();
        }
    }
}