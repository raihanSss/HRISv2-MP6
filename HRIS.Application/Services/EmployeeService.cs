using HRIS.Domain.Dtos;
using HRIS.Domain.Interfaces;
using HRIS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetEmployeeByIdAsync(id);
        }

        public async Task<IEnumerable<object>> GetAllEmployeesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "NameEmp",
        bool sortDesc = false,
        string filterBy = "",
        string filterValue = "")
        {
            return await _employeeRepository.GetAllEmployeesAsync(pageNumber, pageSize, sortBy, sortDesc, filterBy, filterValue);
        }

        public async Task<string> AddEmployeeAsync(Employees employee)
        {
            await _employeeRepository.AddEmployeeAsync(employee);
                return "Data employee berhasil dtambah";
        }

        
        public async Task<string> UpdateEmployeeAsync(Employees employee)
        {
            await _employeeRepository.UpdateEmployeeAsync(employee);
            return "Data employee berhasil di update";
        }

        
        public async Task<string> DeleteEmployeeAsync(int id)
        {
            await _employeeRepository.DeleteEmployeeAsync(id);
            return "Data employee berhasil di hapus";
        }

        public async Task<string> DeactivateEmployeeAsync(int id, string reason)
        {
            try
            {
                await _employeeRepository.DeactivateEmployeeAsync(id, reason);
                return "Employee deactivated successfully";
            }
            catch (KeyNotFoundException ex)
            {
                return ex.Message;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
