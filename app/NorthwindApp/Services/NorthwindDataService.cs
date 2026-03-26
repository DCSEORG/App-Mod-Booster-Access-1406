using Microsoft.Data.SqlClient;
using NorthwindApp.Models;

namespace NorthwindApp.Services;

public interface INorthwindDataService
{
    // Customers
    Task<List<Customer>> GetCustomersAsync(string? filter = null);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<int> CreateCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(int id);

    // Products
    Task<List<Product>> GetProductsAsync(string? filter = null);
    Task<Product?> GetProductByIdAsync(int id);
    Task<int> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);

    // Orders
    Task<List<Order>> GetOrdersAsync(int? statusId = null);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<int> CreateOrderAsync(Order order);
    Task<bool> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(int id);

    // Order Details
    Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId);
    Task<int> CreateOrderDetailAsync(OrderDetail detail);
    Task<bool> UpdateOrderDetailAsync(OrderDetail detail);
    Task<bool> DeleteOrderDetailAsync(int id);

    // Employees
    Task<List<Employee>> GetEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<int> CreateEmployeeAsync(Employee employee);
    Task<bool> UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);

    // Order Status
    Task<List<OrderStatus>> GetOrderStatusesAsync();
}

public class NorthwindDataService : INorthwindDataService
{
    private readonly string _connectionString;
    private readonly ILogger<NorthwindDataService> _logger;

    public NorthwindDataService(IConfiguration configuration, ILogger<NorthwindDataService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

    // ============================================================
    // CUSTOMERS
    // ============================================================

    public async Task<List<Customer>> GetCustomersAsync(string? filter = null)
    {
        var customers = new List<Customer>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetCustomers", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Filter", (object?)filter ?? DBNull.Value);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            customers.Add(MapCustomer(reader));
        return customers;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetCustomerById", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@CustomerID", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCustomer(reader) : null;
    }

