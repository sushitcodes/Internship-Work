namespace SchoolManagementSystem.Models
{
    // ABSTRACT CLASS — unlike an interface, this CAN hold shared implementation
    // (the constructor, GetSummary's shared parts) but still can't be
    // instantiated directly ("new Person(...)" is illegal). It exists purely
    // to be inherited from.
    public abstract class Person
    {
        public int Id { get; }               // ENCAPSULATION — read-only from outside
        public string Name { get; private set; }
        public int Age { get; private set; }

        // CONSTRUCTOR
        protected Person(int id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public void UpdateAge(int newAge)
        {
            if (newAge < 0) throw new ArgumentOutOfRangeException(nameof(newAge));
            Age = newAge;
        }

        // ABSTRACT METHOD — every subclass MUST provide its own version.
        // This is the hook that makes polymorphism possible below.
        public abstract string GetSummary();

        // VIRTUAL METHOD — has a default implementation, but subclasses MAY override it.
        public virtual string GetGreeting() => $"Hello, I'm {Name}.";
    }
}
