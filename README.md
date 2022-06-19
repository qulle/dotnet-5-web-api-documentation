# Web APIs in dotNET 5
This is a work-in-progress-project. The intent is to document concepts and techniques related to Web APIs.

## Architecture model
This model demonstrates the flow that each request is travelling through on its way from the client to the database and back. The yellow boxes are just to show inheritance to framework classes along in what package they resides. The green boxes that are marked with `V1` and `V2` shows what places that requires work when making versions of the API. The red box represents the middleware system that's built into the framework. See [middleware](#11-middlewares) for more info.

![Architecture model](Images/architecture.svg?raw=true "Architecture model")

## Table of contents
[1. Getting Started](#1-getting-started) \
[2. Endpoints](#2-endpoints) \
[3. Swagger](#3-swagger) \
[4. Entity Framework](#4-entity-framework) \
[5. Versioning](#5-versioning) \
[6. Data Transfer Objects](#6-data-transfer-object) \
[7. REST](#7-rest) \
[8. GraphQL](#8-graphql) \
[9. Multithreading](#9-multithreading) \
[10. Dependency Injection](#10-dependency-injection) \
[11. Middlewares](#11-middlewares) \
[12. Cross Origin Resource Sharing](#12-cross-origin-resource-sharing) \
[13. Error handling](#13-error-handling) \
[14. Logging](#14-logging) \
[15. Docker](#15-docker) \
[16. Commands](#16-commands) \
[17. Dependencies](#17-dependencies)

## 1. Get started
Follow this list to get the project up and running on your machine.

### 1.1 System requirements and required tools
Apart from having dotnet 5, Visual Studio Code, Git and SQLExpress installed you also need to have some additional dotnet tools installed. 
```
$ dotnet tool install --global dotnet-ef
```

### 1.2 Clone the repo
```
$ git clone https://github.com/qulle/dotnet-5-web-api-documentation.git
```

### 1.3 MSSQL
The project uses MSSQL and the connection string uses the instance name `.\SQLExpress`. The database that will be created by Entity Framework is called `WebStoreDB`. Use the command `$ sc query MSSQL$SQLEXPRESS` to check if the SQLExpress server is up and running.

### 1.4 MSSQL User account
Create a user account with the **name** `WebStoreAPI` with the **password** `alohomora`. 

Make sure that the server instance **allowes** both `SQL Server and Windows Authentication` mode under `Properties --> Security`. 

Try and log in using the `WebStoreAPI` account in SSMS. If you get a problem saying something like _**Named Pipes Provider, error 40 - Could not open a connection to**_ you might need to **enable** both `TCP/IP` and `Piping` in `SQL Configurations Manager` under `SQL Server Network Configuration --> Protocols for SQLEXPRESS`.

### 1.5 Connection string
The connection string can use `trusted connection / integrated security` instead of the WebStoreAPI account. The connection string is located in  `appsettings.json`.

User account.
```json
"ConnectionStrings": {
    "WebStoreConnection": "Server=.\\SQLExpress;Initial Catalog=WebStoreDB;User ID=WebStoreAPI;Password=alohomora;"
}
```

Trusted connection.
```json
"ConnectionStrings": {
    "WebStoreConnection": "Server=.\\SQLExpress;Initial Catalog=WebStoreDB;Trusted_Connection=True;"
}
``` 

### 1.6 Create database and apply migrations
Run the following two commands to create a migration with the name `Initial-DB-Seed-Migration` and apply the migration on the database.
```
$ dotnet ef migrations add Initial-DB-Seed-Migration
$ dotnet ef database update
```

### 1.7 Test
The setup process should now be complete. To test that everything is working run the following command from the project root directory.
```
$ dotnet run
```

Open a browser and navigate to Swagger UI [https://localhost:5001/swagger/index.html](https://localhost:5001/swagger/index.html) or test the api via [Postman](https://www.postman.com/). You find the available API endpoints in the upcomming chapter.

## 2. Endpoints
This table shows what endpoints that are available in the API. A more detailed view can be seen in the Swagger UI at runtime. 

| Verb | Endpoint | Description | Success | Failure |
| ------------- | ------------- | -------------  | ------------- | ------------- |
| `GET` | /api/v{id}/products | List of products | `200 Ok` | `400 Bad request` `404 Not found` |
| `GET` | /api/v{id}/products/{id} | Single product | `200 Ok` | `400 Bad request` `404 Not found` |
| `POST` | /api/v{id}/products | Add single product | `201 Created` | `400 Bad request` `404 Not Allowed` |
| `PUT` | /api/v{id}/products/{id} | Update single product | `204 No content` | `400 Bad request` `404 Not found` |
| `PATCH` | /api/v{id}/products/{id} | Partial update single product | `204 No content` | `400 Bad request` `404 Not found` |
| `DELETE` | /api/v{id}/products/{id} | Delete single product | `204 No content` | `400 Bad request` `404 Not found` |

The endpoints can be called with both URI segment `/api/v1/products` or URL QueryString `/api/products?api-version=1`

## 3. Swagger
Swagger is built by SmartBear Software and is a set of tools that implements the OpenAPI specification. Swagger is used as a self documenting tool for describing RESTful APIs and is shiped in the Swashbuckle nuget package.

### 3.1 Dependency
```
$ dotnet add package Swashbuckle.AspNetCore
```

### 3.2 Basic setup
Register Swagger as a service in `Startup.ConfigureServices`.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwaggerGen(config => {
        config.SwaggerDoc("V1", new OpenApiInfo {Title = "WebStore API", Version = "V1"});
    });
}
```

Add Swagger middleware in `Startup.Configure`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if(env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "WebStore API V1"));
    }
}
```

### 3.3 Swagger and API versioning
The above code is the most basic usage of Swagger. In order to make it work in an API that uses versioning and has different controllers and routes with the same names, the following config needs to be added.

#### 3.3.1 Dependency
```
$ dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
```

Every route in each controller needs to have the attribute `[MapToApiVersion("")]` with their corresponding version number as an argument.
```csharp
namespace WebStore.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/products")]
    [Route("api/v{version:apiVersion}/products")]
    [Produces("application/json")]
    [ApiController]
    public class ProductsController : ControllerBase 
    {
        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts()
        {

        }
    }
}
```

Register the service in `Startup.ConfigureServices`.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddVersionedApiExplorer(options => 
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
}
```

Update the Swagger UI middleware.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if(env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json", 
                    description.GroupName.ToUpperInvariant()
                );
            }
        });
    }
}
```

Add a new helper file.
```csharp
namespace WebStore.Helpers
{
    public class ConfigureSwaggerOptions : IConfigureNamedOptions<SwaggerGenOptions>
    {
        // To much code. See WebStore/Helpers/ConfigureSwaggerOptions.cs
    }
}
```

Lastly update the single Swagger generation config with the new helper class. **Note** that the `services.AddSwaggerGen()` method not longer have a single api link as it does with the basic usage.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwaggerGen();
    services.ConfigureOptions<ConfigureSwaggerOptions>();
}
```

## 4. Entity Framework
The project uses Entity Framework to handle communication with the database. The core part of the communication between the API and the database resides through the `DbContext`. 

I have mixed feelings about Entity Framework and especcially the `code first approach`. I don't feel like i am in control of what's going on in the database and that i will mess upp the migrations when working with others on the same project. Also to introduce circular references and not keeping the database correctly normalized.  

An alternative to Entity Framework is [Dapper](https://dapper-tutorial.net/). Dapper is also a ORM (Object Relational Mapping) framework developed by people over at StackOverflow. Dapper doesn't convert linq expressions to T-SQL code like EF does, but lets you write your own inline SQL queries or execute stored procedures.

### 4.1 Dependencies 
```
$ dotnet add package Microsoft.EntityFrameworkCore
$ dotnet add package Microsoft.EntityFrameworkCore.Design
$ dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### 4.2 DbContext
Create a class that inherits from the EF `DbContext` and add `DbSet<>` properties that will be tracked by the framework.
```csharp
using Microsoft.EntityFrameworkCore;
using WebStore.Models;

namespace WebStore.DataContext
{
    public class WebStoreContext : DbContext
    {
        public WebStoreContext(DbContextOptions<WebStoreContext> opt) : base(opt) {}

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Seed();
        }
    }
}
```

Register the DbContext in `Startup.ConfigureServices`. The DbContext will in this example be added as scoped lifetime service.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<WebStoreContext>(options => 
        options.UseSqlServer(Configuration.GetConnectionString("WebStoreConnection"))
    );
}
```

### 4.3 Seeding data
To keep the WebStoreContext class and the `OnModelCreating` method clean i have added an extension that will seed the database with some initial data.
```csharp
using Microsoft.EntityFrameworkCore;
using WebStore.Models;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product {Id = 1, Name = "Vortex Race 3", Quantity = 4, Price = 150, VendorGuid = "d406189b-01a1-404b-8147-cf9a81e1c283"},
            new Product {Id = 2, Name = "Varmilo VA88M", Quantity = 0, Price = 180, VendorGuid = "29077cea-6bdc-4dc4-b441-b325fc9a2797"},
            new Product {Id = 3, Name = "Ducky One 2 Mini", Quantity = 1, Price = 139, VendorGuid = "8b43de00-475a-4db2-be34-71a3d1ba40b1"},
            new Product {Id = 4, Name = "Keychron K8", Quantity = 2, Price = 89, VendorGuid = "f534302c-7c1b-4954-9cb9-9576fd1c7dd8"}
        );
    }
}
```

### 4.4 Applying Migrations
In order to be able to run migration commands the following tool must be installed on your system. Should be installed if you followed the get started steps.
```
$ dotnet tool install --global dotnet-ef
```

Entity Framework uses Migrations (in code first approach) to make changes to the database. Migrations are C# code that is automaticly handled by the framework. After making changes to a model that is beeing tracked as a `DbSet` the changes must be staged in a migration. You can also use `.edmx` models on database first approach.

1. The command `$ dotnet ef migrations add <NAME>` is used to create a migration. The migration is staged and stored in the `Migrations` directory. _This can be seen as a commit in Git._

2. To apply the staged changes that are stored in the migration run the command `$ dotnet ef database update`. _This can be seen as a push in Git._

The framework is keeping track of what migrations that have been executed on the database by a helper table in the database called `dbo._EFMigrationsHistory`.

## 5. Versioning
There are (at least) four ways of handling versioning of APIs. 
1. URI `api/v1/users`
2. QueryString parameter `api/users?api-version=1`
3. Custom Header parameter KEY `X-Version` VALUE `1`
4. Accept Header parameter KEY `Accept` VALUE `application/json;version=1`

Which of the above type to choose depends on many parameters. In this repo i have choosen to implement support for the first (1) and second (2). 

There is pros and cons with all the ways regarding, redability, clarity, testing, code structure and how often the API might need to be updated or patched and if just some routes or the hole API needs to be updated. 

An importat note when using `CreatedAtRoute` is that _**the route name needs to be globally unique accross all controllers**_ eaven if the controllers have different namespaces and are accessed through different URIs. Thats way i have choosen to use the `CreatedAtAction` instead.

There is also good points for not to support different version of an API but rather just appending data to the one and only version.

### 5.1 Dependency
```
$ dotnet add package Microsoft.AspNetCore.Mvc.Versioning
```

### 5.2 Versioning setup
Since i opted for URI versioning there is four places in the project that needs to have some work done in order to make the versioning work. The different versions is grouped in corresponding directories with the version name as you can see in the source code and in the [Architecture model](#architecture-model).

#### 5.2.1 Startup.cs
Add the api version configuration inside `Startup.ConfigureServices`.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.Conventions.Add(new VersionByNamespaceConvention());
    });
}
```

#### 5.2.2 Controllers
The ProductsController has two routes `api/products` and `api/v{version:apiVersion}/products`. By doing this the api can be called in two ways, either by `api/v1/products` or via `api/products?api-version=1`. 
```csharp
namespace WebStore.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/products")]
    [Route("api/v{version:apiVersion}/products")]
    [Produces("application/json")]
    [ApiController]
    public class ProductsController : ControllerBase 
    {
        ...
    }
}
```

#### 5.2.3 DtoModels
When making a new version of an API the DtoModels just need to add, remove or update some properties from the real Model class. The DtoModels are located in there own version directory.

#### 5.2.4 DtoProfiles
The DtoProfile is just a new mapping from the real model class and the new DtoModel. The DtoProfiles are also located in there own version directory.

#### 5.2.5 Global error controller versioning
Since i use a global error handling middleware which maps to the route `/error` the ErrorController needs to be decorated with all the available api-versions. The global error handeling system will be described in more detail later on in this article. The attribute `[ApiExplorerSettings(IgnoreApi = true)]` is to exclude this controller and its actions from the Swagger documentation.
```csharp
namespace WebStore.Controllers.Common
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        ...
    }
}
```

## 6. Data Transfer Object
An API is a hard contract between the client and server. This contract can not be violated or changed in any way without risking bugs and crashes. By using DTOs the "raw private" data can be mapped to another object that will be sent to the client. In this project i use `AutoMapper` to map the private model to a DTO model.

## 7. REST
_**Just because an endpoint returns JSON doesn't make it a RESTful API.**_ I have gathered topics around the REST "standard" that i find to be very central.

1. Naming
2. Endpoint schemas, GET, POST, PUT, PATCH, DELETE
3. Status codes
4. Created at route
5. Caching
6. Versioning

## 8. GraphQL
Another open source protocol that could be used instead of REST is GraphQL. This protocol was developed by Facebook and is gaining in popularity. It won't be discussed or used more in this repo.

## 9. Multithreading
Mutithreading is more then just sprinkle `async`, `task` and `await` all over the code untill it compiles. There are many advanced topics to discuse when it comes to multithreading that goes beyond my knowledge at the moment. This will be a brief overview.

### 9.1 The Threadpool
Threads are managed by the Operating System and the threadpool is storage place for where the OS will be pulling new threads whenever the application needs a new background thread. Whenever a new thread is created memory needs to be allocated.

### 9.2 Async all the way
You shouldn’t mix synchronous and asynchronous code without considering the effects on the system. Hard do find bugs can (will) probably occur due to a mix of both types. Or the system will be synchronous even though it might look like a asynchronous application due to synchronous bottlenecks. Make sure that a call chain is asynchronous from the bottom level at the database repository all the way up to the API controller.

### 9.3 What to return and when
Return `Task` or `Task<T>` as the default from an async method. Don't return `void` from an async method unless it's strongly motivated. Here follows a couple of examples with different use cases that might come in handy.

#### 9.3.1 FromResult
An example of a FromResult Task. Keep in mind that this will allocate heap memory and pull thread from the threadpool.
```csharp
public async Task<Product> GetProductByIdAsync(int id)
{
    return await Task.FromResult(new Product 
    {
        Id = id, 
        Name = "Mock Vortex Race 3", 
        Quantity = 4, 
        Price = 150,
        VendorGuid = "428f3089-616b-4f88-99be-0d34abc26701"
    });
}
```

#### 9.3.2 ValueTask
An example of a ValueTask. This creates a `struct` datatype and does not allocate memory on the heap and will not pull a new thread from the threadpool. But on the other hand, a ValueTask creates more overhead because of the struct with more fields then a simple Task reference. The first choice for a async method that does not return a value should be to return a Task. But if performance proves it beneficial should a ValueTask be used instead of a Task.
```csharp
public async ValueTask<int> GetValueAsync(int number)
{
    return await new ValueTask<int>(number * 2);
}
```

#### 9.3.3 CompletedTask
An example of a CompletedTask. Useful when a method needs to return a task that has no value. 
```csharp
public async Task UpdateProductAsync(Product product)
{
    await Task.CompletedTask;
}
```

#### 9.3.4 Task.Run
An example of Task.Run. **Note** that the method is not async and therefore can't be awaited. This way of wrapping an entire synchronous method in a Task is considered bad practise.
```csharp
private Task<int> ComputeSumAsync()
{
    return Task.Run(() =>
    {
        return 1 + 2;
    });
}
```

#### 9.3.5 Void
A method that returns void can't be awaited and can't handle exceptions. If there is an exception in the method that returns void the application might terminate and the exception will go unnoticed and unhandled. Because of this the method can't be properly tested. One use case where a async method should in fact return void is event handlers.
```csharp
private async void button1_Click(object sender, EventArgs e)
{
    await Button1ClickAsync();
}

public async Task Button1ClickAsync()
{
    await Task.Delay(1000);
}
```

## 10. Dependency Injection
Dependency injection is a magical term that seems very complex and technical but is just a structured way of letting the framework handle creation of objects and passing around references to classes in the project. 

The core of dependecy injection is the `Startup.ConfigureServices`. There are three main ways of adding classes to be used by the dependency system. 

1. **AddScoped** lifetime services are created once per request within the scope. It is equivalent to a singleton in the current scope. For example, in MVC it creates one instance for each HTTP request, but it uses the same instance in the other calls within the same web request. This lifetime works best for applications which have different behavior per user.

2. **AddTransient** lifetime services are created each time they are requested. This lifetime works best for lightweight, stateless services.

3. **AddSingleton** creates a single instance throughout the application. It creates the instance for the first time and reuses the same object in the all calls. **Note** that when using a singleton service every visitor uses the same instance of the object. This can sometimes cause unexpected behaviour for exampel when using a Dialog service in a Blazor application users might see a dialog that was intended for another visitor.

| Service type | In the scope of given HTTP request | Across different HTTP requests  |
| ------------- | ------------- | -------------  |
| Transient | New Instance | New Instance |
| Scoped | Same Instance | New Instance |
| Singleton | Same Instance | Same Instance |

### 10.1 What type is AddDbContext adding
Default the `AddDbContext` is adding the connection as `Scoped` lifetime. There is a overload to use to set the `ServiceLifetime` to `Transient` or `Singleton`
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<WebStoreContext>(options => 
        options.UseSqlServer(Configuration.GetConnectionString("WebStoreConnection")),
        ServiceLifetime.Transient
    );
}
``` 

### 10.2 Sharing the connection string
A nice way of getting the connection string from other parts of the code is to use a small helper class that can be added as a singleton service in the dependency injection. This way no hardcoded connection string names is nessesary apart from in the files `Startup.cs` and `appsettings.json`. 
```csharp
public sealed class ConnectionString  
{  
    public ConnectionString(string value)
    {
        Value = value;
    }  
  
    public string Value { get; }  
}
```

Add the ConnectionString as a singleton service in `Startup.ConfigureServices`.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(new ConnectionString(Configuration.GetConnectionString("WebStoreConnection")));
}
```

The ConnectionString class can now be injected into any class.
```csharp
public class SomeClass
{  
    private readonly ConnectionString _connectionString;  
  
    public SomeClass(ConnectionString connectionString)  
    {  
        _connectionString = connectionString;  
    }
}
```

## 11. Middlewares
Just like dependency injection, middleware is a framework construction that makes it possible to intercept http-calls and execute code both before and after each point in the call chain. Due to the fact that the HTTP request is passed through each registered middleware the order becomes very important.

![Middleware model](Images/middleware.svg?raw=true "Middleware model")

Middlewares are registerd in `Startup.Configure`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionProvider)
{
    ...
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    ...
}
```

## 12. Cross origin resource sharing
For the consumer (client) to be able to call the API from any origin the CORS middleware needs to be configured. If CORS is not enabled then all calls from a different origin will be blocked for security resons. When enabeling CORS an additional header is passed along with the response `Access-Control-Allow-Origin` with either a list of trusted origins or a asterics (*) that enables public access from any origin. CORS is acctually a broswer security feature in place to protect the client. 

CORS comes in to play in (at least) four different scenarious.
1. Different domains
2. Different subdomains
3. Different schema HTTP/HTTPS
4. Different ports

There are (at least) three different ways to enable CORS in the API.
1. Middleware (default policy)
2. Middleware and endpoint routing (named policy)
3. Attributes

### 12.1 Middleware - Default policies
Allow any origin will set asterisk (*) as the value of the `Access-Control-Allow-Origin`
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options => 
        options.AddDefaultPolicy(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        )
    );
}
```

**Note** that the CORS specification states that when using `AllowAnyOrigin` it can't be combined with `AllowCredentials`. The origins needs to be specified using `WithOrigins` as in the next example.

To limit access to only certain origins. **Note** that **no** trailing slash is used in the URLs. The methods `AllowAnyHeader`, `AllowAnyMethod` and `AllowCredentials` etc can also be used here.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options => 
        options.AddDefaultPolicy(
            builder => builder.WithOrigins(
                "https://localhost:5001", 
                "https://localhoast:4200"
            )
        )
    );
}
```


When a default policy is used the CORS middleware `app.UseCors()` needs to be added in `Startup.Configure`. It is important that this middleware comes after the `app.UseRouing()` middleware.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionProvider)
{
    ...
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors();
    ...
}
```

### 12.2 Middleware - Named policies
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(options => 
        options.AddPolicy(
            "WebStoreOpenPolicy",
            builder => builder.WithOrigins(
                "https://localhost:5001", 
                "https://localhoast:4200"
            )
        )
    );
}
```

When using a named policy and not a default policy it is not sufficcent to just add the `app.UseCors()` middleware. TODO

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseEndpoints(endpoints => 
    {
        endpoints.MapControllers().RequireCors("WebStoreOpenPolicy");
    });
}
```

### 12.3 Attributes
Insted of enabling the named policy for all controllers in `Startup.cs` the attribute `[EnableCors]` can be added to the controller. Likewise the attribute `[DisableCors]` can be used to block a controller for using cors. 
```csharp
namespace WebStore.Controllers.V1
{
    [EnableCors("WebStoreOpenPolicy")]
    [ApiController]
    public class ProductsController : ControllerBase 
    {

    }
}
```

Specific routes can also enable or block CORS by adding the attribute `[DisableCores]`.
```csharp
[DisableCors]
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts()
{

}
```

## 13. Error handling
Error handling can be done in (at least) three different ways.
1. Try catch
2. Middleware
3. Custom middleware

### 13.1 Try catch
```csharp
public async Task<ActionResult<SomeDto>> GetAllAsync()
{
    try
    {
        result = await _repository.GetAllAsync();
    }
    catch(Exception exception)
    {
        _logger.Error($"An exception was caught in method GetAllAsync '{exception}'");
        return Problem(exception.Error.Message);
    }

    return Ok(result);
}
```

### 13.2 Middleware
Add the `UseExceptionHandler` middleware in `Startup.Configure`. This will map any error thrown to the endpoint `/error`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseExceptionHandler("/error");
}
```

Add an error controller that will have the `/error` route. There are two important notes in the code below. 

- The controller needs to be decorated with all the api-versions that exists in the project or it will not map correctly. 

- The class should not be decorated with a global route attribute for a specific HTTP method. This is to ensure that the route will catch all incoming errors no matter what the method is.
```csharp
namespace WebStore.Controllers.Common
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var statusCode = exception.Error.GetType().Name switch
            {
                "ArgumentException" => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.ServiceUnavailable
            };

            return Problem(detail: exception.Error.Message, statusCode: (int)statusCode);
        } 
    }
}
```

The attribute `[ApiExplorerSettings(IgnoreApi = true)]` is to exclude this controller and its actions from the Swagger documentation.

### 13.3 Custom Middleware
A custom middleware can be created using two parts.
- Custom middleware
- Custom extension

The following code deinfes the custom middleware. Here the `RequestDelegate _next` is the part responsible for creating the linking with other middlewares.
```csharp
namespace WebStore.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.Error($"An exception was caught {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error."
            }.ToString());
        }
    }
}
```

Now add the custom extension that registers the middleware in the `IApplicationBuilder`.
```csharp
namespace WebStore.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
```

The last step is to add the `ConfigureCustomExceptionMiddleware` middleware in `Startup.Configure`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionProvider)
{
    app.ConfigureCustomExceptionMiddleware();
}
```

