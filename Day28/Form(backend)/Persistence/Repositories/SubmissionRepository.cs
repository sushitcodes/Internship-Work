using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Form.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly AppDbContext _context;

    public SubmissionRepository(AppDbContext context) => _context = context;

    public async Task<Submission> AddAsync(Submission submission)
    {
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }
    public async Task<Submission?> GetByIdAsync(Guid id)
    {
        return await _context.Submissions
            .Include(s => s.Education)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var submission = await _context.Submissions
               .Include(s => s.Education)
               .FirstOrDefaultAsync(s => s.Id == id);

        if (submission is null) return false; // nothing to delete

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(List<Submission> Items,int TotalCount)>GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.Submissions.AsQueryable();
        if(!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
            s.FullName.Contains(search) || s.Email.Contains(search));
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .Include(s => s.Education)
            .OrderByDescending(s =>s. CreatedAt)
            .Skip((page - 1) * pageSize)
            .ToListAsync();
        return(items,totalCount);
    }

    public async Task<Submission?>UpdateAsync(Guid id, Submission updated)
    {
        var existing = await _context.Submissions
            .Include(s => s.Education)
            .FirstOrDefaultAsync(s => s.Id == id);
        if(existing is null) 
            return null;
        existing.FullName = updated.FullName;
        existing.Email = updated.Email;
        existing.Phone= updated.Phone;


        if (!string.IsNullOrEmpty(updated.FileUrl))
            existing.FileUrl = updated.FileUrl;

        _context.EducationEntries.RemoveRange(existing.Education);
        existing.Education = updated.Education;

        await _context.SaveChangesAsync();
        return existing;

    }
}
