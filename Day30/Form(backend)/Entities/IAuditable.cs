namespace Form.Entities
{
    public interface IAuditable
    {
        // Any entity that wants automatic CreatedAt/UpdatedAt stamping implements this.

        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset? UpdatedAt { get; set; }
    }
}
