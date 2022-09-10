using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;

namespace TestExtensions;

public static class PyAssert
{
    /// <summary>
    /// Asserts the object is an integer and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyInteger Integer (object data, long value)
    {
        PyInteger val = Type <PyInteger> (data);
        Assert.AreEqual (value, val.Value);
        return val;
    }

    /// <summary>
    /// Asserts the integer has the right value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public static void Integer (PyInteger data, long value)
    {
        Assert.AreEqual (value, data.Value);
    }
    /// <summary>
    /// Asserts the object is an integer and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyBool Bool (object data, bool value)
    {
        PyBool val = Type <PyBool> (data);
        Assert.AreEqual (value, val.Value);
        return val;
    }

    /// <summary>
    /// Asserts the integer has the right value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public static void Bool (PyBool data, bool value)
    {
        Assert.AreEqual (value, data.Value);
    }
    
    /// <summary>
    /// Asserts the object is a decimal and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyDecimal Decimal (object data, double value)
    {
        PyDecimal val = Type <PyDecimal> (data);
        Assert.AreEqual (value, val.Value);
        return val;
    }

    /// <summary>
    /// Asserts the decimal has the right value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public static void Decimal (PyDecimal data, long value)
    {
        Assert.AreEqual (value, data.Value);
    }
    
    /// <summary>
    /// Asserts the object is a string and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyString String (object data, string value)
    {
        PyString str = Type <PyString> (data);
        Assert.AreEqual (value, str.Value);

        return str;
    }

    /// <summary>
    /// Asserts the string has the right value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public static void String (PyString data, string value)
    {
        Assert.AreEqual (value, data.Value);
    }
    
    /// <summary>
    /// Asserts the object is a string and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyToken Token (object data, string value)
    {
        PyToken str = Type <PyToken> (data);
        Assert.AreEqual (value, str.Token);

        return str;
    }

    /// <summary>
    /// Asserts the string has the right value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public static void Token (PyToken data, string value)
    {
        Assert.AreEqual (value, data.Token);
    }

    /// <summary>
    /// Asserts the object is a string and has the given value, being a part of the string table
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <param name="isTableEntry"></param>
    /// <returns></returns>
    public static PyString String (object data, string value, bool isTableEntry)
    {
        PyString str = String (data, value);

        Assert.AreEqual (isTableEntry, str.IsStringTableEntry);

        return str;
    }

    /// <summary>
    /// Asserts the string has the right value and status
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <param name="isTableEntry"></param>
    public static void String (PyString data, string value, bool isTableEntry)
    {
        String (data, value);

        Assert.AreEqual (isTableEntry, data.IsStringTableEntry);
    }
    
    /// <summary>
    /// Asserts the object is a buffer and has the given value
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static PyBuffer Buffer (object data, byte[] value)
    {
        PyBuffer buffer = Type <PyBuffer> (data);

        Assert.AreEqual (value, buffer.Value);
        
        return buffer;
    }

    /// <summary>
    /// Asserts the buffer has the right value
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    public static void Buffer (PyBuffer buffer, byte [] value)
    {
        Assert.AreEqual (value, buffer.Value);
    }

    /// <summary>
    /// Asserts the object is of the given PyDataType type
    /// </summary>
    /// <param name="data">The object to check the type of</param>
    /// <param name="acceptNull">If the object can be null or not (PyNone)</param>
    /// <typeparam name="TExpected"></typeparam>
    public static TExpected Type <TExpected> (object? data, bool acceptNull = false) where TExpected : PyDataType
    {
        if (!acceptNull)
            Assert.IsNotNull (data);
        
        if (data is not null)
            Assert.IsInstanceOf <TExpected> (data);

        return data as TExpected;
    }

