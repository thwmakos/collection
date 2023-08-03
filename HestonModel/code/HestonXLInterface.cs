using System;
using System.Linq;
using System.Collections.Generic;
using ExcelDna.Integration;
using HestonModel;

namespace HestonXL
{
    public class HestonXLInterface
    {
        static LinkedList<string> errorMessages;

        static HestonXLInterface()
        {
            errorMessages = new LinkedList<string>();
        }

        [ExcelFunction(Description = "About HestonXL function")]
        public static string AboutHestonXL()
        {
            return "Heston Excel Interface for OOP with Applications 2017/18.";
        }

        [ExcelFunction(Description = "Display Error Messages for HestonXL.")]
        public static object[,] GetLatestErrors(int number)
        {
            if(number <= 0)
            {
                string[,] toDisplay = new string[1, 1];
                toDisplay[0, 0] = "GetLatestErrors: You must enter a positive number.";
                return toDisplay;
            }
            else
            {
                string[,] toDisplay = new string[number, 1];
                int msgIdx = 0;

                foreach(string errorMsg in errorMessages)
                {
                    toDisplay[msgIdx, 0] = errorMsg;
                    ++msgIdx;

                    if(msgIdx >= number)
                        break;
                }

                for(; msgIdx < number; ++msgIdx)
                {
                    toDisplay[msgIdx, 0] = "";
                }

                return toDisplay;
            }
        }

        public static object HestonOneOptionPrice(
            double underlying,
            double riskFreeRate,
            double kappa,
            double theta,
            double sigma,
            double rho,
            double v0,
            double maturity,
            double strike,
            string type)
        {
            if(ExcelDnaUtil.IsInFunctionWizard())
                return null;

            try
            {
                var prm = new HestonParameters
                {
                    RiskFreeRate = riskFreeRate,
                    V0 = v0,
                    Kappa = kappa,
                    Theta = theta,
                    Sigma = sigma,
                    Rho = rho
                };

                PayoffType call_or_put;

                if(type == "call")
                    call_or_put = PayoffType.Call;
                else if(type == "put")
                    call_or_put = PayoffType.Put;
                else
                    throw new ArgumentException("invalid payoff");

                var opt = new EuropeanOption
                {
                    Type = call_or_put,
                    Maturity = maturity,
                    Strike = strike
                };


                return Heston.HestonOneOptionPrice(underlying, prm, opt);
            }
            catch(Exception e)
            {
                errorMessages.AddFirst("HestonOneOptionPrice: unknown error: " + e.Message);
                return null;
            }
        }

        public static object HestonOneOptionPriceMC(double underlying,
            double riskFreeRate,
            double kappa,
            double theta,
            double sigma,
            double rho,
            double v0,
            double maturity,
            double strike,
            string type,
            int numSamplePaths,
            int numSteps)
        {

            if(ExcelDnaUtil.IsInFunctionWizard())
                return null;
            try
            {
                var prm = new HestonParameters
                {
                    RiskFreeRate = riskFreeRate,
                    V0 = v0,
                    Kappa = kappa,
                    Theta = theta,
                    Sigma = sigma,
                    Rho = rho
                };

                PayoffType call_or_put;

                if(type == "call")
                    call_or_put = PayoffType.Call;
                else if(type == "put")
                    call_or_put = PayoffType.Put;
                else
                    throw new ArgumentException("invalid payoff");

                var opt = new EuropeanOption
                {
                    Type = call_or_put,
                    Maturity = maturity,
                    Strike = strike
                };

                return Heston.HestonOneOptionPriceMC(underlying, prm, opt, 5000, 200 * (int)maturity);
            }
            catch(Exception e)
            {
                errorMessages.AddFirst("HestonOneOptionPriceMC: unknown error: " + e.Message);
                return null;
            }
        }

