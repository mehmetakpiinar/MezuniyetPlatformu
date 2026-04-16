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
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public MessagesController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto createDto)
        {
            var senderIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderIdString))
            {
                return Unauthorized("Token'da gönderen ID'si bulunamadı.");
            }
            var senderId = int.Parse(senderIdString);

            if (senderId == createDto.RecipientUserId)
            {
                return BadRequest("Kullanıcılar kendilerine mesaj gönderemez.");
            }

            var recipientExists = await _context.Users.AnyAsync(u => u.UserId == createDto.RecipientUserId);
            if (!recipientExists)
            {
                return NotFound("Mesaj gönderilmek istenen alıcı bulunamadı.");
            }

            var newMessage = new Message
            {
                SenderUserId = senderId,
                RecipientUserId = createDto.RecipientUserId,
                Content = createDto.Content,
                SentDate = DateTime.UtcNow,
                IsRead = false,
                ReadDate = null
            };

            try
            {
                await _context.Messages.AddAsync(newMessage);
                await _context.SaveChangesAsync();
                return Ok(newMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdString))
            {
                return Unauthorized();
            }
            var currentUserId = int.Parse(currentUserIdString);

            try
            {
                var unreadMessages = await _context.Messages
                    .Where(m => m.SenderUserId == otherUserId &&
                                m.RecipientUserId == currentUserId &&
                                m.IsRead == false)
                    .ToListAsync();

                if (unreadMessages.Any())
                {
                    foreach (var message in unreadMessages)
                    {
                        message.IsRead = true;
                        message.ReadDate = DateTime.UtcNow;
                    }
                    _context.Messages.UpdateRange(unreadMessages);
                    await _context.SaveChangesAsync();
                }


                var conversation = await _context.Messages
                    .Where(m => (m.SenderUserId == currentUserId && m.RecipientUserId == otherUserId) ||
                                (m.SenderUserId == otherUserId && m.RecipientUserId == currentUserId))
                    .OrderBy(m => m.SentDate)
                    .ToListAsync();

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdString)) return Unauthorized();
            var currentUserId = int.Parse(currentUserIdString);

            var allMessages = await _context.Messages
                .Include(m => m.SenderUser).ThenInclude(u => u.AlumniProfile)
                .Include(m => m.SenderUser).ThenInclude(u => u.EmployerProfile).ThenInclude(ep => ep.Company)

                .Include(m => m.RecipientUser).ThenInclude(u => u.AlumniProfile)
                .Include(m => m.RecipientUser).ThenInclude(u => u.EmployerProfile).ThenInclude(ep => ep.Company)

                .Where(m => m.SenderUserId == currentUserId || m.RecipientUserId == currentUserId)
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            var contacts = allMessages
                .GroupBy(m => m.SenderUserId == currentUserId ? m.RecipientUserId : m.SenderUserId)
                .Select(g =>
                {
                    var lastMsg = g.First();

                    var otherUser = (lastMsg.SenderUserId == currentUserId)
                                    ? lastMsg.RecipientUser
                                    : lastMsg.SenderUser;

                    string? photoUrl = null;

                    if (otherUser.AlumniProfile != null)
                    {
                        photoUrl = otherUser.AlumniProfile.ProfilePhotoURL;
                    }
                    else if (otherUser.EmployerProfile != null && otherUser.EmployerProfile.Company != null)
                    {
                        photoUrl = otherUser.EmployerProfile.Company.LogoURL;
                    }

                    var unreadCount = g.Count(m => m.RecipientUserId == currentUserId && !m.IsRead);

                    return new ConversationDto
                    {
                        UserId = otherUser.UserId,
                        FullName = $"{otherUser.FirstName} {otherUser.LastName}",
                        ProfilePhotoUrl = photoUrl,
                        LastMessageContent = lastMsg.Content,
                        LastMessageDate = lastMsg.SentDate,
                        UnreadCount = unreadCount
                    };
                })
                .ToList();

            return Ok(contacts);
        }
    }
}