    /// <summary>
    /// Asserts the object is a tuple
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => not checked</param>
    public static PyTuple Tuple (object data, int? count = null)
    {
        PyTuple enumerable = Type <PyTuple> (data);

        if (count is not null)
            Assert.AreEqual (count, enumerable.Count);

        return enumerable;
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least one</param>
    public static T1 Tuple <T1> (object data, bool acceptNulls = true, int? count = null)
        where T1 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (1, count);

        PyTuple tuple = Tuple (data, count);

        return Type <T1> (tuple [0], acceptNulls);
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least two</param>
    public static (T1, T2) Tuple <T1, T2> (object data, bool acceptNulls = true, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (2, count);
        
        PyTuple tuple    = Tuple (data, count);
        T1      element1 = Type <T1> (tuple [0], acceptNulls);
        T2      element2 = Type <T2> (tuple [1], acceptNulls);

        return (
            element1,
            element2
        );
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least three</param>
    public static (T1, T2, T3) Tuple <T1, T2, T3> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (3, count);
        
        PyTuple tuple    = Tuple (data, count);
        T1      element1 = Type <T1> (tuple [0], acceptNulls);
        T2      element2 = Type <T2> (tuple [1], acceptNulls);
        T3      element3 = Type <T3> (tuple [2], acceptNulls);

        return (
            element1,
            element2,
            element3
        );
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least four</param>
    public static (T1, T2, T3, T4) Tuple <T1, T2, T3, T4> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (4, count);
        
        PyTuple tuple    = Tuple (data, count);
        T1      element1 = Type <T1> (tuple [0], acceptNulls);
        T2      element2 = Type <T2> (tuple [1], acceptNulls);
        T3      element3 = Type <T3> (tuple [2], acceptNulls);
        T4      element4 = Type <T4> (tuple [3], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4
        );
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least five</param>
    public static (T1, T2, T3, T4, T5) Tuple <T1, T2, T3, T4, T5> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
        where T5 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (5, count);
        
        PyTuple tuple    = Tuple (data, count);
        T1      element1 = Type <T1> (tuple [0], acceptNulls);
        T2      element2 = Type <T2> (tuple [1], acceptNulls);
        T3      element3 = Type <T3> (tuple [2], acceptNulls);
        T4      element4 = Type <T4> (tuple [3], acceptNulls);
        T5      element5 = Type <T5> (tuple [4], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4,
            element5
        );
    }
    
    /// <summary>
    /// Asserts the object is a tuple and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least six</param>
    public static (T1, T2, T3, T4, T5, T6) Tuple <T1, T2, T3, T4, T5, T6> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
        where T5 : PyDataType
        where T6 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (6, count);
        
        PyTuple tuple    = Tuple (data, count);
        T1      element1 = Type <T1> (tuple [0], acceptNulls);
        T2      element2 = Type <T2> (tuple [1], acceptNulls);
        T3      element3 = Type <T3> (tuple [2], acceptNulls);
        T4      element4 = Type <T4> (tuple [3], acceptNulls);
        T5      element5 = Type <T5> (tuple [4], acceptNulls);
        T6      element6 = Type <T6> (tuple [5], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4,
            element5,
            element6
        );
    }

