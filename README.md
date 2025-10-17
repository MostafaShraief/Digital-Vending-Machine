# 🥤 Digital Vending Machine (OOP Principles Review)

This project serves as a comprehensive, hands-on review and demonstration of core Object-Oriented Programming (OOP) principles in C#, focusing on architectural best practices like separation of concerns and dependency injection.

The application simulates the backend logic and a basic console interface for a simple vending machine that handles inventory, product dispensing, and cash payments.

---

## 🎯 Project Goal

The primary goal of this project was not just to create a working vending machine, but to use the domain as a framework for applying and enforcing OOP concepts:

1.  **Encapsulation:** Protecting the internal state (inventory, cash) of objects.
2.  **Polymorphism & Inheritance:** Defining product hierarchies and leveraging runtime behavior.
3.  **Abstraction & Interfaces:** Decoupling core business logic (Vending Machine) from implementation details (Payment Processor).
4.  **Single Responsibility Principle (SRP):** Ensuring each class has only one reason to change.
5.  **Composition:** Favoring "has-a" relationships over rigid inheritance.

---

## 🏛️ Project Architecture

The application is cleanly divided into two layers, demonstrating the **Separation of Concerns (SoC)** principle:

### 1. Domain/Business Logic Layer

These classes contain the core rules of the application and are completely agnostic of the user interface (they contain **no** `Console.WriteLine` calls). This layer is highly portable and easily testable.

*   **`VendingMachine`:** The core orchestrator. Manages inventory and delegates payment tasks to the `IPaymentProcessor`.
*   **`Product` (Abstract Base Class):** Defines the common properties of all vendible items.
*   **`Snack`, `Drink`, `Food`:** Concrete classes demonstrating polymorphism via the `GetUsageInstructions()` method.
*   **`IPaymentProcessor`:** An interface defining the contract for handling money (Abstraction).
*   **`CashProcessor`:** The concrete implementation of the cash handling logic.
*   **`PurchaseResult`:** A dedicated class used to return the outcome of a transaction, ensuring methods return rich data rather than simple strings.

### 2. Presentation Layer

*   **`ConsoleUI`:** Handles all input from and output to the user. It depends entirely on the `VendingMachine` and is responsible for calling its public methods and displaying the resulting data.

### 💉 Dependency Injection (DI)

The system uses **Constructor Injection** to wire the components together in the `Main` method (the Composition Root):

```csharp
// VendingMachine doesn't create its dependencies; they are passed in.
var paymentProcessor = new CashProcessor();
var vendingMachine = new VendingMachine(paymentProcessor);
var ui = new ConsoleUI(vendingMachine);
ui.Run();
```

This ensures that the `VendingMachine` is loosely coupled and can easily swap out the `CashProcessor` for a `CardProcessor` or a mock object for testing without any internal changes.

---

## ⚙️ How to Run the Project

This project is a single-file C# console application.

1.  **Save Code:** Save the final code block as a file named `VendingMachineProgram.cs`.
2.  **Compile & Run:** Use the .NET CLI:

    ```bash
    dotnet run VendingMachineProgram.cs
    ```

3.  **Interaction:** Follow the on-screen menu prompts to insert money, view products, and make purchases.