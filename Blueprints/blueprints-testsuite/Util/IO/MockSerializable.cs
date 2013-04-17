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
        string _TestField;

        public string GetTestField() 
        {
            return _TestField;
        }

        public void setTestField(string testField)
        {
            _TestField = testField;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            else if (obj == null) return false;
            else if (!GetType().IsInstanceOfType(obj)) return false;
            MockSerializable m = (MockSerializable)obj;
            if (_TestField == null)
                return (m._TestField == null);
            else
                return _TestField == m._TestField;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
