using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using API.FurnitureStore.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Api.FurnitureStore.Data
{
    public  class APIFurnitureStoreContext : IdentityDbContext
    {
        public APIFurnitureStoreContext(DbContextOptions options):base(options) { }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductsCategories { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OrderDetail>().HasKey( p => new {p.OrderId , p.ProductId});
        }

    }
}
