/*
 * 5/11/2017
 */

using System;

namespace BlackScholes
{
	static class NewtonSolver
	{
		// approximate a root of f using newton's method
		// x_n = x_{n-1} - f(x_{n-1}) / f'(x_{n-1}
		// keep going until abs(f(x_n)) <= tolerance or n exceeds max_iterations
		public static double Solve(Func<double, double> f,
			Func<double, double> f_prime,
			double x_0,
			double tolerance,
			int max_iterations)
		{
			if (f == null || tolerance <= 0.0 || max_iterations <= 0)
			{
				throw new ArgumentException("Invalid input arguments to NewtonSolver");
			}

			// caller did not provide f_prime, replace f'(x) with
			// 1/(2*delta) (f(x+delta) - f(x-delta))
			const double delta = 1e-6;
			if (f_prime == null)
			{
				f_prime = (y) => (1 / (2 * delta)) * (f(y + delta) - f(y - delta));
			}
			int iterations = 0; // how many iterations so far?
			double x = x_0; // current term

			while (System.Math.Abs(f(x)) >= tolerance)
			{
				x = x - (f(x) / f_prime(x));

				if (iterations++ > max_iterations)
				{
					throw new Exception("exceeded max iterations" );
				}
			}

			return x;
		}
	}
}
