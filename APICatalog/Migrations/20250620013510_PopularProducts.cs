using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APICatalog.Migrations
{
    /// <inheritdoc />
    public partial class PopularProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("Insert into Products(Name, Description, Price, ImageUrl, Stock, RegistrationDate, CategoryId) " +
                "Values ('Coca-Cola Diet', 'Refrigerante de cola 350 ml', 5.45, 'cocacola.png', 50, now(), 1)");
            mb.Sql("Insert into Products(Name, Description, Price, ImageUrl, Stock, RegistrationDate, CategoryId) " +
               "Values ('Sanduiche de frango', 'Sanduiche de Frango com maionese', 8.50, 'sanduiche.png', 10, now(), 2)");
            mb.Sql("Insert into Products(Name, Description, Price, ImageUrl, Stock, RegistrationDate, CategoryId) " +
               "Values ('Pudim 100g', 'Pudim de leite condensado 100g', 6.75, 'pudim.png', 20, now(), 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("Delete from Products");
        }
    }
}
