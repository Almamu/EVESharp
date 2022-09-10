using System.Collections.Generic;

namespace EVESharp.Types.Collections;

public interface IPyEnumerator <T> : IEnumerator <T> where T : PyDataType { }