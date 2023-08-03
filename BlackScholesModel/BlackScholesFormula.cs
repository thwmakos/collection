/*
 * 5/11/2017
 */

using System;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace BlackScholes {

	static class BlackScholesFormula
	{
		//
		// Calculate Eurpean call option price, using Black-Scholes formula
		//
		// params
		// S : stock price, assumed costant
		// r : risk free asset rate
		// sigma : volatility
		// T : time to maturity
		// K : strike price of the option
		//
		public static double CalculateCallOptionPrice(double S,
													  double r,
													  double sigma,
													  double T,
													  double K)
		{
			if (S <= 0 || sigma <= 0 || T <= 0 || K <= 0) 
			{
				 throw new ArgumentException("Invalid argument to CalculateCallOptionPrice" );
			}

			double temp = sigma * Math.Sqrt(T);
			double d1 = (1 / temp) * (Math.Log(S / K) + (r + sigma * sigma / 2) * T);
			double d2 = d1 - temp;

			return S * Normal.CDF(0, 1, d1) - K * Math.Exp(-r * T) * Normal.CDF(0, 1, d2);
		}

		//
		// Calculate European put option price, using Black-Scholes formula
		//
		// params
		// S : stock price, assumed costant
		// r : risk free asset rate
		// sigma : volatility
		// T : time to maturity
		// K : strike price of the option
		//
		public static double CalculatePutOptionPrice(
		  double S, double r, double sigma, double T, double K)
		{
			if (S <= 0 || sigma <= 0 || T <= 0 || K <= 0) 
			{
				throw new ArgumentException("Invalid argument to CalculatePutOptionPrice");
			}

			return CalculateCallOptionPrice(S, r, sigma, T, K) + Math.Exp(-r * T) * K - S;
		}
	}

}
