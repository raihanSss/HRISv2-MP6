using HRIS.Domain.Dtos;
using HRIS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRIS.Infrastructure.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly hrisDbContext _context;

        public EmployeeRepository(hrisDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetAllEmployeesAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "NameEmp",
        bool sortDesc = false,
        string filterBy = "",
        string filterValue = "")
        {
            var query = _context.Employees.AsQueryable();

            
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                query = filterBy switch
                {
                    "NameEmp" => query.Where(e => e.NameEmp.Contains(filterValue)),
                    "JobPosition" => query.Where(e => e.JobPosition.Contains(filterValue)),
                    _ => query
                };
            }

            // Sorting
            query = sortDesc ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                             : query.OrderBy(e => EF.Property<object>(e, sortBy));

            // Pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            
            var employees = await query
                .Select(e => new
                {
                    e.NameEmp,
                    e.Ssn,
                    DepartmentName = e.IdDeptNavigation.NameDept, 
                    e.JobPosition,
                    e.Level,
                    e.Type,
                    e.Lastupdate
                })
                .ToListAsync();

            return employees;
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Where(e => e.IdEmp == id)
                .Select(e => new EmployeeDto
                {
                    NameEmp = e.NameEmp,
                    Address = e.Address,
                    Phone = e.Phone,
                    Email = e.Email,
                    JobPosition = e.JobPosition,
                    Type = e.Type,
                    Reason = e.Reason,
                    Status = e.Status
                })
                .FirstOrDefaultAsync();

            return employee;
        }


        public async Task AddEmployeeAsync(Employees employee)
        {
     
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(Employees employee)
        {
            employee.Lastupdate = DateTime.UtcNow;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeactivateEmployeeAsync(int id, string reason)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee tidak ditemukan");
            }

            employee.Status = "Not Active";
            employee.Reason = reason;
            employee.Lastupdate = DateTime.UtcNow;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }
    }
}
