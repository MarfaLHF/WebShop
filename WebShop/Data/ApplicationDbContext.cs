using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using WebShop.Models;

namespace WebShop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Product> Product { get; set; }
        public async Task Initialize(ApplicationDbContext context)
        {
            // Добавление ролей
            var roles = new List<IdentityRole>
    {
        new IdentityRole
        {
            Id = "2",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "31.03.2024"
        },
        new IdentityRole
        {
            Id = "1",
            Name = "User",
            NormalizedName = "USER",
            ConcurrencyStamp = "31.03.2024"
        }
    };

            foreach (var role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role.Name))
                {
                    context.Roles.Add(role);
                }
            }

            // Создание пользователя с ролью администратора
            var adminUser = new IdentityUser
            {
                Id = "1",
                UserName = "admin@mail.ru",
                Email = "admin@mail.ru",
                NormalizedUserName = "ADMIN@MAIL.RU",
                NormalizedEmail = "ADMIN@MAIL.RU",
                EmailConfirmed = true,
            };

            if (!context.Users.Any(u => u.UserName == adminUser.UserName))
            {
                var passwordHasher = new PasswordHasher<IdentityUser>();
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Qwerty123!");


                context.Users.Add(adminUser);

                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        RoleId = adminRole.Id,
                        UserId = adminUser.Id
                    });
                }
            }
            await context.SaveChangesAsync();
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == "1");
            if (user != null)
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var userRole = new IdentityUserRole<string>
                    {
                        RoleId = adminRole.Id,
                        UserId = user.Id
                    };

                    if (!context.UserRoles.Any(ur => ur.RoleId == userRole.RoleId && ur.UserId == userRole.UserId))
                    {
                        context.UserRoles.Add(userRole);
                        await context.SaveChangesAsync();
                    }
                }
            }
            var products = new List<Product>
    {
        new Product
        {
            ProductName = "Product 1",
            Description = "Description for Product 1",
            Price = 100,
            QuantityInStock = 50
        },
        new Product
        {
            ProductName = "Product 2",
            Description = "Description for Product 2",
            Price = 150,
            QuantityInStock = 30
        },
        new Product
        {
            ProductName = "Product 3",
            Description = "Description for Product 3",
            Price = 200,
            QuantityInStock = 20
        }
    };

            foreach (var product in products)
            {
                if (!context.Product.Any(p => p.ProductName == product.ProductName))
                {
                    context.Product.Add(product);
                }
            }


            await context.SaveChangesAsync();
        }


    }
}