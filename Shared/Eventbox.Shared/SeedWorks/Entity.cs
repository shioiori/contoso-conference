namespace Eventbox.Shared.SeedWorks
{
    public abstract class Entity<T>(T id)
    {
        public T Id { get; set; } = id;
    }
}
