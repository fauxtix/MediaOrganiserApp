# Implementation Guide for Favorites and Recently Accessed Feature

## Overview
This document provides a comprehensive implementation guide for the Favorites and Recently Accessed feature in the Media Organiser App. The guide includes details on the database schemas, service implementation, UI components, tests, localization strings, service registration, migration scripts, implementation checklist, and success metrics.

## Database Schemas

### Favorites Table
- **Id** (Primary Key, UUID)
- **UserId** (Foreign Key, references Users)
- **ItemId** (Foreign Key, references Items)
- **CreatedAt** (DateTime)

### Recently Accessed Table
- **Id** (Primary Key, UUID)
- **UserId** (Foreign Key, references Users)
- **ItemId** (Foreign Key, references Items)
- **AccessedAt** (DateTime)

### Indexes
- Create an index on `UserId` for both tables to improve query performance.

### Triggers
- Implement a trigger to maintain the `Recently Accessed` history by deleting records older than 30 days.

## C# Service Implementation

### CRUD Operations
Implement the following operations in the FavoritesService class:
1. **AddToFavorites(userId, itemId)**
2. **RemoveFromFavorites(userId, itemId)**
3. **GetFavorites(userId)**
4. **GetRecentlyAccessed(userId)**

### DTOs
Define the DTOs with computed properties:
- **FavoriteDTO**: Contains properties of favorite items.
- **RecentlyAccessedDTO**: Contains properties of recently accessed items.

## Blazor UI Components
- **FavoritesComponent**:
  - Displays the list of favorite items.
  - CSS styling to ensure a visually appealing interface.
- **RecentlyAccessedComponent**:
  - Displays the list of recently accessed items.
  - CSS styling for better user experience.

## Unit and Integration Tests
- Implement unit tests for all service methods.
- Create integration tests for database interaction.

## Localization Strings
- English:
  - `Favorites`: 'Favorites'
  - `Recently Accessed`: 'Recently Accessed'
- Portuguese:
  - `Favorites`: 'Favoritos'
  - `Recently Accessed`: 'Acessados Recentemente'

## Service Registration Code
```csharp
services.AddScoped<FavoritesService>();
```

## Migration Scripts
- Migration scripts to create and update the Favorites and Recently Accessed tables.
- Include rollback scripts as well.

## Implementation Checklist
1. [ ] Database designs are finalized.
2. [ ] Service methods implemented and tested.
3. [ ] Blazor components created.
4. [ ] Localization strings are added.
5. [ ] Migration scripts are ready.

## Success Metrics
- Track the number of users utilizing Favorites and Recently Accessed features.
- Monitor user engagement and retention rates.

---

This guide should serve as the foundation for implementing the Favorites and Recently Accessed feature in the Media Organiser App. Please ensure all components are tested and validated before deployment.