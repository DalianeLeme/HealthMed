﻿using HealthMed.Auth.Application.Models;
using HealthMed.Auth.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthMed.Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var success = await _authService.RegisterUserAsync(
                request.Name, request.Email, request.Password, request.Role, request.CRM, request.CPF
            );

            if (!success) return BadRequest("Usuário já existe.");
            return Ok("Usuário registrado com sucesso.");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _authService.AuthenticateUserAsync(request.Identifier, request.Password);
            if (user == null) return Unauthorized("Credenciais inválidas.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.CRM,
                    user.CPF
                }
            });
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var result = await _authService.GetAllDoctorsAsync();
            return Ok(result);
        }

    }
}
