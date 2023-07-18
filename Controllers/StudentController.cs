using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Test.Data;
using Test.DTOs;
using Test.Models;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private string secretkey;

        public StudentController(DataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            this.secretkey = configuration.GetValue<string>("Jwt:Key");
        }

        [HttpGet("{page}")]
        public async Task<ActionResult<List<Student>>> GetStudents(int page)
        {
            var pageResults = 3f;
            var pageCount = Math.Ceiling(_dataContext.Students.Count() / pageResults);
            var students = await _dataContext.Students.Skip((page - 1) * (int)pageResults).Take((int)pageResults).ToListAsync();

            var response = new ResponseDTO
            {
                Students = students,
                CurrentPage = page,
                Page = (int)pageCount
            };
            return Ok(response);
        }

        [HttpGet("GetItems")]
        public async Task<ActionResult<IEnumerable<Student>>> Get()
        {
            try
            {
                var items = await _dataContext.Students.ToListAsync();
                if (items == null)
                {
                    return BadRequest();
                }
                return Ok(items);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetItemsById")]
        public async Task<ActionResult<Student>> Get(int studid)
        {
            try
            {
                var items = _dataContext.Students.FirstOrDefault(e => e.StudentId == studid);
                if (items == null)
                {
                    throw new Exception("Invalid entry");
                }
                return items;
            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
        }

        [HttpGet("GetItemsByName")]
        public async Task<ActionResult<Student>> Get(string studname)
        {
            try
            {
                var items = _dataContext.Students.FirstOrDefault(e => e.Name == studname);
                if (items == null)
                {
                    throw new Exception("Invalid entry");
                }
                return items;
            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
        }

        [HttpPost("PostItems")]
        public async Task<ActionResult<Student>> Post(Student item)
        {
            try
            {
                if (item == null)
                {
                    throw new Exception("Invalid entry");
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = Encoding.ASCII.GetBytes(secretkey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, item.Name),


                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new(new SymmetricSecurityKey(token), SecurityAlgorithms.HmacSha256)
                };
                var jwttoken = tokenHandler.CreateToken(tokenDescriptor);
                _dataContext.Students.Add(item);
                await _dataContext.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), item , new JwtSecurityTokenHandler().WriteToken(jwttoken));
            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
        }

        [HttpPut("PutItems")]
        public async Task<ActionResult<Student>> Put(int Id, Student item)
        {
            try
            {
                if (item == null)
                {
                    throw new Exception("Invalid entry");
                }
                var itm = _dataContext.Students.FirstOrDefault(x => x.StudentId == Id);
                if (itm == null)
                {
                    throw new Exception("Not Found");
                }
                itm.Name = item.Name;
                itm.Age = item.Age;
                itm.DOB = item.DOB;
                itm.Course = item.Course;
                await _dataContext.SaveChangesAsync();
                return itm;

            }
            catch (Exception ex)
            {
                throw;
                return null;
            }
        }

        [Authorize]
        [HttpDelete("DeleteItems")]
        public async Task<bool> Delete(int Id)
        {
            try
            {
                var itm = _dataContext.Students.FirstOrDefault(x => x.StudentId == Id);
                if (itm == null)
                {
                    throw new Exception("Not Found");
                }
                _dataContext.Students.Remove(itm);
                await _dataContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
                return false;
            }
        }
    }
}
