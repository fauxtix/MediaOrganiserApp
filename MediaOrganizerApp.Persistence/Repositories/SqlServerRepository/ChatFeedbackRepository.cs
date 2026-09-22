using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.DTOs.AI;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    public class ChatFeedbackRepository : IChatFeedbackRepository
    {
        private readonly IDapperContext _context;

        public ChatFeedbackRepository(IDapperContext context)
        {
            _context = context;
        }

        public async Task UpsertAsync(ChatFeedbackDto feedback, CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@UserID", feedback.UserId ?? string.Empty);
            parameters.Add("@Interface", feedback.Interface);
            parameters.Add("@ConversationId", feedback.ConversationId);
            parameters.Add("@MessageIndex", feedback.MessageIndex);
            parameters.Add("@PromptText", feedback.PromptText);
            parameters.Add("@ResponseText", feedback.ResponseText);
            parameters.Add("@ModelName", feedback.ModelName);
            parameters.Add("@Rating", feedback.Rating);

            await connection.ExecuteAsync(
                "usp_ChatFeedback_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<ChatFeedbackDto>> GetByConversationAsync(
            string userId,
            string @interface,
            string? conversationId,
            CancellationToken ct = default)
        {
            using var connection = _context.CreateConnection();

            var rows = await connection.QueryAsync<ChatFeedbackDto>(
                "usp_ChatFeedback_GetByConversation",
                new { UserID = userId, Interface = @interface, ConversationId = conversationId },
                commandType: CommandType.StoredProcedure);

            return rows?.ToList() ?? new List<ChatFeedbackDto>();
        }
    }
}