# Feature: Reading Progress Tracker

## Implementation Guide

This document outlines the complete implementation guide for the Reading Progress Tracker feature in the MediaOrganiserApp.

### Database Schema  
#### ReadingProgress Table  
- **Id** (int, Primary Key)  
- **UserId** (int, Foreign Key)  
- **BookId** (int, Foreign Key)  
- **ProgressPercentage** (int)  
- **LastUpdated** (DateTime)

#### ReadingHistory Table  
- **Id** (int, Primary Key)  
- **UserId** (int, Foreign Key)  
- **BookId** (int, Foreign Key)  
- **Timestamp** (DateTime)  
- **ProgressPercentage** (int)

### C# Domain Models  
#### ReadingProgress Class  
```csharp  
public class ReadingProgress  
{  
    public int Id { get; set; }  
    public int UserId { get; set; }  
    public int BookId { get; set; }  
    public int ProgressPercentage { get; set; }  
    public DateTime LastUpdated { get; set; }  
}
```

#### ReadingStatus Enum  
```csharp  
enum ReadingStatus  
{  
    NotStarted,  
    InProgress,  
    Completed  
}
```

### Service Interface  
#### IReadingProgressService  
```csharp  
public interface IReadingProgressService  
{  
    Task UpdateProgressAsync(int userId, int bookId, int progressPercentage);  
    Task<ReadingProgress> GetStatisticsAsync(int userId, int bookId);  
}
```

### Full Service Implementation  
```csharp  
public class ReadingProgressService : IReadingProgressService  
{  
    private readonly DbContext _context;  

    public ReadingProgressService(DbContext context)  
    {  
        _context = context;  
    }

    public async Task UpdateProgressAsync(int userId, int bookId, int progressPercentage)  
    {  
        var progress = await _context.ReadingProgress.FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);  
        if (progress == null)  
        {  
            progress = new ReadingProgress { UserId = userId, BookId = bookId };  
            _context.ReadingProgress.Add(progress);  
        }
        progress.ProgressPercentage = progressPercentage;  
        progress.LastUpdated = DateTime.UtcNow;  
        await _context.SaveChangesAsync();  
    }

    public async Task<ReadingProgress> GetStatisticsAsync(int userId, int bookId)  
    {  
        return await _context.ReadingProgress.FindAsync(userId, bookId);  
    }  
}
```

### Blazor Components  
#### Progress Tracker UI  
```razor  
@page "/reading-progress/{bookId:int}"
@inject IReadingProgressService ReadingProgressService

<h3>Reading Progress</h3>
<div>
    <InputNumber @bind-Value="progressPercentage" />
    <button @onclick="UpdateProgress">Update Progress</button>
    <div style="height: 30px; width: 100%; background: #ccc;">
        <div style="height: 100%; width: @progressPercentage%; background: #4caf50;"></div>
    </div>
</div>

@code {
    [Parameter] public int BookId { get; set; }
    private int progressPercentage;

    protected override async Task OnInitializedAsync()
    {
        var stats = await ReadingProgressService.GetStatisticsAsync(UserId, BookId);
        progressPercentage = stats?.ProgressPercentage ?? 0;
    }

    private async Task UpdateProgress()
    {
        await ReadingProgressService.UpdateProgressAsync(UserId, BookId, progressPercentage);
    }
}
```

### CSS Styling  
#### Progress Bars  
```css  
div.progress {  
    height: 30px;  
    width: 100%;  
    background: #ccc;  
}

div.progress > div {  
    height: 100%;  
    background: #4caf50;
}
```

### Unit Tests  
```csharp  
[TestClass]  
public class ReadingProgressServiceTests  
{  
    [TestMethod]  
    public async Task UpdateProgressAsync_UpdatesProgress()  
    {  
        // Arrange  
        // ...  

        // Act  
        await service.UpdateProgressAsync(userId, bookId, 70);

        // Assert  
        var progress = await service.GetStatisticsAsync(userId, bookId);
        Assert.AreEqual(70, progress.ProgressPercentage);
    }
}
```

### Localization Strings  
```json  
{  
    "ProgressUpdated": "Your progress has been updated!",  
    "ProgressNotStarted": "You have not started this book yet."  
}
```

### Service Registration Code  
```csharp  
services.AddScoped<IReadingProgressService, ReadingProgressService>();
```

### Implementation Checklist  
- [ ] Design database schema  
- [ ] Implement domain models  
- [ ] Create service interface and implementation  
- [ ] Develop Blazor components  
- [ ] Add CSS styling  
- [ ] Write unit tests  
- [ ] Implement localization  
- [ ] Register services

### Success Metrics  
- User engagement rates with the reading progress feature  
- Reduction in the number of unfinished books  
- Feedback ratings from users on the usefulness of the feature