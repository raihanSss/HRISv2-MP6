using HRIS.Application.Services;
using HRIS.Domain.Interfaces;
using HRIS.Domain.Models;
using HRIS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers
{
    [Route("api/employee")]
    [ApiController]
    
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAuthService _authService;

        public EmployeeController(IEmployeeService employeeService, IAuthService authService)
        {
            _employeeService = employeeService;
            _authService = authService; 
        }

        [HttpGet]
        [Authorize(Roles = "Administrator,HR Manager,Department Manager,Employee Supervisor")]
        public async Task<IActionResult> GetAllEmployees(
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "NameEmp",
        bool sortDesc = false,
        string filterBy = "",
        string filterValue = "")
        {
            var employees = await _employeeService.GetAllEmployeesAsync(pageNumber, pageSize, sortBy, sortDesc, filterBy, filterValue);

            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,HR Manager,Department Manager,Employee Supervisor,Employee")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPost("AddUserToExistingEmployee")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddUserToExistingEmployee([FromBody] AddUserToExistingEmployeeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            
            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            
            var result = await _authService.RegisterAsync(new RegisterEmployeeModel
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                // Menyertakan informasi employee
                NameEmp = employee.NameEmp,
                Ssn = employee.Ssn,
                Dob = employee.Dob,
                Address = employee.Address,
                Phone = employee.Phone,
                JobPosition = employee.JobPosition,
                Level = employee.Level,
                Type = employee.Type,
                Status = employee.Status,
                Reason = employee.Reason
            });

            if (result.Status == "Success")
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,HR Manager")]
        public async Task<IActionResult> PutEmployee(int id, Employees employee)
        {
            if (id != employee.IdEmp)
            {
                return BadRequest();
            }

            await _employeeService.UpdateEmployeeAsync(employee);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,HR Manager")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }

        [HttpPut("deactivate/{id}")]
        [Authorize(Roles = "Administrator,HR Manager")]
        public async Task<IActionResult> DeactivateEmployee(int id, [FromBody] string reason)
        {
            var result = await _employeeService.DeactivateEmployeeAsync(id, reason);

            if (result == "Employee deactivated successfully")
            {
                return Ok(result);
            }
            else if (result == "Employee not found")
            {
                return NotFound(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
