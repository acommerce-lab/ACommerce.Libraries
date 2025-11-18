# ACommerce.SharedKernel.CQRS

CQRS (Command Query Responsibility Segregation) implementation using MediatR.

## Overview

Complete CQRS implementation with base command/query classes, handlers, validators, and pipeline behaviors. Built on MediatR for clean separation of commands and queries.

## Key Features

🔥 **CQRS Pattern** - Separate commands and queries  
🔥 **MediatR Integration** - Industry-standard mediator  
✅ **Validation Pipeline** - FluentValidation integration  
✅ **Logging Pipeline** - Automatic request logging  
✅ **Base Classes** - Reusable command/query bases  
✅ **Result Pattern** - Consistent response handling  

## Core Components

### Commands

#### ICommand / ICommand<TResponse>
Marker interfaces for commands
```csharp
// Command without response
public class DeleteProductCommand : ICommand
{
    public Guid ProductId { get; set; }
}

// Command with response
public class CreateProductCommand : ICommand<Product>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

#### ICommandHandler<TCommand> / ICommandHandler<TCommand, TResponse>
Command handler interfaces
```csharp
public class CreateProductHandler : ICommandHandler<CreateProductCommand, Product>
{
    public async Task<Product> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price
        };
        
        await _repository.AddAsync(product, ct);
        return product;
    }
}
```

### Queries

#### IQuery<TResponse>
Query interface
```csharp
public class GetProductByIdQuery : IQuery<Product>
{
    public Guid ProductId { get; set; }
}

public class GetProductsQuery : IQuery<PagedResult<Product>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

#### IQueryHandler<TQuery, TResponse>
Query handler interface
```csharp
public class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, Product>
{
    private readonly IBaseAsyncRepository<Product> _repository;
    
    public async Task<Product?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(request.ProductId, ct);
    }
}
```

## Pipeline Behaviors

### ValidationBehavior
Automatic FluentValidation
```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Automatically executed before handler
```

### LoggingBehavior
Request/Response logging
```csharp
// Automatically logs:
// - Request details
// - Execution time
// - Response/Errors
// - User context
```

### PerformanceBehavior
Performance monitoring
```csharp
// Logs slow requests (> 500ms)
// Tracks execution metrics
```

## Base Classes

### BaseCommand / BaseCommand<TResponse>
```csharp
public abstract class BaseCommand : ICommand
{
    public string? UserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public abstract class BaseCommand<TResponse> : ICommand<TResponse>
{
    public string? UserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
```

### BaseQuery<TResponse>
```csharp
public abstract class BaseQuery<TResponse> : IQuery<TResponse>
{
    public string? UserId { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
```

## Usage

### Setup
```csharp
// In Program.cs
services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// Add pipeline behaviors
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

// Add validators
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

### In Controllers
```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(CreateProductCommand command)
    {
        var product = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(Guid id)
    {
        var query = new GetProductByIdQuery { ProductId = id };
        var product = await _mediator.Send(query);
        
        if (product == null)
            return NotFound();
            
        return Ok(product);
    }
}
```

### In Services
```csharp
public class ProductService
{
    private readonly IMediator _mediator;
    
    public async Task<Product> CreateProduct(string name, decimal price)
    {
        var command = new CreateProductCommand
        {
            Name = name,
            Price = price
        };
        
        return await _mediator.Send(command);
    }
}
```

## Advanced Features

### Command with Result Pattern
```csharp
public class UpdateProductCommand : ICommand<Result<Product>>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class UpdateProductHandler : ICommandHandler<UpdateProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        
        if (product == null)
            return Result<Product>.Failure("Product not found");
        
        product.Name = request.Name;
        await _repository.UpdateAsync(product, ct);
        
        return Result<Product>.Success(product);
    }
}
```

### Composite Queries
```csharp
public class GetProductWithReviewsQuery : IQuery<ProductWithReviews>
{
    public Guid ProductId { get; set; }
}

public class GetProductWithReviewsHandler : IQueryHandler<GetProductWithReviewsQuery, ProductWithReviews>
{
    private readonly IMediator _mediator;
    
    public async Task<ProductWithReviews> Handle(GetProductWithReviewsQuery request, CancellationToken ct)
    {
        // Compose multiple queries
        var product = await _mediator.Send(new GetProductByIdQuery { ProductId = request.ProductId }, ct);
        var reviews = await _mediator.Send(new GetProductReviewsQuery { ProductId = request.ProductId }, ct);
        
        return new ProductWithReviews
        {
            Product = product,
            Reviews = reviews
        };
    }
}
```

## Installation
```bash
dotnet add package ACommerce.SharedKernel.CQRS
```

## Dependencies

- ACommerce.SharedKernel.Abstractions
- MediatR (12.4.1)
- FluentValidation (11.10.0)

## Used By

- ACommerce.SharedKernel.AspNetCore
- All Ashare.* APIs

## License

MIT