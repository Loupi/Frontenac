using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO
{
    [Serializable]
    public class MockSerializable
    {
        string _testField;

        public string getTestField() 
        {
            return _testField;
        }

        public void setTestField(string testField)
        {
            _testField = testField;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            else if (obj == null) return false;
            else if (!GetType().IsInstanceOfType(obj)) return false;
            MockSerializable m = (MockSerializable)obj;
            if (_testField == null)
                return (m._testField == null);
            else
                return _testField == m._testField;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
