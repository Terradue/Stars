using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Services
{
    public class BlockingStream : Stream
    {
        private object _lockForRead;
        private object _lockForAll;
        private Queue<byte[]> _chunks;
        private byte[] _currentChunk;
        private int _currentChunkPosition;
        private ManualResetEvent _doneWriting;
        private ManualResetEvent _dataAvailable;
        private WaitHandle[] _events;
        private int _doneWritingHandleIndex;
        private volatile bool _illegalToWrite;
        private volatile bool _writeClosed;
        private ulong? contentRequestLength;
        private readonly int _maxChunk;
        private ulong totalRead;


        public BlockingStream(int maxChunk = 100)
        {
            _chunks = new Queue<byte[]>();
            _doneWriting = new ManualResetEvent(false);
            _dataAvailable = new ManualResetEvent(false);
            _events = new WaitHandle[] { _dataAvailable, _doneWriting };
            _doneWritingHandleIndex = 1;
            _lockForRead = new object();
            _lockForAll = new object();
            this._maxChunk = maxChunk;
        }

        public BlockingStream(ulong? contentRequestLength, int maxChunk = 100) : this(maxChunk)
        {
            this.contentRequestLength = contentRequestLength;

        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return !_illegalToWrite; } }

        public override void Flush() { }
        public override long Length
        {
            get
            {
                // Console.Out.WriteLine("GetLength: " + contentRequestLength);
                if (!contentRequestLength.HasValue)
                    throw new NotSupportedException();
                return Convert.ToInt64(contentRequestLength);
            }
        }
        public override long Position
        {
            get { return Convert.ToInt64(totalRead); }
            set { throw new NotSupportedException(); }
        }

        public int MaxChunk => _maxChunk;

        public override long Seek(long offset, SeekOrigin origin)
        {
            // Console.Out.WriteLine("Seek: " + offset + " " + origin);
            return Convert.ToInt64(totalRead);
            // throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            // Console.Out.WriteLine("SetLength");
            contentRequestLength = Convert.ToUInt64(value);
        }

        public override int Read(byte[] buffer, int offset, int rcount)
        {
            // Console.Out.WriteLine(string.Format("Read: {0}, {1}, {2}", buffer.Length, offset, rcount));
            if (buffer.Length == 0 && rcount == 0)
            {
                Console.Out.WriteLine(string.Format("Read: {0}, {1}, {2}", buffer.Length, offset, rcount));
                return 0;
            }
            int count = rcount > 0 ? rcount : buffer.Length;
            int chunkReadCount = 0;
            int totalChunkRead = 0;
            int latestRead = 0;
            do
            {
                latestRead = ReadChunk(buffer, offset + totalChunkRead, count - totalChunkRead);
                chunkReadCount++;
                totalChunkRead += latestRead;
            }
            while (totalChunkRead < count && latestRead > 0);
            // Console.Out.WriteLine(string.Format("Read: {0}, {1}, {2} --> {3} ({4} : {5})", buffer.Length, offset, rcount, totalChunkRead, _chunks.Count, totalRead));
            totalRead += Convert.ToUInt64(totalChunkRead);
            return totalChunkRead;
        }

        private int ReadChunk(byte[] buffer, int offset, int rcount)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (rcount < 0 || offset + rcount > buffer.Length)
                throw new ArgumentOutOfRangeException("count");
            if (_dataAvailable == null)
                throw new ObjectDisposedException(GetType().Name);

            int count = rcount > 0 ? rcount : buffer.Length;

            if (count == 0) return 0;

            while (true)
            {
                int handleIndex = WaitHandle.WaitAny(_events, TimeSpan.FromMilliseconds(100));
                lock (_lockForRead)
                {
                    lock (_lockForAll)
                    {
                        if (_currentChunk == null)
                        {
                            if (_chunks.Count == 0)
                            {
                                if (handleIndex == _doneWritingHandleIndex)
                                    return 0;
                                else continue;
                            }
                            _currentChunk = _chunks.Dequeue();
                            _currentChunkPosition = 0;
                        }
                    }

                    int bytesAvailable =
                        _currentChunk.Length - _currentChunkPosition;
                    int bytesToCopy;
                    if (bytesAvailable > count)
                    {
                        bytesToCopy = count;
                        Buffer.BlockCopy(_currentChunk, _currentChunkPosition,
                            buffer, offset, count);
                        _currentChunkPosition += count;
                    }
                    else
                    {
                        bytesToCopy = bytesAvailable;
                        Buffer.BlockCopy(_currentChunk, _currentChunkPosition,
                            buffer, offset, bytesToCopy);
                        _currentChunk = null;
                        _currentChunkPosition = 0;
                        lock (_lockForAll)
                        {
                            if (_chunks.Count == 0) _dataAvailable.Reset();
                        }
                    }
                    // Console.Out.WriteLine(string.Format("ReadChunk: {0}, {1}, {2} --> {3}", buffer.Length, offset, rcount, bytesToCopy));
                    return bytesToCopy;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Console.Out.WriteLine(string.Format("Write: {0}, {1}, {2}", buffer.Length, offset, count));
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");
            if (_dataAvailable == null)
                throw new ObjectDisposedException(GetType().Name);

            if (count == 0) return;

            byte[] chunk = new byte[count];
            Buffer.BlockCopy(buffer, offset, chunk, 0, count);
            while (true)
            {
                lock (_lockForAll)
                {
                    if (_chunks.Count >= _maxChunk)
                        continue;
                    if (_illegalToWrite)
                        throw new InvalidOperationException(
                            "Writing has already been completed.");
                    _chunks.Enqueue(chunk);
                    _dataAvailable.Set();
                    break;
                }
            }
        }

        public void SetEndOfStream()
        {
            if (_dataAvailable == null)
                throw new ObjectDisposedException(GetType().Name);
            lock (_lockForAll)
            {
                _illegalToWrite = true;
                _doneWriting.Set();
            }
        }

        public override void Close()
        {
            base.Close();
            if (_writeClosed)
            {

                if (_dataAvailable != null)
                {
                    _dataAvailable.Close();
                    _dataAvailable = null;
                }
                if (_doneWriting != null)
                {
                    _doneWriting.Close();
                    _doneWriting = null;
                }
            }
            else
            {
                lock (_lockForAll)
                {
                    SetEndOfStream();
                    _writeClosed = true;
                }
            }
        }

        protected String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount <= 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public override Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");
            if (!CanRead && !CanWrite)
                throw new ObjectDisposedException(null);
            if (!destination.CanRead && !destination.CanWrite)
                throw new ObjectDisposedException("destination");
            if (!CanRead)
                throw new NotSupportedException();
            if (!destination.CanWrite)
                throw new NotSupportedException();
 
            return CopyToAsyncInternal(destination, bufferSize, cancellationToken);
        }
 
        private async Task CopyToAsyncInternal(Stream destination, Int32 bufferSize, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            bytesRead = Read(buffer, 0, buffer.Length);
            while (bytesRead > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                bytesRead = Read(buffer, 0, buffer.Length);
            }
        }
    }

}