using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesDashboard.Api.Domain;

namespace SalesDashboard.Api.Data;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> b)
    {
        b.Property(x => x.FullName).HasMaxLength(100);
        b.Property(x => x.Team).HasMaxLength(50);
        b.Property(x => x.Position).HasMaxLength(50);
        b.Property(x => x.AvatarUrl).HasMaxLength(500);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.Company).HasMaxLength(150);
        b.Property(x => x.Segment).HasConversion<string>().HasMaxLength(16);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.Property(x => x.Name).HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.Property(x => x.Sku).HasMaxLength(32);
        b.Property(x => x.Name).HasMaxLength(150);
        b.HasIndex(x => x.Sku).IsUnique();

        b.HasOne(x => x.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_products_list_price", "list_price >= 0");
            t.HasCheckConstraint("ck_products_unit_cost", "unit_cost >= 0");
        });
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

        b.HasOne(x => x.Manager)
            .WithMany(m => m.Sales)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
            .WithOne(i => i.Sale)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Любой запрос dashboard фильтрует по диапазону дат
        b.HasIndex(x => x.SoldAt);
        // Рейтинг и разрезы по менеджеру за период; заодно покрывает FK manager_id
        b.HasIndex(x => new { x.ManagerId, x.SoldAt });

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_sales_status", "status IN ('Paid', 'Cancelled', 'Refunded')");
            t.HasCheckConstraint("ck_sales_totals", "total_amount >= 0 AND total_cost >= 0");
        });
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> b)
    {
        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t =>
        {
            t.HasCheckConstraint("ck_sale_items_quantity", "quantity > 0");
            t.HasCheckConstraint("ck_sale_items_prices", "unit_price >= 0 AND unit_cost >= 0");
        });
    }
}