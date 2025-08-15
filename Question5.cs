using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public interface IInventoryEntity
{
    int Id { get; }
}

public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

public class InventoryLogger<T> where T : record, IInventoryEntity
{
    private readonly List<T> _log = new();

    private readonly string _filePath;

    public InventoryLogger(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public void Add(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _log.Add(item);
    }

    public List<T> GetAll()
    {
        return new List<T>(_log);
    }

    public void SaveToFile()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(_log, options);

            using var writer = new StreamWriter(_filePath, false);
            writer.Write(json);
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error saving log to file: {ex.Message}");
        }
    }

    public void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Log file not found, starting with empty log.");
                _log.Clear();
                return;
            }

            using var reader = new StreamReader(_filePath);
            string json = reader.ReadToEnd();

            var items = JsonSerializer.Deserialize<List<T>>(json);
            if (items != null)
            {
                _log.Clear();
                _log.AddRange(items);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading log from file: {ex.Message}");
        }
    }
}

public class InventoryApp
{
    private readonly InventoryLogger<InventoryItem> _logger;

    public InventoryApp(string dataFilePath)
    {
        _logger = new InventoryLogger<InventoryItem>(dataFilePath);
    }

    public void SeedSampleData()
    {
        _logger.Add(new InventoryItem(1, "Laptop", 10, DateTime.Now.AddDays(-30)));
        _logger.Add(new InventoryItem(2, "Desk Chair", 25, DateTime.Now.AddDays(-15)));
        _logger.Add(new InventoryItem(3, "Headphones", 50, DateTime.Now.AddDays(-45)));
        _logger.Add(new InventoryItem(4, "Keyboard", 40, DateTime.Now.AddDays(-10)));
        _logger.Add(new InventoryItem(5, "Monitor", 20, DateTime.Now.AddDays(-5)));
    }

    public void SaveData()
    {
        _logger.SaveToFile();
    }

    public void LoadData()
    {
        _logger.LoadFromFile();
    }

    public void PrintAllItems()
    {
        var items = _logger.GetAll();
        if (items.Count == 0)
        {
            Console.WriteLine("No inventory items to display.");
            return;
        }

        Console.WriteLine("Inventory Items:");
        foreach (var item in items)
        {
            Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Quantity: {item.Quantity}, Date Added: {item.DateAdded:d}");
        }
    }
}

public class Program
{
    public static void Main()
    {
        const string dataFilePath = "inventory_data.json";

        var app = new InventoryApp(dataFilePath);
        Console.WriteLine("Seeding sample data...");
        app.SeedSampleData();

        Console.WriteLine("Saving data to file...");
        app.SaveData();

        Console.WriteLine("\nSimulating new session...");
        var newApp = new InventoryApp(dataFilePath);

        Console.WriteLine("Loading data from file...");
        newApp.LoadData();

        Console.WriteLine("Printing loaded inventory items:");
        newApp.PrintAllItems();
    }
}
