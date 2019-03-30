using System;

namespace Mvvm.Sample
{
    public class Model
    {
        static int idCount = 0;

        public int Id { get; set; }
        public string Name { get; set; }

        public Model() => Id = idCount++;
        public Model(string name) : this() => Name = name;

        public override string ToString() => Name;
    }
}
