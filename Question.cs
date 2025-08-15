using System;
using System.Collections.Generic;

public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

public interface ITransactionProcessor
{
    void Process(Transaction transaction);
}

public class BankTransferProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[BankTransfer] Processing transaction #{transaction.Id}: Amount = {transaction.Amount:C}, Category = {transaction.Category}");
    }
}

public class MobileMoneyProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[MobileMoney] Processing transaction #{transaction.Id}: Amount = {transaction.Amount:C}, Category = {transaction.Category}");
    }
}

public class CryptoWalletProcessor : ITransactionProcessor
{
    public void Process(Transaction transaction)
    {
        Console.WriteLine($"[CryptoWallet] Processing transaction #{transaction.Id}: Amount = {transaction.Amount:C}, Category = {transaction.Category}");
    }
}

public class Account
{
    public string AccountNumber { get; }
    public decimal Balance { get; protected set; }

    public Account(string accountNumber, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(accountNumber)) 
            throw new ArgumentException("AccountNumber cannot be null or empty.", nameof(accountNumber));
        if (initialBalance < 0) 
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));

        AccountNumber = accountNumber;
        Balance = initialBalance;
    }

    public virtual void ApplyTransaction(Transaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction.Amount < 0) throw new ArgumentException("Transaction amount cannot be negative.", nameof(transaction.Amount));

        Balance -= transaction.Amount;
        Console.WriteLine($"Account {AccountNumber}: Transaction applied. New balance: {Balance:C}");
    }
}

public sealed class SavingsAccount : Account
{
    public SavingsAccount(string accountNumber, decimal initialBalance) 
        : base(accountNumber, initialBalance)
    {
    }

    public override void ApplyTransaction(Transaction transaction)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        if (transaction.Amount < 0) throw new ArgumentException("Transaction amount cannot be negative.", nameof(transaction.Amount));

        if (transaction.Amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
        }
        else
        {
            Balance -= transaction.Amount;
            Console.WriteLine($"Account {AccountNumber}: Transaction applied. New balance: {Balance:C}");
        }
    }
}

public class FinanceApp
{
    private readonly List<Transaction> _transactions = new();

    public void Run()
    {
        var savingsAccount = new SavingsAccount("SA-12345", 1000m);

        var t1 = new Transaction(1, DateTime.Now, 150m, "Groceries");
        var t2 = new Transaction(2, DateTime.Now, 300m, "Utilities");
        var t3 = new Transaction(3, DateTime.Now, 700m, "Entertainment");

        ITransactionProcessor processor1 = new MobileMoneyProcessor();
        ITransactionProcessor processor2 = new BankTransferProcessor();
        ITransactionProcessor processor3 = new CryptoWalletProcessor();

        processor1.Process(t1);
        processor2.Process(t2);
        processor3.Process(t3);

        savingsAccount.ApplyTransaction(t1);
        savingsAccount.ApplyTransaction(t2);
        savingsAccount.ApplyTransaction(t3);

        _transactions.AddRange(new[] { t1, t2, t3 });
    }
}

public class Program
{
    public static void Main()
    {
        var app = new FinanceApp();
        app.Run();
    }
}
