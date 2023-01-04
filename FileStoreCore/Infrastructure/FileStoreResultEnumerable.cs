using System.Collections;
using Microsoft.EntityFrameworkCore.Storage;

namespace FileStoreCore.Infrastructure;

public class FileStoreResultEnumerable : IEnumerable<ValueBuffer>
{
    private readonly Func<ValueBuffer> _getElement;

    public FileStoreResultEnumerable(Func<ValueBuffer> getElement)
    {
        _getElement = getElement;
    }

    public IEnumerator<ValueBuffer> GetEnumerator() => new FileStoreResultEnumerator(_getElement());


    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    private sealed class FileStoreResultEnumerator : IEnumerator<ValueBuffer>
    {
        private readonly ValueBuffer _value;
        private bool _moved;

        public FileStoreResultEnumerator(ValueBuffer value)
        {
            _value = value;
            _moved = _value.IsEmpty;
        }

        public bool MoveNext()
        {
            if (!_moved)
            {
                _moved = true;

                return _moved;
            }

            return false;
        }

        public void Reset()
        {
            _moved = false;
        }

        object IEnumerator.Current => Current;

        public ValueBuffer Current => !_moved ? ValueBuffer.Empty : _value;

        void IDisposable.Dispose()
        {
        }
    }
}