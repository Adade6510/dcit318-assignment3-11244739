using System;
using System.Collections.Generic;

public interface IInventoryItem
{
    int Id { get; }
    string Name { get; }
    int Quantity { get; set; }
}


public class DuplicateItemException : Exception
{
    public DuplicateItemException(string message) : base(message) { }
}

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message) : base(message) { }
}

public class InvalidQuantityException : Exception
{
    public InvalidQuantityException(string message) : base(message) { }
}

public class ElectronicItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public string Brand { get; }
    public int WarrantyMonths { get; }

    public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
    {
        if (quantity < 0) throw new InvalidQuantityException("Quantity cannot be negative");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(brand)) throw new ArgumentException("Brand cannot be empty", nameof(brand));
        if (warrantyMonths < 0) throw new ArgumentException("Warranty months cannot be negative", nameof(warrantyMonths));

        Id = id;
        Name = name;
        Quantity = quantity;
        Brand = brand;
        WarrantyMonths = warrantyMonths;
    }

    public override string ToString()
    {
        return $"ElectronicItem [Id={Id}, Name={Name}, Quantity={Quantity}, Brand={Brand}, WarrantyMonths={WarrantyMonths}]";
    }
}

public class GroceryItem : IInventoryItem
{
    public int Id { get; }
    public string Name { get; }
    public int Quantity { get; set; }
    public DateTime ExpiryDate { get; }

    public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
    {
        if (quantity < 0) throw new InvalidQuantityException("Quantity cannot be negative");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (expiryDate < DateTime.Today) throw new ArgumentException("Expiry date cannot be in the past", nameof(expiryDate));

        Id = id;
        Name = name;
        Quantity = quantity;
        ExpiryDate = expiryDate;
    }

    public override string ToString()
    {
        return $"GroceryItem [Id={Id}, Name={Name}, Quantity={Quantity}, Expiry={ExpiryDate:yyyy-MM-dd}]";
    }
}

public class InventoryRepository<T> where T : IInventoryItem
{
    private readonly Dictionary<int, T> _items = new();

    public void AddItem(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (_items.ContainsKey(item.Id))
            throw new DuplicateItemException($"An item with Id {item.Id} already exists.");

        _items.Add(item.Id, item);
    }

    public T GetItemById(int id)
    {
        if (!_items.TryGetValue(id, out T item))
            throw new ItemNotFoundException($"Item with Id {id} not found.");

        return item;
    }

    public void RemoveItem(int id)
    {
        if (!_items.Remove(id))
            throw new ItemNotFoundException($"Cannot remove: Item with Id {id} not found.");
    }

    public List<T> GetAllItems()
    {
        return new List<T>(_items.Values);
    }

    public void UpdateQuantity(int id, int newQuantity)
    {
        if (newQuantity < 0)
            throw new InvalidQuantityException("Quantity cannot be negative.");

        var item = GetItemById(id); 
        item.Quantity = newQuantity;
    }
}

public class WareHouseManager
{
    private readonly InventoryRepository<ElectronicItem> _electronics = new();
    private readonly InventoryRepository<GroceryItem> _groceries = new();

    public void SeedData()
    {
        try
        {
            _electronics.AddItem(new ElectronicItem(1, "Smartphone", 50, "BrandX", 24));
            _electronics.AddItem(new ElectronicItem(2, "Laptop", 30, "BrandY", 12));
            _electronics.AddItem(new ElectronicItem(3, "Headphones", 100, "BrandZ", 6));

            _groceries.AddItem(new GroceryItem(101, "Milk", 200, DateTime.Today.AddDays(10)));
            _groceries.AddItem(new GroceryItem(102, "Bread", 150, DateTime.Today.AddDays(5)));
            _groceries.AddItem(new GroceryItem(103, "Eggs", 300, DateTime.Today.AddDays(20)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding data: {ex.Message}");
        }
    }

    public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
    {
        Console.WriteLine($"Inventory for {typeof(T).Name}:");
        foreach (var item in repo.GetAllItems())
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();
    }

    public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
    {
        if (quantity <= 0)
        {
            Console.WriteLine("Quantity to increase must be positive.");
            return;
        }

        try
        {
            var item = repo.GetItemById(id);
            int newQuantity = item.Quantity + quantity;
            repo.UpdateQuantity(id, newQuantity);
            Console.WriteLine($"Increased stock for item Id={id} by {quantity}. New quantity: {newQuantity}");
        }
        catch (DuplicateItemException dex)
        {
            Console.WriteLine($"DuplicateItemException: {dex.Message}");
        }
        catch (ItemNotFoundException infex)
        {
            Console.WriteLine($"ItemNotFoundException: {infex.Message}");
        }
        catch (InvalidQuantityException iqex)
        {
            Console.WriteLine($"InvalidQuantityException: {iqex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
    {
        try
        {
            repo.RemoveItem(id);
            Console.WriteLine($"Item with Id={id} removed successfully.");
        }
        catch (ItemNotFoundException infex)
        {
            Console.WriteLine($"ItemNotFoundException: {infex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    public InventoryRepository<ElectronicItem> Electronics => _electronics;
    public InventoryRepository<GroceryItem> Groceries => _groceries;
}

public class Program
{
    public static void Main()
    {
        var manager = new WareHouseManager();

        Console.WriteLine("Seeding inventory data...");
        manager.SeedData();

        Console.WriteLine("Printing all grocery items:");
        manager.PrintAllItems(manager.Groceries);

        Console.WriteLine("Printing all electronic items:");
        manager.PrintAllItems(manager.Electronics);

        Console.WriteLine("Trying to add a duplicate electronic item...");
        try
        {
            manager.Electronics.AddItem(new ElectronicItem(1, "Tablet", 10, "BrandA", 18));
        }
        catch (DuplicateItemException dex)
        {
            Console.WriteLine($"Caught DuplicateItemException: {dex.Message}");
        }

        Console.WriteLine("\nTrying to remove a non-existent grocery item...");
        manager.RemoveItemById(manager.Groceries, 999);  

        Console.WriteLine("\nTrying to update quantity to invalid value...");
        try
        {
            manager.Electronics.UpdateQuantity(2, -5); 
        }
        catch (InvalidQuantityException iqex)
        {
            Console.WriteLine($"Caught InvalidQuantityException: {iqex.Message}");
        }

        Console.WriteLine("\nIncreasing stock of existing electronic item (Id=2) by 15:");
        manager.IncreaseStock(manager.Electronics, 2, 15);

        Console.WriteLine("\nFinal electronic stock:");
        manager.PrintAllItems(manager.Electronics);
    }
}
