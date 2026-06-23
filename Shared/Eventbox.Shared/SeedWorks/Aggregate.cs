namespace Eventbox.Shared.SeedWorks
{
    public abstract class Aggregate<T> : AuditableEntity<T>
    {
        protected Aggregate() { }
        protected Aggregate(T id) : base(id) { }
    }
}