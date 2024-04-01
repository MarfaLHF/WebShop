using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebShop.Data;
using WebShop.Models;

namespace WebShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Cart
        public async Task<IActionResult> Cart()
        {
            var cartItems = GetCartItemsFromCookie();
            if (cartItems != null)
            {
                var totalPrice = cartItems.Sum(item => item.Quantity * item.Price);
                var cartViewModel = new CartViewModel
                {
                    Items = cartItems,
                    TotalPrice = totalPrice
                };
                return View(cartViewModel);
            }
            return View();
        }


        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Product.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {   
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            List<CartItem> cartItems = GetCartItemsFromCookie();

            var itemToRemove = cartItems.FirstOrDefault(item => item.ProductID == productId);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
            }

            SaveCartItemsToCookie(cartItems);

            return RedirectToAction("Cart");
        }
        [HttpPost]
        public async Task<IActionResult> DecrementItemQuantity(int productId)
        {
            List<CartItem> cartItems = GetCartItemsFromCookie();

            var itemToDecrement = cartItems.FirstOrDefault(item => item.ProductID == productId);

            if (itemToDecrement != null)
            {
                if (itemToDecrement.Quantity > 1)
                {
                    itemToDecrement.Quantity--;
                }
                else
                {
                    cartItems.Remove(itemToDecrement);
                }
            }

            SaveCartItemsToCookie(cartItems);

            return RedirectToAction("Cart");
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            List<CartItem> cart = GetCartItemsFromCookie();
            if (cart == null)
            {
                cart = new List<CartItem>();
            }

            var cartItem = cart.FirstOrDefault(item => item.ProductID == id);
            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            SaveCartItemsToCookie(cart);

            return RedirectToAction("index"); 
        }

        private List<CartItem> GetCartItemsFromCookie()
        {
            var cookieValue = Request.Cookies["ShoppingCart"];
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(cookieValue);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SaveCartItemsToCookie(List<CartItem> cart)
        {
            var serializedCart = JsonConvert.SerializeObject(cart);
            Response.Cookies.Append("ShoppingCart", serializedCart,
                new CookieOptions { Expires = DateTime.UtcNow.AddDays(30) }); 
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductID,ProductName,Description,Price,QuantityInStock")] Product product)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product) ;
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,ProductName,Description,Price,QuantityInStock")] Product product)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }


            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductID == id);
        }
        public async Task<IActionResult> Orders()
        {
            return View(await _context.Product.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            List<CartItem> cartItems = GetCartItemsFromCookie();
            var user = await _userManager.FindByEmailAsync(User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Identity");
            else
            {
                Order order = new Order
                {
                    UserID = user.Id,
                    Status = "Pending",
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = (int)cartItems.Sum(item => item.Quantity * item.Price)//
                };

                List<OrderDetail> orderDetails = new List<OrderDetail>();
                foreach (CartItem cartItem in cartItems)
                {
                    orderDetails.Add(new OrderDetail
                    {
                        ProductID = cartItem.ProductID,
                        Quantity = cartItem.Quantity,
                        Price = (int)cartItem.Price,//
                        Order = order
                    });

                    var product = await _context.Product.FindAsync(cartItem.ProductID);
                    if (product != null)
                    {
                        product.QuantityInStock -= cartItem.Quantity;
                        _context.Product.Update(product);
                    }
                }

                order.OrderDetails = orderDetails;

                _context.Add(order);
                await _context.SaveChangesAsync();

                SaveCartItemsToCookie(new List<CartItem>());

                return RedirectToAction("Index", "Orders");
            }  
        }
    }
}
