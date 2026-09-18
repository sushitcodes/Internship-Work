using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
namespace Form.Tests.Services;

public class SubmissionServiceTests
{
    // Same reasoning as AuthServiceTests: one place to build the four
    // dependencies so each test only sets up what it needs.
    private static (SubmissionService sut, Mock<ISubmissionRepository> repo, Mock<IFileStorageService> fileStorage,
        Mock<IUserProfileRepository> profileRepo, Mock<IEnrollmentService> enrollmentService) CreateSut()
    {
        var repo = new Mock<ISubmissionRepository>();
        var fileStorage = new Mock<IFileStorageService>();
        var profileRepo = new Mock<IUserProfileRepository>();
        var enrollmentService = new Mock<IEnrollmentService>();
        var sut = new SubmissionService(repo.Object, fileStorage.Object, profileRepo.Object, enrollmentService.Object);
        return (sut, repo, fileStorage, profileRepo, enrollmentService);
    }

    // A fake IFormFile — Moq can mock interfaces, and IFormFile is one,
    // so we don't need a real uploaded file to test validation logic.
    private static Mock<IFormFile> FakeFile(string fileName, long length)
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(length);
        return file;
    }

    [Fact]
    public async Task CreateSubmissionAsync_UnknownRollNo_ThrowsAndNeverSaves()
    {
        // ARRANGE: a valid-looking file, but no UserProfile has this
        // RollNo — this is the "silent orphaned record" case the code
        // comment explicitly calls out as the reason for this check.
        var (sut, repo, fileStorage, profileRepo, enrollmentService) = CreateSut();
        var file = FakeFile("resume.pdf", 1024);
        fileStorage.Setup(f => f.SaveFileAsync(file.Object)).ReturnsAsync("uploads/fake-guid.pdf");
        profileRepo.Setup(p => p.GetByMemberNumberAsync(9999)).ReturnsAsync((UserProfile?)null);

        var request = new CreateSubmissionRequest
        {
            FullName = "Nobody",
            ClassRoomId = Guid.NewGuid(),
            RollNo = 9999,
            File = file.Object,
            CreatedByUserId = Guid.NewGuid(),
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateSubmissionAsync(request));

        // Nothing about this attempt should have been persisted or
        // triggered enrollment — the file was uploaded before the roll
        // number check runs (that ordering is worth knowing, but the
        // DB record itself must never be created for a bad roll number).
        repo.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Never);
        enrollmentService.Verify(e => e.EnsureEnrolledAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateSubmissionAsync_ValidRollNo_EnsuresEnrollmentForCorrectStudent()
    {
        // ARRANGE: this time the roll number matches a real profile.
        var studentUserId = Guid.NewGuid();
        var classRoomId = Guid.NewGuid();
        var profile = new UserProfile { UserId = studentUserId, MemberNumber = 42 };

        var (sut, repo, fileStorage, profileRepo, enrollmentService) = CreateSut();
        var file = FakeFile("resume.pdf", 1024);
        fileStorage.Setup(f => f.SaveFileAsync(file.Object)).ReturnsAsync("uploads/fake-guid.pdf");
        profileRepo.Setup(p => p.GetByMemberNumberAsync(42)).ReturnsAsync(profile);
        repo.Setup(r => r.AddAsync(It.IsAny<Submission>()))
            .ReturnsAsync((Submission s) => s); // echo back whatever was passed in, like EF would after saving

        var request = new CreateSubmissionRequest
        {
            FullName = "Real Student",
            ClassRoomId = classRoomId,
            RollNo = 42,
            File = file.Object,
            CreatedByUserId = Guid.NewGuid(),
        };

        // ACT
        await sut.CreateSubmissionAsync(request);

        // ASSERT: enrollment gets ensured for the STUDENT whose roll
        // number this is, not the staff member who uploaded it on their
        // behalf — CreatedByUserId and the enrolled student are different
        // people, and it's easy to accidentally wire the wrong id here.
        enrollmentService.Verify(e => e.EnsureEnrolledAsync(studentUserId, classRoomId), Times.Once);
    }

    [Fact]
    public async Task CreateSubmissionAsync_DisallowedFileExtension_ThrowsBeforeUploading()
    {
        // ARRANGE: a .exe disguised with a normal-looking name — extension
        // check happens first, before anything touches file storage.
        var (sut, _, fileStorage, _, _) = CreateSut();
        var file = FakeFile("virus.exe", 1024);

        var request = new CreateSubmissionRequest
        {
            FullName = "Test",
            ClassRoomId = Guid.NewGuid(),
            RollNo = 1,
            File = file.Object,
            CreatedByUserId = Guid.NewGuid(),
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateSubmissionAsync(request));

        // The file must never reach disk for a rejected extension.
        fileStorage.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>()), Times.Never);
    }

    [Fact]
    public async Task CreateSubmissionAsync_FileTooLarge_Throws()
    {
        // ARRANGE: a valid extension, but over the 5 MB limit.
        var (sut, _, fileStorage, _, _) = CreateSut();
        var file = FakeFile("big-scan.pdf", 6 * 1024 * 1024); // 6 MB

        var request = new CreateSubmissionRequest
        {
            FullName = "Test",
            ClassRoomId = Guid.NewGuid(),
            RollNo = 1,
            File = file.Object,
            CreatedByUserId = Guid.NewGuid(),
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateSubmissionAsync(request));
        fileStorage.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>()), Times.Never);
    }
}