using System;
using System.Collections.Generic;
using System.Text;

namespace FileStoreCore.Example.Data.Entities
{
    public class GenericTest<T> : Base
    {
        public T Value { get; set; }

    }
}
