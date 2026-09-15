using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Application.Interfaces.Services;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.Persistence.Services.Chat
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _repo;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(IConversationRepository repo, ILogger<ConversationService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<Conversation>> GetAllAsync(string? userId = null, bool isAdmin = false, CancellationToken ct = default)
        {
            try
            {
                return await _repo.GetAllAsync(userId, isAdmin, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all conversations");
                throw;
            }
        }

        public async Task<Conversation?> GetByIdAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken ct = default)
        {
            try
            {
                return await _repo.GetByIdAsync(id, userId, isAdmin, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching conversation {ConversationId}", id);
                throw;
            }
        }

        public async Task<Conversation> UpsertAsync(Conversation conv, string? userId = null, bool isAdmin = false, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(conv.Id))
                    conv.Id = Guid.NewGuid().ToString();

                if (!string.IsNullOrWhiteSpace(userId))
                    conv.UserId = userId;

                var existing = await _repo.GetByIdAsync(conv.Id, userId, isAdmin, ct);
                conv.UpdatedAt = DateTime.UtcNow;
                if (existing == null)
                {
                    conv.CreatedAt = DateTime.UtcNow;
                    conv.UserId = string.IsNullOrWhiteSpace(userId) ? conv.UserId : userId;
                    await _repo.InsertAsync(conv, ct);
                }
                else
                {
                    await _repo.UpdateAsync(conv, userId, isAdmin, ct);
                }
                return conv;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting conversation {ConversationId}", conv?.Id);
                throw;
            }
        }

        public async Task DeleteAsync(string id, string? userId = null, bool isAdmin = false, CancellationToken ct = default)
        {
            try
            {
                await _repo.DeleteAsync(id, userId, isAdmin, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
                throw;
            }
        }
    }
}