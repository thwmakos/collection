/*
 * 5/11/2017
 */

using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;

using System;
using System.Diagnostics;

namespace BlackScholes
{
	class BlackScholesFiniteDifferenceSolver
	{
		private static BiCgStab solver = new BiCgStab();
		private static IterationCountStopCriterion<double> iter_stop_crit = new IterationCountStopCriterion<double>(20000);
		private static ResidualStopCriterion<double> res_stop_crit = new ResidualStopCriterion<double>(1e-4);
		private static Iterator<double> monitor = new Iterator<double>(iter_stop_crit, res_stop_crit);

		private static double x_plus(double x) { return Math.Max(x, 0); }

		private static double x_minus(double x) { return -Math.Min(x, 0); }

		public static double Solve(
		  double S, // stock price
		  double T, // maturity
		  Func<double, double> payoff_func, // payoff function
		  double r, // risk free rate
		  double sigma, // volatility
		  double R, // solve in (0, R)
		  int num_time_steps,
		  int num_space_partitions)
		{
			if (S <= 0 || T <= 0 || payoff_func == null || sigma <= 0 || R <= 0 ||
				num_time_steps <= 0 || num_space_partitions <= 0) 
			{
				throw new ArgumentException(
				  "invalid arguments to BlackScholesFiniteDifferenceSolver::Solve");
			}

			if (S > R) 
			{
				throw new Exception("stock price should be <= R");
			}

			double tau = T / num_time_steps;
			double h = R / (num_space_partitions - 1);

			// approximate initial condition
			var = Vector<Double>.Build.Dense(num_space_partitions);
			for (int i = 0; i < num_space_partitions; i++)
				B[i] = payoff_func(i * h);

			// create matrix A
			Func<int, double> a = (m) =>
			{
				Debug.Assert(m >= 1 && m <= num_space_partitions - 2);
				
				return (-1.0 / 2.0) * sigma * sigma * m * m - x_minus(r) * m;
			};

			Func<int, double> b = (m) =>
			{
				Debug.Assert(m >= 1 && m <= num_space_partitions - 2);
				return sigma * sigma * m * m + x_plus(r) * m + x_minus(r) * m + r;
			};

			Func<int, double> c = (m) =>
			{
				Debug.Assert(m >= 1 && m <= num_space_partitions - 2);
				return (-1.0 / 2.0) * sigma * sigma * m * m - x_plus(r) * m;
			};

			double gamma = payoff_func((num_space_partitions - 1) * h) -
				payoff_func((num_space_partitions - 2) * h);

			Func<int, int, double> matrix_entries = (i, j) =>
			{
				Debug.Assert(i <= num_space_partitions - 1 && j <= num_space_partitions - 1);

				if(i == 0 && j == 0)
					return 1.0;

				if(i == 0)
					return 0.0;

				if(i == num_space_partitions - 1 && (j == num_space_partitions - 1 || j == num_space_partitions - 2))
					return 1.0;
				if(i == num_space_partitions - 1)
					return 0.0;	
		
				if (j == i + 1)
					return c(j - 1);
					
				if (j == i - 1)
					return a(j + 1);
				
				return 0.0;
			};

			var A = Matrix<double>.Build.Sparse(num_space_partitions, num_space_partitions, matrix_entries);
			var I = Matrix<double>.Build.SparseIdentity(num_space_partitions);
			var M = I + tau * A;
			// Console.WriteLine(A);
			//  approximate the solution u_R on the line theta = T
			var U = Vector<double>.Build.Dense(num_space_partitions);

			for (int n = 0; n < num_time_steps; n++) 
			{
				U = M.SolveIterative(B, solver, monitor);
				B = U;
				B[0] = Math.Exp(-r * tau * n) * payoff_func(0.0);
				B[num_space_partitions - 1] = gamma;
			}

			// find between which elements S is
			int index = (int)Math.Floor(S / h); // if S / h is large the floor might not fit in an int
			// and lerp
			return (1 / h) *
				   ((S - index * h) * U[index + 1] + ((index + 1) * h - S) * U[index]);
		}

		private double T;
		private Func<double, double> payoff_func;
		private double r;
		private double sigma;
		private double R;
		private int N;
		private int M;

		// provide the interface the exercise wants
		public BlackScholesFiniteDifferenceSolver(
		  double maturity,
		  Func<double, double> payoff_function,
		  double risk_free_rate,
		  double sigma,
		  double R,
		  int num_time_steps,
		  int num_space_partitions)
		{
			T = maturity;
			payoff_func = payoff_function;
			r = risk_free_rate;
			this.sigma = sigma;
			this.R = R;
			N = num_time_steps;
			M = num_space_partitions;
		}
		public double Price(double S)
		{
			return Solve(S, T, payoff_func, r, sigma, R, N, M);
		}
	}
}
