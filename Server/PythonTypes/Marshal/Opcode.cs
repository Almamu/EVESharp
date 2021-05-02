namespace PythonTypes.Marshal
{
    /// <summary>
    /// List of opcodes the marshal/unmarshal functions can use to read/write data to communicate
    /// with/from python
    /// </summary>
    public enum Opcode
    {
        Error,
        None,
        Token,
        IntegerLongLong,
        IntegerLong,
        IntegerSignedShort,
        IntegerByte,
        IntegerMinusOne,
        IntegerZero,
        IntegerOne,
        Real,
        RealZero,
        Buffer = 13,
        StringEmpty,
        StringChar,
        StringShort,
        StringTable,
        WStringUCS2,
        StringLong,
        Tuple,
        List,
        Dictionary,
        ObjectData = 23,
        BlueWrapped = 24,
        SubStruct = 25,
        SavedStreamElement = 27,
        ChecksumedStream,
        BoolTrue = 31,
        BoolFalse,
        Pickle = 33,
        ObjectType1 = 34,
        ObjectType2,
        TupleEmpty,
        TupleOne,
        ListEmpty,
        ListOne,
        WStringEmpty,
        WStringUCS2Char,
        PackedRow,
        SubStream,
        TupleTwo,
        WStringUTF8 = 46,
        IntegerVar
    }
}