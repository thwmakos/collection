/*
 * 10/01/2018
 * ~ thwmakos ~
*/

using System;
using System.Numerics;
using System.Linq;

using MathNet.Numerics;
using MathNet.Numerics.Integration;
namespace HestonModel
{
    // this is with formulas from 2.4
    public class HestonFormula2
    {
        public HestonFormula2(double _r, double _v0, double _kappa, double _theta, double _sigma, double _rho)
        {
            kappa = _kappa;
            theta = _theta;
            sigma = _sigma;
            rho = _rho;
            r = _r;
            v0 = _v0;
            a = kappa * theta;
            b1 = kappa - rho * sigma;
            b2 = kappa;
        }

        public double PriceEuropeanCallOption(double S, double K, double T)
        {
            var P1 = Pj(1, 0.0, Math.Log(K), v0, K, T, r);
            var P2 = Pj(2, 0.0, Math.Log(K), v0, K, T, r);

            return S * P1 - K * Math.Exp(-r * T) * P2;
        }

        // put / call parity used for put options
        public double PriceEuropeanPutOption(double S, double K, double T)
        {
            var t1 = PriceEuropeanCallOption(S, K, T);
            var t2 = Math.Exp(-r * T) * K;

            //return PriceEuropeanCallOption(S, K, T) + (Math.Exp(-r * T) * K) - S;
            return t1 + t2 - S;
        }

        private Complex dj(double phi, double bj, double uj)
        {
            var c1 = rho * sigma * phi * Complex.ImaginaryOne - bj;
            c1 = c1 * c1; // square it
            var c2 = (sigma * sigma) * (2.0 * uj * phi * Complex.ImaginaryOne - phi * phi);

            return Complex.Sqrt(c1 - c2);
        }

        private Complex gj(double phi, double bj, double uj)
        {
            var deej = dj(phi, bj, uj);
            var c1 = bj - rho * sigma * phi * Complex.ImaginaryOne;

            return (c1 - deej) / (c1 + deej);
        }

        private Complex Cj(double tau, double phi, double r, double bj, double uj)
        {
            var deej = dj(phi, bj, uj);
            var geej = gj(phi, bj, uj);

            var c1 = (bj - (rho * sigma * phi) * Complex.ImaginaryOne - deej) * tau;
            var c2 = 2.0 * Complex.Log((1.0 - geej * Complex.Exp(-tau * deej)) / (1.0 - geej));

            return r * phi * tau * Complex.ImaginaryOne + (a / sigma * sigma) * (c1 - c2);
        }

        private Complex Dj(double tau, double phi, double bj, double uj)
        {
            var deej = dj(phi, bj, uj);
            var geej = gj(phi, bj, uj);
            var c1 = (bj - rho * sigma * phi * Complex.ImaginaryOne - deej) / (sigma * sigma);
            var c2 = (1.0 - Complex.Exp(-tau * deej)) / (1.0 - geej * Complex.Exp(-tau * deej));

            return c1 * c2;
        }

        private Complex Phij(double time, double x, double v, double phi, double T, double r, double bj, double uj)
        {
            return Complex.Exp(Cj(T - time, phi, r, bj, uj) + Dj(T - time, phi, bj, uj) * v
                + phi * x * Complex.ImaginaryOne);
        }

        private double Pj(int j, double time, double x, double v, double K, double T, double r)
        {
            System.Diagnostics.Debug.Assert(j == 1 || j == 2);

            var bj = (j == 1) ? b1 : b2;

            var uj = (j == 1) ? 0.5 : -0.5;

            // you can actually have function in function ?
            double integrand(double phi)
            {
                var c1 = Complex.Exp(-phi * Math.Log(K) * Complex.ImaginaryOne);
                var temp = ((c1 * Phij(time, x, v, phi, T, r, bj, uj)) / (phi * Complex.ImaginaryOne)).Real;<

                return temp;
            }

            // in Heston's original paper he says that the integral decays fast so integrating from 0 to 200 should be
            // fine, I actually start from 0.1 to avoid divisio by zero
            var integral = SimpsonRule.IntegrateComposite(integrand, 0.001, 100.0, 200);
            //var integral = MathNet.Numerics.Integration.SimpsonRule.IntegrateComposite(integrand, 0.1, 10000.0, 10000);

            return 0.5 + (1.0 / Math.PI) * integral;
        }

        private readonly double kappa;
        private readonly double theta;
        private readonly double sigma;
        private readonly double rho;
        private readonly double r;
        private readonly double v0;
        private readonly double a;
        private readonly double b1;
        private readonly double b2;
    }

    public class HestonFormula
    {
        public HestonFormula(double _r, double _v0, double _kappa, double _theta, double _sigma, double _rho)
        {
            kappa = _kappa;
            theta = _theta;
            sigma = _sigma;
            rho   = _rho;
            r     = _r;
            v0    = _v0;

            a = kappa * theta;
            b1 = kappa - rho * sigma;
            b2 = kappa;
        }

        public double PriceEuropeanCallOption(double S, double K, double T)
        {
            var P1 = Pj(Math.Log(S), T, b1, 0.5);
            var P2 = Pj(Math.Log(S), T, b2, -0.5);

            return S * P1 - K * Math.Exp(-r * T) * P2;
        }

        // put call parity
        public double PriceEuropeanPutOption(double S, double K, double T)
        {
            return PriceEuropeanCallOption(S, K, T) + Math.Exp(-r * T) * K - S;
        }

        private double Pj(double x, double tau, double bj, double uj)
        {
            double integrand(double phi) // you can have functions within function wow
            {
                var f = Phi_j(x, tau, bj, uj, phi);

                return (Complex.Exp(-phi * x * Complex.ImaginaryOne) * f / (phi * Complex.ImaginaryOne)).Real;
            }

            // start from 0.01 to avoid division by zero
            // supposedly the integrand decays quickly so integrating from 0 to 100
            // should suffice
            var intgrl = SimpsonRule.IntegrateComposite(integrand, 0.01, 100.0, 200);

            return 0.5 + (1.0 / Math.PI) * intgrl;
        }

        private Complex Phi_j(double x, double tau, double bj, double uj, double phi)
        {
            var xj = bj - rho * sigma * phi * Complex.ImaginaryOne;
            var dj = Complex.Sqrt(xj * xj - (sigma * sigma) * (2 * uj * phi * Complex.ImaginaryOne - phi * phi));
            var gj = (xj + dj) / (xj - dj);
            var D  = (xj + dj) / (sigma * sigma) * (1.0 - Complex.Exp(dj * tau)) / (1.0 - gj * Complex.Exp(dj * tau));
            var xx = (1.0 - gj * Complex.Exp(dj * tau)) / (1.0 - gj);
            var C  = r * phi * tau * Complex.ImaginaryOne + a / (sigma * sigma) * ((xj + dj) * tau - 2.0 * Complex.Log(xx));

            return Complex.Exp(C + D * v0 + phi * x * Complex.ImaginaryOne);
        }

        private readonly double kappa;
        private readonly double theta;
        private readonly double sigma;
        private readonly double rho;
        private readonly double r;
        private readonly double v0;
        private readonly double a;
        private readonly double b1;
        private readonly double b2;
    }
}
