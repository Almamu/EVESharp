namespace Marshal
{

    public enum MarshalOpcode
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
        Dict,
        Object = 23,
        SubStruct = 25,
        SavedStreamElement = 27,
        ChecksumedStream,
        BoolTrue = 31,
        BoolFalse,
        ObjectEx1 = 34,
        ObjectEx2,
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
