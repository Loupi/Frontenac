using System.Collections.Generic;

namespace Grave.Entities
{
    public class Person
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public float Weight { get; set; }
        public IEnumerable<Person> Childs { get; set; }
        public Person Wife { get; set; }
    }
}
