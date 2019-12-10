namespace BizHawk.Bizware.BizwareGL
{
	public struct GuiRendererVertexData
	{
		public float X;
		public float Y;
		public float U;
		public float V;
		public float R;
		public float G;
		public float B;
		public float A;

		public GuiRendererVertexData(float x, float y, float u, float v, OpenTK.Graphics.Color4 color)
		{
			X = x;
			Y = y;
			U = u;
			V = v;
			R = color.R;
			G = color.G;
			B = color.B;
			A = color.A;
		}
	}
}
