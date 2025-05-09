using System.Collections.Generic;
using System.Threading.Tasks;
using TechXpress.Data.Models;

namespace TechXpress.Services.CustomersService
{
    public interface ICustomerService
    {
        Task<IEnumerable<ApplicationUser>> GetAllCustomersAsync();
        Task<ApplicationUser> GetCustomerByIdAsync(string id);
        Task AddCustomerAsync(ApplicationUser customer);
        Task UpdateCustomerAsync(ApplicationUser customer);
        Task DeleteCustomerAsync(string id);
    }
}
