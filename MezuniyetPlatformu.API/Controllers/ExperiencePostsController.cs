using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExperiencePostsController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;
        public ExperiencePostsController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }
 
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _context.ExperiencePosts
                    .Include(p => p.AuthorUser)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById(int id)
        {
            try
            {
                var post = await _context.ExperiencePosts
                    .Include(p => p.AuthorUser)
                    .FirstOrDefaultAsync(p => p.PostId == id);

                if (post == null)
                {
                    return NotFound("Paylaşım bulunamadı.");
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Ogrenci, Mezun")] 
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createDto)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized("Token'da kullanıcı ID'si bulunamadı.");
            }
            var authorUserId = int.Parse(kullaniciIdString);

            var newPost = new ExperiencePost
            {
                AuthorUserId = authorUserId,
                Title = createDto.Title,
                Content = createDto.Content,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = null
            };

            try
            {
                await _context.ExperiencePosts.AddAsync(newPost);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPostById), new { id = newPost.PostId }, newPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto updateDto)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized(); 
            }
            var authorUserId = int.Parse(kullaniciIdString);
            var post = await _context.ExperiencePosts.FindAsync(id);

            if (post == null)
            {
                return NotFound("Güncellenecek paylaşım bulunamadı.");
            }

            if (post.AuthorUserId != authorUserId)
            {
                return Forbid("Bu paylaşımı güncelleme yetkiniz yok.");
            }

            post.Title = updateDto.Title;
            post.Content = updateDto.Content;
            post.UpdatedDate = DateTime.UtcNow;

            try
            {
                _context.ExperiencePosts.Update(post);
                await _context.SaveChangesAsync();

                return Ok(post);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized();
            }
            var authorUserId = int.Parse(kullaniciIdString);
            var post = await _context.ExperiencePosts.FindAsync(id);

            if (post == null)
            {
                return NotFound("Silinecek paylaşım bulunamadı.");
            }

            if (post.AuthorUserId != authorUserId)
            {
                return Forbid("Bu paylaşımı silme yetkiniz yok.");
            }

            try
            {
                _context.ExperiencePosts.Remove(post);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}