### 14. Logging
Logging is an essential part of any project. There are many popular logging frameworks such as `NLog`, `log4net` and `Serilog`. In this project i have choosen to use [Serilog](https://serilog.net/). Serilog uses a concept called `Sink`. A Sink is a way of outputting the logged information to different locations and applications. In this project i have choosen to use three common Sinks `Console`, `File` and `MSSqlServer`. We will use different Sinks depending on if we are in development or production mode. There are many other [Sinks](https://github.com/serilog/serilog/wiki/Provided-Sinks) available.

![Logging snippet](Images/logging.png?raw=true "Logging snippet")
In the image above a request to the route `/api/v1/products` is made. The log contains useful information about what code was run and at what time. Even the SQL query is logged from Entity Framework.

#### 14.1 Dependencies
```
$ dotnet add package Serilog.AspNetCore
$ dotnet add package Serilog.Sinks.Console
$ dotnet add package Serilog.Sinks.File
$ dotnet add package Serilog.Sinks.MSSqlServer
```

#### 14.2 Configuration
In the `CreateHostBuilder` method in `Program.cs` append the UseSerilog provider method. This will set Serilog as the logging provider.
```csharp
public static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        });
}
```

Serilog can be configured using two approaches, `by code` or by config in the `appsettings.json` files. In both approaches start by removing the default logging configuration from `appsettings.json` and `appsettings.Development.json`. The part to remove looks like this.
```json
"Logging": {
    "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
    }
}
```

For **development mode** add this json object to the `appsettings.Development.json` file.
```json
"Serilog": {
    "MinimumLevel": {
        "Default": "Debug",
        "Override": {
            "Microsoft": "Information",
            "System": "Information"
        }
    },
    "Using": ["Serilog.Sinks.Console"],
    "WriteTo": [{ 
        "Name": "Console"
    }, {
        "Name": "File",
        "Args": { 
            "path": "Logs/log.txt", 
            "rollingInterval": "Day"
        }
    }]
}
```

The above config can also be done `by code`.
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

For **production mode** we will use the `MSSqlServer` sink instead of the `Console`. Add this json object to the `appsettings.json` file.
```json
"Serilog": {
    "MinimumLevel": {
        "Default": "Information",
        "Override": {
            "Microsoft": "Error",
            "System": "Error"
        },
        "Using": ["Serilog.Sinks.MSSqlServer"]
    },
    "WriteTo": [{
        "Name": "MSSqlServer",
        "Args": {
            "connectionString": "Server=.\\SQLExpress;Initial Catalog=WebStoreDB;User ID=WebStoreAPI;Password=alohomora;",
            "tableName": "Logs",
            "autoCreateSqlTable": true
        }
    }]
}
```

#### 14.3 Doing the logging
Injecting the logger to any class is just as simple as any other dependency injected service. Start by adding the logger as a service inside `Startup.ConfigureServices`.
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(Log.Logger);
}
```

Then inject the logger in any class. **Note** that the ILogger comes from the `using Serilog` and not from `using Microsoft.Extensions.Logging`.
```csharp
public ProductsController(IWebStoreRepository repository, IMapper mapper, ILogger logger)
{
    _repository = repository;
    _mapper = mapper;
    _logger = logger;
}
```

To automatically log all HTTP requests to the API use the Serilog middleware inside `Startup.Configure`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseSerilogRequestLogging();
}
```

