using System;

namespace BizHawk.Client.Common
{
	public static class PolarRectConversion
	{
		/// <param name="r">radial displacement in range 0..181</param>
		/// <param name="θ">angle (in degrees) in range 0..359</param>
		/// <returns>rectangular (Cartesian) coordinates (x, y) where x and y may be outside the range -128..127 allowed by <see cref="byte"/></returns>
		/// <seealso cref="RectToPolarLookup"/>
		public static Tuple<short, short> PolarToRectLookup(ushort r, ushort θ) => new Tuple<short, short>(PolarRectConversionData._rθ2x.Value[r, θ], PolarRectConversionData._rθ2y.Value[r, θ]);

		/// <returns>polar coordinates (r, θ); where r is radial displacement in range 0..181 and θ is angle (in degrees) in range 0..359</returns>
		/// <remarks>does intentional integer (byte) overflow so <paramref name="x"/> and <paramref name="y"/> can be used as array indices in <see cref="TranslatedRectToPolarLookup"/></remarks>
		/// <seealso cref="PolarToRectLookup"/>
		public static Tuple<ushort, ushort> RectToPolarLookup(sbyte x, sbyte y) => unchecked (TranslatedRectToPolarLookup((byte) x, (byte) y));

		private static Tuple<ushort, ushort> TranslatedRectToPolarLookup(byte x, byte y) => new Tuple<ushort, ushort>(PolarRectConversionData._xy2r.Value[x, y], PolarRectConversionData._xy2θ.Value[x, y]);
	}
}