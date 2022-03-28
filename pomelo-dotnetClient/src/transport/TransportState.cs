using System;

namespace Pomelo.DotNetClient
{

    public enum TransportState
    {
        readHead = 1,		// on read head
        readBody = 2,		// on read body
        closed = 3			// connection closed, will ignore all the message and wait for clean up
    }
    public class StateObject
    {
        private int BufferSize = 1024;
        internal byte[] buffer = null;

        public StateObject(int size) {
            BufferSize = size;
            buffer  = new byte[BufferSize];
        }
    }
}