namespace Grave.Entities
{
    public class Test
    {
        public Test(int day)
        {
            Day = day;
        }

        public int Number { get; set; }
        public string Name { get; set; }
        public int Day { get; private set; }
        public float Score { get; set; }
    }
}