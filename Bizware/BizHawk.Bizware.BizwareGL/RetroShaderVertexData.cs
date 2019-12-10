namespace BizHawk.Bizware.BizwareGL
{
	public struct RetroShaderVertexData
	{
		public float X;
		public float Y;
		public float Z;
		public float W;
		public float R;
		public float G;
		public float B;
		public float A;
		public float U;
		public float V;

		public RetroShaderVertexData(float x, float y, float z, float w, float r, float g, float b, float a, float u, float v)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
			R = r;
			G = g;
			B = b;
			A = a;
			U = u;
			V = v;
		}
	}
}
