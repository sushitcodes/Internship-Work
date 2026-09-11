using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class SubmissionRepository(AppDbContext _context) : ISubmissionRepository
{
   public async Task<Submission> AddAsync(Submission submission)
    {
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task<Submission?> GetByIdAsync(Guid id)
    {
        return await _context.Submissions
            .Include(s => s.ClassRoom)
            
            // Changed: Include ClassRoom instead of Education
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int> GetCountAsync() => await _context.Submissions.CountAsync();

    public async Task<bool> DeleteAsync(Guid id)
    {
        var submission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.Id == id);  // Removed: .Include(s => s.Education)

        if (submission is null) return false;

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(List<Submission> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.Submissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.FullName.Contains(search) ||
                s.RollNo.ToString().Contains(search) ||  // Added: Search by RollNo
                s.ClassRoom.Name.Contains(search));      // Added: Search by ClassRoom name
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Include(s => s.ClassRoom)  // Changed: Include ClassRoom instead of Education
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Submission?> UpdateAsync(Guid id, Submission updated)
    {
        var existing = await _context.Submissions
            .FirstOrDefaultAsync(s => s.Id == id);  // Removed: .Include(s => s.Education)

        if (existing is null)
            return null;

        // Update only the properties that exist in the new entity
        existing.FullName = updated.FullName;
        existing.ClassRoomId = updated.ClassRoomId;  // Added: Update ClassRoomId
        existing.RollNo = updated.RollNo;            // Added: Update RollNo

        if (!string.IsNullOrEmpty(updated.FileUrl))
            existing.FileUrl = updated.FileUrl;

        // REMOVED: Email, Phone, and Education updates
        // REMOVED: EducationEntries.RemoveRange and related code

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<int> GetCountByUserAsync(Guid userId) =>
        await _context.Submissions.CountAsync(s => s.CreatedByUserId == userId);

    public async Task<List<Submission>> GetRecentAsync(int count) =>
        await _context.Submissions
            .Include(s => s.ClassRoom)  // Added: Include ClassRoom for display
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<List<Submission>> GetRecentByUserAsync(Guid userId, int count) =>
        await _context.Submissions
            .Where(s => s.CreatedByUserId == userId)
            .Include(s => s.ClassRoom)  // Added: Include ClassRoom for display
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .ToListAsync();
}