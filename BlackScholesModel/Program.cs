/*
 * 5/11/2017
 */

// test the Newton solver for Black Scholes model

using System;

namespace BlackScholes
{

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("================== BlackScholesFormula ==============");

		Console.WriteLine("S = 100, r = 0.05, sigma = 0.1, T = 1, K = 100" );
		Console.WriteLine("\tcall: {0}, put: {1}", BlackScholesFormula.CalculateCallOptionPrice(100, 0.05, 0.1, 1, 100),
			BlackScholesFormula.CalculatePutOptionPrice(100, 0.05, 0.1, 1, 100));

		Console.WriteLine("S = 100, r = 0.00, sigma = 0.1, T = 1, K = 100" );
		Console.WriteLine("\tcall: {0}, put: {1}", BlackScholesFormula.CalculateCallOptionPrice(100, 0.00, 0.1, 1, 100),
			BlackScholesFormula.CalculatePutOptionPrice(100, 0.00, 0.1, 1,100));

		Console.WriteLine("S = 100, r = 0.05, sigma = 0.2, T = 1, K = 100" );
		Console.WriteLine("\tcall: {0}, put: {1}", BlackScholesFormula.CalculateCallOptionPrice(100, 0.05, 0.2, 1, 100),
			 BlackScholesFormula.CalculatePutOptionPrice(100, 0.05, 0.2, 1, 100));

		Console.WriteLine("S = 100, r = 0.05, sigma = 0.1, T = 10, K = 100");
		Console.WriteLine("\tcall: {0}, put: {1}", BlackScholesFormula.CalculateCallOptionPrice(100, 0.05, 0.1, 10, 100),
			BlackScholesFormula.CalculatePutOptionPrice(100, 0.05, 0.1, 10,	100));

		Console.WriteLine("S = 100, r = 0.05, sigma = 0.1, T = 1, K = 120" );
		Console.WriteLine("\tcall: {0}, put: {1}",	BlackScholesFormula.CalculateCallOptionPrice(100, 0.05, 0.1, 1, 120),
			BlackScholesFormula.CalculatePutOptionPrice(100, 0.05, 0.1, 1, 120));

		Console.WriteLine("\n================== BlackScholesImpliedVolatility =================");

		Console.WriteLine("call = 10, S = 100, r = 0.05, T = 1, K = 100, initial_guess = 0.5");
		Console.WriteLine("\t implied volatility: {0}", BlackScholesImpliedVolatility .CalculateImpliedVolatility(10, 100, 0.05, 1, 100, 0.5));

		Console.WriteLine("put = 3, S = 100, r = 0.05, T = 1, K = 100, initial_guess = 0.5");
		Console.WriteLine("\t implied volatility: {0}",	BlackScholesImpliedVolatility .CalculatePutImpliedVolatility(3,	100, 0.05, 1, 100, 0.5));

		Console.WriteLine("\n==================	BlackScholesFiniteDifferenceSolver =============" );
		
		double r = 0.05;
		double sigma = 0.1;
		double K = 100;
		double T = 1;
		double S0 = 100;
		double bs_price = BlackScholesFormula.CalculatePutOptionPrice(S0, r, sigma, T, K);
		Func<double, double> put = (S) => Math.Max(K - S, 0);

		int N = 200;
		int	M = 100;
		int refinements = 5;
		
		for(int i = 0; i < refinements; i++, M *= 2)
		{
			double error = Math.Abs(bs_price - BlackScholesFiniteDifferenceSolver .Solve(S0, T, put, r, sigma, 5 * K, N, M));
			Console.WriteLine("space_partitions: {0}, time_steps: {1}, error: {2}", M, N, error);
		}
		
		N = 10; 
		M = 5500;
		Console.WriteLine();

		// this takes too long to run, reduce partitions and refinements
		for(int i = 0; i < refinements - 2; i++, N *= 2)
		{
			double error = Math.Abs(bs_price - BlackScholesFiniteDifferenceSolver .Solve(S0, T, put, r, sigma,5 * K, N, M));
			Console.WriteLine("space_partitions: {0}, time_steps: {1},error: {2}", M, N, error);
		}
	}
}
