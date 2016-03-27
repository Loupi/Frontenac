using System;

namespace Frontenac.Blueprints.Util.IO
{
    [Serializable]
    public class MockSerializable
    {
        public MockSerializable(string testField)
        {
            TestField = testField;
        }

        public string TestField { get; }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (!GetType().IsInstanceOfType(obj)) return false;
            var m = (MockSerializable) obj;
            if (TestField == null)
                return m.TestField == null;
            return TestField == m.TestField;
        }

        public override int GetHashCode()
        {
            return TestField.GetHashCode();
        }
    }
}