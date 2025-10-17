using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;

// REFACTOR: Renamed the outer class to better reflect the project.
public class VendingMachineProgram
{
    // The Product classes are perfect, no changes needed here.
    public abstract class Product
    {
        public string Name { get; }
        public decimal Price { get; }
        public int QuantityAvailable { get; private set; }

        public Product(string name, decimal price, int quantityAvailable)
        {
            Name = name;
            Price = price;
            QuantityAvailable = quantityAvailable;
        }

        public bool DispenseOne()
        {
            if (QuantityAvailable > 0)
            {
                QuantityAvailable--;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return $"Product: {Name,-10} | Price: {Price:C} | In Stock: {QuantityAvailable}";
        }

        public abstract string GetUsageInstructions();
    }

    public class Snack : Product
    {
        // C# 12 Primary Constructor syntax for conciseness (optional but modern)
        public Snack(string name, decimal price, int quantityAvailable)
            : base(name, price, quantityAvailable) { }
        public override string GetUsageInstructions() => "Just open the wrapper and enjoy!";
    }

    public class Drink : Product
    {
        public Drink(string name, decimal price, int quantityAvailable)
            : base(name, price, quantityAvailable) { }
        public override string GetUsageInstructions() => "Open the cap and sip carefully.";
    }

    public class Food : Product
    {
        public Food(string name, decimal price, int quantityAvailable)
            : base(name, price, quantityAvailable) { }
        public override string GetUsageInstructions() => "Please heat in a microwave for 2 minutes.";
    }

    // REFACTOR: A new class to encapsulate the result of a purchase.
    public class PurchaseResult
    {
        public bool WasSuccessful { get; }
        public string Message { get; }
        public Product? PurchasedProduct { get; }

        public PurchaseResult(bool success, string message, Product? product = null)
        {
            WasSuccessful = success;
            Message = message;
            PurchasedProduct = product;
        }
    }

    public interface IPaymentProcessor
    {
        public void InsertMoney(decimal amount);
        public decimal GetCurrentAmount();
        public bool TryProcessPayment(decimal price);
        public decimal ReturnMoney();
    }

    public class CashProcessor : IPaymentProcessor
    {
        private decimal _currentAmountInserted;
        public void InsertMoney(decimal amount)
        {
            if (amount > 0)
            {
                _currentAmountInserted += amount;
            }
        }
        public decimal GetCurrentAmount() => _currentAmountInserted;
        public bool TryProcessPayment(decimal price)
        {
            if (price <= _currentAmountInserted)
            {
                _currentAmountInserted -= price;
                return true;
            }
            return false;
        }
        public decimal ReturnMoney()
        {
            var change = _currentAmountInserted;
            _currentAmountInserted = 0;
            return change;
        }
    }

    public class VendingMachine
    {
        // REFACTOR: Make the inventory accessible for display purposes, but only readable.
        public IReadOnlyList<Product> Inventory => _inventory;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly List<Product> _inventory;
        private decimal _totalMoneyInMachine;

        public VendingMachine(IPaymentProcessor paymentProcessor)
        {
            _inventory = new List<Product>
            {
                new Drink("Soda", 1.50m, 10),
                new Snack("Chips", 1.00m, 5),
                new Snack("Candy", 0.75m, 20),
                new Food("Sandwich", 3.50m, 7)
            };
            _paymentProcessor = paymentProcessor;
        }

        // Let the UI ask for the current balance instead of printing it.
        public decimal GetCurrentBalance() => _paymentProcessor.GetCurrentAmount();

        public void InsertMoney(decimal amount) =>
            _paymentProcessor.InsertMoney(amount);

        // REFACTOR: This method now returns a rich result object and does NO printing.
        public PurchaseResult Purchase(string productName)
        {
            var product = _inventory.FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

            if (product == null)
                return new PurchaseResult(false, $"Error: Product '{productName}' not found.");

            if (product.QuantityAvailable <= 0)
                return new PurchaseResult(false, $"Error: Product '{productName}' is out of stock.");

            if (!_paymentProcessor.TryProcessPayment(product.Price))
                return new PurchaseResult(false, $"Error: Insufficient funds for '{productName}'.");

            _totalMoneyInMachine += product.Price;
            product.DispenseOne();

            // Return a successful result with the product object included.
            return new PurchaseResult(true, $"Thank you for purchasing '{productName}'.", product);
        }

