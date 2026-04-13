namespace NorthwindApp.Models;

public class Customer
{
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PrimaryContactLastName { get; set; }
    public string? PrimaryContactFirstName { get; set; }
    public string? PrimaryContactJobTitle { get; set; }
    public string? PrimaryContactEmailAddress { get; set; }
    public string? BusinessPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class Product
{
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class Order
{
    public int OrderID { get; set; }
    public int? EmployeeID { get; set; }
    public int CustomerID { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    public int? StatusID { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    // Joined fields
    public string? CustomerName { get; set; }
    public string? EmployeeName { get; set; }
    public string? StatusName { get; set; }
}

public class OrderDetail
{
    public int OrderDetailID { get; set; }
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    // Joined fields
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
}

public class Employee
{
    public int EmployeeID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FullNameFNLN { get; set; }
    public string? FullNameLNFN { get; set; }
    public string? EmailAddress { get; set; }
    public string? JobTitle { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public string? WindowsUserName { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class OrderStatus
{
    public int StatusID { get; set; }
    public string? StatusCode { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? AddedBy { get; set; }
    public DateTime? AddedOn { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
