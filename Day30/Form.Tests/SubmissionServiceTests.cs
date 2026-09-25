using Form.DTOs;
using Form.Entities;
using Form.Interface;
using Form.Interfaces;
using Form.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Form.Tests.Services;

public class SubmissionServiceTests
{
    // One place to build every dependency, so each test only sets up what
    // it actually needs. Returns all six mocks — tests destructure with
    // underscore discards for the ones they don't care about.
    private static (
        SubmissionService sut,
        Mock<ISubmissionRepository> repo,
        Mock<IFileStorageService> fileStorage,
        Mock<IUserProfileRepository> profileRepo,
        Mock<IEnrollmentService> enrollmentService,
        Mock<INotificationService> notificationService,
        Mock<IClassRoomRepository> classRoomRepo) CreateSut()
    {
        var repo = new Mock<ISubmissionRepository>();
        var fileStorage = new Mock<IFileStorageService>();
        var profileRepo = new Mock<IUserProfileRepository>();
        var enrollmentService = new Mock<IEnrollmentService>();
        var notificationService = new Mock<INotificationService>();
        var classRoomRepo = new Mock<IClassRoomRepository>();

        var sut = new SubmissionService(
            repo.Object,
            fileStorage.Object,
            profileRepo.Object,
            enrollmentService.Object,
            notificationService.Object,
            classRoomRepo.Object);

        return (sut, repo, fileStorage, profileRepo, enrollmentService,
                notificationService, classRoomRepo);
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

    // ------------------------------------------------------------------
    // Validation: bad roll number
    // ------------------------------------------------------------------
    [Fact]
    public async Task CreateSubmissionAsync_UnknownRollNo_ThrowsAndNeverSaves()
    {
        // ARRANGE: a valid-looking file, but no UserProfile has this
        // RollNo — this is the "silent orphaned record" case the code
        // comment explicitly calls out as the reason for this check.
        var (sut, repo, fileStorage, profileRepo, enrollmentService, _, _) =
            CreateSut();

        var file = FakeFile("resume.pdf", 1024);
        fileStorage.Setup(f => f.SaveFileAsync(file.Object))
                   .ReturnsAsync("uploads/fake-guid.pdf");
        profileRepo.Setup(p => p.GetByMemberNumberAsync(9999))
                   .ReturnsAsync((UserProfile?)null);

        var request = new CreateSubmissionRequest
        {
            FullName = "Nobody",
            ClassRoomId = Guid.NewGuid(),
            RollNo = 9999,
            File = file.Object,
            CreatedByUserId = Guid.NewGuid(),
        };

        // ACT + ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateSubmissionAsync(request));

        // Nothing about this attempt should have been persisted or
        // triggered enrollment — the file was uploaded before the roll
        // number check runs (that ordering is worth knowing, but the
        // DB record itself must never be created for a bad roll number).
        repo.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Never);
        enrollmentService.Verify(
            e => e.EnsureEnrolledAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    // Happy path: valid roll number, no class teacher assigned
    [Fact]
    public async Task CreateSubmissionAsync_ValidRollNo_EnsuresEnrollmentForCorrectStudent()
    {
        // ARRANGE: this time the roll number matches a real profile.
        var studentUserId = Guid.NewGuid();
        var classRoomId = Guid.NewGuid();
        var profile = new UserProfile { UserId = studentUserId, MemberNumber = 42 };

        var (sut, repo, fileStorage, profileRepo, enrollmentService, _, classRoomRepo) =
            CreateSut();

        var file = FakeFile("resume.pdf", 1024);
        fileStorage.Setup(f => f.SaveFileAsync(file.Object))
                   .ReturnsAsync("uploads/fake-guid.pdf");
        profileRepo.Setup(p => p.GetByMemberNumberAsync(42))
                   .ReturnsAsync(profile);
        repo.Setup(r => r.AddAsync(It.IsAny<Submission>()))
            .ReturnsAsync((Submission s) => s); // echo back, like EF after saving

        // No teachers assigned → notification branch is skipped, keeping
        // this test focused on the enrollment behavior it's named for.
        classRoomRepo
            .Setup(c => c.GetTeacherUserIdsAsync(classRoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());

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
        enrollmentService.Verify(
            e => e.EnsureEnrolledAsync(studentUserId, classRoomId),
            Times.Once);
    }

    // Validation: bad file extension
    [Fact]
    public async Task CreateSubmissionAsync_DisallowedFileExtension_ThrowsBeforeUploading()
    {
        // ARRANGE: a .exe disguised with a normal-looking name — extension
        // check happens first, before anything touches file storage.
        var (sut, _, fileStorage, _, _, _, _) = CreateSut();
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
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateSubmissionAsync(request));

        // The file must never reach disk for a rejected extension.
        fileStorage.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>()), Times.Never);
    }

    // Validation: oversized file
    [Fact]
    public async Task CreateSubmissionAsync_FileTooLarge_Throws()
    {
        // ARRANGE: a valid extension, but over the 5 MB limit.
        var (sut, _, fileStorage, _, _, _, _) = CreateSut();
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
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateSubmissionAsync(request));
        fileStorage.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>()), Times.Never);
    }

    // Notification: submission with an assigned class teacher
    [Fact]
    public async Task CreateSubmissionAsync_WithClassTeacher_NotifiesThatTeacher()
    {
        // ARRANGE: a normal submission, with one teacher assigned to the
        // class. This test is the one that pins down the whole wiring:
        //   - recipients are resolved from the class, not from the caller
        //   - the notification carries the class name, not a placeholder
        //   - the submissionId passed in is the one that was actually saved
        var studentUserId = Guid.NewGuid();
        var teacherUserId = Guid.NewGuid();
        var classRoomId = Guid.NewGuid();
        var savedSubmissionId = Guid.NewGuid();

        var profile = new UserProfile { UserId = studentUserId, MemberNumber = 42 };

        var (sut, repo, fileStorage, profileRepo, _, notificationService, classRoomRepo) =
            CreateSut();

        var file = FakeFile("resume.pdf", 1024);
        fileStorage.Setup(f => f.SaveFileAsync(file.Object))
                   .ReturnsAsync("uploads/fake-guid.pdf");
        profileRepo.Setup(p => p.GetByMemberNumberAsync(42))
                   .ReturnsAsync(profile);

        // The repository "saves" and gives back the submission with a
        // concrete Id — set that Id explicitly so the assertion below can
        // check it was forwarded correctly.
        repo.Setup(r => r.AddAsync(It.IsAny<Submission>()))
            .ReturnsAsync((Submission s) =>
            {
                s.Id = savedSubmissionId;
                return s;
            });

        classRoomRepo
            .Setup(c => c.GetTeacherUserIdsAsync(classRoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { teacherUserId });
        classRoomRepo
            .Setup(c => c.GetNameAsync(classRoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("CS101");

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

        // ASSERT: the notification was sent to the CLASS TEACHER, with
        // the class name in the body and the actual saved submission id.
        notificationService.Verify(
            n => n.NotifyNewSubmissionAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(teacherUserId)),
                "Real Student",
                "CS101",
                savedSubmissionId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}