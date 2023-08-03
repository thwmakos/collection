/*
 * 5/11/2017
 */

 using System;

 namespace BlackScholes
 {
	static class BlackScholesImpliedVolatility
	{
		//
		// calculate the implied volatility given the price of call
		//
		// params
		// call_price : the price of the call
		// S : stock price
		// r : risk free asset rate
		// T : time to maturity
		// K : strike price
		// initial_guess : first step of Newton's algorithm
		//
		public static double CalculateImpliedVolatility( double call_price,
			double S, double r, double T, double K, double initial_guess = 0.5)
		 {
		 	if(S <= 0 || T <= 0 || K <= 0)
		 	{
		 		throw new ArgumentException("Invalid argument to CalculateImpliedVolatility" );
			}

			Func<double, double> f = (x) => call_price - BlackScholesFormula.CalculateCallOptionPrice(S, r, x, T, K);

		 	return NewtonSolver.Solve(f, null, initial_guess, 0.001, 3000);
		 }

		 public static double CalculatePutImpliedVolatility( double put_price,
			double S, double r, double T, double K, double initial_guess = 0.5)
		 {
			 if(S <= 0 || T <= 0 || K <= 0)
			 {
			 	throw new ArgumentException("Invalid argument to CalculatePutImpliedVolatility" );
			 }
			 Func<double, double> f = (x) => put_price - BlackScholesFormula.CalculatePutOptionPrice(S, r, x, T, K);

			 return NewtonSolver.Solve(f, null, initial_guess, 0.001, 3000);
		}
	}
}
