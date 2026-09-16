namespace Form.Entities
    {
        public class Subject
        {
            public Guid Id { get; set; }
            public Guid ClassRoomId { get; set; }

            public string Name { get; set; } = string.Empty;

        public bool IsArchived { get; set; }

        public DateTimeOffset? Archieved { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

            public ClassRoom ClassRoom { get; set; } = null!;
            public ICollection<Grade> Grades { get; set; } = new List<Grade>();



        }
    }
