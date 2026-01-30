using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026011402, "Seed initial employee data")]
public sealed class V2026011402_SeedEmployeeData : Migration
{
    public override void Up()
    {
        Log.Information("Seeding initial employee test data");
        Execute.Sql(@"
            INSERT INTO employee (first_name, last_name, email, phone, ssn, date_of_birth) VALUES
            ('John', 'Doe', 'john.doe@example.com', '555-1234', '123-45-6789', '1980-01-01'),
            ('Jane', 'Smith', 'jane.smith@example.com', '555-5678', '987-65-4321', '1985-05-15'),
            ('Alice', 'Johnson', 'alice.johnson@example.com', '555-9012', '456-78-9012', '1990-08-20'),
            ('Bob', 'Brown', 'bob.brown@example.com', '555-3456', '321-09-8765', '1988-12-10'),
            ('Charlie', 'Davis', 'charlie.davis@example.com', '555-7890', '654-32-1098', '1992-03-25'),
            ('Eve', 'Wilson', 'eve.wilson@example.com', '555-2468', '543-21-0987', '1995-07-10'),
            ('Frank', 'Miller', 'frank.miller@example.com', '555-4680', '765-43-2109', '1987-09-15'),
            ('Grace', 'Taylor', 'grace.taylor@example.com', '555-8024', '876-54-3210', '1993-11-05'),
            ('Hank', 'Anderson', 'hank.anderson@example.com', '555-1357', '234-56-7890', '1989-02-14');
        ");
    }

    public override void Down()
    {
        Log.Information("Deleting seeded employee test data");
        Delete.FromTable("employee").AllRows();
    }
}
