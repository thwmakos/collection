/*
 *
 * HestonCalibrator.cs
 *
 * 14/01/2018
 *
 * ~thwmakos~
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HestonModel
{
    public class HestonParams
    {
        public double kappa;
        public double theta;
        public double sigma;
        public double rho;
        public double v0;

        public const int NumParams = 5;
    }

    public struct MarketDataEntry
    {
        public MarketDataEntry(PayoffType _type, double _S, double _K, double _T,
            double _price)
        {
            type = _type;
            S = _S;
            K = _K;
            T = _T;
            price = _price;
        }

        public PayoffType type;
        public double S;
        public double K;
        public double T;
        public double price;
    }

    public class HestonCalibrator
    {
        public HestonCalibrator(double _r0, HestonParams initial)
        {
            r0 = _r0;
            market_data = new List<MarketDataEntry>();
            outcome = CalibrationOutcome.NotStarted;
            // this does not copy but w/e
            initial_guess = calibrated_params = initial;
        }

        public double MeanSquareError(HestonFormula hf)
        {
            var mean_sq_error = 0.0;

            foreach(var option in market_data)
            {
                var model_price = (option.type == PayoffType.Call) ?
                hf.PriceEuropeanCallOption(option.S, option.K, option.T) : hf.PriceEuropeanPutOption(option.S,
                 option.K, option.T);

                var diff = model_price - option.price;
                mean_sq_error += diff * diff;
            }

            return mean_sq_error;
        }

        public void ObjectiveFunction(double[] parameters, ref double func, object obj)
        {
            // parameters are in the following order:
            // kappa, theta, sigma, rho, v0
            var hf = new HestonFormula(r0, parameters[4], parameters[0], parameters[1],
                parameters[2], parameters[3]);

            func = MeanSquareError(hf);
        }

        public void Calibrate(double accuracy = 1.0e-2, int max_iterations = 100)
        {
            outcome = CalibrationOutcome.NotStarted;

            double[] initial_params = new double[HestonParams.NumParams]
            {
                initial_guess.kappa,
                initial_guess.theta,
                initial_guess.sigma,
                initial_guess.rho,
                initial_guess.v0
            };

            double differentation_step = 1.0e-4;
            double stpmax = 0.5;

            alglib.minlbfgsstate  state;
            alglib.minlbfgsreport report;
            // create and set up the optimizer
            alglib.minlbfgscreatef(1, initial_params, differentation_step, out state);
            alglib.minlbfgssetcond(state, accuracy, accuracy, accuracy, max_iterations);
            alglib.minlbfgssetstpmax(state, stpmax);

            // actually do optimize and retrieve results
            alglib.minlbfgsoptimize(state, ObjectiveFunction, null, null);

            var result_params = new double[HestonParams.NumParams];

            alglib.minlbfgsresults(state, out result_params, out report);

            // copy, but dont use before checking exit flag
            calibrated_params.kappa = result_params[0]; // no memcpy in C#
            calibrated_params.theta = result_params[1];
            calibrated_params.sigma = result_params[2];
            calibrated_params.rho   = result_params[3];
            calibrated_params.v0    = result_params[4];

            if((new int[] { 1, 2, 4}).Contains(report.terminationtype))
            {
                outcome = CalibrationOutcome.FinishedOK;
            }
            else if ((new int[] { 5 }).Contains(report.terminationtype))
            {
                outcome = CalibrationOutcome.FailedMaxItReached;
            }
            else
            {
                outcome = CalibrationOutcome.FailedOtherReason;

                throw new ArithmeticException("Calibration failed :(");
            }
        }

        public List<MarketDataEntry> market_data; // public so ppl can add stuff
        public CalibrationOutcome outcome { get; private set; }
        public HestonParams calibrated_params { get; private set; }
        private HestonParams initial_guess;
        private double r0; // risk free rate, not calibrated
    }
}
