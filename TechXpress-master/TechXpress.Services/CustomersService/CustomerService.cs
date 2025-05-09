using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data;
using TechXpress.Data.Models;
using TechXpress.Data.Models.Contexts;
using TechXpress.Services.CustomersService;

namespace TechXpress.Services.CustomersService
{
    public class CustomerService : ICustomerService
    {
        private readonly TechXpressDbContext _context;

        public CustomerService(TechXpressDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllCustomersAsync()
        {
            return await _context.Users.ToListAsync(); // Fetch all customers
        }

        public async Task<ApplicationUser> GetCustomerByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id); // Fetch customer by ID
        }

        public async Task AddCustomerAsync(ApplicationUser customer)
        {
            _context.Users.Add(customer);
            await _context.SaveChangesAsync(); // Save new customer
        }

        public async Task UpdateCustomerAsync(ApplicationUser customer)
        {
            _context.Users.Update(customer);
            await _context.SaveChangesAsync(); // Update customer details
        }

        public async Task DeleteCustomerAsync(string id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer != null)
            {
                _context.Users.Remove(customer);
                await _context.SaveChangesAsync(); // Delete customer
            }
        }
    }
}
