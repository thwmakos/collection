/*
 * HestonMC.cs - 13/01/2018
 */
using System;
using System.Linq;

namespace HestonModel
{
    // option pricing using monte carlo
    public class HestonMC
    {
        public HestonMC(double _r, double _v0, double _kappa, double _theta, double _sigma, double _rho)
        {
            kappa = _kappa;
            theta = _theta;
            sigma = _sigma;
            rho = _rho;
            r = _r;
            v0 = _v0;

            if(2.0 * kappa * theta <= sigma * sigma)
                throw new ArgumentException("Feller condition violated!");

            pg = new PathGenerator(r, v0, kappa, theta, sigma, rho);
        }

        public double GetCallOptionPrice(double S, // curr price
            double K, // strike
            double T, // maturity
            int num_paths = 5000,
            int num_timesteps = 1000)
        {
            var paths = GenEuropeanOptionSample(S, K, T, num_paths, num_timesteps);
            // LINQ is actually usefull
            // I hope it doens't just add all numbers together
            // and then divide because this will cause terrible
            // fp inaccuracies for large numbers of paths or it
            // may cause overflow
            // FIXME: exception is thrown in case of overflow?
            var temp = paths.Average((x) => Math.Max(x - K, 0.0));

            return Math.Exp(-r * T) * temp;
        }

        public double GetPutOptionPrice(double S,
            double K,
            double T,
            int num_paths = 5000,
            int num_timesteps = 1000)
        {
            var paths = GenEuropeanOptionSample(S, K, T, num_paths, num_timesteps);
            var temp = paths.Average(x => Math.Max(K - x, 0.0));

            return Math.Exp(-r * T) * temp;
        }

        // arithmetic asian call option
        public double GetAsianCallOptionPrice(double S,
            double K,
            double T,
            double[] observe_times,
            int num_paths = 5000,
            int num_timesteps = 1000)
        {
            var samples = GenAsianOptionSample(S, K, T, observe_times, num_paths, num_timesteps);

            return Math.Exp(-r * T) * samples.Average((x) => Math.Max(x - K, 0));
        }

        // arithmetic asian put
        public double GetAsianPutOptionPrice(double S,
            double K,
            double T,
            double[] observe_times,
            int num_paths = 5000,
            int num_timesteps = 100)
        {
            var samples = GenAsianOptionSample(S, K, T, observe_times, num_paths, num_timesteps);

            return Math.Exp(-r * T) * samples.Average(x => Math.Max(K - x, 0));
        }

        // lookback option
        public double GetLookbackOptionPrice(double S,
            double T,
            int num_paths = 5000,
            int num_timesteps = 1000)
        {
            var t = pg.GenPaths(num_paths, num_timesteps, T, S, true);
            var samples = t.Item1;
            var mins = t.Item2;
            var diffs = Enumerable.Zip(samples, mins, (x, y) => x - y);

            return Math.Exp(-r * T) * diffs.Average();
        }

        // this simply generates sample paths for use in European call/put
        // pricing
        private double[] GenEuropeanOptionSample(double S,
            double K,
            double T,
            int num_paths,
            int num_timesteps)
        {
            return pg.GenPaths(num_paths, num_timesteps, T, S).Item1;
        }

        // same as above, but for asian put and call
        // generate the sums 1/M S(T_m)
        private double[] GenAsianOptionSample(double S,
            double K,
            double T,
            double[] observe_times,
            int num_paths,
            int num_timesteps)
        {
            int num_timesteps_per_year = (int)(num_timesteps / T);
            // sums of S(T_m) for all m for each path
            var sums = new double[num_paths];
            for(var i = 0; i < num_paths; i++)
                sums[i] = 0;

            foreach(var paths in pg.GenPaths(num_paths, num_timesteps_per_year, T, S, observe_times))
            {
                for(var i = 0; i < num_paths; i++)
                    sums[i] += paths[i];
            }

            for(var i = 0; i < num_paths; i++)
                sums[i] /= observe_times.Length; // divide by M

            //var averages = sums.Select(x => x / observe_times.Length);

            return sums;
        }

        // class state, immutable if you want to change these
        // just create a new instance
        private readonly double kappa;
        private readonly double theta;
        private readonly double sigma;
        private readonly double rho;
        private readonly double r;
        private readonly double v0;

        private readonly PathGenerator pg;
    }
}
