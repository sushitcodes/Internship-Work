namespace Form.DTOs
{
    public class ClassRoomDto
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int StudentCount { get; set; }
    }
    public class CreateClassRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
    }
}

