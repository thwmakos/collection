/*
 * 11/01/2018
 * ~thwmakos~
 */

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

using MathNet.Numerics.Distributions;

namespace HestonModel
{
    public class PathGenerator
    {
        // simply copy the arguments and calculate some extra parameters needed for
        // the discretization of the volatility
        public PathGenerator(double _r, double _v0, double _kappa, double _theta, double _sigma, double _rho)
        {
            r  = _r;
            v0 = _v0;
            kappa = _kappa;
            theta = _theta;
            sigma = _sigma;
            rho = _rho;

            alpha = (4 * kappa * theta - sigma * sigma) / 8.0;
            beta = -kappa / 2.0;
            gamma = sigma / 2.0;

            one_minus_sqrt_rho = Math.Sqrt(1 - rho * rho);
        }

        // get S(T) for num_paths number of paths with initial condition S(0) = 0, v(0)=v0
        // used for european options
        // set the bool flag to true to keep the minimum of the
        // approximation sequence (used in lookback options)
        // returns a tuple, first item is the value of the paths,second is the minimum of each path
        // if ret_mins == false the second items of the tuple is null
        public Tuple<double[], double[]> GenPaths(int num_paths, int num_timesteps, double T, double S0,
            bool ret_mins = false)
        {
            Debug.Assert(num_paths > 0);
            Debug.Assert(num_timesteps > 0);
            Debug.Assert(v0 > 0.0);

            var paths = new double[num_paths];
            double[] mins = null;

            if(ret_mins == true)
                mins = new double[num_paths];

            var step = T / num_timesteps;
            var sqrt_step = Math.Sqrt(step);
            var sqrt_v0 = Math.Sqrt(v0);

            Parallel.For(0, num_paths, (i) =>
            {
                var S = S0;
                var y = sqrt_v0;
                var min = S0;

                for(int j = 0; j < num_timesteps; j++)
                {
                    var next = NextStep(S, y, step, sqrt_step);

                    S = next.Item1;
                    y = next.Item2;

                    if(min >= S)
                        min = S;
                }

                paths[i] = S;

                if(ret_mins == true)
                    mins[i] = min;
            });

            return Tuple.Create(paths, mins);
        }

        // this is used in asian arithmetic options
        // for each time T_m we want to average on the function will yield return the current state of the paths to the
        // caller
        // to do whatever, then control is reverted to this function to continue evolving the paths
        // the function DOES NOT return the paths at time T
        // BE CAREFUL: second param here number of timesteps per year(per 1.0 T) and NOT total number of timesteps as in
        // the above overload
        public IEnumerable<double[]> GenPaths(int num_paths, int num_timesteps_per_year, double T, double S0,
            double[] observe_times)
        {
            var m = observe_times.Length;

            var num_timesteps = new int[m];

            // uncomment this to include path states at time T in the returned values
            //var num_timesteps = new int[m + 1];

            // how many timesteps between consecutive T_m's ?
            // i.e. the number of timesteps between the numbers 0, T_1, T_2, ..., T_m, T
            num_timesteps[0] = (int)(num_timesteps_per_year * observe_times[0]);
            for(var i = 1; i < m; i++)
                num_timesteps[i] = (int)(num_timesteps_per_year * (observe_times[i] - observe_times[i - 1]));

            // uncomment this to include time T
            // T - T_M
            //num_timesteps[m] = (int)(num_timesteps_per_year * (T - observe_times[m - 1]));
            var paths = new double[num_paths];

            var step = 1.0 / num_timesteps_per_year; // time step
            var sqrt_step = Math.Sqrt(step); // precalc to avoid calling sqrt all the time
            var sqrt_v0 = Math.Sqrt(v0); // same

            for(var i = 0; i < num_timesteps.Length; i++)
            {
                Parallel.For(0, num_paths, j =>
                {
                    var S = S0;
                    var y = sqrt_v0;

                    // evolve the path until T_i
                    for(var k = 0; k < num_timesteps[i]; k++)
                    {
                        var next = NextStep(S, y, step, sqrt_step);
                    }

                    S = next.Item1;
                    y = next.Item2;

                    paths[j] = S;
                });
            }

            yield return paths; // let the caller see what we have so far before continuing
        }

        // push S and y step time forward
        private Tuple<double, double> NextStep(double S, double y, double step, double sqrt_step)
        {
            var one_minus_beta_step = 1.0 - beta * step;
            var one_minus_beta_step_square = one_minus_beta_step * one_minus_beta_step;
            var constant = alpha * step / one_minus_beta_step;

            // FIXME: we call Sample() in parallel
            var x1 = Normal.Sample(0.0, 1.0);
            var x2 = Normal.Sample(0.0, 1.0);

            var z1 = sqrt_step * x1;
            var z2 = sqrt_step * (rho * x1 + one_minus_sqrt_rho * x2);

            var next_S = S + r * S * step + y * S * z1;
            var temp = ((y + gamma * z2) * (y + gamma * z2)) / (4.0 * one_minus_beta_step_square) + constant;
            var next_y = (y + gamma * z2) / (2.0 * one_minus_beta_step) + Math.Sqrt(temp);

            return Tuple.Create(next_S, next_y);
            //return new Tuple<double, double>(next_S, next_y);
        }

        private readonly double r;
        private readonly double v0;
        private readonly double kappa;
        private readonly double theta;
        private readonly double sigma;
        private readonly double rho;
        private readonly double alpha;
        private readonly double beta;
        private readonly double gamma;
        private readonly double one_minus_sqrt_rho;
    }
}
