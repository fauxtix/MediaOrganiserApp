# Feature: [FEATURE_NAME]

> **Architecture**: Clean Architecture + DDD  
> **Backend**: ASP.NET Core Web API (fauxtix/MediaOrganiserApp.Backend)  
> **Frontend**: Blazor WebAssembly (fauxtix/MediaOrganiserApp.Web.Blazor)  
> **Data Access**: Dapper + SQL Server  
> **Pattern**: Repository Pattern with Dependency Injection

---

## Table of Contents
1. [Feature Overview](#feature-overview)
2. [Backend Implementation](#backend-implementation)
   - [Domain Layer](#1-domain-layer)
   - [Application Layer](#2-application-layer)
   - [Persistence Layer](#3-persistence-layer)
   - [WebAPI Layer](#4-webapi-layer)
3. [Frontend Implementation](#frontend-implementation)
   - [Services](#1-services)
   - [Blazor Components](#2-blazor-components)
   - [Models/DTOs](#3-models-dtos)
4. [Database](#database-implementation)
5. [Testing](#testing-implementation)
6. [Localization](#localization)
7. [Implementation Checklist](#implementation-checklist)
8. [Success Metrics](#success-metrics)

---

## Feature Overview

### Business Requirements
- **Primary Goal**: [Describe the main purpose]
- **Target Users**: [Who will use this feature]
- **Business Value**: [Why this feature matters]

### Functional Requirements
1. [Requirement 1]
2. [Requirement 2]
3. [Requirement 3]

### Technical Requirements
- **Performance**: [Expected response times, load requirements]
- **Security**: [Authentication, authorization requirements]
- **Scalability**: [Expected growth, concurrent users]

---

## Backend Implementation

### 1. Domain Layer
**Location**: `MediaOrganiserApp.Backend/Domain/Entities/`

#### 1.1 Domain Entity

```csharp name=Domain/Entities/[FeatureName].cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// Represents a [FeatureName] entity in the domain
    /// </summary>
    public class [FeatureName]
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// [Property description]
        /// </summary>
        [Required(ErrorMessage = "Field is required")]
        [StringLength(200, ErrorMessage = "Maximum length is 200 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// [Property description]
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// User who created the record
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Record creation timestamp
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Record last update timestamp
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User who created the record
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// User who last modified the record
        /// </summary>
        public string LastModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
```

#### 1.2 View Model (for complex queries)

```csharp name=Domain/Entities/[FeatureName]ViewModel.cs
namespace MediaOrganizerApp.Domain.Entities
{
    /// <summary>
    /// View model for [FeatureName] with related data
    /// </summary>
    public class [FeatureName]ViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Related entity properties
        public int RelatedEntityId { get; set; }
        public string RelatedEntityName { get; set; } = string.Empty;
        
        // Computed properties
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
```

---

### 2. Application Layer
**Location**: `MediaOrganiserApp.Backend/Application/`

#### 2.1 DTOs (Data Transfer Objects)

```csharp name=Application/DTOs/[FeatureName]DTO.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganizerApp.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for [FeatureName]
    /// Used for API communication between backend and frontend
    /// </summary>
    public class [FeatureName]DTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string LastModifiedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating new [FeatureName]
    /// </summary>
    public class Create[FeatureName]DTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }

    /// <summary>
    /// DTO for updating existing [FeatureName]
    /// </summary>
    public class Update[FeatureName]DTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string LastModifiedBy { get; set; } = string.Empty;
    }
}
```

#### 2.2 AutoMapper Profile

```csharp name=Application/Mappings/[FeatureName]MappingProfile.cs
using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Mappings
{
    public class [FeatureName]MappingProfile : Profile
    {
        public [FeatureName]MappingProfile()
        {
            // Entity <-> DTO mappings
            CreateMap<[FeatureName], [FeatureName]DTO>().ReverseMap();
            CreateMap<[FeatureName]ViewModel, [FeatureName]DTO>().ReverseMap();
            
            // Create DTO -> Entity
            CreateMap<Create[FeatureName]DTO, [FeatureName]>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Update DTO -> Entity
            CreateMap<Update[FeatureName]DTO, [FeatureName]>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
```

#### 2.3 Repository Interface

```csharp name=Application/Interfaces/Repositories/I[FeatureName]Repository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MediaOrganizerApp.Domain.Entities;

namespace MediaOrganizerApp.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for [FeatureName] data access
    /// </summary>
    public interface I[FeatureName]Repository : IBaseRepository<[FeatureName]>
    {
        // Custom query methods specific to this feature
        
        /// <summary>
        /// Gets all [FeatureName] items for a specific user
        /// </summary>
        Task<IEnumerable<[FeatureName]>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Gets [FeatureName] with related data
        /// </summary>
        Task<[FeatureName]ViewModel> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// Gets all [FeatureName] items with related data
        /// </summary>
        Task<IEnumerable<[FeatureName]ViewModel>> GetAllWithDetailsAsync();

        /// <summary>
        /// Searches [FeatureName] by name
        /// </summary>
        Task<IEnumerable<[FeatureName]>> SearchByNameAsync(string searchTerm);

        /// <summary>
        /// Checks if [FeatureName] exists by name
        /// </summary>
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);

        /// <summary>
        /// Gets statistics for [FeatureName]
        /// </summary>
        Task<Dictionary<string, int>> GetStatisticsAsync(int userId);

        /// <summary>
        /// Soft deletes a [FeatureName] record
        /// </summary>
        Task<bool> SoftDeleteAsync(int id);

        /// <summary>
        /// Restores a soft-deleted [FeatureName] record
        /// </summary>
        Task<bool> RestoreAsync(int id);
    }
}
```

---

### 3. Persistence Layer
**Location**: `MediaOrganiserApp.Backend/MediaOrganizerApp.Persistence/`

#### 3.1 Repository Implementation (SQL Server with Dapper)

```csharp name=MediaOrganizerApp.Persistence/Repositories/SqlServerRepository/[FeatureName]Repository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MediaOrganizerApp.Application.Interfaces;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.Persistence.Repositories.SqlServerRepository
{
    /// <summary>
    /// SQL Server implementation of [FeatureName] repository using Dapper
    /// </summary>
    public class [FeatureName]Repository : SQLServerBaseRepository<[FeatureName]>, I[FeatureName]Repository
    {
        private readonly IDapperContext _context;
        private readonly ILogger<[FeatureName]Repository> _logger;

        public [FeatureName]Repository(IDapperContext context, ILogger<[FeatureName]Repository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations

        /// <summary>
        /// Gets all [FeatureName] items for a specific user
        /// </summary>
        public async Task<IEnumerable<[FeatureName]>> GetByUserIdAsync(int userId)
        {
            var sql = @"
                SELECT 
                    Id, Name, Description, UserId, 
                    CreatedAt, UpdatedAt, CreatedBy, LastModifiedBy, IsDeleted
                FROM [FeatureName]
                WHERE UserId = @UserId AND IsDeleted = 0
                ORDER BY CreatedAt DESC";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<[FeatureName]>(sql, new { UserId = userId });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting [FeatureName] for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Gets [FeatureName] with related data using SQL JOIN
        /// </summary>
        public async Task<[FeatureName]ViewModel> GetByIdWithDetailsAsync(int id)
        {
            var sql = @"
                SELECT 
                    f.Id, f.Name, f.Description,
                    f.CreatedAt, f.UpdatedAt,
                    r.Id AS RelatedEntityId,
                    r.Name AS RelatedEntityName,
                    COUNT(i.Id) AS ItemCount
                FROM [FeatureName] f
                LEFT JOIN RelatedEntity r ON f.RelatedEntityId = r.Id
                LEFT JOIN Items i ON i.[FeatureName]Id = f.Id
                WHERE f.Id = @Id AND f.IsDeleted = 0
                GROUP BY f.Id, f.Name, f.Description, f.CreatedAt, f.UpdatedAt, 
                         r.Id, r.Name";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<[FeatureName]ViewModel>(
                        sql, 
                        new { Id = id });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting [FeatureName] details for id {id}");
                throw;
            }
        }

        /// <summary>
        /// Gets all [FeatureName] items with related data
        /// </summary>
        public async Task<IEnumerable<[FeatureName]ViewModel>> GetAllWithDetailsAsync()
        {
            var sql = @"
                SELECT 
                    f.Id, f.Name, f.Description,
                    f.CreatedAt, f.UpdatedAt,
                    r.Id AS RelatedEntityId,
                    r.Name AS RelatedEntityName,
                    COUNT(i.Id) AS ItemCount
                FROM [FeatureName] f
                LEFT JOIN RelatedEntity r ON f.RelatedEntityId = r.Id
                LEFT JOIN Items i ON i.[FeatureName]Id = f.Id
                WHERE f.IsDeleted = 0
                GROUP BY f.Id, f.Name, f.Description, f.CreatedAt, f.UpdatedAt, 
                         r.Id, r.Name
                ORDER BY f.CreatedAt DESC";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<[FeatureName]ViewModel>(sql);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all [FeatureName] with details");
                throw;
            }
        }

        /// <summary>
        /// Searches [FeatureName] by name using LIKE
        /// </summary>
        public async Task<IEnumerable<[FeatureName]>> SearchByNameAsync(string searchTerm)
        {
            var sql = @"
                SELECT 
                    Id, Name, Description, UserId, 
                    CreatedAt, UpdatedAt, CreatedBy, LastModifiedBy, IsDeleted
                FROM [FeatureName]
                WHERE Name LIKE @SearchTerm AND IsDeleted = 0
                ORDER BY Name";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryAsync<[FeatureName]>(
                        sql, 
                        new { SearchTerm = $"%{searchTerm}%" });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching [FeatureName] with term '{searchTerm}'");
                throw;
            }
        }

        /// <summary>
        /// Checks if [FeatureName] exists by name (for duplicate validation)
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM [FeatureName]
                    WHERE Name = @Name 
                    AND (@ExcludeId IS NULL OR Id != @ExcludeId)
                    AND IsDeleted = 0
                ) THEN 1 ELSE 0 END AS BIT)";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.ExecuteScalarAsync<bool>(
                        sql, 
                        new { Name = name, ExcludeId = excludeId });
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if [FeatureName] exists with name '{name}'");
                throw;
            }
        }

        /// <summary>
        /// Gets statistics for [FeatureName]
        /// </summary>
        public async Task<Dictionary<string, int>> GetStatisticsAsync(int userId)
        {
            var sql = @"
                SELECT 
                    COUNT(*) AS TotalCount,
                    COUNT(CASE WHEN CreatedAt >= DATEADD(day, -30, GETDATE()) THEN 1 END) AS Last30Days,
                    COUNT(CASE WHEN CreatedAt >= DATEADD(day, -7, GETDATE()) THEN 1 END) AS Last7Days
                FROM [FeatureName]
                WHERE UserId = @UserId AND IsDeleted = 0";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync(sql, new { UserId = userId });
                    
                    return new Dictionary<string, int>
                    {
                        { "TotalCount", result.TotalCount },
                        { "Last30Days", result.Last30Days },
                        { "Last7Days", result.Last7Days }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting statistics for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Soft deletes a [FeatureName] record
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var sql = @"
                UPDATE [FeatureName]
                SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting [FeatureName] with id {id}");
                throw;
            }
        }

        /// <summary>
        /// Restores a soft-deleted [FeatureName] record
        /// </summary>
        public async Task<bool> RestoreAsync(int id)
        {
            var sql = @"
                UPDATE [FeatureName]
                SET IsDeleted = 0, UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            try
            {
                using (var connection = _context.CreateConnection())
                {
                    var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring [FeatureName] with id {id}");
                throw;
            }
        }

        #endregion
    }
}
```

#### 3.2 Service Registration

```csharp name=MediaOrganizerApp.Persistence/ServiceRegistration.cs
// Add to existing ServiceRegistration.cs

namespace MediaOrganizerApp.Persistence
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services)
        {
            // ... existing registrations ...
            
            // Add [FeatureName] repository
            services.AddScoped<I[FeatureName]Repository, [FeatureName]Repository>();
            
            // ... rest of registrations ...
        }
    }
}
```

---

### 4. WebAPI Layer
**Location**: `MediaOrganiserApp.Backend/MediaOrganizerApp.WebApi/`

#### 4.1 API Controller

```csharp name=MediaOrganizerApp.WebApi/Controllers/[FeatureName]Controller.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MediaOrganizerApp.WebApi.Controllers
{
    /// <summary>
    /// API Controller for [FeatureName] management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class [FeatureName]Controller : ControllerBase
    {
        private readonly I[FeatureName]Repository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<[FeatureName]Controller> _logger;

        public [FeatureName]Controller(
            I[FeatureName]Repository repository,
            IMapper mapper,
            ILogger<[FeatureName]Controller> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region GET Endpoints

        /// <summary>
        /// Gets all [FeatureName] items
        /// </summary>
        /// <returns>List of [FeatureName] items</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<[FeatureName]DTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Getting all [FeatureName] items");

                var items = await _repository.Query_Async();
                
                if (items == null || !items.Any())
                {
                    _logger.LogWarning($"{location}: No items found");
                    return NotFound("No items found");
                }

                var result = _mapper.Map<IEnumerable<[FeatureName]DTO>>(items);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error getting all items");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a specific [FeatureName] item by ID
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <returns>[FeatureName] item</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof([FeatureName]DTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Getting [FeatureName] with id {id}");

                var item = await _repository.Query_ById_Async(id);
                
                if (item == null)
                {
                    _logger.LogWarning($"{location}: Item with id {id} not found");
                    return NotFound($"Item with id {id} not found");
                }

                var result = _mapper.Map<[FeatureName]DTO>(item);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error getting item with id {id}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets [FeatureName] items for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of [FeatureName] items for the user</returns>
        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(IEnumerable<[FeatureName]DTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Getting [FeatureName] items for user {userId}");

                var items = await _repository.GetByUserIdAsync(userId);
                
                if (items == null || !items.Any())
                {
                    return Ok(Enumerable.Empty<[FeatureName]DTO>());
                }

                var result = _mapper.Map<IEnumerable<[FeatureName]DTO>>(items);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error getting items for user {userId}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets [FeatureName] with detailed information
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <returns>[FeatureName] with related data</returns>
        [HttpGet("{id:int}/details")]
        [ProducesResponseType(typeof([FeatureName]DTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWithDetails(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Getting [FeatureName] details for id {id}");

                var item = await _repository.GetByIdWithDetailsAsync(id);
                
                if (item == null)
                {
                    return NotFound($"Item with id {id} not found");
                }

                var result = _mapper.Map<[FeatureName]DTO>(item);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error getting details for id {id}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Searches [FeatureName] by name
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <returns>List of matching [FeatureName] items</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<[FeatureName]DTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Searching for '{searchTerm}'");

                var items = await _repository.SearchByNameAsync(searchTerm);
                
                var result = _mapper.Map<IEnumerable<[FeatureName]DTO>>(items);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error searching for '{searchTerm}'");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets statistics for [FeatureName]
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Statistics dictionary</returns>
        [HttpGet("statistics/{userId:int}")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStatistics(int userId)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Getting statistics for user {userId}");

                var stats = await _repository.GetStatisticsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error getting statistics");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Creates a new [FeatureName] item
        /// </summary>
        /// <param name="dto">The item to create</param>
        /// <returns>Created [FeatureName] item</returns>
        [HttpPost]
        [ProducesResponseType(typeof([FeatureName]DTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] Create[FeatureName]DTO dto)
        {
            var location = GetControllerActionNames();
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"{location}: Invalid model state");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation($"{location}: Creating new [FeatureName]");

                // Check for duplicates
                var exists = await _repository.ExistsByNameAsync(dto.Name);
                if (exists)
                {
                    return BadRequest($"[FeatureName] with name '{dto.Name}' already exists");
                }

                var entity = _mapper.Map<[FeatureName]>(dto);
                var id = await _repository.Insert_Async(entity);
                
                entity.Id = (int)id;
                var result = _mapper.Map<[FeatureName]DTO>(entity);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error creating [FeatureName]");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Updates an existing [FeatureName] item
        /// </summary>
        /// <param name="id">The ID of the item to update</param>
        /// <param name="dto">The updated item data</param>
        /// <returns>Updated [FeatureName] item</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof([FeatureName]DTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] Update[FeatureName]DTO dto)
        {
            var location = GetControllerActionNames();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != dto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                _logger.LogInformation($"{location}: Updating [FeatureName] with id {id}");

                var existing = await _repository.Query_ById_Async(id);
                if (existing == null)
                {
                    return NotFound($"Item with id {id} not found");
                }

                // Check for duplicates (excluding current item)
                var exists = await _repository.ExistsByNameAsync(dto.Name, id);
                if (exists)
                {
                    return BadRequest($"[FeatureName] with name '{dto.Name}' already exists");
                }

                var entity = _mapper.Map<[FeatureName]>(dto);
                await _repository.Update_Async(entity);

                var result = _mapper.Map<[FeatureName]DTO>(entity);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error updating [FeatureName] with id {id}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Deletes a [FeatureName] item (soft delete)
        /// </summary>
        /// <param name="id">The ID of the item to delete</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Deleting [FeatureName] with id {id}");

                var existing = await _repository.Query_ById_Async(id);
                if (existing == null)
                {
                    return NotFound($"Item with id {id} not found");
                }

                var success = await _repository.SoftDeleteAsync(id);
                
                if (!success)
                {
                    return InternalError($"{location}: Failed to delete item");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error deleting [FeatureName] with id {id}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores a soft-deleted [FeatureName] item
        /// </summary>
        /// <param name="id">The ID of the item to restore</param>
        /// <returns>No content</returns>
        [HttpPost("{id:int}/restore")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Restore(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInformation($"{location}: Restoring [FeatureName] with id {id}");

                var success = await _repository.RestoreAsync(id);
                
                if (!success)
                {
                    return NotFound($"Item with id {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{location}: Error restoring [FeatureName] with id {id}");
                return InternalError($"{location}: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, $"Something went wrong. Please contact the Administrator: {message}");
        }

        #endregion
    }
}
```

---

## Frontend Implementation

### 1. Services
**Location**: `MediaOrganiserApp.Web.Blazor/Services/`

#### 1.1 HTTP Service

```csharp name=Services/[FeatureName]Service.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MediaOrganiserApp.Web.Blazor.Models;
using Microsoft.Extensions.Logging;

namespace MediaOrganiserApp.Web.Blazor.Services
{
    /// <summary>
    /// Service for [FeatureName] HTTP operations
    /// </summary>
    public class [FeatureName]Service : I[FeatureName]Service
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<[FeatureName]Service> _logger;
        private const string BaseUrl = "api/[FeatureName]";

        public [FeatureName]Service(HttpClient httpClient, ILogger<[FeatureName]Service> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<[FeatureName]DTO>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all [FeatureName] items");
                
                var response = await _httpClient.GetAsync(BaseUrl);
                response.EnsureSuccessStatusCode();
                
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<[FeatureName]DTO>>();
                return items ?? new List<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all [FeatureName] items");
                throw;
            }
        }

        public async Task<[FeatureName]DTO> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Getting [FeatureName] with id {id}");
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting [FeatureName] with id {id}");
                throw;
            }
        }

        public async Task<IEnumerable<[FeatureName]DTO>> GetByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Getting [FeatureName] items for user {userId}");
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/user/{userId}");
                response.EnsureSuccessStatusCode();
                
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<[FeatureName]DTO>>();
                return items ?? new List<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting [FeatureName] items for user {userId}");
                throw;
            }
        }

        public async Task<[FeatureName]DTO> GetWithDetailsAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Getting [FeatureName] details for id {id}");
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/{id}/details");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting [FeatureName] details for id {id}");
                throw;
            }
        }

        public async Task<IEnumerable<[FeatureName]DTO>> SearchAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation($"Searching [FeatureName] with term '{searchTerm}'");
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/search?searchTerm={searchTerm}");
                response.EnsureSuccessStatusCode();
                
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<[FeatureName]DTO>>();
                return items ?? new List<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching [FeatureName]");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetStatisticsAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Getting statistics for user {userId}");
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/statistics/{userId}");
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                throw;
            }
        }

        public async Task<[FeatureName]DTO> CreateAsync(Create[FeatureName]DTO dto)
        {
            try
            {
                _logger.LogInformation("Creating new [FeatureName]");
                
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, dto);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating [FeatureName]");
                throw;
            }
        }

        public async Task<[FeatureName]DTO> UpdateAsync(int id, Update[FeatureName]DTO dto)
        {
            try
            {
                _logger.LogInformation($"Updating [FeatureName] with id {id}");
                
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", dto);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<[FeatureName]DTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating [FeatureName] with id {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting [FeatureName] with id {id}");
                
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting [FeatureName] with id {id}");
                throw;
            }
        }

        public async Task<bool> RestoreAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Restoring [FeatureName] with id {id}");
                
                var response = await _httpClient.PostAsync($"{BaseUrl}/{id}/restore", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring [FeatureName] with id {id}");
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for [FeatureName]Service
    /// </summary>
    public interface I[FeatureName]Service
    {
        Task<IEnumerable<[FeatureName]DTO>> GetAllAsync();
        Task<[FeatureName]DTO> GetByIdAsync(int id);
        Task<IEnumerable<[FeatureName]DTO>> GetByUserIdAsync(int userId);
        Task<[FeatureName]DTO> GetWithDetailsAsync(int id);
        Task<IEnumerable<[FeatureName]DTO>> SearchAsync(string searchTerm);
        Task<Dictionary<string, int>> GetStatisticsAsync(int userId);
        Task<[FeatureName]DTO> CreateAsync(Create[FeatureName]DTO dto);
        Task<[FeatureName]DTO> UpdateAsync(int id, Update[FeatureName]DTO dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
```

#### 1.2 Service Registration in Program.cs

```csharp name=Program.cs
// Add to Program.cs in Blazor project

builder.Services.AddScoped<I[FeatureName]Service, [FeatureName]Service>();
```

---

### 2. Blazor Components
**Location**: `MediaOrganiserApp.Web.Blazor/Pages/[FeatureName]/`

#### 2.1 List Component

```razor name=Pages/[FeatureName]/[FeatureName]List.razor
@page "/[featurename]"
@using MediaOrganiserApp.Web.Blazor.Models
@using MediaOrganiserApp.Web.Blazor.Services
@inject I[FeatureName]Service [FeatureName]Service
@inject NavigationManager NavigationManager
@inject ILogger<[FeatureName]List> Logger

<PageTitle>[FeatureName] List</PageTitle>

<div class="container-fluid">
    <div class="row mb-3">
        <div class="col">
            <h3>[FeatureName] Management</h3>
        </div>
        <div class="col-auto">
            <button class="btn btn-primary" @onclick="NavigateToCreate">
                <i class="bi bi-plus-circle"></i> Create New
            </button>
        </div>
    </div>

    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger" role="alert">
            @errorMessage
        </div>
    }
    else if (items == null || !items.Any())
    {
        <div class="alert alert-info" role="alert">
            No items found. Create your first [FeatureName]!
        </div>
    }
    else
    {
        <!-- Search Bar -->
        <div class="row mb-3">
            <div class="col-md-6">
                <div class="input-group">
                    <input type="text" 
                           class="form-control" 
                           placeholder="Search by name..." 
                           @bind="searchTerm"
                           @bind:event="oninput"
                           @onkeyup="OnSearchKeyUp" />
                    <button class="btn btn-outline-secondary" @onclick="ClearSearch">
                        <i class="bi bi-x-circle"></i>
                    </button>
                </div>
            </div>
        </div>

        <!-- Items Grid/Table -->
        <div class="table-responsive">
            <table class="table table-hover">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Created</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in filteredItems)
                    {
                        <tr>
                            <td>@item.Id</td>
                            <td>@item.Name</td>
                            <td>@(item.Description?.Length > 50 
                                ? item.Description.Substring(0, 50) + "..." 
                                : item.Description)</td>
                            <td>@item.CreatedAt.ToString("yyyy-MM-dd")</td>
                            <td>
                                <button class="btn btn-sm btn-info" @onclick="() => NavigateToDetails(item.Id)">
                                    <i class="bi bi-eye"></i> View
                                </button>
                                <button class="btn btn-sm btn-warning" @onclick="() => NavigateToEdit(item.Id)">
                                    <i class="bi bi-pencil"></i> Edit
                                </button>
                                <button class="btn btn-sm btn-danger" @onclick="() => ShowDeleteConfirmation(item)">
                                    <i class="bi bi-trash"></i> Delete
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>

        <!-- Pagination (optional) -->
        <nav aria-label="Page navigation">
            <ul class="pagination justify-content-center">
                <!-- Add pagination logic here if needed -->
            </ul>
        </nav>
    }
</div>

<!-- Delete Confirmation Modal -->
@if (showDeleteModal)
{
    <div class="modal fade show d-block" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Confirm Delete</h5>
                    <button type="button" class="btn-close" @onclick="CancelDelete"></button>
                </div>
                <div class="modal-body">
                    <p>Are you sure you want to delete <strong>@itemToDelete?.Name</strong>?</p>
                    <p class="text-danger">This action cannot be undone.</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @onclick="CancelDelete">Cancel</button>
                    <button type="button" class="btn btn-danger" @onclick="ConfirmDelete">Delete</button>
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop fade show"></div>
}

@code {
    private List<[FeatureName]DTO> items = new();
    private List<[FeatureName]DTO> filteredItems = new();
    private bool isLoading = true;
    private string errorMessage = string.Empty;
    private string searchTerm = string.Empty;
    
    // Delete modal
    private bool showDeleteModal = false;
    private [FeatureName]DTO? itemToDelete = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;

            var result = await [FeatureName]Service.GetAllAsync();
            items = result.ToList();
            filteredItems = items;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading [FeatureName] items");
            errorMessage = "Error loading items. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnSearchKeyUp(KeyboardEventArgs e)
    {
        FilterItems();
    }

    private void FilterItems()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredItems = items;
        }
        else
        {
            filteredItems = items
                .Where(i => i.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (i.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
    }

    private void ClearSearch()
    {
        searchTerm = string.Empty;
        FilterItems();
    }

    private void NavigateToCreate()
    {
        NavigationManager.NavigateTo("/[featurename]/create");
    }

    private void NavigateToDetails(int id)
    {
        NavigationManager.NavigateTo($"/[featurename]/{id}");
    }

    private void NavigateToEdit(int id)
    {
        NavigationManager.NavigateTo($"/[featurename]/edit/{id}");
    }

    private void ShowDeleteConfirmation([FeatureName]DTO item)
    {
        itemToDelete = item;
        showDeleteModal = true;
    }

    private void CancelDelete()
    {
        itemToDelete = null;
        showDeleteModal = false;
    }

    private async Task ConfirmDelete()
    {
        if (itemToDelete == null) return;

        try
        {
            var success = await [FeatureName]Service.DeleteAsync(itemToDelete.Id);
            
            if (success)
            {
                items.Remove(itemToDelete);
                FilterItems();
                // Show success message (use toast notification library if available)
            }
            else
            {
                errorMessage = "Failed to delete item.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error deleting [FeatureName] with id {itemToDelete.Id}");
            errorMessage = "Error deleting item. Please try again.";
        }
        finally
        {
            CancelDelete();
        }
    }
}
```

#### 2.2 Create/Edit Component

```razor name=Pages/[FeatureName]/[FeatureName]Form.razor
@page "/[featurename]/create"
@page "/[featurename]/edit/{Id:int}"
@using MediaOrganiserApp.Web.Blazor.Models
@using MediaOrganiserApp.Web.Blazor.Services
@using System.ComponentModel.DataAnnotations
@inject I[FeatureName]Service [FeatureName]Service
@inject NavigationManager NavigationManager
@inject ILogger<[FeatureName]Form> Logger

<PageTitle>@(IsEditMode ? "Edit" : "Create") [FeatureName]</PageTitle>

<div class="container">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h3>@(IsEditMode ? "Edit" : "Create New") [FeatureName]</h3>
                </div>
                <div class="card-body">
                    @if (!string.IsNullOrEmpty(errorMessage))
                    {
                        <div class="alert alert-danger" role="alert">
                            @errorMessage
                        </div>
                    }

                    <EditForm Model="@model" OnValidSubmit="HandleSubmit">
                        <DataAnnotationsValidator />
                        <ValidationSummary class="text-danger" />

                        <div class="mb-3">
                            <label for="name" class="form-label">Name *</label>
                            <InputText id="name" 
                                      class="form-control" 
                                      @bind-Value="model.Name" 
                                      placeholder="Enter name" />
                            <ValidationMessage For="@(() => model.Name)" class="text-danger" />
                        </div>

                        <div class="mb-3">
                            <label for="description" class="form-label">Description</label>
                            <InputTextArea id="description" 
                                          class="form-control" 
                                          @bind-Value="model.Description" 
                                          rows="4"
                                          placeholder="Enter description (optional)" />
                            <ValidationMessage For="@(() => model.Description)" class="text-danger" />
                        </div>

                        <!-- Add more fields as needed -->

                        <div class="d-flex justify-content-between">
                            <button type="button" class="btn btn-secondary" @onclick="Cancel">
                                <i class="bi bi-x-circle"></i> Cancel
                            </button>
                            <button type="submit" class="btn btn-primary" disabled="@isSubmitting">
                                @if (isSubmitting)
                                {
                                    <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                                    <span> Saving...</span>
                                }
                                else
                                {
                                    <i class="bi bi-save"></i>
                                    <span> Save</span>
                                }
                            </button>
                        </div>
                    </EditForm>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter]
    public int? Id { get; set; }

    private [FeatureName]FormModel model = new();
    private bool isSubmitting = false;
    private string errorMessage = string.Empty;
    private bool IsEditMode => Id.HasValue && Id.Value > 0;

    protected override async Task OnInitializedAsync()
    {
        if (IsEditMode)
        {
            await LoadItemAsync();
        }
    }

    private async Task LoadItemAsync()
    {
        try
        {
            var item = await [FeatureName]Service.GetByIdAsync(Id!.Value);
            
            if (item != null)
            {
                model = new [FeatureName]FormModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description
                    // Map other properties
                };
            }
            else
            {
                errorMessage = "Item not found.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error loading [FeatureName] with id {Id}");
            errorMessage = "Error loading item. Please try again.";
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = string.Empty;

            if (IsEditMode)
            {
                var updateDto = new Update[FeatureName]DTO
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    LastModifiedBy = "CurrentUser" // Get from auth service
                };

                await [FeatureName]Service.UpdateAsync(model.Id, updateDto);
            }
            else
            {
                var createDto = new Create[FeatureName]DTO
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = 1 // Get from auth service
                };

                await [FeatureName]Service.CreateAsync(createDto);
            }

            // Navigate back to list
            NavigationManager.NavigateTo("/[featurename]");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error {(IsEditMode ? "updating" : "creating")} [FeatureName]");
            errorMessage = $"Error {(IsEditMode ? "updating" : "creating")} item. Please try again.";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo("/[featurename]");
    }

    // Form model class
    private class [FeatureName]FormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
    }
}
```

#### 2.3 Details Component

```razor name=Pages/[FeatureName]/[FeatureName]Details.razor
@page "/[featurename]/{Id:int}"
@using MediaOrganiserApp.Web.Blazor.Models
@using MediaOrganiserApp.Web.Blazor.Services
@inject I[FeatureName]Service [FeatureName]Service
@inject NavigationManager NavigationManager
@inject ILogger<[FeatureName]Details> Logger

<PageTitle>[FeatureName] Details</PageTitle>

<div class="container">
    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    }
    else if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger" role="alert">
            @errorMessage
        </div>
    }
    else if (item != null)
    {
        <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <h3>@item.Name</h3>
                <div>
                    <button class="btn btn-warning" @onclick="NavigateToEdit">
                        <i class="bi bi-pencil"></i> Edit
                    </button>
                    <button class="btn btn-secondary" @onclick="NavigateToList">
                        <i class="bi bi-arrow-left"></i> Back to List
                    </button>
                </div>
            </div>
            <div class="card-body">
                <dl class="row">
                    <dt class="col-sm-3">ID</dt>
                    <dd class="col-sm-9">@item.Id</dd>

                    <dt class="col-sm-3">Name</dt>
                    <dd class="col-sm-9">@item.Name</dd>

                    <dt class="col-sm-3">Description</dt>
                    <dd class="col-sm-9">@(item.Description ?? "N/A")</dd>

                    <dt class="col-sm-3">Created</dt>
                    <dd class="col-sm-9">@item.CreatedAt.ToString("yyyy-MM-dd HH:mm")</dd>

                    @if (item.UpdatedAt.HasValue)
                    {
                        <dt class="col-sm-3">Last Updated</dt>
                        <dd class="col-sm-9">@item.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm")</dd>
                    }

                    <dt class="col-sm-3">Created By</dt>
                    <dd class="col-sm-9">@(item.CreatedBy ?? "N/A")</dd>

                    @if (!string.IsNullOrEmpty(item.LastModifiedBy))
                    {
                        <dt class="col-sm-3">Last Modified By</dt>
                        <dd class="col-sm-9">@item.LastModifiedBy</dd>
                    }
                </dl>

                <!-- Add related data display here -->
            </div>
        </div>
    }
</div>

@code {
    [Parameter]
    public int Id { get; set; }

    private [FeatureName]DTO? item;
    private bool isLoading = true;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadItemAsync();
    }

    private async Task LoadItemAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = string.Empty;

            item = await [FeatureName]Service.GetWithDetailsAsync(Id);
            
            if (item == null)
            {
                errorMessage = "Item not found.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error loading [FeatureName] with id {Id}");
            errorMessage = "Error loading item details. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToEdit()
    {
        NavigationManager.NavigateTo($"/[featurename]/edit/{Id}");
    }

    private void NavigateToList()
    {
        NavigationManager.NavigateTo("/[featurename]");
    }
}
```

---

### 3. Models/DTOs
**Location**: `MediaOrganiserApp.Web.Blazor/Models/`

```csharp name=Models/[FeatureName]Models.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MediaOrganiserApp.Web.Blazor.Models
{
    /// <summary>
    /// Client-side DTO for [FeatureName]
    /// </summary>
    public class [FeatureName]DTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string LastModifiedBy { get; set; } = string.Empty;
    }

    public class Create[FeatureName]DTO
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }

    public class Update[FeatureName]DTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string LastModifiedBy { get; set; } = string.Empty;
    }
}
```

---

## Database Implementation

### Database Schema

```sql name=Database/Migrations/Create_[FeatureName]_Table.sql
-- =============================================
-- Create [FeatureName] Table
-- =============================================

-- Drop table if exists (for development only)
IF OBJECT_ID('dbo.[FeatureName]', 'U') IS NOT NULL
    DROP TABLE dbo.[FeatureName];
GO

-- Create table
CREATE TABLE dbo.[FeatureName]
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    UpdatedAt DATETIME2(7) NULL,
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT ('System'),
    LastModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT (0),
    
    CONSTRAINT PK_[FeatureName] PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_[FeatureName]_User FOREIGN KEY (UserId) 
        REFERENCES dbo.[User] (Id) ON DELETE CASCADE
);
GO

-- Create indexes
CREATE NONCLUSTERED INDEX IX_[FeatureName]_UserId 
    ON dbo.[FeatureName](UserId);
GO

CREATE NONCLUSTERED INDEX IX_[FeatureName]_Name 
    ON dbo.[FeatureName](Name);
GO

CREATE NONCLUSTERED INDEX IX_[FeatureName]_IsDeleted 
    ON dbo.[FeatureName](IsDeleted)
    INCLUDE (Id, Name, UserId);
GO

-- Add extended properties for documentation
EXEC sys.sp_addextendedproperty 
    @name=N'MS_Description', 
    @value=N'[FeatureName] table stores ...' , 
    @level0type=N'SCHEMA',
    @level0name=N'dbo', 
    @level1type=N'TABLE',
    @level1name=N'[FeatureName]';
GO
```

### Stored Procedures (Optional)

```sql name=Database/StoredProcedures/usp_[FeatureName]_GetByUserId.sql
-- =============================================
-- Stored Procedure: Get [FeatureName] by User ID
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_[FeatureName]_GetByUserId
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Name, Description, UserId,
        CreatedAt, UpdatedAt, CreatedBy, LastModifiedBy, IsDeleted
    FROM dbo.[FeatureName]
    WHERE UserId = @UserId AND IsDeleted = 0
    ORDER BY CreatedAt DESC;
END;
GO
```

---

## Testing Implementation

### Unit Tests
**Location**: `MediaOrganiserApp.Backend/MediaOrganizerApp.Tests/`

```csharp name=MediaOrganizerApp.Tests/Repositories/[FeatureName]RepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaOrganizerApp.Application.Interfaces.Repositories;
using MediaOrganizerApp.Domain.Entities;
using MediaOrganizerApp.Persistence.Repositories.SqlServerRepository;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaOrganizerApp.Tests.Repositories
{
    public class [FeatureName]RepositoryTests
    {
        private readonly Mock<IDapperContext> _mockContext;
        private readonly Mock<ILogger<[FeatureName]Repository>> _mockLogger;
        private readonly [FeatureName]Repository _repository;

        public [FeatureName]RepositoryTests()
        {
            _mockContext = new Mock<IDapperContext>();
            _mockLogger = new Mock<ILogger<[FeatureName]Repository>>();
            _repository = new [FeatureName]Repository(_mockContext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsItems_WhenItemsExist()
        {
            // Arrange
            int userId = 1;
            var expectedItems = new List<[FeatureName]>
            {
                new [FeatureName] { Id = 1, Name = "Test 1", UserId = userId },
                new [FeatureName] { Id = 2, Name = "Test 2", UserId = userId }
            };

            // Mock setup here...

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ExistsByNameAsync_ReturnsTrue_WhenItemExists()
        {
            // Arrange
            string name = "Test Item";

            // Mock setup here...

            // Act
            var result = await _repository.ExistsByNameAsync(name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SoftDeleteAsync_ReturnsTrue_WhenItemIsDeleted()
        {
            // Arrange
            int id = 1;

            // Mock setup here...

            // Act
            var result = await _repository.SoftDeleteAsync(id);

            // Assert
            Assert.True(result);
        }
    }
}
```

---

## Localization

### Resource Files

```xml name=Resources/Features.[FeatureName].en.resx
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Title" xml:space="preserve">
    <value>[FeatureName]</value>
  </data>
  <data name="CreateNew" xml:space="preserve">
    <value>Create New [FeatureName]</value>
  </data>
  <data name="Name" xml:space="preserve">
    <value>Name</value>
  </data>
  <data name="Description" xml:space="preserve">
    <value>Description</value>
  </data>
  <data name="CreatedAt" xml:space="preserve">
    <value>Created</value>
  </data>
  <data name="Actions" xml:space="preserve">
    <value>Actions</value>
  </data>
  <data name="Edit" xml:space="preserve">
    <value>Edit</value>
  </data>
  <data name="Delete" xml:space="preserve">
    <value>Delete</value>
  </data>
  <data name="DeleteConfirmation" xml:space="preserve">
    <value>Are you sure you want to delete this item?</value>
  </data>
  <data name="SaveSuccess" xml:space="preserve">
    <value>[FeatureName] saved successfully</value>
  </data>
  <data name="DeleteSuccess" xml:space="preserve">
    <value>[FeatureName] deleted successfully</value>
  </data>
  <data name="ErrorLoading" xml:space="preserve">
    <value>Error loading data</value>
  </data>
  <data name="ErrorSaving" xml:space="preserve">
    <value>Error saving data</value>
  </data>
</root>
```

```xml name=Resources/Features.[FeatureName].pt.resx
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Title" xml:space="preserve">
    <value>[Nome da Funcionalidade]</value>
  </data>
  <data name="CreateNew" xml:space="preserve">
    <value>Criar Novo [Nome da Funcionalidade]</value>
  </data>
  <data name="Name" xml:space="preserve">
    <value>Nome</value>
  </data>
  <data name="Description" xml:space="preserve">
    <value>Descrição</value>
  </data>
  <data name="CreatedAt" xml:space="preserve">
    <value>Criado em</value>
  </data>
  <data name="Actions" xml:space="preserve">
    <value>Ações</value>
  </data>
  <data name="Edit" xml:space="preserve">
    <value>Editar</value>
  </data>
  <data name="Delete" xml:space="preserve">
    <value>Eliminar</value>
  </data>
  <data name="DeleteConfirmation" xml:space="preserve">
    <value>Tem certeza que deseja eliminar este item?</value>
  </data>
  <data name="SaveSuccess" xml:space="preserve">
    <value>[Nome da Funcionalidade] guardado com sucesso</value>
  </data>
  <data name="DeleteSuccess" xml:space="preserve">
    <value>[Nome da Funcionalidade] eliminado com sucesso</value>
  </data>
  <data name="ErrorLoading" xml:space="preserve">
    <value>Erro ao carregar dados</value>
  </data>
  <data name="ErrorSaving" xml:space="preserve">
    <value>Erro ao guardar dados</value>
  </data>
</root>
```

---

## Implementation Checklist

### Backend
- [ ] **Domain Layer**
  - [ ] Create domain entity class
  - [ ] Create view model class (if needed)
  - [ ] Add data annotations for validation
  
- [ ] **Application Layer**
  - [ ] Create DTOs (DTO, CreateDTO, UpdateDTO)
  - [ ] Create AutoMapper profile
  - [ ] Create repository interface
  - [ ] Add any business logic interfaces
  
- [ ] **Persistence Layer**
  - [ ] Create repository implementation
  - [ ] Implement all CRUD methods
  - [ ] Implement custom query methods
  - [ ] Add proper error handling and logging
  - [ ] Register repository in ServiceRegistration
  
- [ ] **WebAPI Layer**
  - [ ] Create API controller
  - [ ] Implement all endpoints (GET, POST, PUT, DELETE)
  - [ ] Add proper response codes
  - [ ] Add XML documentation comments
  - [ ] Test endpoints with Postman/Swagger

### Database
- [ ] **Schema**
  - [ ] Create table schema
  - [ ] Add appropriate indexes
  - [ ] Add foreign key constraints
  - [ ] Add default values
  - [ ] Create stored procedures (if needed)
  
- [ ] **Migration**
  - [ ] Create up migration script
  - [ ] Create down migration script (rollback)
  - [ ] Test migration on development database

### Frontend
- [ ] **Services**
  - [ ] Create service interface
  - [ ] Create service implementation
  - [ ] Register service in Program.cs
  - [ ] Add error handling
  
- [ ] **Components**
  - [ ] Create list/grid component
  - [ ] Create form component (create/edit)
  - [ ] Create details component
  - [ ] Add proper validation
  - [ ] Add loading states
  - [ ] Add error messages
  
- [ ] **Models**
  - [ ] Create client-side DTOs
  - [ ] Add validation attributes
  
- [ ] **Styling**
  - [ ] Create/update CSS for components
  - [ ] Ensure responsive design
  - [ ] Add icons and visual feedback

### Testing
- [ ] **Unit Tests**
  - [ ] Repository tests
  - [ ] Service tests
  - [ ] Controller tests
  - [ ] Achieve 70%+ code coverage
  
- [ ] **Integration Tests**
  - [ ] API endpoint tests
  - [ ] Database integration tests
  
- [ ] **UI Tests** (optional)
  - [ ] Component tests
  - [ ] E2E tests with bUnit or Playwright

### Localization
- [ ] Create English resource file
- [ ] Create Portuguese resource file
- [ ] Apply localization to all UI text
- [ ] Test language switching

### Documentation
- [ ] Update API documentation
- [ ] Update user documentation
- [ ] Add code comments
- [ ] Update README if needed

---

## Success Metrics

### Performance Metrics
- **API Response Time**: 
  - GET operations: < 200ms
  - POST/PUT operations: < 500ms
  - DELETE operations: < 300ms
  
- **Database Query Performance**:
  - Simple queries: < 50ms
  - Complex queries with joins: < 150ms
  - Search operations: < 200ms

### User Engagement
- **Usage Statistics**:
  - Track number of items created per week
  - Track number of active users
  - Track most used features
  
- **User Satisfaction**:
  - Error rate: < 1%
  - User feedback score: > 4/5
  - Task completion rate: > 95%

### Code Quality
- **Test Coverage**: > 70%
- **Code Review**: All code reviewed before merge
- **Documentation**: 100% of public APIs documented
- **Performance**: No regressions in benchmarks

### Monitoring
- **Logging**:
  - All errors logged with context
  - Performance metrics logged
  - User actions tracked (GDPR compliant)
  
- **Alerts**:
  - Alert on error rate > 5%
  - Alert on response time > 1s
  - Alert on database connection failures

---

## Additional Resources

### Related Documentation
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Dapper ORM Documentation](https://github.com/DapperLib/Dapper)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

### Tools
- **API Testing**: Postman, Swagger/OpenAPI
- **Database**: SQL Server Management Studio, Azure Data Studio
- **Logging**: Serilog, Application Insights
- **Testing**: xUnit, Moq, bUnit
- **CI/CD**: GitHub Actions, Azure DevOps

---

**Version**: 1.0  
**Last Updated**: 2026-02-11  
**Author**: MediaOrganiser Development Team
