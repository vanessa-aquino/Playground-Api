using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APICatalog.Migrations
{
    /// <inheritdoc />
    public partial class PopularCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            // O método Up é responsável por aplicar as alterações no banco de dados.
            // Aqui vou popular a tabela de categorias com alguns dados iniciais.
            mb.Sql("Insert into Categories (Name, ImageUrl) Values ('Bebidas', 'bebidas.jpg')");
            mb.Sql("Insert into Categories (Name, ImageUrl) Values ('Lanches', 'lanches.jpg')");
            mb.Sql("Insert into Categories (Name, ImageUrl) Values ('Sobremesas', 'sobremesas.jpg')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            // O método Down é responsável por reverter as alterações feitas no método Up, caso eu remova a migration.
            mb.Sql("Delete from Categories)");
        }
    }
}
