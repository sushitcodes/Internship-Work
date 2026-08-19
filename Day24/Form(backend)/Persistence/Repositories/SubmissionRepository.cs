using Form.Interfaces;
using Form.Entities;
using Microsoft.EntityFrameworkCore;
using Form.Persistence;

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

    public async Task<List<Submission>> GetAllAsync()
    {
        // Include() is required — without it EF won't load related
        // EducationEntry rows and .Education will be an empty list.
        return await _context.Submissions
            .Include(s => s.Education)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
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
}
