namespace Eventbox.Shared.SeedWorks
{ 
    public abstract class Aggregate<T>(T id) : AuditableEntity<T>(id);
}