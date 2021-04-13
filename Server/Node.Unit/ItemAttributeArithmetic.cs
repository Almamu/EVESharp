using Node.Inventory.Items.Attributes;
using NUnit.Framework;
using AttributeInfo = Node.StaticData.Inventory.Attribute;

namespace Node.Unit
{
    public class Tests
    {
        private const int FIRST_VALUE_INT = 50;
        private const int SECOND_VALUE_INT = 20;
        private const double FIRST_VALUE_DOUBLE = 50.5;
        private const double SECOND_VALUE_DOUBLE = 20.5;
        
        private AttributeInfo mAttribute;
        
        [SetUp]
        public void Setup()
        {
            // build a attributeinfo so the arithmetics can be tested
            mAttribute = new AttributeInfo(
                0, "Test", 0, "Test attribute", null, 0, 0, 0, 0.0, 1, "Test", 0, 1, 1, 0
            );
        }

        [Test]
        public void ItemAttributeArithmetic_Integer()
        {
            // build two attributes with integer values only
            Attribute first = new Attribute(mAttribute, FIRST_VALUE_INT);
            Attribute second = new Attribute(mAttribute, SECOND_VALUE_INT);
            
            // do some calculations
            Attribute addResult = first + second;
            Attribute subResult = first - second;
            Attribute mulResult = first * second;
            Attribute divResult = first / second;
            
            // now test expected results
            
            // first check typing
            Assert.AreEqual(Attribute.ItemAttributeValueType.Integer, addResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Integer, subResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Integer, mulResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, divResult.ValueType);
            
            // finally check values
            Assert.AreEqual(FIRST_VALUE_INT + SECOND_VALUE_INT, addResult.Integer);
            Assert.AreEqual(FIRST_VALUE_INT - SECOND_VALUE_INT, subResult.Integer);
            Assert.AreEqual(FIRST_VALUE_INT * SECOND_VALUE_INT, mulResult.Integer);
            Assert.AreEqual(FIRST_VALUE_INT / (double) SECOND_VALUE_INT, divResult.Float);
        }

        [Test]
        public void ItemAttributeArithmetic_Double()
        {
            // build two attributes with double values only
            Attribute first = new Attribute(mAttribute, FIRST_VALUE_DOUBLE);
            Attribute second = new Attribute(mAttribute, SECOND_VALUE_DOUBLE);
            
            // do some calculations
            Attribute addResult = first + second;
            Attribute subResult = first - second;
            Attribute mulResult = first * second;
            Attribute divResult = first / second;
            
            // now test expected results
            
            // first check typing
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, addResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, subResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, mulResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, divResult.ValueType);
            
            // finally check values
            Assert.AreEqual(FIRST_VALUE_DOUBLE + SECOND_VALUE_DOUBLE, addResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE - SECOND_VALUE_DOUBLE, subResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE * SECOND_VALUE_DOUBLE, mulResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE / SECOND_VALUE_DOUBLE, divResult.Float);
        }

        [Test]
        public void ItemAttributeArithmetic_DoubleInteger()
        {
            // build two attributes with double values only
            Attribute first = new Attribute(mAttribute, FIRST_VALUE_DOUBLE);
            Attribute second = new Attribute(mAttribute, SECOND_VALUE_INT);
            
            // do some calculations
            Attribute addResult = first + second;
            Attribute subResult = first - second;
            Attribute mulResult = first * second;
            Attribute divResult = first / second;
            
            // now test expected results
            
            // first check typing
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, addResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, subResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, mulResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, divResult.ValueType);
            
            // finally check values
            Assert.AreEqual(FIRST_VALUE_DOUBLE + SECOND_VALUE_INT, addResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE - SECOND_VALUE_INT, subResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE * SECOND_VALUE_INT, mulResult.Float);
            Assert.AreEqual(FIRST_VALUE_DOUBLE / SECOND_VALUE_INT, divResult.Float);
        }

        [Test]
        public void ItemAttributeArithmetic_IntegerDouble()
        {
            // build two attributes with double values only
            Attribute first = new Attribute(mAttribute, FIRST_VALUE_INT);
            Attribute second = new Attribute(mAttribute, SECOND_VALUE_DOUBLE);
            
            // do some calculations
            Attribute addResult = first + second;
            Attribute subResult = first - second;
            Attribute mulResult = first * second;
            Attribute divResult = first / second;
            
            // now test expected results
            
            // first check typing
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, addResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, subResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, mulResult.ValueType);
            Assert.AreEqual(Attribute.ItemAttributeValueType.Double, divResult.ValueType);
            
            // finally check values
            Assert.AreEqual(FIRST_VALUE_INT + SECOND_VALUE_DOUBLE, addResult.Float);
            Assert.AreEqual(FIRST_VALUE_INT - SECOND_VALUE_DOUBLE, subResult.Float);
            Assert.AreEqual(FIRST_VALUE_INT * SECOND_VALUE_DOUBLE, mulResult.Float);
            Assert.AreEqual(FIRST_VALUE_INT / SECOND_VALUE_DOUBLE, divResult.Float);
        }
    }
}