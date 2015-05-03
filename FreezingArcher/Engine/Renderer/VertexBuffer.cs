using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;


namespace FreezingArcher.Renderer
{
    public class VertexBuffer : GraphicsResource
    {
        public int SizeInBytes { get; private set; }
        public RendererBufferUsage BufferUsage { get; private set; }

        internal VertexBuffer(string name, int id, int sizeinbytes, RendererBufferUsage rbu) : base(name, id, GraphicsResourceType.VertexBuffer)
        {
            SizeInBytes = sizeinbytes;
            BufferUsage = rbu;
        }

        public void BindBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);
        }

        public void UnbindBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void UnmapBuffer()
        {
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public IntPtr MapBuffer(int offset, int size, RendererBufferAccess rba)
        {
            if (!Created)
                throw new Exception("Resource is not created!");

            BufferAccessMask bam = BufferAccessMask.MapUnsynchronizedBit;

            if (rba == RendererBufferAccess.ReadWrite)
            {
                bam |= BufferAccessMask.MapReadBit | BufferAccessMask.MapWriteBit;
                bam &= ~BufferAccessMask.MapUnsynchronizedBit;
            }
            else if (rba == RendererBufferAccess.WriteOnly)
                bam |= BufferAccessMask.MapWriteBit;
            else if (rba == RendererBufferAccess.ReadOnly)
            {
                bam |= BufferAccessMask.MapReadBit;
                bam &= ~BufferAccessMask.MapUnsynchronizedBit;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);

            return GL.MapBufferRange(BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)size, bam);
        }

        public void UpdateBuffer<T>(T[] data, int size) where T : struct
        {
            if (!Created)
                throw new Exception("Resource is not created!");

            if (size != SizeInBytes)
                throw new Exception("Size does not match buffer size!");

            GL.BindBuffer(BufferTarget.ArrayBuffer, ID);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)SizeInBytes, IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BufferData<T>(BufferTarget.ArrayBuffer, (IntPtr)SizeInBytes, data, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
