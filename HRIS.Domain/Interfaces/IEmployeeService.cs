using HRIS.Domain.Dtos;
using HRIS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Domain.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<IEnumerable<object>> GetAllEmployeesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "Name",
            bool sortDesc = false,
            string filterBy = "",
            string filterValue = "");
        Task <string> AddEmployeeAsync(Employees employee);
        Task<string> UpdateEmployeeAsync(Employees employee);
        Task<string> DeleteEmployeeAsync(int id);

        Task<string> DeactivateEmployeeAsync(int id, string reason);
    }
}
