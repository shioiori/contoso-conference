namespace Eventbox.Shared.SeedWorks
{
    public abstract class Entity<T>
    {
        public T Id { get; set; }
        protected Entity() { }        
        protected Entity(T id) => Id = id;
    }
}
