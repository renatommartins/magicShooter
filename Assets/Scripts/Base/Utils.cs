using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils {
	/// <summary>
	/// Gets the first set bit from the least to most significant.
	/// </summary>
	/// <returns>The set bit number.</returns>
	/// <param name="value">Value.</param>
	static public int GetSetBit(int value){
		int setBit = 0;
		for(int i=0; i<32;i++){	if((value & 1 << i) != 0){	setBit = i;	break;	}	}
		return setBit;
	}
}

public static class PhysicsUtils{
	public static float airDensity = 1.225f;

	public static float CalculateDrag(float speed, float crossSection, float coefficient){
		return 0.5f * airDensity * coefficient * crossSection;
	}

	public static float CalculateBallisticAngle(float x, float y, float speed, bool shortArc){
		x*=-1;
		float angle1, angle2;
		float speed2 = speed * speed;
		float speed4 = speed2 * speed2;
		float gravity = Physics.gravity.y;
		float squareRoot = Mathf.Sqrt( 1 + (2*gravity*y/speed2) - (x*x*gravity*gravity/speed4) );

		angle1 = Mathf.Atan2(speed2*(1+squareRoot),gravity*x) * Mathf.Rad2Deg;
		angle2 = Mathf.Atan2(speed2*(1-squareRoot),gravity*x) * Mathf.Rad2Deg;

		return shortArc? angle2 : angle1;
	}
}

public static class ExtensionUtils{
	public static T Clone<T>(this T source){
		if(!typeof(T).IsSerializable) {
			Debug.LogError("The type must be serializable.");
			return default(T);
		}
		if(object.ReferenceEquals(source,null)){	return default(T);	}

		System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		System.IO.Stream stream = new System.IO.MemoryStream();
		using(stream){
			formatter.Serialize(stream,source);
			stream.Seek(0,System.IO.SeekOrigin.Begin);
			return (T) formatter.Deserialize(stream);
		}
	}

	private static double[] normalDistributionLUT = {0.00013383,
		0.000163256,0.000198655,0.000241127,0.000291947,0.000352596,0.00042478,0.000510465,0.000611902,
		0.000731664,0.000872683,0.001038281,0.001232219,0.001458731,0.001722569,0.002029048,0.002384088,
		0.002794258,0.003266819,0.003809762,0.004431848,0.005142641,0.005952532,0.006872767,0.007915452,
		0.009093563,0.010420935,0.011912244,0.013582969,0.015449347,0.0175283,0.019837354,0.02239453,
		0.02521822,0.028327038,0.031739652,0.035474593,0.039550042,0.043983596,0.048792019,0.053990967,
		0.059594706,0.065615815,0.072064874,0.078950158,0.086277319,0.094049077,0.102264925,0.110920835,
		0.120009001,0.129517596,0.139430566,0.149727466,0.160383327,0.171368592,0.182649085,0.194186055,
		0.205936269,0.217852177,0.229882141,0.241970725,0.254059056,0.26608525,0.277984886,0.289691553,
		0.301137432,0.312253933,0.32297236,0.333224603,0.342943855,0.352065327,0.360526962,0.36827014,
		0.375240347,0.381387815,0.386668117,0.391042694,0.394479331,0.396952547,0.398443914,0.39894228,
		0.398443914,0.396952547,0.394479331,0.391042694,0.386668117,0.381387815,0.375240347,0.36827014,
		0.360526962,0.352065327,0.342943855,0.333224603,0.32297236,0.312253933,0.301137432,0.289691553,
		0.277984886,0.26608525,0.254059056,0.241970725,0.229882141,0.217852177,0.205936269,0.194186055,
		0.182649085,0.171368592,0.160383327,0.149727466,0.139430566,0.129517596,0.120009001,0.110920835,
		0.102264925,0.094049077,0.086277319,0.078950158,0.072064874,0.065615815,0.059594706,0.053990967,
		0.048792019,0.043983596,0.039550042,0.035474593,0.031739652,0.028327038,0.02521822,0.02239453,
		0.019837354,0.0175283,0.015449347,0.013582969,0.011912244,0.010420935,0.009093563,0.007915452,
		0.006872767,0.005952532,0.005142641,0.004431848,0.003809762,0.003266819,0.002794258,0.002384088,
		0.002029048,0.001722569,0.001458731,0.001232219,0.001038281,0.000872683,0.000731664,0.000611902,
		0.000510465,0.00042478,0.000352596,0.000291947,0.000241127,0.000198655,0.000163256,0.00013383
		};

	public static double EvaluateNormalDistribution(this Mathf extension, float t){
		float totalT = t*normalDistributionLUT.Length;
		int firstIndex = Mathf.FloorToInt(totalT);
		int lastIndex = Mathf.CeilToInt(totalT);
		float correctedT = totalT-(float)firstIndex;

		return Mathf.Lerp((float)normalDistributionLUT[firstIndex],(float)normalDistributionLUT[lastIndex],correctedT)*2.5f;
	}

	public static int GetStableHash(this string text){
		unchecked{
			int hash = 23;
			foreach(char c in text){
				hash = hash*31+c;
			}
			return hash;
		}
	}
}
