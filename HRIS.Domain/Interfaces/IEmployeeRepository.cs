using HRIS.Domain.Dtos;
using HRIS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<IEnumerable<object>> GetAllEmployeesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Name",
            bool sortDesc = false,
            string filterBy = "",
            string filterValue = "");
        Task AddEmployeeAsync(Employees employee);
        Task UpdateEmployeeAsync(Employees employee);
        Task DeleteEmployeeAsync(int id);

        Task DeactivateEmployeeAsync(int id, string reason);
    }
}