        public static object[,] CalibrateHestonParameters(object guessModelParameters,
            double riskFreeRate,
            double underlyingPrice,
            object strikes,
            object maturities,
            object type,
            object observedPrices,
            double accuracy,
            int maxIterations)
        {
            if(ExcelDnaUtil.IsInFunctionWizard())
                return null;

            try
            {
                var strikes_array = ConvertToArray<double>(strikes);
                var maturities_array = ConvertToArray<double>(maturities);
                var types_array = ConvertToArray<string>(type).Select(t =>
                {
                    switch(t)
                    {
                        case "put":
                            return PayoffType.Put;
                        case "call":
                            return PayoffType.Call;
                        default:
                            throw new ArgumentException("invalid payoof type");
                    }
                }).ToArray();

                var observed_prices_array = ConvertToArray<double>(observedPrices);

                if(strikes_array.Length != maturities_array.Length ||
                    maturities_array.Length != types_array.Length ||
                    types_array.Length != observed_prices_array.Length)
                {
                    errorMessages.AddFirst("market data lengths do not match");
                    return null;
                }
                var param_pairs = ConvertToKeyValuePairs(guessModelParameters);

                if(param_pairs.Length != 5)
                {
                    errorMessages.AddFirst("guess model must be 5 key-value pairs ");
                    return null;
                }

                var guess_params = new HestonParameters
                {
                    RiskFreeRate = riskFreeRate
                };

                for(var i = 0; i < param_pairs.Length; i++)
                {
                    var pair = param_pairs[i];

                    if(pair.Key.Equals("kappa"))
                        guess_params.Kappa = pair.Value;
                    else if(pair.Key.Equals("theta"))
                        guess_params.Theta = pair.Value;
                    else if(pair.Key.Equals("sigma"))
                        guess_params.Sigma = pair.Value;
                    else if(pair.Key.Equals("rho"))
                        guess_params.Rho = pair.Value;
                    else if(pair.Key.Equals("v0"))
                        guess_params.V0 = pair.Value;
                    else
                    {
                        errorMessages.AddFirst("invalid pair key");
                        return null;
                    }
                }

                var data = new List<IOptionMarketData<IEuropeanOption>>();

                for(var i = 0; i < strikes_array.Length; i++)
                {
                    IEuropeanOption option = new EuropeanOption
                    {
                        Strike = strikes_array[i],
                        Maturity = maturities_array[i],
                        Type = types_array[i]
                    };

                    IOptionMarketData<IEuropeanOption> d = new OptionMarketData<IEuropeanOption>
                    {
                    Option = option,
                    Price = observed_prices_array[i]
                    };

                    data.Add(d);
                }

                var cal_result = Heston.CalibrateHestonParameters(underlyingPrice, guess_params, data, accuracy, maxIterations);

                var ret = new object[7, 2];

                ret[0, 0] = "kappa"; ret[0, 1] = cal_result.Parameters.Kappa;
                ret[1, 0] = "theta"; ret[1, 1] = cal_result.Parameters.Theta;
                ret[2, 0] = "sigma"; ret[2, 1] = cal_result.Parameters.Sigma;
                ret[3, 0] = "rho"; ret[3, 1] = cal_result.Parameters.Rho;
                ret[4, 0] = "v0"; ret[4, 1] = cal_result.Parameters.V0;
                ret[5, 0] = "minimizer status";

                switch(cal_result.MinimizerStatus)
                {
                    case CalibrationOutcome.FinishedOK:
                        ret[5, 1] = "ok";
                        break;
                    case CalibrationOutcome.FailedMaxItReached:
                        ret[5, 1] = "max iters reached";
                        break;
                    case CalibrationOutcome.FailedOtherReason:
                        ret[5, 1] = "unknown fail";
                        break;
                }

                ret[6, 0] = "pricing error"; ret[6, 1] = cal_result.PricingError;

                return ret;
            }
            catch(Exception e)
            {
                errorMessages.AddFirst("CalibrateHestonParameters: unknown error: " + e.Message);
            }

            return null;
        }