    /// <summary>
    /// Asserts the object is a List
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => not checked</param>
    public static PyList List (object data, int? count = null)
    {
        PyList enumerable = Type <PyList> (data);

        if (count is not null)
            Assert.AreEqual (count, enumerable.Count);

        return enumerable;
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least one</param>
    public static T1 List <T1> (object data, bool acceptNulls = true, int? count = null)
        where T1 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (1, count);

        PyList list = List (data, count);

        return Type <T1> (list [0], acceptNulls);
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least two</param>
    public static (T1, T2) List <T1, T2> (object data, bool acceptNulls = true, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (2, count);

        PyList list     = List (data, count);
        T1     element1 = Type <T1> (list [0], acceptNulls);
        T2     element2 = Type <T2> (list [1], acceptNulls);

        return (
            element1,
            element2
        );
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least three</param>
    public static (T1, T2, T3) List <T1, T2, T3> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (3, count);
        
        PyList list     = List (data, count);
        T1     element1 = Type <T1> (list [0], acceptNulls);
        T2     element2 = Type <T2> (list [1], acceptNulls);
        T3     element3 = Type <T3> (list [2], acceptNulls);

        return (
            element1,
            element2,
            element3
        );
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least four</param>
    public static (T1, T2, T3, T4) List <T1, T2, T3, T4> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (4, count);
        
        PyList list     = List (data, count);
        T1     element1 = Type <T1> (list [0], acceptNulls);
        T2     element2 = Type <T2> (list [1], acceptNulls);
        T3     element3 = Type <T3> (list [2], acceptNulls);
        T4     element4 = Type <T4> (list [3], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4
        );
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least five</param>
    public static (T1, T2, T3, T4, T5) List <T1, T2, T3, T4, T5> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
        where T5 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (5, count);
        
        PyList list     = List (data, count);
        T1     element1 = Type <T1> (list [0], acceptNulls);
        T2     element2 = Type <T2> (list [1], acceptNulls);
        T3     element3 = Type <T3> (list [2], acceptNulls);
        T4     element4 = Type <T4> (list [3], acceptNulls);
        T5     element5 = Type <T5> (list [4], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4,
            element5
        );
    }
    
    /// <summary>
    /// Asserts the object is a List and the elements inside are of the right type
    ///
    /// Returns the data inside
    /// </summary>
    /// <param name="data"></param>
    /// <param name="count">The count of elements, null => ensure at least six</param>
    public static (T1, T2, T3, T4, T5, T6) List <T1, T2, T3, T4, T5, T6> (object data, bool acceptNulls = false, int? count = null)
        where T1 : PyDataType
        where T2 : PyDataType
        where T3 : PyDataType
        where T4 : PyDataType
        where T5 : PyDataType
        where T6 : PyDataType
    {
        if (count is not null)
            Assert.GreaterOrEqual (6, count);
        
        PyList list     = List (data, count);
        T1     element1 = Type <T1> (list [0], acceptNulls);
        T2     element2 = Type <T2> (list [1], acceptNulls);
        T3     element3 = Type <T3> (list [2], acceptNulls);
        T4     element4 = Type <T4> (list [3], acceptNulls);
        T5     element5 = Type <T5> (list [4], acceptNulls);
        T6     element6 = Type <T6> (list [5], acceptNulls);

        return (
            element1,
            element2,
            element3,
            element4,
            element5,
            element6
        );
    }

    /// <summary>
    /// Asserts the object is an ObjectData and that the name is right
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static PyObjectData ObjectData (object data, string? name = null)
    {
        PyObjectData objectData = Type <PyObjectData> (data);

        if (name is not null)
            String (objectData.Name, name);

        return objectData;
    }

    /// <summary>
    /// Asserts the object is an ObjectData and that the name is right
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static PyObjectData ObjectData (object data, string? name, bool isStringTable)
    {
        PyObjectData objectData = Type <PyObjectData> (data);

        if (name is not null)
            String (objectData.Name, name, isStringTable);

        return objectData;
    }

    /// <summary>
    /// Asserts the object is an ObjectData, the name is right and the arguments are of the given type
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <typeparam name="TArguments">The type of the arguments</typeparam>
    /// <returns></returns>
    public static TArguments ObjectData <TArguments> (object data, string? name = null) where TArguments : PyDataType
    {
        PyObjectData objectData = ObjectData (data, name);
        
        Assert.IsInstanceOf<TArguments> (objectData.Arguments);

        return objectData.Arguments as TArguments;
    }

    /// <summary>
    /// Asserts the object is an ObjectData, the name is right and the arguments are of the given type
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    /// <typeparam name="TArguments">The type of the arguments</typeparam>
    /// <returns></returns>
    public static TArguments ObjectData <TArguments> (object data, string? name, bool isStringTable) where TArguments : PyDataType
    {
        PyObjectData objectData = ObjectData (data, name, isStringTable);
        
        Assert.IsInstanceOf<TArguments> (objectData.Arguments);

        return objectData.Arguments as TArguments;
    }

    /// <summary>
    /// Asserts the object is an Object
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isType2">If the object has to be of type two or not</param>
    /// <returns></returns>
    public static PyObject Object (object data, bool isType2 = false)
    {
        PyObject obj = Type <PyObject> (data);

        Assert.AreEqual (isType2, obj.IsType2);
        
        return obj;
    }

    /// <summary>
    /// Asserts the object is a PyDictionary
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static PyDictionary Dict (object data)
    {
        return Type <PyDictionary> (data);
    }

    /// <summary>
    /// Ensures that the given key exists inside the dictionary and returns it's value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T DictKey <T> (PyDictionary dict, string key) where T : PyDataType
    {
        Assert.AreEqual (true, dict.ContainsKey (key));

        return Type <T> (dict [key]);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void DictInteger (PyDictionary dict, string key, int value)
    {
        Integer (DictKey <PyInteger> (dict, key), value);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void DictString (PyDictionary dict, string key, string value)
    {
        String (DictKey <PyString> (dict, key), value);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="isStringTableEntry"></param>
    public static void DictString (PyDictionary dict, string key, string value, bool isStringTableEntry)
    {
        String (DictKey <PyString> (dict, key), value, isStringTableEntry);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void DictDecimal (PyDictionary dict, string key, double value)
    {
        Decimal (DictKey <PyDecimal> (dict, key), value);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void DictToken (PyDictionary dict, string key, string value)
    {
        Token (DictKey <PyToken> (dict, key), value);
    }

    /// <summary>
    /// Ensures the given key exists and has the right value
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// public static PyObject Object (object data, bool isType2 = false)
    public static PyObject DictObject (PyDictionary dict, string key, bool isType2 = false)
    {
        return Object (DictKey <PyObject> (dict, key), isType2);
    }
}