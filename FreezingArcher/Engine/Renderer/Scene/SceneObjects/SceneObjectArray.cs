﻿//
//  SceneObjectArray.cs
//
//  Author:
//       dboeg <${AuthorEmail}>
//
//  Copyright (c) 2015 dboeg
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
using System;
using System.Linq;
using System.Collections.Generic;

using FreezingArcher.Math;

namespace FreezingArcher.Renderer.Scene.SceneObjects
{
    public struct SceneObjectArrayInstanceData
    {
        public static int SIZE = sizeof(float) * 4 * 4 + sizeof(float) * 4 * 2;

        //Matrix
        float X1, X2, X3, X4;
        float Y1, Y2, Y3, Y4;
        float Z1, Z2, Z3, Z4;
        float W1, W2, W3, W4;

        //Vector4 1
        float V1X, V1Y, V1Z, V1W;

        //Vector4 2
        float V2X, V2Y, V2Z, V2W;

        public Matrix World
        {
            get
            {
                return new Matrix(X1, X2, X3, X4,
                    Y1, Y2, Y3, Y4,
                    Z1, Z2, Z3, Z4,
                    W1, W2, W3, W4);
            }

            set
            {
                X1 = value.M11;
                X2 = value.M12;
                X3 = value.M13;
                X4 = value.M14;

                Y1 = value.M21;
                Y2 = value.M22;
                Y3 = value.M23;
                Y4 = value.M24;

                Z1 = value.M31;
                Z2 = value.M32;
                Z3 = value.M33;
                Z4 = value.M34;

                W1 = value.M41;
                W2 = value.M42;
                W3 = value.M43;
                W4 = value.M44;
            }
        }

        public Vector4 Other1
        {
            get
            {
                return new Vector4(V1X, V1Y, V1Z, V1W);
            }

            set
            {
                V1X = value.X;
                V1Y = value.Y;
                V1Z = value.Z;
                V1W = value.W;
            }
        }

        public Vector4 Other2
        {
            get
            {
                return new Vector4(V2X, V2Y, V2Z, V2W);
            }

            set
            {
                V2X = value.X;
                V2Y = value.Y;
                V2Z = value.Z;
                V2W = value.W;
            }
        }
    }

    public class SceneObjectArray : SceneObject
    {
        private object ListLock = new object();
        private List<int> ObjectsChanged;

        public List<SceneObject> SceneObjects;
        public string ObjectName { get; private set;}

        public uint LayoutLocationOffset { get; set;}

        private RendererContext PrivateRendererContext;

        private VertexBuffer InstanceDataVertexBuffer = null;

        public SceneObjectArray(string object_name)
        {
            SceneObjects = new List<SceneObject>();

            ObjectsChanged = new List<int>();

            ObjectName = object_name;
        }
            
        public void AddObject(SceneObject obj)
        {
            if (obj.GetName() == ObjectName)
            {
                SceneObjects.Add(obj);
                obj.SceneObjectChanged += SceneObjectChangedHandler;

                PrepareBuffer();

                if (InstanceDataVertexBuffer != null)
                {
                    InstanceDataVertexBuffer.UpdateSubBuffer<SceneObjectArrayInstanceData>(new SceneObjectArrayInstanceData[]{obj.GetData()}, 
                        SceneObjects.IndexOf(obj) * SceneObjectArrayInstanceData.SIZE,
                        SceneObjectArrayInstanceData.SIZE);
                }
            }
            else
                FreezingArcher.Output.Logger.Log.AddLogEntry(FreezingArcher.Output.LogLevel.Error, "SceneObjectArray", 
                    FreezingArcher.Core.Status.YouShallNotPassNull, "Object name does not match");
        }
         