        public static object HestonAsianOptionPriceMC(
            double underlying,
            double riskFreeRate,
            double kappa,
            double theta,
            double sigma,
            double rho,
            double v0,
            double maturity,
            double strike,
            object monitoringTimes,
            string type,
            int numSamplePaths,
            int numSteps)
        {
            if(ExcelDnaUtil.IsInFunctionWizard())
                return null;
            try
            {
                double[] observe_times = ConvertToArray<double>(monitoringTimes);

                var prm = new HestonParameters
                {
                    RiskFreeRate = riskFreeRate,
                    V0 = v0,
                    Kappa = kappa,
                    Theta = theta,
                    Sigma = sigma,
                    Rho = rho
                };

                PayoffType call_or_put;

                if(type == "call")
                    call_or_put = PayoffType.Call;
                else if(type == "put")
                    call_or_put = PayoffType.Put;
                else
                    throw new ArgumentException("invalid payoff");

                return Heston.HestonAsianOptionPriceMC(underlying, prm, maturity, strike, observe_times, call_or_put,
                    numSamplePaths, numSteps);
            }
            catch(Exception e)
            {
                errorMessages.AddFirst("HestonAsianOptionPriceMC error: " + e.Message);
            }

            return null;
        }

        public static object HestonLookbackOptionPriceMC(
            double underlying,
            double riskFreeRate,
            double kappa,
            double theta,
            double sigma,
            double rho,
            double v0,
            double maturity,
            int numSamplePaths,
            int numSteps)
        {
            if(ExcelDnaUtil.IsInFunctionWizard())
                return null;

            try
            {
                var prm = new HestonParameters
                {
                    RiskFreeRate = riskFreeRate,
                    V0 = v0,
                    Kappa = kappa,
                    Theta = theta,
                    Sigma = sigma,
                    Rho = rho
                };

                return Heston.HestonLookbackOptionPriceMC(underlying, prm, maturity, 5000, 200 * (int)maturity);
            }
            catch(Exception e)
            {
                errorMessages.AddFirst("HestonLookbackOptionPriceMC error: " + e.Message);
            }

            return null;
        }

        // Helper function: try to convert input object
        // into specific type
        private static T ConvertTo<T>(object In)
        {
            try
            {
                return (T)In;
            }
            catch (Exception e)
            {
                errorMessages.AddFirst("Could not convert object to " + typeof(T).ToString());
                throw e;
            }
        }

        // Helper function: try to convert input object
        // into array of specific type
        // if the input is a 2D array then it only returns
        // the first column
        private static T[] ConvertToArray<T>(object In)
        {
            T[] V;
            try
            {
                object[] InVec;

                if(In.GetType() == typeof(object[]))
                {
                    InVec = (object[])In;
                    int length = InVec.GetLength(0);
                    V = new T[length];

                    for(int i = 0; i < length; i++)
                    {
                        V[i] = ConvertTo<T>(InVec[i]);
                    }
                    return V;

                }
                else if(In.GetType() == typeof(object[,]))
                {
                    object[,] InM = (object[,])In;
                    int rows = InM.GetLength(0);
                    V = new T[rows];

                    for(int i = 0; i < rows; i++)
                    {
                        V[i] = ConvertTo<T>(InM[i, 0]);
                    }
                    return V;
                }
                else
                {
                    errorMessages.AddFirst("Could not convert input to array of type " + typeof(T).ToString());
                    return null;
                }
            }
            catch(Exception)
            {
                errorMessages.AddFirst("Could not convert input to array of type " + typeof(T).ToString());
                return null;
            }
        }

        // Helper function: try to convert input object
        // into a key value pair of string and double
        private static KeyValuePair<string, double>[] ConvertToKeyValuePairs(object In)
        {
            KeyValuePair<string, double>[] keyValPairs;
            try
            {
                object[,] In2D = (object[,])In;
                int rows = In2D.GetLength(0);
                int cols = In2D.GetLength(1);

                if (cols != 2)
                {
                    Console.WriteLine("Need two colums!");
                    return null;
                }

                keyValPairs = new KeyValuePair<string, double>[rows];

                for (int i = 0; i < rows; i++)
                {
                    string key = ConvertTo<string>(In2D[i, 0]);
                    double value = ConvertTo<double>(In2D[i, 1]);
                    KeyValuePair<string, double> pair = new KeyValuePair<string, double>(key, value);
                    keyValPairs[i] = pair;
                }

                return keyValPairs;
            }
            catch (Exception)
            {
                errorMessages.AddFirst("Could not create key - value pair.");
                return null;
            }
        }
    }
}