Since a global error handler is used all errors can be logged in one place. **Note that if the middleware `app.UseSerilogRequestLogging()` is used the error will be logged twice in the sinks.
```csharp
[Route("/error")]
public IActionResult Error()
{
    var clientIP = HttpContext.Connection?.RemoteIpAddress.MapToIPv4();
    var clientDNS = Dns.GetHostEntry(clientIP);

    var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
    var statusCode = exception.Error.GetType().Name switch
    {
        "ArgumentException" => HttpStatusCode.BadRequest,
        _ => HttpStatusCode.ServiceUnavailable
    };

    _logger.Information($"Client '{clientDNS.HostName}' with ip '{clientIP}'");
    _logger.Error($"An exception was caught with code '{(int)statusCode} - {statusCode}' and message '{exception.Error.Message}'");

    return Problem(detail: exception.Error.Message, statusCode: (int)statusCode);
}
```

#### 14.4 Log levels
The Serilog ILogger reference exposes six different logging leves with corresponding method names that can be called on the ILogger reference.
| Level | Usage |
| ------------- | ------------- |
| `Verbose` | Verbose is the noisiest level, rarely (if ever) enabled for a production app. |
| `Debug` | Debug is used for internal system events that are not necessarily observable from the outside, but useful when determining how something happened. |
| `Information` | Information events describe things happening in the system that correspond to its responsibilities and functions. Generally these are the observable actions the system can perform. |
| `Warning` | When service is degraded, endangered, or may be behaving outside of its expected parameters, Warning level events are used. |
| `Error` | When functionality is unavailable or expectations broken, an Error event is used. |
| `Fatal` | The most critical level, Fatal events demand immediate attention. |