        public decimal ReturnMoney() =>
            _paymentProcessor.ReturnMoney();
    }

    class ConsoleUI
    {
        enum enChoice
        {
            InsertMoney = 1,
            Purchase,
            Exit
        }
        private enChoice _userChoice { get; set; }
        private VendingMachine _vendingMachine { get; }

        public ConsoleUI(VendingMachine vendingMachine)
        {
            _vendingMachine = vendingMachine;
        }

        void ErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
            Pause();
        }

        void Pause()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nPress any key to continue..");
            Console.ReadKey();
            Console.ResetColor();
        }

        void InsertMoney()
        {
            DrawHeader("Insert Money");
            string? input = Prompt("Enter amount to insert (e.g., 1.00)");
            if (string.IsNullOrEmpty(input))
            {
                ErrorMessage("Amount cannot be empty.");
                return;
            }
            decimal price;
            if (!decimal.TryParse(input, out price))
            {
                ErrorMessage("Cannot parse amount. Please enter a valid decimal number.");
                return;
            }
            if (price <= 0)
            {
                ErrorMessage("Amount cannot be negative or zero.");
                return;
            }
            _vendingMachine.InsertMoney(price);
        }

        void PrintPurchaseResult(PurchaseResult result)
        {
            if (!result.WasSuccessful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n{result.Message}");
                Console.ResetColor();
                Pause();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{result.Message}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (result.PurchasedProduct is not null)
                Console.WriteLine($"\n{result.PurchasedProduct.GetUsageInstructions()}");
            Console.ResetColor();
            Pause();
        }

        void Purchase()
        {
            DrawHeader("Purchase Product");
            ShowProducts();
            DisplayMessage($"Your balance: {_vendingMachine.GetCurrentBalance():C}", ConsoleColor.DarkGreen);
            string? productName = Prompt("Insert product name");
            if (string.IsNullOrEmpty(productName))
            {
                ErrorMessage("Product name cannot be empty.");
                return;
            }
            PurchaseResult result = _vendingMachine.Purchase(productName);
            PrintPurchaseResult(result);
        }

        void OptionSelect()
        {
            switch (_userChoice)
            {
                case enChoice.InsertMoney:
                    InsertMoney();
                    break;
                case enChoice.Purchase:
                    Purchase();
                    break;
                case enChoice.Exit:
                    break;
                default:
                    break;
            }
        }

        void DrawHeader(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("==================================================");
            Console.WriteLine($"      {title}");
            Console.WriteLine("==================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        void ShowProducts()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--------------- Available Products ---------------");
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var product in _vendingMachine.Inventory)
            {
                Console.WriteLine(product.ToString());
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--------------------------------------------------");
            Console.ResetColor();
            Console.WriteLine();
        }

        void ShowOptions()
        {
            Console.WriteLine("Choose an option:\n");
            Console.WriteLine("  1. Insert Money");
            Console.WriteLine("  2. Purchase Product");
            Console.WriteLine("  3. Exit\n");
        }

        void DisplayMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{message}");
            Console.ResetColor();
            Console.WriteLine();
        }

        void ShowMainMenu()
        {
            DrawHeader("Vending Machine");
            ShowProducts();
            DisplayMessage($"Your balance: {_vendingMachine.GetCurrentBalance():C}", ConsoleColor.DarkGreen);
            ShowOptions();
        }

        void SetUserOption()
        {
            enChoice userChoice;
            string promptMessage = "Select Option";
            while (!Enum.TryParse(Prompt(promptMessage), true, out userChoice) ||
                (userChoice > enChoice.Exit || userChoice < enChoice.InsertMoney))
            {
                promptMessage = "Invalid choice. Please try again";
            }
            _userChoice = userChoice;
        }

        string? Prompt(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write($"{message}: ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim();
        }

        public void RUN()
        {
            do
            {
                ShowMainMenu();
                SetUserOption();
                OptionSelect();
            }
            while (_userChoice is not enChoice.Exit);
        }
    }

    static void Main()
    {
        CashProcessor cashProcessor = new();
        VendingMachine vendingMachine = new(cashProcessor);
        ConsoleUI consoleUI = new(vendingMachine);
        consoleUI.RUN();
    }
}