    public async Task<int> CreateCustomerAsync(Customer c)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_CreateCustomer", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@CustomerName", c.CustomerName);
        cmd.Parameters.AddWithValue("@PrimaryContactLastName", (object?)c.PrimaryContactLastName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactFirstName", (object?)c.PrimaryContactFirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactJobTitle", (object?)c.PrimaryContactJobTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactEmailAddress", (object?)c.PrimaryContactEmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BusinessPhone", (object?)c.BusinessPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address", (object?)c.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", (object?)c.City ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@State", (object?)c.State ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Zip", (object?)c.Zip ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Website", (object?)c.Website ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)c.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AddedBy", (object?)c.AddedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateCustomerAsync(Customer c)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_UpdateCustomer", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@CustomerID", c.CustomerID);
        cmd.Parameters.AddWithValue("@CustomerName", c.CustomerName);
        cmd.Parameters.AddWithValue("@PrimaryContactLastName", (object?)c.PrimaryContactLastName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactFirstName", (object?)c.PrimaryContactFirstName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactJobTitle", (object?)c.PrimaryContactJobTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryContactEmailAddress", (object?)c.PrimaryContactEmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BusinessPhone", (object?)c.BusinessPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address", (object?)c.Address ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", (object?)c.City ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@State", (object?)c.State ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Zip", (object?)c.Zip ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Website", (object?)c.Website ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)c.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ModifiedBy", (object?)c.ModifiedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_DeleteCustomer", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@CustomerID", id);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ============================================================
    // PRODUCTS
    // ============================================================

    public async Task<List<Product>> GetProductsAsync(string? filter = null)
    {
        var products = new List<Product>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetProducts", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Filter", (object?)filter ?? DBNull.Value);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            products.Add(MapProduct(reader));
        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetProductById", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ProductID", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProduct(reader) : null;
    }

    public async Task<int> CreateProductAsync(Product p)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_CreateProduct", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ProductCode", (object?)p.ProductCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
        cmd.Parameters.AddWithValue("@ProductDescription", (object?)p.ProductDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UnitPrice", p.UnitPrice);
        cmd.Parameters.AddWithValue("@AddedBy", (object?)p.AddedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateProductAsync(Product p)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_UpdateProduct", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ProductID", p.ProductID);
        cmd.Parameters.AddWithValue("@ProductCode", (object?)p.ProductCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ProductName", p.ProductName);
        cmd.Parameters.AddWithValue("@ProductDescription", (object?)p.ProductDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UnitPrice", p.UnitPrice);
        cmd.Parameters.AddWithValue("@ModifiedBy", (object?)p.ModifiedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_DeleteProduct", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ProductID", id);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ============================================================
    // ORDERS
    // ============================================================

    public async Task<List<Order>> GetOrdersAsync(int? statusId = null)
    {
        var orders = new List<Order>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetOrders", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@StatusID", (object?)statusId ?? DBNull.Value);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            orders.Add(MapOrder(reader));
        return orders;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetOrderById", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderID", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapOrder(reader) : null;
    }

    public async Task<int> CreateOrderAsync(Order o)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_CreateOrder", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@EmployeeID", (object?)o.EmployeeID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CustomerID", o.CustomerID);
        cmd.Parameters.AddWithValue("@OrderDate", (object?)o.OrderDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)o.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusID", (object?)o.StatusID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AddedBy", (object?)o.AddedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateOrderAsync(Order o)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_UpdateOrder", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderID", o.OrderID);
        cmd.Parameters.AddWithValue("@EmployeeID", (object?)o.EmployeeID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CustomerID", o.CustomerID);
        cmd.Parameters.AddWithValue("@OrderDate", (object?)o.OrderDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ShippedDate", (object?)o.ShippedDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PaidDate", (object?)o.PaidDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)o.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusID", (object?)o.StatusID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ModifiedBy", (object?)o.ModifiedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_DeleteOrder", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderID", id);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ============================================================
    // ORDER DETAILS
    // ============================================================

    public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
    {
        var details = new List<OrderDetail>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetOrderDetails", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderID", orderId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            details.Add(MapOrderDetail(reader));
        return details;
    }

    public async Task<int> CreateOrderDetailAsync(OrderDetail d)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_CreateOrderDetail", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderID", d.OrderID);
        cmd.Parameters.AddWithValue("@ProductID", d.ProductID);
        cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
        cmd.Parameters.AddWithValue("@UnitPrice", d.UnitPrice);
        cmd.Parameters.AddWithValue("@AddedBy", (object?)d.AddedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateOrderDetailAsync(OrderDetail d)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_UpdateOrderDetail", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderDetailID", d.OrderDetailID);
        cmd.Parameters.AddWithValue("@ProductID", d.ProductID);
        cmd.Parameters.AddWithValue("@Quantity", d.Quantity);
        cmd.Parameters.AddWithValue("@UnitPrice", d.UnitPrice);
        cmd.Parameters.AddWithValue("@ModifiedBy", (object?)d.ModifiedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DeleteOrderDetailAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_DeleteOrderDetail", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@OrderDetailID", id);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ============================================================
    // EMPLOYEES
    // ============================================================

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        var employees = new List<Employee>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetEmployees", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            employees.Add(MapEmployee(reader));
        return employees;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetEmployeeById", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@EmployeeID", id);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapEmployee(reader) : null;
    }

    public async Task<int> CreateEmployeeAsync(Employee e)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_CreateEmployee", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@FirstName", e.FirstName);
        cmd.Parameters.AddWithValue("@LastName", e.LastName);
        cmd.Parameters.AddWithValue("@EmailAddress", (object?)e.EmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@JobTitle", (object?)e.JobTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryPhone", (object?)e.PrimaryPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SecondaryPhone", (object?)e.SecondaryPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Title", (object?)e.Title ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)e.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@WindowsUserName", (object?)e.WindowsUserName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AddedBy", (object?)e.AddedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateEmployeeAsync(Employee e)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_UpdateEmployee", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@EmployeeID", e.EmployeeID);
        cmd.Parameters.AddWithValue("@FirstName", e.FirstName);
        cmd.Parameters.AddWithValue("@LastName", e.LastName);
        cmd.Parameters.AddWithValue("@EmailAddress", (object?)e.EmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@JobTitle", (object?)e.JobTitle ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PrimaryPhone", (object?)e.PrimaryPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SecondaryPhone", (object?)e.SecondaryPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Title", (object?)e.Title ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes", (object?)e.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@WindowsUserName", (object?)e.WindowsUserName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ModifiedBy", (object?)e.ModifiedBy ?? DBNull.Value);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_DeleteEmployee", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@EmployeeID", id);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ============================================================
    // ORDER STATUS
    // ============================================================

    public async Task<List<OrderStatus>> GetOrderStatusesAsync()
    {
        var statuses = new List<OrderStatus>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SqlCommand("usp_GetOrderStatuses", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            statuses.Add(MapOrderStatus(reader));
        return statuses;
    }

    // ============================================================
    // MAPPERS
    // ============================================================

    private static Customer MapCustomer(SqlDataReader r) => new()
    {
        CustomerID = r.GetInt32(r.GetOrdinal("CustomerID")),
        CustomerName = r.IsDBNull(r.GetOrdinal("CustomerName")) ? string.Empty : r.GetString(r.GetOrdinal("CustomerName")),
        PrimaryContactLastName = r.IsDBNull(r.GetOrdinal("PrimaryContactLastName")) ? null : r.GetString(r.GetOrdinal("PrimaryContactLastName")),
        PrimaryContactFirstName = r.IsDBNull(r.GetOrdinal("PrimaryContactFirstName")) ? null : r.GetString(r.GetOrdinal("PrimaryContactFirstName")),
        PrimaryContactJobTitle = r.IsDBNull(r.GetOrdinal("PrimaryContactJobTitle")) ? null : r.GetString(r.GetOrdinal("PrimaryContactJobTitle")),
        PrimaryContactEmailAddress = r.IsDBNull(r.GetOrdinal("PrimaryContactEmailAddress")) ? null : r.GetString(r.GetOrdinal("PrimaryContactEmailAddress")),
        BusinessPhone = r.IsDBNull(r.GetOrdinal("BusinessPhone")) ? null : r.GetString(r.GetOrdinal("BusinessPhone")),
        Address = r.IsDBNull(r.GetOrdinal("Address")) ? null : r.GetString(r.GetOrdinal("Address")),
        City = r.IsDBNull(r.GetOrdinal("City")) ? null : r.GetString(r.GetOrdinal("City")),
        State = r.IsDBNull(r.GetOrdinal("State")) ? null : r.GetString(r.GetOrdinal("State")),
        Zip = r.IsDBNull(r.GetOrdinal("Zip")) ? null : r.GetString(r.GetOrdinal("Zip")),
        Website = r.IsDBNull(r.GetOrdinal("Website")) ? null : r.GetString(r.GetOrdinal("Website")),
        Notes = r.IsDBNull(r.GetOrdinal("Notes")) ? null : r.GetString(r.GetOrdinal("Notes")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn"))
    };

    private static Product MapProduct(SqlDataReader r) => new()
    {
        ProductID = r.GetInt32(r.GetOrdinal("ProductID")),
        ProductCode = r.IsDBNull(r.GetOrdinal("ProductCode")) ? null : r.GetString(r.GetOrdinal("ProductCode")),
        ProductName = r.IsDBNull(r.GetOrdinal("ProductName")) ? string.Empty : r.GetString(r.GetOrdinal("ProductName")),
        ProductDescription = r.IsDBNull(r.GetOrdinal("ProductDescription")) ? null : r.GetString(r.GetOrdinal("ProductDescription")),
        UnitPrice = r.IsDBNull(r.GetOrdinal("UnitPrice")) ? 0 : r.GetDecimal(r.GetOrdinal("UnitPrice")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn"))
    };

    private static Order MapOrder(SqlDataReader r) => new()
    {
        OrderID = r.GetInt32(r.GetOrdinal("OrderID")),
        EmployeeID = r.IsDBNull(r.GetOrdinal("EmployeeID")) ? null : r.GetInt32(r.GetOrdinal("EmployeeID")),
        CustomerID = r.GetInt32(r.GetOrdinal("CustomerID")),
        OrderDate = r.IsDBNull(r.GetOrdinal("OrderDate")) ? null : r.GetDateTime(r.GetOrdinal("OrderDate")),
        ShippedDate = r.IsDBNull(r.GetOrdinal("ShippedDate")) ? null : r.GetDateTime(r.GetOrdinal("ShippedDate")),
        PaidDate = r.IsDBNull(r.GetOrdinal("PaidDate")) ? null : r.GetDateTime(r.GetOrdinal("PaidDate")),
        Notes = r.IsDBNull(r.GetOrdinal("Notes")) ? null : r.GetString(r.GetOrdinal("Notes")),
        StatusID = r.IsDBNull(r.GetOrdinal("StatusID")) ? null : r.GetInt32(r.GetOrdinal("StatusID")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn")),
        CustomerName = r.IsDBNull(r.GetOrdinal("CustomerName")) ? null : r.GetString(r.GetOrdinal("CustomerName")),
        EmployeeName = r.IsDBNull(r.GetOrdinal("EmployeeName")) ? null : r.GetString(r.GetOrdinal("EmployeeName")),
        StatusName = r.IsDBNull(r.GetOrdinal("StatusName")) ? null : r.GetString(r.GetOrdinal("StatusName"))
    };

    private static OrderDetail MapOrderDetail(SqlDataReader r) => new()
    {
        OrderDetailID = r.GetInt32(r.GetOrdinal("OrderDetailID")),
        OrderID = r.GetInt32(r.GetOrdinal("OrderID")),
        ProductID = r.GetInt32(r.GetOrdinal("ProductID")),
        Quantity = r.IsDBNull(r.GetOrdinal("Quantity")) ? 1 : r.GetInt32(r.GetOrdinal("Quantity")),
        UnitPrice = r.IsDBNull(r.GetOrdinal("UnitPrice")) ? 0 : r.GetDecimal(r.GetOrdinal("UnitPrice")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn")),
        ProductName = r.IsDBNull(r.GetOrdinal("ProductName")) ? null : r.GetString(r.GetOrdinal("ProductName")),
        ProductCode = r.IsDBNull(r.GetOrdinal("ProductCode")) ? null : r.GetString(r.GetOrdinal("ProductCode"))
    };

    private static Employee MapEmployee(SqlDataReader r) => new()
    {
        EmployeeID = r.GetInt32(r.GetOrdinal("EmployeeID")),
        FirstName = r.IsDBNull(r.GetOrdinal("FirstName")) ? string.Empty : r.GetString(r.GetOrdinal("FirstName")),
        LastName = r.IsDBNull(r.GetOrdinal("LastName")) ? string.Empty : r.GetString(r.GetOrdinal("LastName")),
        FullNameFNLN = r.IsDBNull(r.GetOrdinal("FullNameFNLN")) ? null : r.GetString(r.GetOrdinal("FullNameFNLN")),
        FullNameLNFN = r.IsDBNull(r.GetOrdinal("FullNameLNFN")) ? null : r.GetString(r.GetOrdinal("FullNameLNFN")),
        EmailAddress = r.IsDBNull(r.GetOrdinal("EmailAddress")) ? null : r.GetString(r.GetOrdinal("EmailAddress")),
        JobTitle = r.IsDBNull(r.GetOrdinal("JobTitle")) ? null : r.GetString(r.GetOrdinal("JobTitle")),
        PrimaryPhone = r.IsDBNull(r.GetOrdinal("PrimaryPhone")) ? null : r.GetString(r.GetOrdinal("PrimaryPhone")),
        SecondaryPhone = r.IsDBNull(r.GetOrdinal("SecondaryPhone")) ? null : r.GetString(r.GetOrdinal("SecondaryPhone")),
        Title = r.IsDBNull(r.GetOrdinal("Title")) ? null : r.GetString(r.GetOrdinal("Title")),
        Notes = r.IsDBNull(r.GetOrdinal("Notes")) ? null : r.GetString(r.GetOrdinal("Notes")),
        WindowsUserName = r.IsDBNull(r.GetOrdinal("WindowsUserName")) ? null : r.GetString(r.GetOrdinal("WindowsUserName")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn"))
    };

    private static OrderStatus MapOrderStatus(SqlDataReader r) => new()
    {
        StatusID = r.GetInt32(r.GetOrdinal("StatusID")),
        StatusCode = r.IsDBNull(r.GetOrdinal("StatusCode")) ? null : r.GetString(r.GetOrdinal("StatusCode")),
        StatusName = r.IsDBNull(r.GetOrdinal("StatusName")) ? string.Empty : r.GetString(r.GetOrdinal("StatusName")),
        SortOrder = r.IsDBNull(r.GetOrdinal("SortOrder")) ? 0 : r.GetInt32(r.GetOrdinal("SortOrder")),
        AddedBy = r.IsDBNull(r.GetOrdinal("AddedBy")) ? null : r.GetString(r.GetOrdinal("AddedBy")),
        AddedOn = r.IsDBNull(r.GetOrdinal("AddedOn")) ? null : r.GetDateTime(r.GetOrdinal("AddedOn")),
        ModifiedBy = r.IsDBNull(r.GetOrdinal("ModifiedBy")) ? null : r.GetString(r.GetOrdinal("ModifiedBy")),
        ModifiedOn = r.IsDBNull(r.GetOrdinal("ModifiedOn")) ? null : r.GetDateTime(r.GetOrdinal("ModifiedOn"))
    };
}
