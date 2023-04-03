using Binance.Net.Clients;
using Binance.Net.Clients.SpotApi;
using Binance.Net.Enums;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CryptoExchange.Net.Interfaces;
using System;
using Newtonsoft.Json.Linq;
using Binance.Net.Interfaces.Clients;
using System.Runtime.CompilerServices;
using CryptoExchange.Net.CommonObjects;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace Indicators_CSharp
{
    class Program
    {
        static string baseQ = "BTC";
        static string quoteQ = "USDT";
        static string symbol = baseQ + quoteQ;

        //MACD
        //MACD candlestick period settings are in the method itself,
        //use CTRL+H to replace TIMEFRAME (OneHour, FiveMinutes, etc.)
        static int fastEMA = 12;
        static int slowEMA = 26;
        static int signalEMA = 9;

        //Supertrend
        //The supertrend line may lag behind the exchange values when the Factor increases,
        //it is recommended not more than 1-2
        static int STPeriod = 10; 
        static double STFactor = 3;

        //Stoch
        static int fastK1m = 14;
        static int slowD1m = 1;
        static int smooth1m = 3;

        //Bolliger
        static int period = 20;
        static double deviation = 2;

        //DonchianChannels
        static int DonchianPeriod = 20;

        //RSI Period
        static int RSI_Period = 14;

        //Ichimoku Cloud
        static int tenkanSenPeriod = 9;
        static int kijunSenPeriod = 26;
        static int senkouSpanBPeriod = 52;




        static void Main(string[] args)
        {

            CheckIndicators();

        }


        public static void CheckIndicators()
        {

            MACD macd = new MACD();

            dynamic closePrices1m = new List<decimal>();
            dynamic highPrices1m = new List<decimal>();
            dynamic lowPrices1m = new List<decimal>();
            /*dynamic openPrices1m = new List<decimal>();*/

            var client1m = new BinanceClient();
            while (true)
            {
                var result1m = client1m.SpotApi.ExchangeData.GetKlinesAsync(symbol, KlineInterval.OneMinute).Result;
                if (result1m.Success)
                {
                    foreach (var candle in result1m.Data)
                    {
                        closePrices1m.Add(candle.ClosePrice);
                        highPrices1m.Add(candle.HighPrice);
                        lowPrices1m.Add(candle.LowPrice);
                        /*openPrices1m.Add(candle.OpenPrice);*/

                    }
                }
                else
                {
                    Console.WriteLine($"Error: {result1m.Error.Message}");
                }
                double[] arrCloses1m = ConvertToDouble(closePrices1m);
                /*double[] arrOpen1m = ConvertToDouble(openPrices1m);*/
                double[] arrHigh1m = ConvertToDouble(highPrices1m);
                double[] arrLow1m = ConvertToDouble(lowPrices1m);

                int enter_points = 0;

                //SUPERTREND**********************************************
                var (supper777, check) = Supertrend.Calculate(arrHigh1m, arrLow1m, arrCloses1m, STPeriod, STFactor);
                var fastKer777_val1mSUPP = supper777[supper777.Length - 1];
                var fastKer777_val1mSUPP_DOUBLE = Convert.ToDouble(Math.Round(fastKer777_val1mSUPP, 2));

                Console.WriteLine("Supertrend: " + fastKer777_val1mSUPP_DOUBLE);
                Console.WriteLine("Supertrend Check: " + check); //False = Bullish Trend, True = Bearish Trend


                //MACD**********************************************
                var (macdLine1m, signalLine1m) = macd.MainMACD(fastEMA, slowEMA, signalEMA);
                var macd1m_val = macdLine1m[macdLine1m.Length - 1];
                var signalLine_val = signalLine1m[signalLine1m.Length - 1];
                var macd1m_val_format = Math.Round(macd1m_val, 3);
                var sign1m_val_format = Math.Round(signalLine_val, 3);

                Console.WriteLine("Long MACD: {0}, Signal: {1}", macd1m_val_format, sign1m_val_format);


                //SMA10*30*********************************************
                decimal[] arrSMA = closePrices1m.ToArray();

                var SMA1m10 = SMA(arrCloses1m, 10);
                var SMA1m10_val = SMA1m10[SMA1m10.Length - 1];
                var SMA1m10_val_format1m = Math.Round(SMA1m10_val, 2);

                var SMA1m30 = SMA(arrCloses1m, 30);
                var SMA1m30_val = SMA1m30[SMA1m30.Length - 1];
                var SMA1m30_val_format1m = Math.Round(SMA1m30_val, 2);

                Console.WriteLine("SMA10: " + SMA1m10_val_format1m);
                Console.WriteLine("SMA30: " + SMA1m30_val_format1m);


                //RSI**********************************************
                var RSI1m = RSI(arrCloses1m, RSI_Period);
                var rsi1m_val = RSI1m[RSI1m.Length - 1];
                var rsi_val_format1m = Math.Round(rsi1m_val, 2);

                Console.WriteLine("RSI: " + rsi_val_format1m);


                //STOCH**********************************************
                var (fastKValues1m, slowKValues1m) = STOCH(arrHigh1m, arrLow1m, arrCloses1m, fastK1m, slowD1m, smooth1m);
                var fastK_val1m = fastKValues1m[fastKValues1m.Length - 1];
                var slowD_val1m = slowKValues1m[slowKValues1m.Length - 1];
                var fastK_val_format1m = Math.Round(fastK_val1m, 3);
                var slowD_val_format1m = Math.Round(slowD_val1m, 3);

                Console.WriteLine("STOCH Fast: " + fastK_val_format1m + ", STOCH Slow: " + slowD_val_format1m);


                //BollingerBands**********************************************
                double[] prices = arrCloses1m;
                double[] sma;
                double[] upperBand;
                double[] lowerBand;
                BollingerBands(arrCloses1m, period, deviation, out sma, out upperBand, out lowerBand);

                Console.WriteLine("BOLINGER: Upper Band: {2}, SMA: {1}, Lower Band: {3}, Price: {0}", Math.Round(prices[prices.Length - 1], 2), Math.Round(sma[sma.Length - 1], 2), Math.Round(upperBand[upperBand.Length - 1], 2), Math.Round(lowerBand[lowerBand.Length - 1], 2));


                //DonchianChannels**********************************************
                var (upper, lower, middle) = DonchianChannels.Calculate(arrHigh1m, arrLow1m, DonchianPeriod);
                Console.WriteLine($"DONCHIAN: Upper: {upper[upper.Length - 1]}, Middle: {middle[middle.Length - 1]}, Lower: {lower[lower.Length - 1]}");


                //IchimokuCloud**********************************************
                IchimokuCloudIndicator ichimoku = new IchimokuCloudIndicator(arrHigh1m, arrLow1m, tenkanSenPeriod, kijunSenPeriod, senkouSpanBPeriod);
                double[] tenkanSen = ichimoku.GetTenkanSen();
                double[] kijunSen = ichimoku.GetKijunSen();
                double[] senkouSpanA = ichimoku.GetSenkouSpanA();
                double senkouSpanAFormat = Math.Round(senkouSpanA[senkouSpanA.Length - 1], 2);
                double[] senkouSpanB = ichimoku.GetSenkouSpanB();
                double kijunSenFormat = Math.Round(kijunSen[kijunSen.Length - 1], 2);
                bool bear = false;
                bool bull = false;

                Console.WriteLine($"ICHIMOKU: Tenkan-sen: {tenkanSen[tenkanSen.Length - 1]}, Kijun-sen: {kijunSenFormat}, Close: {arrCloses1m[arrCloses1m.Length - 1]}, Senkou Span A: {senkouSpanAFormat}, Senkou Span B: {senkouSpanB[senkouSpanB.Length - 1]}");

                for (int i = 0; i < arrHigh1m.Length; i++)
                {
                    if (ichimoku.IsBullish(i))
                    {
                        bull = true;

                    }
                    else if (ichimoku.IsBearish(i))
                    {
                        bear = true;

                    }
                }

                if (bull == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("ICHIMOKU: Bullish signal");
                    Console.ResetColor();
                }
                else if (bear == true)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ICHIMOKU: Bearish signal");
                    Console.ResetColor();
                }

                Thread.Sleep(3000);
                Console.WriteLine();


                ///////////////////////////////////////////////////////////////////////////////
                //Experemental 


                //The experimental method calculates the minimum value of the price for a certain period
                //***************************************************************

                /*var client2 = new BinanceClient();
                var interval = 1;
                var MinMaxPeriod = 100;

                var candles = client2.SpotApi.ExchangeData.GetKlinesAsync(symbol, KlineInterval.OneMinute, DateTime.UtcNow.AddMinutes(-interval * period), DateTime.UtcNow, MinMaxPeriod).Result;

                if (candles.Success)
                {
                    decimal min = decimal.MaxValue;
                    decimal max = decimal.MinValue;
                    foreach (var candle in candles.Data)
                    {
                        if (candle.LowPrice < min) min = (decimal)candle.LowPrice;
                        if (candle.HighPrice > max) max = (decimal)candle.HighPrice;
                    }

                    *//*decimal currentPrice = candles.Data.Last().ClosePrice;*//*
                    var ticker = client2.SpotApi.ExchangeData.GetPriceAsync(symbol).Result;
                    if (ticker.Data.Price <= min)
                    {
                        // The price is at the minimum, perform a certain logic, for example, buy on the market
                        enter_points += 1;
                        Console.WriteLine("The candlestick minimum indicator has triggered, the current value: " + enter_points);

                    }

                    //***************************************************************************
                    //Fibonacci has not been tested properly ************************************
                    decimal currentPrice = result1m.Data.Last().ClosePrice;

                    // Checking Fibonacci levels
                    var fibonacciLevels = GetFibonacciLevels(currentPrice, min, max);

                    // Check if the price is at the 23.6% Fibonacci level
                    if (currentPrice < fibonacciLevels.Level1)
                    {
                        enter_points += 1;
                        Console.WriteLine("Fibonacci indicator triggered, current value: " + enter_points);
                    }
                }*/
            }
        }





        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //INDICATORS AND SOFT METODS////////////////////////////////////////////////////////////////////////////
        private class FibonacciLevels
        {
            public decimal Level1 { get; set; }
            public decimal Level2 { get; set; }
            public decimal Level3 { get; set; }
            public decimal Level4 { get; set; }
            public decimal Level5 { get; set; }
            public decimal Level6 { get; set; }
        }

        private static FibonacciLevels GetFibonacciLevels(decimal currentPrice, decimal low, decimal high)
        {
            var range = high - low;
            return new FibonacciLevels
            {
                Level1 = high - range * 0.236m,
                Level2 = high - range * 0.382m,
                Level3 = high - range * 0.5m,
                Level4 = high - range * 0.618m,
                Level5 = high - range * 0.786m,
                Level6 = low + range * 1.618m
            };

        }

        public static (double[], double[]) STOCH(double[] high, double[] low, double[] close, int fastK_Period, int slowK_Period, int slowD_Period)
        {
            int len = high.Length;
            double[] fastK = new double[len];
            double[] slowK = new double[len];
            double[] slowD = new double[len];

            for (int i = 0; i < len; i++)
            {
                double highestHigh = high.Skip(i - fastK_Period + 1).Take(fastK_Period).Max();
                double lowestLow = low.Skip(i - fastK_Period + 1).Take(fastK_Period).Min();
                double currentClose = close[i];

                fastK[i] = (currentClose - lowestLow) / (highestHigh - lowestLow) * 100;
            }

            for (int i = 0; i < len; i++)
            {
                slowK[i] = fastK.Skip(i - slowK_Period + 1).Take(slowK_Period).Average();
            }

            for (int i = 0; i < len; i++)
            {
                slowD[i] = slowK.Skip(i - slowD_Period + 1).Take(slowD_Period).Average();
            }

            return (slowK, slowD);
        }

        public static (double[], double[]) STOCHRSI(double[] close, int period, int fastk_period, int fastd_period)
        {
            var rsi = RSI(close, period);
            return STOCH(rsi, rsi, rsi, 3, fastk_period, fastd_period);
        }


        public class MACD
        {
            public (double[], double[]) MainMACD(int fastEMA, int slowEMA, int signalEMA)
            {

                var client111m = new BinanceClient();
                dynamic closePrices1m = new List<decimal>();


                var result1m = client111m.SpotApi.ExchangeData.GetKlinesAsync(symbol, KlineInterval.OneMinute).Result;
                if (result1m.Success)
                {
                    foreach (var candle in result1m.Data)
                    {
                        closePrices1m.Add(candle.ClosePrice);

                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка: {result1m.Error.Message}");
                }
                double[] closingPrices = ConvertToDouble(closePrices1m);


                double[] emaSlow = EMA(closingPrices, slowEMA); // расчет медленной EMA
                double[] emaFast = EMA(closingPrices, fastEMA); // расчет быстрой EMA

                double[] macdLine = new double[500]; // массив значений MACD-линии
                for (int i = 0; i < 500; i++)
                {
                    macdLine[i] = emaFast[i] - emaSlow[i]; // вычисление значения MACD-линии
                }

                double[] signalLine = EMA(macdLine, signalEMA); // расчет сигнальной линии


                // вывод значений MACD-линии и сигнальной линии

                return (macdLine, signalLine);

            }

            // функция для расчета EMA
            static double[] EMA(double[] closingPrices, int period)
            {
                double[] ema = new double[closingPrices.Length];

                double k = 2.0 / (period + 1);

                ema[0] = closingPrices[0];

                for (int i = 1; i < closingPrices.Length; i++)
                {
                    ema[i] = closingPrices[i] * k + ema[i - 1] * (1 - k);
                }

                return ema;
            }
        }


        public static double[] EMA(double[] data, int period)
        {
            double[] ema = new double[data.Length];
            double multiplier = 2.0 / (period + 1);

            // Calculate initial SMA
            double sma = 0;
            for (int i = 0; i < period; i++)
            {
                sma += data[i];
            }
            sma /= period;
            ema[period - 1] = sma;

            // Calculate subsequent EMAs
            for (int i = period; i < data.Length; i++)
            {
                ema[i] = (data[i] - ema[i - 1]) * multiplier + ema[i - 1];
            }

            return ema;
        }


        public static double[] RSI(double[] close, int period)
        {
            
            double[] prices = close;


            double[] gains = new double[prices.Length];
            double[] losses = new double[prices.Length];

            for (int i = 1; i < prices.Length; i++)
            {
                double diff = prices[i] - prices[i - 1];

                if (diff > 0)
                {
                    gains[i] = diff;
                    losses[i] = 0;
                }
                else
                {
                    gains[i] = 0;
                    losses[i] = Math.Abs(diff);
                }
            }

            double[] avgGains = new double[prices.Length];
            double[] avgLosses = new double[prices.Length];

            avgGains[period] = gains.Take(period).Average();
            avgLosses[period] = losses.Take(period).Average();

            for (int i = period + 1; i < prices.Length; i++)
            {
                avgGains[i] = ((avgGains[i - 1] * (period - 1)) + gains[i]) / period;
                avgLosses[i] = ((avgLosses[i - 1] * (period - 1)) + losses[i]) / period;
            }

            double[] rs = new double[prices.Length];

            for (int i = period; i < prices.Length; i++)
            {
                if (avgLosses[i] == 0)
                {
                    rs[i] = 100;
                }
                else
                {
                    rs[i] = (avgGains[i] / avgLosses[i]);
                }
            }

            double[] rsi = new double[prices.Length];

            for (int i = period; i < prices.Length; i++)
            {
                rsi[i] = 100 - (100 / (1 + rs[i]));
            }

            return rsi;
        }


        public static double[] CCI(double[] high, double[] low, double[] close, int period)
        {
            double[] typicalPrice = new double[high.Length];
            for (int i = 0; i < high.Length; i++)
            {
                typicalPrice[i] = (high[i] + low[i] + close[i]) / 3;
            }

            double[] cci = new double[typicalPrice.Length];
            for (int i = period - 1; i < typicalPrice.Length; i++)
            {
                double sum = 0;
                for (int j = i; j > i - period; j--)
                {
                    sum += typicalPrice[j];
                }
                double sma = sum / period;
                double meanDeviation = 0;
                for (int j = i; j > i - period; j--)
                {
                    meanDeviation += Math.Abs(typicalPrice[j] - sma);
                }
                double cciValue = (typicalPrice[i] - sma) / (0.015 * meanDeviation / period);
                cci[i] = cciValue;
            }
            return cci;
        }

        public class Supertrend
        {
            public static (double[], bool) Calculate(double[] high, double[] low, double[] close, int period, double multiplier)
            {
                int length = close.Length;
                double[] trendUp = new double[length];
                double[] trendDown = new double[length];
                double[] atr = ATR(high, low, close, period);

                trendUp[0] = (high[0] + low[0]) / 2;
                trendDown[0] = (high[0] + low[0]) / 2;

                for (int i = 1; i < length; i++)
                {
                    double basicUp = (high[i] + low[i]) / 2 + multiplier * atr[i];
                    double basicDown = (high[i] + low[i]) / 2 - multiplier * atr[i];

                    trendUp[i] = (basicUp < trendUp[i - 1] || close[i - 1] > trendUp[i - 1]) ? basicUp : trendUp[i - 1];
                    trendDown[i] = (basicDown > trendDown[i - 1] || close[i - 1] < trendDown[i - 1]) ? basicDown : trendDown[i - 1];
                }

                double[] supertrend = new double[length];
                bool uptrend = true;

                for (int i = 0; i < length; i++)
                {
                    if (i == 0)
                    {
                        supertrend[i] = (uptrend) ? trendUp[i] : trendDown[i];
                    }
                    else
                    {
                        double prevClose = close[i - 1];

                        if (uptrend)
                        {
                            if (prevClose > trendUp[i - 1])
                            {
                                uptrend = false;
                                supertrend[i] = trendDown[i];

                            }
                            else
                            {
                                supertrend[i] = trendUp[i];

                            }
                        }
                        else
                        {
                            if (prevClose < trendDown[i - 1])
                            {
                                uptrend = true;
                                supertrend[i] = trendUp[i];

                            }
                            else
                            {
                                supertrend[i] = trendDown[i];

                            }
                        }
                    }
                }

                return (supertrend, uptrend);
            }


        }
    
        public static double[] SMA(double[] input, int period)
        {
            int length = input.Length;
            double[] output = new double[length];

            for (int i = period; i < length; i++)
            {
                double sum = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += input[j];
                }
                output[i] = sum / period;
            }

            return output;
        }

        public static double[] ATR(double[] high, double[] low, double[] close, int period)
        {
            double[] atrValues = new double[close.Length];
            double sum = 0;

            for (int i = 0; i < period; i++)
            {
                sum += Math.Max(high[i] - low[i], Math.Max(Math.Abs(high[i] - close[i]), Math.Abs(low[i] - close[i])));
                atrValues[i] = 0;
            }

            double atr = sum / period;

            for (int i = period; i < close.Length; i++)
            {
                double tr = Math.Max(high[i] - low[i], Math.Max(Math.Abs(high[i] - close[i - 1]), Math.Abs(low[i] - close[i - 1])));
                atr = ((period - 1) * atr + tr) / period;
                atrValues[i] = atr;
            }

            return atrValues;
        }


        public static (double[], double[], double[]) DonchianChannel(double[] data, int period, out double[] upper, out double[] lower, out double[] middle)
        {
            upper = new double[data.Length];
            lower = new double[data.Length];
            middle = new double[data.Length];

            for (int i = period - 1; i < data.Length; i++)
            {
                double highest = double.MinValue;
                double lowest = double.MaxValue;

                for (int j = i - period + 1; j <= i; j++)
                {
                    if (data[j] > highest)
                    {
                        highest = data[j];
                    }
                    if (data[j] < lowest)
                    {
                        lowest = data[j];
                    }
                }

                upper[i] = highest;
                lower[i] = lowest;
                middle[i] = (highest + lowest) / 2;
            }
            return (upper, lower, middle);
        }


        public static class DonchianChannels
        {
            public static (double[] upper, double[] lower, double[] middle) Calculate(double[] high, double[] low, int period)
            {
                double[] upper = new double[high.Length];
                double[] lower = new double[low.Length];
                double[] middle = new double[high.Length];

                for (int i = period; i < high.Length; i++)
                {
                    double[] range = high[(i - period + 1)..(i + 1)];
                    upper[i] = range.Max();
                    range = low[(i - period + 1)..(i + 1)];
                    lower[i] = range.Min();
                    middle[i] = (upper[i] + lower[i]) / 2;
                }

                return (upper, lower, middle);
            }
        }

        public static void BollingerBands(double[] prices, int period, double deviation, out double[] sma, out double[] upperBand, out double[] lowerBand)
        {
            sma = new double[prices.Length - period + 1];
            upperBand = new double[prices.Length - period + 1];
            lowerBand = new double[prices.Length - period + 1];

            for (int i = period - 1; i < prices.Length; i++)
            {
                double sum = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    sum += prices[j];
                }
                double movingAverage = sum / period;
                sma[i - period + 1] = movingAverage;

                double sumOfSquares = 0;
                for (int j = i - period + 1; j <= i; j++)
                {
                    double difference = prices[j] - movingAverage;
                    sumOfSquares += difference * difference;
                }
                double standardDeviation = Math.Sqrt(sumOfSquares / period);

                double upper = movingAverage + deviation * standardDeviation;
                double lower = movingAverage - deviation * standardDeviation;
                upperBand[i - period + 1] = upper;
                lowerBand[i - period + 1] = lower;
            }
        }


        public class IchimokuCloudIndicator
        {
            private double[] highValues;
            private double[] lowValues;
            private double[] tenkanSen;
            private double[] kijunSen;
            private double[] senkouSpanA;
            private double[] senkouSpanB;

            public IchimokuCloudIndicator(double[] highValues, double[] lowValues, int tenkanSenPeriod, int kijunSenPeriod, int senkouSpanBPeriod)
            {
                this.highValues = highValues;
                this.lowValues = lowValues;

                // Calculate Tenkan-sen
                double[] tenkanSenHigh = new double[highValues.Length];
                double[] tenkanSenLow = new double[lowValues.Length];
                for (int i = 0; i < highValues.Length; i++)
                {
                    if (i >= tenkanSenPeriod - 1)
                    {
                        double highestHigh = highValues.Skip(i - tenkanSenPeriod + 1).Take(tenkanSenPeriod).Max();
                        double lowestLow = lowValues.Skip(i - tenkanSenPeriod + 1).Take(tenkanSenPeriod).Min();
                        tenkanSenHigh[i] = highestHigh;
                        tenkanSenLow[i] = lowestLow;
                    }
                }
                tenkanSen = tenkanSenHigh.Zip(tenkanSenLow, (h, l) => (h + l) / 2).ToArray();

                // Calculate Kijun-sen
                double[] kijunSenHigh = new double[highValues.Length];
                double[] kijunSenLow = new double[lowValues.Length];
                for (int i = 0; i < highValues.Length; i++)
                {
                    if (i >= kijunSenPeriod - 1)
                    {
                        double highestHigh = highValues.Skip(i - kijunSenPeriod + 1).Take(kijunSenPeriod).Max();
                        double lowestLow = lowValues.Skip(i - kijunSenPeriod + 1).Take(kijunSenPeriod).Min();
                        kijunSenHigh[i] = highestHigh;
                        kijunSenLow[i] = lowestLow;
                    }
                }
                kijunSen = kijunSenHigh.Zip(kijunSenLow, (h, l) => (h + l) / 2).ToArray();

                // Calculate Senkou Span A
                senkouSpanA = new double[highValues.Length];
                for (int i = 0; i < highValues.Length; i++)
                {
                    int senkouSpanAIndex = i;
                    if (senkouSpanAIndex >= kijunSenPeriod && senkouSpanAIndex < senkouSpanA.Length)
                    {
                        senkouSpanA[senkouSpanAIndex] = (tenkanSen[i] + kijunSen[i]) / 2;
                    }
                }

                // Calculate Senkou Span B
                double[] senkouSpanBHigh = new double[highValues.Length];
                double[] senkouSpanBLow = new double[lowValues.Length];
                for (int i = 0; i < highValues.Length; i++)
                {
                    if (i >= senkouSpanBPeriod - 1)
                    {
                        double highestHigh = highValues.Skip(i - senkouSpanBPeriod + 1).Take(senkouSpanBPeriod).Max();
                        double lowestLow = lowValues.Skip(i - senkouSpanBPeriod + 1).Take(senkouSpanBPeriod).Min();
                        senkouSpanBHigh[i] = highestHigh;
                        senkouSpanBLow[i] = lowestLow;
                    }
                }
                senkouSpanB = senkouSpanBHigh.Zip(senkouSpanBLow, (h, l) => (h + l) / 2).ToArray();
            }

            public double[] GetTenkanSen()
            {
                return tenkanSen;
            }

            public double[] GetKijunSen()
            {
                return kijunSen;
            }

            public double[] GetSenkouSpanA()
            {
                return senkouSpanA;
            }

            public double[] GetSenkouSpanB()
            {
                return senkouSpanB;
            }

            public bool IsBullish(int index)
            {
                return senkouSpanA[index] > senkouSpanB[index] && highValues[index] > senkouSpanA[index] && lowValues[index] > senkouSpanB[index];
            }

            public bool IsBearish(int index)
            {
                return senkouSpanA[index] < senkouSpanB[index] && highValues[index] < senkouSpanA[index] && lowValues[index] < senkouSpanB[index];
            }
        }


        static double[] ConvertToDouble(List<decimal> list)
        {
            double[] result = new double[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = (double)list[i];
            }
            return result;
        }

    }
}