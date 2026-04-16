using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using Microsoft.AspNetCore.Authorization;
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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            if (!string.IsNullOrEmpty(userIdString)) int.TryParse(userIdString, out currentUserId);

            var posts = await _context.ExperiencePosts
                .Include(p => p.AuthorUser)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    p.PostId,
                    p.Title,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedDate,
                    AuthorUser = new
                    {
                        p.AuthorUser.FirstName,
                        p.AuthorUser.LastName
                    },
                    LikeCount = _context.ExperiencePostLikes.Count(l => l.PostId == p.PostId),
                    IsLikedByCurrentUser = _context.ExperiencePostLikes.Any(l => l.PostId == p.PostId && l.UserId == currentUserId),

                    Comments = _context.ExperiencePostComments
                                       .Where(c => c.PostId == p.PostId)
                                       .Select(c => new { c.CommentId })
                                       .ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            if (!string.IsNullOrEmpty(userIdString)) int.TryParse(userIdString, out currentUserId);

            var post = await _context.ExperiencePosts
                .Include(p => p.AuthorUser)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound("Paylaşım bulunamadı.");

            var comments = await _context.ExperiencePostComments
                .Include(c => c.User)
                .Where(c => c.PostId == id)
                .OrderBy(c => c.CommentDate)
                .Select(c => new
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    CommentDate = c.CommentDate,
                    UserName = c.User.FirstName + " " + c.User.LastName
                })
                .ToListAsync();

            var result = new
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedDate = post.CreatedDate,
                AuthorUser = new
                {
                    FirstName = post.AuthorUser.FirstName,
                    LastName = post.AuthorUser.LastName
                },
                LikeCount = await _context.ExperiencePostLikes.CountAsync(l => l.PostId == id),
                IsLikedByCurrentUser = await _context.ExperiencePostLikes.AnyAsync(l => l.PostId == id && l.UserId == currentUserId),
                Comments = comments
            };

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Ogrenci, Mezun, Isveren")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var authorUserId = int.Parse(userIdString);

            var newPost = new ExperiencePost
            {
                AuthorUserId = authorUserId,
                Title = createDto.Title,
                Content = createDto.Content,
                ImageUrl = createDto.ImageUrl,
                CreatedDate = DateTime.UtcNow
            };

            await _context.ExperiencePosts.AddAsync(newPost);
            await _context.SaveChangesAsync();

            return Ok(newPost);
        }

        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var existingLike = await _context.ExperiencePostLikes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (existingLike != null)
            {
                _context.ExperiencePostLikes.Remove(existingLike);
            }
            else
            {
                await _context.ExperiencePostLikes.AddAsync(new ExperiencePostLike { PostId = id, UserId = userId, LikedDate = DateTime.UtcNow });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPost("{id}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentDto commentDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            if (commentDto == null || string.IsNullOrWhiteSpace(commentDto.CommentText))
            {
                return BadRequest("Yorum metni boş olamaz.");
            }

            var newComment = new ExperiencePostComment
            {
                PostId = id,
                UserId = userId,
                CommentText = commentDto.CommentText,
                CommentDate = DateTime.UtcNow
            };

            try
            {
                await _context.ExperiencePostComments.AddAsync(newComment);
                await _context.SaveChangesAsync();
                return Ok(newComment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Veritabanı hatası: " + ex.Message);
            }
        }
    }
}