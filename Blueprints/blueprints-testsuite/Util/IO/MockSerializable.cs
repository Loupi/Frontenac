using System;

namespace Frontenac.Blueprints.Util.IO
{
    [Serializable]
    public class MockSerializable
    {
        private readonly string _testField;

        public MockSerializable(string testField)
        {
            _testField = testField;
        }

        public string TestField
        {
            get { return _testField; }
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (!GetType().IsInstanceOfType(obj)) return false;
            var m = (MockSerializable) obj;
            if (_testField == null)
                return (m._testField == null);
            return _testField == m._testField;
        }

        public override int GetHashCode()
        {
            return _testField.GetHashCode();
        }
    }
}