There are many other configuration possibilities like filtering, overriding log leves in different scenarious etc. But these are the ones i feel are most common.

## 15. Docker
Using a docker container to hoast the API.

```dockerfile
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore
 
COPY . ./
RUN dotnet publish -c Release -o out
 
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "WebStore.dll"]
```

To build the docker image enter the following command.
```
$ docker build -t <IMAGE-NAME:TAG>
```

To run the container enter the following command which will set up port mapping.
```
$ docker run -p 8080:80 <IMAGE-NAME:TAG>
```

## 16. Commands
A list of useful commands when working on the project.

```
$ dotnet new webapi -n WebStore              # Create a new webapi project
$ dotnet build                               # Build the API
$ dotnet run                                 # Run the API https://localhost:5001 http://localhost:5000
$ dotnet clean                               # Delete old build data
$ dotnet add package <NAME>                  # Add dependencies to the project
$ dotnet tool install --global dotnet-ef     # Command to install the EF tools
$ dotnet tool update --global dotnet-ef      # Command to update the EF tools
$ dotnet ef migrations add <NAME>            # Create a migration when changes have been made in the models
$ dotnet ef migrations remove                # If the migration scripts are not to your satisfaction, undo and fix before add once again
$ dotnet ef database update                  # Perform migrations to the database
```

## 17. Dependencies
A list of all dependencies used in the project.

- AutoMapper.Extensions.Microsoft.DependencyInjection
- Microsoft.AspNetCore.JsonPatch
- Microsoft.AspNetCore.Mvc.NewtonsoftJson
- Microsoft.AspNetCore.Mvc.Versioning
- Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Microsoft.EntityFrameworkCore.SqlServer
- Swashbuckle.AspNetCore
- Serilog
- Serilog.Sinks.Console
- Serilog.Sinks.File
- Serilog.Sinks.MSSqlServer

## Author
[Qulle](https://github.com/qulle/)