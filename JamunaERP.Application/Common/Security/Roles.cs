namespace JamunaERP.Application.Common.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string AdminOrManager = Admin + "," + Manager;

    public static readonly string[] All = { Admin, Manager, Employee };
}