        private void PrepareBuffer()
        {
            if (InstanceDataVertexBuffer == null)
            {
                if (SceneObjects.Count > 0)
                {
                    InstanceDataVertexBuffer = PrivateRendererContext.CreateVertexBuffer(IntPtr.Zero,
                        SceneObjectArrayInstanceData.SIZE, 
                        RendererBufferUsage.DynamicDraw,"SceneObjectArrayInstanceData_VBO_" + ObjectName + "_" + DateTime.Now.Ticks);
                }

                SceneObjectArrayInstanceData[] data = CollectData();

                InstanceDataVertexBuffer.Clear();

                InstanceDataVertexBuffer.UpdateSubBuffer<SceneObjectArrayInstanceData>(data, 0, data.Length * SceneObjectArrayInstanceData.SIZE);
            }
            else
            {
                if ((SceneObjects.Count * SceneObjectArrayInstanceData.SIZE) > InstanceDataVertexBuffer.SizeInBytes)
                {
                    int actual_size = InstanceDataVertexBuffer.SizeInBytes;

                    PrivateRendererContext.DeleteGraphicsResource(InstanceDataVertexBuffer);

                    if (SceneObjects.Count > 0)
                    {
                        InstanceDataVertexBuffer = PrivateRendererContext.CreateVertexBuffer(IntPtr.Zero, actual_size * 2, 
                           RendererBufferUsage.DynamicDraw, "SceneObjectArrayInstanceData_VBO_" + ObjectName + "_" + DateTime.Now.Ticks);
                    }

                    SceneObjectArrayInstanceData[] data = CollectData();

                    InstanceDataVertexBuffer.Clear();

                    InstanceDataVertexBuffer.UpdateSubBuffer<SceneObjectArrayInstanceData>(data, 0, data.Length * SceneObjectArrayInstanceData.SIZE);
                }
            }
        }
            
        private SceneObjectArrayInstanceData[] CollectData()
        {
            List<SceneObjectArrayInstanceData> data = new List<SceneObjectArrayInstanceData>();

            SceneObjects.ForEach(item => data.Add(item.GetData()));

            return data.ToArray();
        }

        public override bool Init(RendererContext rc)
        {
            PrivateRendererContext = rc;

            return true;
        }

        public bool SceneObjectChangedHandler(SceneObject obj)
        {
            //Different thread.... It is all fucking broken!
            lock (ListLock)
            {
                int index = SceneObjects.IndexOf(obj);
                ObjectsChanged.Add(index);
            }

            return true;
        }

        public override void Update()
        {
            lock (ListLock)
            {
                foreach (int index in ObjectsChanged)
                {
                    SceneObjectArrayInstanceData[] data = new SceneObjectArrayInstanceData[] { SceneObjects[index].GetData() };

                    if (InstanceDataVertexBuffer != null)
                    {
                        InstanceDataVertexBuffer.UpdateSubBuffer<SceneObjectArrayInstanceData>(data, index * SceneObjectArrayInstanceData.SIZE,
                            SceneObjectArrayInstanceData.SIZE);
                    }
                }
            }
        }

        public override void Draw(RendererContext rc)
        {
            //Preparing
            if (InstanceDataVertexBuffer != null && SceneObjects.Count > 0)
            {
                InstanceDataVertexBuffer.BindBuffer();

                VertexBufferLayoutKind[] vblk = new VertexBufferLayoutKind[6];

                //Set information
                for (int i = 0; i < 4; i++)
                {
                    VertexBufferLayoutKind v = new VertexBufferLayoutKind();
                    v.AttributeID = (uint)LayoutLocationOffset + (uint)i;
                    v.AttributeSize = 4;
                    v.AttributeType = RendererVertexAttribType.Float;
                    v.Normalized = false;
                    v.Offset = sizeof(float) * 4 * i;
                    v.Stride = SceneObjectArrayInstanceData.SIZE;

                    vblk[i] = v;
                }

                vblk[4] = new VertexBufferLayoutKind()
                    {AttributeID = (uint) LayoutLocationOffset + 4, AttributeSize = 4, AttributeType = RendererVertexAttribType.Float,
                    Normalized = false, Offset = sizeof(float) * 4 * 4, Stride = SceneObjectArrayInstanceData.SIZE
                };

                vblk[5] = new VertexBufferLayoutKind()
                    {AttributeID = (uint) LayoutLocationOffset + 5, AttributeSize = 4, AttributeType = RendererVertexAttribType.Float,
                    Normalized = false, Offset = sizeof(float) * 4 * 4 + sizeof(float) * 4, Stride = SceneObjectArrayInstanceData.SIZE
                };

                //Send it to Renderer
                for (int i = 0; i < 6; i++)
                {
                    rc.EnableVertexAttribute((int)LayoutLocationOffset + i);
                    rc.VertexAttributePointer(vblk[i]);
                    rc.VertexAttributeDivisor((int)LayoutLocationOffset + i, 1);
                }

                SceneObjects[0].DrawInstanced(rc, SceneObjects.Count);

                //Disable all attributes used by instancing
                for (int i = 0; i < 6; i++)
                    rc.DisableVertexAttribute((int)LayoutLocationOffset + i);

                InstanceDataVertexBuffer.UnbindBuffer();
            }
        }

        public override void DrawInstanced(RendererContext rc, int count)
        {
            //Whooo, how should I draw myself multiple times? It would be crazy
        }

        public override string GetName()
        {
            return "SceneObjectArray";
        }
    }
}
