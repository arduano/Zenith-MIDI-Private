﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Black_Midi_Render
{
    class NoteRender
    {
        RenderSettings settings;

        int noteShader;

        int vertexBufferID;
        int colorBufferID;
        int attrib1BufferID;

        int quadBufferLength = 2048;
        double[] quadVertexbuff;
        float[] quadColorbuff;
        double[] quadAttribbuff;
        int quadBufferPos = 0;

        int indexBufferId;
        uint[] indexes = new uint[2048 * 16];

        public NoteRender(RenderSettings rendersettings)
        {
            settings = rendersettings;
            noteShader = GLUtils.MakeShaderProgram(@"Shaders\notes");

            quadVertexbuff = new double[quadBufferLength * 8];
            quadColorbuff = new float[quadBufferLength * 16];
            quadAttribbuff = new double[quadBufferLength * 8];

            GL.GenBuffers(1, out vertexBufferID);
            GL.GenBuffers(1, out colorBufferID);
            GL.GenBuffers(1, out attrib1BufferID);
            for (uint i = 0; i < indexes.Length; i++) indexes[i] = i;
            for (int i = 0; i < quadAttribbuff.Length;)
            {

                quadAttribbuff[i++] = 0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = 0.5;
                quadAttribbuff[i++] = 0;
                quadAttribbuff[i++] = -0.3;
                quadAttribbuff[i++] = 0;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, attrib1BufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadAttribbuff.Length * 8),
                quadAttribbuff,
                BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out indexBufferId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                (IntPtr)(indexes.Length * 4),
                indexes,
                BufferUsageHint.StaticDraw);
        }

    
        void FlushQuadBuffer()
        {
            if (quadBufferPos == 0) return;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadVertexbuff.Length * 8),
                quadVertexbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorBufferID);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(quadColorbuff.Length * 4),
                quadColorbuff,
                BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 16, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, attrib1BufferID);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Double, false, 16, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);
            GL.DrawElements(PrimitiveType.Quads, quadBufferPos * 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
            quadBufferPos = 0;
        }
        
        public Color4[] Render(FastList<Note> notes, double midiTime)
        {
            Color4[] keyColors = new Color4[256];
            for (int i = 0; i < 256; i++) keyColors[i] = Color4.Transparent;
            int firstNote = settings.firstNote;
            int lastNote = settings.lastNote;
            int deltaTimeOnScreen = settings.deltaTimeOnScreen;
            double pianoHeight = settings.pianoHeight;
            lock (notes)
            {
                double wdth;
                quadBufferPos = 0;
                float r, g, b, a;
                foreach (Note n in notes)
                {
                    double renderCutoff = midiTime - deltaTimeOnScreen;
                    if (n.end >= renderCutoff || !n.hasEnded)
                        if (n.start < midiTime)
                        {
                            if (n.note >= firstNote && n.note < lastNote)
                            {
                                Color4 col = n.track.trkColor;
                                int k = n.note;
                                if (n.start < renderCutoff)
                                    keyColors[k] = col;

                                double x1;
                                x1 = (float)(k - firstNote) / (lastNote - firstNote);
                                wdth = 1.0f / (lastNote - firstNote);
                                double x2 = x1 + wdth;

                                double y1 = 1 - (midiTime - n.end) / deltaTimeOnScreen * (1 - pianoHeight);
                                double y2 = 1 - (midiTime - n.start) / deltaTimeOnScreen * (1 - pianoHeight);
                                if (!n.hasEnded)
                                    y1 = 1;

                                int pos = quadBufferPos * 8;
                                quadVertexbuff[pos++] = x1;
                                quadVertexbuff[pos++] = y2;
                                quadVertexbuff[pos++] = x2;
                                quadVertexbuff[pos++] = y2;
                                quadVertexbuff[pos++] = x2;
                                quadVertexbuff[pos++] = y1;
                                quadVertexbuff[pos++] = x1;
                                quadVertexbuff[pos] = y1;

                                pos = quadBufferPos * 16;
                                r = col.R;
                                g = col.G;
                                b = col.B;
                                a = col.A;
                                quadColorbuff[pos++] = r;
                                quadColorbuff[pos++] = g;
                                quadColorbuff[pos++] = b;
                                quadColorbuff[pos++] = a;
                                quadColorbuff[pos++] = r;
                                quadColorbuff[pos++] = g;
                                quadColorbuff[pos++] = b;
                                quadColorbuff[pos++] = a;
                                quadColorbuff[pos++] = r;
                                quadColorbuff[pos++] = g;
                                quadColorbuff[pos++] = b;
                                quadColorbuff[pos++] = a;
                                quadColorbuff[pos++] = r;
                                quadColorbuff[pos++] = g;
                                quadColorbuff[pos++] = b;
                                quadColorbuff[pos++] = a;

                                quadBufferPos++;
                                if (quadBufferPos >= quadBufferLength)
                                {
                                    FlushQuadBuffer();
                                }
                            }
                        }
                        else break;
                }

                FlushQuadBuffer();
                quadBufferPos = 0;
            }
            return keyColors;
        }

        bool isBlackNote(int n)
        {
            n = n % 12;
            return n == 1 || n == 3 || n == 6 || n == 8 || n == 10;
        }
    }
}
