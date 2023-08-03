using System;
using System.Linq;
using System.Collections.Generic;

namespace HestonModel
{
    public interface ICalibrationResult
    {
        CalibrationOutcome MinimizerStatus { get; }
        double PricingError { get; } // what's this supposed to be set to?
    }

    public interface IHestonCalibrationResult : ICalibrationResult
    {
        IHestonParameters Parameters { get; }
    }

    public interface IHestonParameters
    {
        double RiskFreeRate { get; } // Continuously compounded risk free rate
        double Kappa { get; } // Mean reversion speed in Heston model
        double Theta { get; } // The long-term mean in Heston model
        double Sigma { get; } // The vol of vol in Heston model
        double Rho { get; } // The correlation between asset price and vol of vol in Heston model
        double V0 { get; } // Initial variance in Heston model
    }

    public interface IOption
    {
        double Maturity { get; }
        //Option maturity as a year fraction (i.e. 1 means one year)
    }

    public interface IEuropeanOption : IOption
    {
        double Strike { get; }
        PayoffType Type { get; }
    }

    public interface IOptionMarketData<T> where T : IOption
    {
        T Option { get; }
        double Price { get; }
    }

    public enum PayoffType { Call, Put };
    public enum CalibrationOutcome { NotStarted, FinishedOK, FailedMaxItReached, FailedOtherReason };

    public struct HestonCalibrationResult : IHestonCalibrationResult
    {
        public CalibrationOutcome MinimizerStatus { get; set; }
        public double PricingError { get; set; }
    }
    public IHestonParameters Parameters { get; set; }

    public struct HestonParameters : IHestonParameters
    {
        public double RiskFreeRate { get; set; }
        public double Kappa { get; set; }
        public double Theta { get; set; }
        public double Sigma { get; set; }
        public double Rho { get; set; }
        public double V0 { get; set; }
    }

    public struct EuropeanOption : IEuropeanOption
    {
        public EuropeanOption(PayoffType _type, double T, double K)
        {
            Type = _type;
            Maturity = T;
            Strike = K;
        }

        public PayoffType Type { get; set; }
        public double Maturity { get; set; }
        public double Strike { get; set; }
    }

    public class OptionMarketData<T> : IOptionMarketData<T> where T : IOption
    {
        public T Option { get; set; }
        public double Price { get; set; }
    }

    /// <summary>
    /// This class will be used for grading. Please keep it within "HestonModel" namespace,
    /// feel free to add to it as needed, but don't remove the existing methods or modify their signatures
    /// </summary>
    public static class Heston
    {
        /// <summary>
        /// Method for calibrating the heston model parameters.
        /// </summary>
        /// <param name="underlying">The current stock price</param>
        /// <param name="guessModelParameters">Object implementing IHestonParameters interface containing the risk-free
        /// rate
        /// and initial guess parameters to be used in calibration.</param>
        /// <param name="referenceData">A colection of objects implementing IOptionMarketData<IEuropeanOption> interface c
        /// These should contain the reference data used for calibration.</param>
        /// <param name="accuracy">A parameter influencing the accuracy the minimization algorithm is trying to achieve.
        /// Note that we are
        /// allowing more options than parameters so we don't necessarily expect to be able to re-price all the
        /// options.</param>
        /// <param name="maxIterations">The maximum number of iterations you allow the minimization algorithm to use.
        /// Note that even 10 iterations
        /// can take more than a few seconds!</param>
        /// <returns>Object implementing IHestonCalibrationResult interface which contains calibrated model parameters
        /// and addtional diagnostic information</returns>
        public static IHestonCalibrationResult CalibrateHestonParameters(double underlying, IHestonParameters,
            guessModelParameters, IEnumerable<IOptionMarketData<IEuropeanOption>> referenceData, double accuracy, int maxIterations)
        {
            if(accuracy <= 0.0 || maxIterations <= 0 || underlying <= 0 || !referenceData.Any())
                throw new ArgumentException("invalid arguments in calibration");

            var cal = new HestonCalibrator(guessModelParameters.RiskFreeRate, new HestonParams
            {
                kappa = guessModelParameters.Kappa,
                theta = guessModelParameters.Theta,
                sigma = guessModelParameters.Sigma,
                rho
                = guessModelParameters.Rho,
                v0
                = guessModelParameters.V0
            });

            foreach(var option in referenceData)
            {
                cal.market_data.Add(new MarketDataEntry
                {
                    type = option.Option.Type,
                    S = underlying,
                    K = option.Option.Strike,
                    T = option.Option.Maturity,
                    price = option.Price

                });

            cal.Calibrate(accuracy, maxIterations);

            var p = new HestonParameters
            {
                RiskFreeRate = guessModelParameters.RiskFreeRate,
                Kappa = cal.calibrated_params.kappa,
                Theta = cal.calibrated_params.theta,
                Sigma = cal.calibrated_params.sigma,
                Rho = cal.calibrated_params.rho,
                V0 = cal.calibrated_params.v0
            };

            var res = new HestonCalibrationResult
            {
                MinimizerStatus = cal.outcome,
                Parameters = p,
                PricingError = cal.MeanSquareError(new HestonFormula(p.RiskFreeRate, p.V0, p.Kappa, p.Theta, p.Sigma, p.Rho))
                };
            }
            return res;
        }

