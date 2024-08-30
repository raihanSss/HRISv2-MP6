using HRIS.Domain.Dtos;
using HRIS.Domain.Interfaces;
using HRIS.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using HRIS.Infrastructure;


namespace HRIS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, IEmployeeRepository employeeRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
        }



        public async Task<ResponeModel> RegisterAsync(RegisterEmployeeModel model)
        {
 
            var employee = new Employees
            {
                NameEmp = model.NameEmp,
                Ssn = model.Ssn,
                Dob = model.Dob,
                Address = model.Address,
                Sex = model.Sex,
                Email = model.Email,
                Phone = model.Phone,
                JobPosition = model.JobPosition,
                Level = model.Level,
                Type = model.Type,
                Status = model.Status,
                Reason = model.Reason,
                Lastupdate = DateTime.UtcNow
            };

            try
            {
                
                await _employeeRepository.AddEmployeeAsync(employee);

                
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmployeeId = employee.IdEmp 
                };

               
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

                    if (roleResult.Succeeded)
                    {
                        return new ResponeModel
                        {
                            Status = "Success",
                            Message = "User registered successfully."
                        };
                    }
                    else
                    {
                        
                        await _userManager.DeleteAsync(user);
                        await _employeeRepository.DeleteEmployeeAsync(employee.IdEmp);

                        return new ResponeModel
                        {
                            Status = "Error",
                            Message = "User registered but failed to assign role: " + string.Join(", ", roleResult.Errors.Select(e => e.Description))
                        };
                    }
                }

                
                await _employeeRepository.DeleteEmployeeAsync(employee.IdEmp);

                return new ResponeModel
                {
                    Status = "Error",
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            catch (Exception ex)
            {
               
                if (employee.IdEmp != 0)
                {
                    await _employeeRepository.DeleteEmployeeAsync(employee.IdEmp);
                }

                return new ResponeModel
                {
                    Status = "Error",
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }


        public async Task<LoginResponseDto> LoginAsync(LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return new LoginResponseDto { Status = "Error", Message = "Invalid login attempt." };
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return new LoginResponseDto { Status = "Error", Message = "Invalid username or password." };
            }

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    expires: DateTime.Now.AddMinutes(10),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                string refreshToken;
                if (user.RefreshToken != null && user.RefreshTokenExpiryTime > DateTime.UtcNow)
                {

                    refreshToken = user.RefreshToken;
                }
                else
                {

                    refreshToken = await GenerateRefreshTokenAsync(user);
                }

                return new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    ExpiredOn = token.ValidTo,
                    Status = "Success",
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = userRoles.FirstOrDefault()
                };
            }

            return new LoginResponseDto { Status = "Error", Message = "Invalid username or password." };
        }

        public async Task<AuthRespone> CreateRoleAsync(string rolename)
        {
            if (!await _roleManager.RoleExistsAsync(rolename))
            {
                await _roleManager.CreateAsync(new IdentityRole(rolename));
            }
            return new AuthRespone { Status = "Success", Message = "Role created successfully!" };
        }

        private async Task<string> GenerateRefreshTokenAsync(AppUser user)
        {
            var newRefreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(2);
            await _userManager.UpdateAsync(user);
            return newRefreshToken;
        }

        public async Task<AuthRespone> LogoutAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);
            }
            return new AuthRespone
            {
                Message = "Success Logout",
                Status = "Success",
            };
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestModel model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return new RefreshTokenResponseDto { Status = "Error", Message = "Invalid refresh token." };
            }

            var user = await _userManager.Users
                .SingleOrDefaultAsync(u => u.RefreshToken == model.RefreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);

            if (user == null)
            {
                return new RefreshTokenResponseDto { Status = "Error", Message = "Invalid or expired refresh token." };
            }

            // Generate akses token baru
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));

            var newToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(10),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Optionally generate a new refresh token
            string newRefreshToken = await GenerateRefreshTokenAsync(user);

            return new RefreshTokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newToken),
                RefreshToken = newRefreshToken,
                ExpiredOn = newToken.ValidTo,
                Status = "Success"
            };
        }


        public async Task<ResponeModel> UpdateUserAsync(string userId, UpdateUserModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ResponeModel
                {
                    Status = "Error",
                    Message = "User not found."
                };
            }

            user.UserName = model.Username ?? user.UserName;
            user.Email = model.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new ResponeModel
                {
                    Status = "Success",
                    Message = "User updated successfully."
                };
            }

            return new ResponeModel
            {
                Status = "Error",
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<ResponeModel> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ResponeModel
                {
                    Status = "Error",
                    Message = "User not found."
                };
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return new ResponeModel
                {
                    Status = "Success",
                    Message = "User deleted successfully."
                };
            }

            return new ResponeModel
            {
                Status = "Error",
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }


    }
}
