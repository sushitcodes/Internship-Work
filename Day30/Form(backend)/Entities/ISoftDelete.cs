namespace Form.Entities
{
    // Any entity that wants "delete" to mean "hide" instead of "destroy" implements this.

    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
