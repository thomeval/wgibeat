// RoundLine.cs
// By Michael D. Anderson
// Version 3.00, Mar 12 2009
//
// A class to efficiently draw thick lines with rounded ends.
// Note: This library has been modified by the WGiBeat developers to provide minor
// optimizations, refactoring and coding standards compilance. The original version
// is available here: http://roundline.codeplex.com/
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace RoundLineCode
{
    /// <summary>
    /// Represents a single line segment.  Drawing is handled by the RoundLineManager class.
    /// </summary>
    public class RoundLine
    {
        private Vector2 _p0; // Begin point of the line
        private Vector2 _p1; // End point of the line
        private float _rho; // Length of the line
        private float _theta; // Angle of the line

        public Vector2 P0 
        { 
            get 
            { 
                return _p0; 
            }
            set
            {
                _p0 = value;
                RecalcRhoTheta();
            }
        }
        public Vector2 P1 
        {
            get 
            { 
                return _p1; 
            }
            set
            {
                _p1 = value;
                RecalcRhoTheta();
            }
        }
        public float Rho { get { return _rho; } }
        public float Theta { get { return _theta; } }

        
        public RoundLine(Vector2 p0, Vector2 p1)
        {
            this._p0 = p0;
            this._p1 = p1;
            RecalcRhoTheta();
        }


        public RoundLine(float x0, float y0, float x1, float y1)
        {
            this._p0 = new Vector2(x0, y0);
            this._p1 = new Vector2(x1, y1);
            RecalcRhoTheta();
        }


        protected void RecalcRhoTheta()
        {
            Vector2 delta = P1 - P0;
            _rho = delta.Length();
            _theta = (float)Math.Atan2(delta.Y, delta.X);
        }
    };


    // A "degenerate" RoundLine where both endpoints are equal
    public class Disc : RoundLine
    {
        public Disc(Vector2 p) : base(p, p) { }
        public Disc(float x, float y) : base(x, y, x, y) { }
        public Vector2 Pos 
        {
            get 
            {
                return P0; 
            }
            set
            {
                P0 = value;
                P1 = value;
            }
        }
    };

    // A vertex type for drawing RoundLines, including an instance index
    struct RoundLineVertex
    {
        public Vector3 Pos;
        public Vector2 RhoTheta;
        public Vector2 ScaleTrans;
        public float Index;


        public RoundLineVertex(Vector3 pos, Vector2 norm, Vector2 tex, float index)
        {
            this.Pos = pos;
            this.RhoTheta = norm;
            this.ScaleTrans = tex;
            this.Index = index;
        }

        public const int SIZE_IN_BYTES = 8*sizeof (float);

        public static readonly VertexElement[] VertexElements = new[] 
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, 12, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.Normal, 0),
                new VertexElement(0, 20, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, 28, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1)
            };
    }

    /// <summary>
    /// Class to handle drawing a list of RoundLines.
    /// </summary>
    public class RoundLineManager
    {
        private GraphicsDevice _device;
        private Effect _effect;
        private EffectParameter _viewProjMatrixParameter;
        private EffectParameter _instanceDataParameter;
        private EffectParameter _timeParameter;
        private EffectParameter _lineRadiusParameter;
        private EffectParameter _lineColorParameter;
        private EffectParameter _blurThresholdParameter;
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private VertexDeclaration _vdecl;
        private int _numInstances;
        private int _numVertices;
        private int _numIndices;
        private int _numPrimitivesPerInstance;
        private int _numPrimitives;
        private int _bytesPerVertex;
        float[] _translationData;

        public int NumLinesDrawn;
        public float BlurThreshold = 0.97f;
        public Matrix ViewProjMatrix { get; set; }


        public void Init(GraphicsDevice device, ContentManager content, Matrix viewProjMatrix)
        {

            ViewProjMatrix = viewProjMatrix;
            this._device = device;
            _effect = content.Load<Effect>("RoundLine");
            _viewProjMatrixParameter = _effect.Parameters["viewProj"];
            _instanceDataParameter = _effect.Parameters["instanceData"];
            _timeParameter = _effect.Parameters["time"];
            _lineRadiusParameter = _effect.Parameters["lineRadius"];
            _lineColorParameter = _effect.Parameters["lineColor"];
            _blurThresholdParameter = _effect.Parameters["blurThreshold"];

            CreateRoundLineMesh();
        }

        public string[] TechniqueNames
        {
            get
            {
                var names = new string[_effect.Techniques.Count];
                int index = 0;
                foreach (var technique in _effect.Techniques)
                    names[index++] = technique.Name;
                return names;
            }
        }

        private static RoundLineManager _instance;
        public static RoundLineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RoundLineManager();
                   
                }
                return _instance;
            }
   
        }

        /// <summary>
        /// Create a mesh for a RoundLine.
        /// </summary>
        /// <remarks>
        /// The RoundLine mesh has 3 sections:
        /// 1.  Two quads, from 0 to 1 (left to right)
        /// 2.  A half-disc, off the left side of the quad
        /// 3.  A half-disc, off the right side of the quad
        ///
        /// The X and Y coordinates of the "normal" encode the rho and theta of each vertex
        /// The "texture" encodes whether to scale and translate the vertex horizontally by length and radius
        /// </remarks>
        private void CreateRoundLineMesh()
        {
            const int PRIMS_PER_CAP = 12; // A higher primsPerCap produces rounder endcaps at the cost of more vertices
            const int VERTICES_PER_CAP = PRIMS_PER_CAP * 2 + 2;
            const int PRIMS_PER_CORE = 4;
            const int VERTICES_PER_CORE = 8;

            _numInstances = 200;
            _numVertices = (VERTICES_PER_CORE + VERTICES_PER_CAP + VERTICES_PER_CAP) * _numInstances;
            _numPrimitivesPerInstance = PRIMS_PER_CORE + PRIMS_PER_CAP + PRIMS_PER_CAP;
            _numPrimitives = _numPrimitivesPerInstance * _numInstances;
            _numIndices = 3 * _numPrimitives;
            var indices = new short[_numIndices];
            _bytesPerVertex = RoundLineVertex.SIZE_IN_BYTES;
            var tri = new RoundLineVertex[_numVertices];
            _translationData = new float[_numInstances * 4]; // Used in Draw()

            int iv = 0;
            int ii = 0;
            int iVertex;
            int iIndex;
            for (int instance = 0; instance < _numInstances; instance++)
            {
                // core vertices
                const float pi2 = MathHelper.PiOver2;
                const float threePi2 = 3 * pi2;
                iVertex = iv;
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, -1.0f, 0), new Vector2(1, threePi2), new Vector2(0, 0), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, -1.0f, 0), new Vector2(1, threePi2), new Vector2(0, 1), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 0.0f, 0), new Vector2(0, threePi2), new Vector2(0, 1), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 0.0f, 0), new Vector2(0, threePi2), new Vector2(0, 0), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 0.0f, 0), new Vector2(0, pi2), new Vector2(0, 1), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 0.0f, 0), new Vector2(0, pi2), new Vector2(0, 0), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 1.0f, 0), new Vector2(1, pi2), new Vector2(0, 1), instance);
                tri[iv++] = new RoundLineVertex(new Vector3(0.0f, 1.0f, 0), new Vector2(1, pi2), new Vector2(0, 0), instance);

                // core indices
                indices[ii++] = (short)(iVertex + 0);
                indices[ii++] = (short)(iVertex + 1);
                indices[ii++] = (short)(iVertex + 2);
                indices[ii++] = (short)(iVertex + 2);
                indices[ii++] = (short)(iVertex + 3);
                indices[ii++] = (short)(iVertex + 0);

                indices[ii++] = (short)(iVertex + 4);
                indices[ii++] = (short)(iVertex + 6);
                indices[ii++] = (short)(iVertex + 5);
                indices[ii++] = (short)(iVertex + 6);
                indices[ii++] = (short)(iVertex + 7);
                indices[ii++] = (short)(iVertex + 5);

                // left halfdisc
                iVertex = iv;
                iIndex = ii;
                for (int i = 0; i < PRIMS_PER_CAP + 1; i++)
                {
                    float deltaTheta = MathHelper.Pi / PRIMS_PER_CAP;
                    float theta0 = MathHelper.PiOver2 + i * deltaTheta;
                    float theta1 = theta0 + deltaTheta / 2;
                    // even-numbered indices are at the center of the halfdisc
                    tri[iVertex + 0] = new RoundLineVertex(new Vector3(0, 0, 0), new Vector2(0, theta1), new Vector2(0, 0), instance);

                    // odd-numbered indices are at the perimeter of the halfdisc
                    float x = (float)Math.Cos(theta0);
                    float y = (float)Math.Sin(theta0);
                    tri[iVertex + 1] = new RoundLineVertex(new Vector3(x, y, 0), new Vector2(1, theta0), new Vector2(1, 0), instance);

                    if (i < PRIMS_PER_CAP)
                    {
                        // indices follow this pattern: (0, 1, 3), (2, 3, 5), (4, 5, 7), ...
                        indices[iIndex + 0] = (short)(iVertex + 0);
                        indices[iIndex + 1] = (short)(iVertex + 1);
                        indices[iIndex + 2] = (short)(iVertex + 3);
                        iIndex += 3;
                        ii += 3;
                    }
                    iVertex += 2;
                    iv += 2;
                }

                // right halfdisc
                for (int i = 0; i < PRIMS_PER_CAP + 1; i++)
                {
                    float deltaTheta = MathHelper.Pi / PRIMS_PER_CAP;
                    float theta0 = 3 * MathHelper.PiOver2 + i * deltaTheta;
                    float theta1 = theta0 + deltaTheta / 2;
                    // even-numbered indices are at the center of the halfdisc
                    tri[iVertex + 0] = new RoundLineVertex(new Vector3(0, 0, 0), new Vector2(0, theta1), new Vector2(0, 1), instance);

                    // odd-numbered indices are at the perimeter of the halfdisc
                    float x = (float)Math.Cos(theta0);
                    float y = (float)Math.Sin(theta0);
                    tri[iVertex + 1] = new RoundLineVertex(new Vector3(x, y, 0), new Vector2(1, theta0), new Vector2(1, 1), instance);

                    if (i < PRIMS_PER_CAP)
                    {
                        // indices follow this pattern: (0, 1, 3), (2, 3, 5), (4, 5, 7), ...
                        indices[iIndex + 0] = (short)(iVertex + 0);
                        indices[iIndex + 1] = (short)(iVertex + 1);
                        indices[iIndex + 2] = (short)(iVertex + 3);
                        iIndex += 3;
                        ii += 3;
                    }
                    iVertex += 2;
                    iv += 2;
                }
            }

            _vb = new VertexBuffer(_device, _numVertices * _bytesPerVertex, BufferUsage.None);
            _vb.SetData(tri);
            _vdecl = new VertexDeclaration(_device, RoundLineVertex.VertexElements);

            _ib = new IndexBuffer(_device, _numIndices * 2, BufferUsage.None, IndexElementSize.SixteenBits);
            _ib.SetData(indices);
        }



        /// <summary>
        /// Compute a reasonable "BlurThreshold" value to use when drawing RoundLines.
        /// See how wide lines of the specified radius will be (in pixels) when drawn
        /// to the back buffer.  Then apply an empirically-determined mapping to get
        /// a good BlurThreshold for such lines.
        /// </summary>
        public float ComputeBlurThreshold(float lineRadius, Matrix viewProjMatrix, float viewportWidth)
        {
            var lineRadiusTestBase = new Vector4(0, 0, 0, 1);
            var lineRadiusTest = new Vector4(lineRadius, 0, 0, 1);
            Vector4 delta = lineRadiusTest - lineRadiusTestBase;
            Vector4 output = Vector4.Transform(delta, viewProjMatrix);
            output.X *= viewportWidth;

            double newBlur = 0.125 * Math.Log(output.X) + 0.4;

            return MathHelper.Clamp((float)newBlur, 0.5f, 0.99f);
        }


        /// <summary>
        /// Draw a single RoundLine.  Usually you want to draw a list of RoundLines
        /// at a time instead for better performance.
        /// </summary>
        public void Draw(RoundLine roundLine, float lineRadius, Color lineColor,
            float time, string techniqueName)
        {
            if (_device == null)
            {
                throw new Exception("RoundlineManager Draw was called before initializing.");
            }

            SetParams(lineRadius, lineColor, time, techniqueName);

            int iData = 0;
            _translationData[iData++] = roundLine.P0.X;
            _translationData[iData++] = roundLine.P0.Y;
            _translationData[iData++] = roundLine.Rho;
            _translationData[iData] = roundLine.Theta;
            _instanceDataParameter.SetValue(_translationData);

            _effect.Begin();
            var pass = _effect.CurrentTechnique.Passes[0];

            pass.Begin();

            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _numVertices, 0, _numPrimitivesPerInstance * 1);
            NumLinesDrawn += 1;
            
            pass.End();

            _effect.End();
        }

        private void SetParams(float lineRadius, Color lineColor, float time, string techniqueName)
        {
            _device.VertexDeclaration = _vdecl;
            _device.Vertices[0].SetSource(_vb, 0, _bytesPerVertex);
            _device.Indices = _ib;

            _viewProjMatrixParameter.SetValue(ViewProjMatrix);
            _timeParameter.SetValue(time);
            _lineColorParameter.SetValue(lineColor.ToVector4());
            _lineRadiusParameter.SetValue(lineRadius);
            _blurThresholdParameter.SetValue(BlurThreshold);


            if (techniqueName == null)
                _effect.CurrentTechnique = _effect.Techniques[0];
            else
                _effect.CurrentTechnique = _effect.Techniques[techniqueName];
        }


        /// <summary>
        /// Draw a list of Lines.
        /// </summary>
        public void Draw(IEnumerable<RoundLine> roundLines, float lineRadius, Color lineColor, 
            float time, string techniqueName)
        {
            if (_device == null)
            {
                throw new Exception("RoundlineManager Draw was called before initializing.");
            }


            _device.VertexDeclaration = _vdecl;
            _device.Vertices[0].SetSource(_vb, 0, _bytesPerVertex);
            _device.Indices = _ib;

            _viewProjMatrixParameter.SetValue(ViewProjMatrix);
            _timeParameter.SetValue(time);
            _lineColorParameter.SetValue(lineColor.ToVector4());
            _lineRadiusParameter.SetValue(lineRadius);
            _blurThresholdParameter.SetValue(BlurThreshold);

            if (techniqueName == null)
                _effect.CurrentTechnique = _effect.Techniques[0];
            else
                _effect.CurrentTechnique = _effect.Techniques[techniqueName];
            _effect.Begin();
            var pass = _effect.CurrentTechnique.Passes[0];

            pass.Begin();

            int iData = 0;
            int numInstancesThisDraw = 0;
            foreach (RoundLine roundLine in roundLines)
            {
                _translationData[iData++] = roundLine.P0.X;
                _translationData[iData++] = roundLine.P0.Y;
                _translationData[iData++] = roundLine.Rho;
                _translationData[iData++] = roundLine.Theta;
                numInstancesThisDraw++;

                if (numInstancesThisDraw == _numInstances)
                {
                    _instanceDataParameter.SetValue(_translationData);
                    _effect.CommitChanges();
                    _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _numVertices, 0, _numPrimitivesPerInstance * numInstancesThisDraw);
                    NumLinesDrawn += numInstancesThisDraw;
                    numInstancesThisDraw = 0;
                    iData = 0;
                }
            }
            if (numInstancesThisDraw > 0)
            {
                _instanceDataParameter.SetValue(_translationData);
                _effect.CommitChanges();
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _numVertices, 0, _numPrimitivesPerInstance * numInstancesThisDraw);
                NumLinesDrawn += numInstancesThisDraw;
            }
            pass.End();

            _effect.End();
        }

        /// <summary>
        /// Draws a list of lines with the given line radius and colour, using the default drawing technique and no animation.
        /// </summary>
        /// <param name="roundLines">A collection of RoundLines to draw.</param>
        /// <param name="lineRadius">The radius (half width) of the lines to draw.</param>
        /// <param name="lineColor">The colour of the lines to draw.</param>
        public void Draw(IEnumerable<RoundLine> roundLines, int lineRadius, Color lineColor)
        {
            Draw(roundLines,lineRadius,lineColor,0,null);
        }
    }
}
