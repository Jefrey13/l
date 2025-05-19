//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using CustomerService.API.Dtos.ResponseDtos;
//using CustomerService.API.Repositories.Interfaces;
//using CustomerService.API.Utils;
//using CustomerService.API.Utils.Enums;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace CustomerService.API.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class DashboardController : ControllerBase
//    {
//        private readonly IUnitOfWork _uow;

//        public DashboardController(IUnitOfWork uow)
//            => _uow = uow;

//        [HttpGet("ActiveConversations")]
//        public async Task<ActionResult<ApiResponse<int>>> GetActiveConversationsCount()
//        {
//            var count = await _uow.Conversations
//                .GetAll()
//                .CountAsync(c => c.Status != ConversationStatus.Closed);

//            return Ok(new ApiResponse<int>(count));
//        }

//        [HttpGet("NewConversations")]
//        public async Task<ActionResult<ApiResponse<int>>> GetNewConversationsCount(
//            [FromQuery] DateTime? date = null)
//        {
//            var target = date?.Date ?? DateTime.UtcNow.Date;
//            var count = await _uow.Conversations
//                .GetAll()
//                .CountAsync(c => c.CreatedAt.Date == target);

//            return Ok(new ApiResponse<int>(count));
//        }

//        [HttpGet("OnlineAgentsCount")]
//        public async Task<ActionResult<ApiResponse<int>>> GetOnlineAgentsCount()
//        {
//            var threshold = DateTime.UtcNow.AddMinutes(-5);
//            var count = await _uow.Users
//                .GetAll()
//                .Where(u => u.IsActive
//                            && u.UserRoles.Any(ur => ur.Role.RoleName == "Agent")
//                            && u.UpdatedAt >= threshold)
//                .CountAsync();

//            return Ok(new ApiResponse<int>(count));
//        }

//        [HttpGet("ConversationsPerDay")]
//        public async Task<ActionResult<ApiResponse<IEnumerable<TemporalCountDto>>>> GetConversationsPerDay(
//            [FromQuery] DateTime from,
//            [FromQuery] DateTime to)
//        {
//            var list = await _uow.Conversations
//                .GetAll()
//                .Where(c => c.CreatedAt.Date >= from.Date
//                         && c.CreatedAt.Date <= to.Date)
//                .GroupBy(c => c.CreatedAt.Date)
//                .Select(g => new TemporalCountDto { Date = g.Key, Count = g.Count() })
//                .OrderBy(x => x.Date)
//                .ToListAsync();

//            return Ok(new ApiResponse<IEnumerable<TemporalCountDto>>(list));
//        }

//        [HttpGet("MessagesSentReceived")]
//        public async Task<ActionResult<ApiResponse<MessagesSentReceivedDto>>> GetMessagesSentReceived(
//            [FromQuery] DateTime from,
//            [FromQuery] DateTime to)
//        {
//            var baseQ = _uow.Messages
//                .GetAll()
//                .Where(m => m.ReceivedAt.Date >= from.Date
//                         && m.ReceivedAt.Date <= to.Date);

//            var sent = await baseQ
//                .Where(m => m.SenderUserId != null)
//                .GroupBy(m => m.ReceivedAt.Date)
//                .Select(g => new TemporalCountDto { Date = g.Key, Count = g.Count() })
//                .OrderBy(x => x.Date)
//                .ToListAsync();

//            var received = await baseQ
//                .Where(m => m.SenderContactId != null)
//                .GroupBy(m => m.ReceivedAt.Date)
//                .Select(g => new TemporalCountDto { Date = g.Key, Count = g.Count() })
//                .OrderBy(x => x.Date)
//                .ToListAsync();

//            var dto = new MessagesSentReceivedDto
//            {
//                Sent = sent,
//                Received = received
//            };

//            return Ok(new ApiResponse<MessagesSentReceivedDto>(dto));
//        }

//        [HttpGet("ActiveUsersPerDay")]
//        public async Task<ActionResult<ApiResponse<IEnumerable<TemporalCountDto>>>> GetActiveUsersPerDay(
//            [FromQuery] DateTime from,
//            [FromQuery] DateTime to)
//        {
//            var msgQ = _uow.Messages
//                .GetAll()
//                .Where(m => m.ReceivedAt.Date >= from.Date
//                         && m.ReceivedAt.Date <= to.Date);

//            var list = await msgQ
//                .GroupBy(m => m.ReceivedAt.Date)
//                .Select(g => new TemporalCountDto
//                {
//                    Date = g.Key,
//                    Count = g.Select(m => m.SenderUserId ?? m.SenderContactId)
//                             .Distinct()
//                             .Count()
//                })
//                .OrderBy(x => x.Date)
//                .ToListAsync();

//            return Ok(new ApiResponse<IEnumerable<TemporalCountDto>>(list));
//        }
//    }
//}