        /// <summary>
        /// Price a call or put European option in the Heston model using the
        /// Heston formula. This should be accurate to 5 decimal places
        /// </summary>
        /// <param name="underlying">The current stock price</param>
        /// <param name="parameters">Object implementing IHestonParameters interface containing the risk-free rate
        /// and the Heston model parameters.</param>
        /// <param name="option">Object implementing IEuropeanOption interface, containing the option parameters.</param>
        /// <returns>Option price</returns>
        public static double HestonOneOptionPrice(double underlyingPrice, IHestonParameters parameters, IEuropeanOption option)
        {
            if(underlyingPrice <= 0.0 || option.Maturity < 0.0 || option.Strike < 0.0)
                throw new ArgumentException();

            var hf = new HestonFormula(parameters.RiskFreeRate, parameters.V0, parameters.Kappa, parameters.Theta, parameters.Sigma, parameters.Rho);

            return (option.Type == PayoffType.Call) ?
                hf.PriceEuropeanCallOption(underlyingPrice, option.Strike, option.Maturity) :
                hf.PriceEuropeanPutOption(underlyingPrice, option.Strike, option.Maturity);
        }

        /// <summary>
        /// Price a call or put European option in the Heston model using the
        /// Monte-Carlo method. Accuracy will depend on number of time steps and samples
        /// </summary>
        /// <param name="underlying">The current stock price</param>
        /// <param name="parameters">Object implementing IHestonParameters interface containing the risk-free rate
        /// and the Heston model parameters.</param>
        /// <param name="option">Object implementing IEuropeanOption interface, containing the option parameters.</param>
        /// <param name="numSamplePaths">The number of sample paths generated for Monte-Carlo valuation</param>
        /// <param name="numSteps">The number of time steps for each path</param>
        /// <returns>Option price</returns>
        public static double HestonOneOptionPriceMC(double underlying, IHestonParameters parameters, IEuropeanOption option, int numSamplePaths, int numSteps)
        {
            // feller condition is checked in HestonMC ctor
            if(underlying <= 0.0 || option.Maturity < 0.0 || option.Strike < 0.0 || numSamplePaths <= 0 || numSteps <= 0)
                throw new ArgumentException();

            var mc = new HestonMC(parameters.RiskFreeRate, parameters.V0, parameters.Kappa, parameters.Theta, parameters.Sigma, parameters.Rho);

            return (option.Type == PayoffType.Call) ?
                mc.GetCallOptionPrice(underlying, option.Strike, option.Maturity, numSamplePaths, numSteps) :
                mc.GetPutOptionPrice(underlying, option.Strike, option.Maturity, numSamplePaths, numSteps);
        }

        /// <summary>
        /// Price a call or put Asian option in the Heston model using the
        /// Monte-Carlo method. Accuracy will depend on number of time steps and samples</summary>
        /// <param name="underlying">The current stock price</param>
        /// <param name="parameters">Object implementing IHestonParameters interface containing the risk-free rate
        /// and the Heston model parameters.</param>
        /// <param name="maturity">Option maturity</param>
        /// <param name="strike">Option strike</param>
        /// <param name="monitoringTimes">Collection of times (expressed as year fraction)
        /// denoting the times over which the average is calculated.</param>
        /// <param name="payoffType">Payoff type</param>
        /// <param name="numSamplePaths">The number of sample paths generated for Monte-Carlo valuation</param>
        /// <param name="numSteps">The number of time steps for each path</param>
        /// <returns>Option price</returns>
        public static double HestonAsianOptionPriceMC(double underlying, IHestonParameters parameters, double maturity, double strike,
            IEnumerable<double> monitoringTimes, PayoffType payoffType, int numSamplePaths, int numSteps)
        {
            // feller condition is checked in HestonMC ctor
            if(underlying <= 0.0 || maturity < 0.0 || strike < 0.0 || numSamplePaths <= 0 || numSteps <= 0)
                throw new ArgumentException();

            // CAREFUL: I do not consider the maturity as a monitoring time (unless it is included in the monitoring
            // times)
            // see HestonMC.cs too
            // this is in contrast to the paper on Monter Carlo in the course website where it is considered
            // automatically in the monitoring times

            var observe_times = monitoringTimes.ToArray();
            Array.Sort(observe_times);

            if(observe_times.Max() > maturity || observe_times.Min() < 0.0)
                throw new ArgumentException("fail monitoring times ");

            var mc = new HestonMC(parameters.RiskFreeRate, parameters.V0, parameters.Kappa, parameters.Theta, parameters.Sigma, parameters.Rho);

            return (payoffType == PayoffType.Call) ?
                mc.GetAsianCallOptionPrice(underlying, strike, maturity, observe_times, numSamplePaths, numSteps) :
                mc.GetAsianPutOptionPrice(underlying, strike, maturity, observe_times, numSamplePaths, numSteps);
        }

        /// <summary>
        /// Price a lookback option in the Heston model using the
        /// a Monte-Carlo method. Accuracy will depend on number of time steps and samples </summary>
        /// <param name="underlying">The current stock price</param>
        /// <param name="parameters">Object implementing IHestonParameters interface containing the risk-free rate
        /// and the Heston model parameters.</param>
        /// <param name="maturity">Option maturity</param>
        /// <param name="numSamplePaths">The number of sample paths generated for Monte-Carlo valuation</param>
        /// <param name="numSteps">The number of time steps for each path</param>
        /// <returns>Option price</returns>
        public static double HestonLookbackOptionPriceMC(double underlying, IHestonParameters parameters, double maturity, int numSamplePaths,
            int numSteps)
        {
            // feller condition is checked in HestonMC ctor
            if(underlying <= 0.0 || maturity < 0.0 || numSamplePaths <= 0 || numSteps <= 0)
                throw new ArgumentException();

            var mc = new HestonMC(parameters.RiskFreeRate, parameters.V0, parameters.Kappa, parameters.Theta, parameters.Sigma, parameters.Rho);

            return mc.GetLookbackOptionPrice(underlying, maturity, numSamplePaths, numSteps);
        }
    }
}
