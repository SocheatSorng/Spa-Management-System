# Spa Management System - Design Pattern Migration

This project demonstrates the migration from Repository design pattern to various Gang of Four (GoF) design patterns for improved application design.

## GoF Design Patterns Implemented

### Command Pattern
The Command pattern encapsulates a request as an object, which lets you parameterize clients with different requests, queue or log requests, and support undoable operations.

**Implementation:**
- `UserFormCommand.cs` - Contains base command classes and interfaces
- `OrderCommands.cs` - Concrete command implementations for Order form
- Used in forms to handle UI actions (Insert, Update, Delete, New, Clear)

**Benefits:**
- Separates UI logic from business logic
- Makes operations testable
- Supports undo/redo (if needed)
- Centralizes validation logic

### State Pattern
The State pattern allows an object to alter its behavior when its internal state changes, appearing as if the object changed its class.

**Implementation:**
- `UserFormState.cs` - Contains state interface and concrete state implementations
- Used to manage form states (View mode, Edit mode)

**Benefits:**
- Clearly defines valid state transitions
- Encapsulates state-specific behavior
- Makes the UI more predictable

### Other GoF Patterns

**Template Method Pattern (`OrderTemplateMethod.cs`):**
- Defines the skeleton of an algorithm in a method, deferring some steps to subclasses
- Used for standardizing data access operations

**Strategy Pattern (`InvoiceStrategy.cs`):**
- Defines a family of algorithms, encapsulates each one, and makes them interchangeable
- Used for implementing different invoice data access strategies

**Proxy Pattern (`CardProxy.cs`):**
- Provides a surrogate or placeholder for another object to control access to it
- Used for adding validation, logging, and access control to card operations

## How to Implement in Your Form

To implement these patterns in a form:

1. **Create command classes** for each form operation (Insert, Update, Delete)
2. **Initialize form state** management in the form constructor
3. **Wire up commands** to button click event handlers
4. **Implement validation** in the commands rather than in the form

## Example Code

```csharp
// Button click implementation using Command pattern
private void BtnInsert_Click(object sender, EventArgs e)
{
    _commandInvoker.SetCommand(new InsertOrderCommand(this, _orderManager, GetOrderFromForm));
    _commandInvoker.ExecuteCommand();
}

// State transition example
public override void HandleSave()
{
    _form.Tag = new ViewState(_form, _enableControls, _disableControls, _clearControls);
    ((IFormState)_form.Tag).Enter();
}
```

## Benefits of GoF Patterns vs Repository Pattern

1. **Separation of Concerns** - Each pattern focuses on a specific aspect
2. **Flexibility** - Easier to extend and modify
3. **Testability** - Components can be tested in isolation
4. **Maintainability** - Clearer code organization
5. **Reusability** - Patterns can be reused across the application 