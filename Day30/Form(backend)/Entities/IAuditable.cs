namespace Form.Entities
{
    public interface IAuditable
    {
        // Any entity that wants automatic CreatedAt/UpdatedAt stamping implements this.

        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
