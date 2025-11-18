namespace TimberHaul.API.Models;

public enum UserRole
{
    Forester,
    Delivery,
    Customer
}

public enum LoadStatus
{
    Pending,
    OnTruck,
    InTransit,
    Delivered
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Overdue
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Other
}

public enum EquipmentType
{
    Chainsaw,
    Truck,
    Winch,
    Other
}

public enum WoodType
{
    Oak,
    Beech,
    Pine,
    Spruce,
    Birch,
    Other
}