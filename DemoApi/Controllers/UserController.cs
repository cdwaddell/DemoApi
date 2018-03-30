using System.Linq;
using System.Threading.Tasks;
using DemoApi.Data;
using DemoApi.Models;
using DemoApi.Models.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoApi.Controllers
{
    [Route("Users")]
    //[ApiVersion(ApiConstants.V10)]
    //[ApiVersion(ApiConstants.V11Beta)]
    public class UserController:Controller
    {
        private readonly DemoAppDbContext _context;

        public UserController(DemoAppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the details about a user
        /// </summary>
        /// <param name="sub">The sub of the user requested</param>
        /// <returns>The user details</returns>
        [HttpGet("{sub}")]
        [Produces(typeof(UserViewModel))]
        public async Task<IActionResult> GetUser([FromRoute] string sub)
        {
            var user = await _context.UserProfiles.SingleOrDefaultAsync(u => u.Sub == sub);
            if (user == null)
                return NotFound();

            return Ok(new UserViewModel
            {
                Name = user.DisplayName,
                RouteValues = new { sub = user.Sub }
            });
        }

        /// <summary>
        /// Update your user's details
        /// </summary>
        /// <param name="sub">The sub of the user being updated</param>
        /// <param name="model">The new details</param>
        /// <returns></returns>
        [HttpPost("{sub}")]
        [Produces(typeof(UserViewModel))]
        public async Task<IActionResult> UpdateUser([FromRoute] string sub, UserUpdateViewmodel model)
        {
            if (sub != User.FindFirst("sub").Value)
                return Forbid();

            var user = await _context.UserProfiles.SingleOrDefaultAsync(u => u.Sub == sub);
            if (user == null)
                return NotFound();

            user.DisplayName = model.Name;

            await _context.SaveChangesAsync();

            return Ok(new UserViewModel
            {
                Name = user.DisplayName,
                RouteValues = new { sub = user.Sub }
            });
        }
    }
}
