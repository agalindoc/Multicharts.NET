// 
// Copyright (C) 2021 Alejandro Galindo Cháirez
// 
using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;

namespace PowerLanguage.Indicator
{
	public class ZiSchaffTrendCycle : IndicatorObject
    {

		private IPlotObject plot1;
		private IPlotObject plot2;
		private IPlotObject plot3;
		private IPlotObject plot4;
		private IPlotObject plot5;

		public ZiSchaffTrendCycle(object _ctx) : base(_ctx)
		{
			MACDFast = 12;
			MACDSlow = 26;
			MACDSmooth = 9;
			Period = 14;
			Factor = 0.5;
		}

		[Input]
		public int MACDFast { get; set; }
		[Input]
		public int MACDSlow { get; set; }
		[Input]
		public int MACDSmooth { get; set; }
		[Input]
		public int Period { get; set; }
		[Input]
		public double Factor { get; set; }

		private VariableSeries<Double> sFrac1;
		private VariableSeries<Double> sFrac2;
		private VariableSeries<Double> sPF;
		private VariableSeries<Double> sPFF;

		private Function.MACD iMACD;
		private VariableSeries<Double> sMACD;
		private VariableSeries<Double> sMIN;
		private VariableSeries<Double> sMAX;
		private VariableSeries<Double> pMIN;
		private VariableSeries<Double> pMAX;

		protected override void Create()
		{
			// create variable objects, function objects, plot objects etc.
			plot1 = AddPlot(new PlotAttributes("Cycle", EPlotShapes.Line, Color.Orange));
			plot2 = AddPlot(new PlotAttributes("banda100", EPlotShapes.Line, Color.LightBlue));
			plot3 = AddPlot(new PlotAttributes("bandaUpper", EPlotShapes.Line, Color.LightBlue));
			plot4 = AddPlot(new PlotAttributes("bandaLower", EPlotShapes.Line, Color.LightBlue));
			plot5 = AddPlot(new PlotAttributes("banda0", EPlotShapes.Line, Color.LightBlue));

			sFrac1 = new VariableSeries<Double>(this);
			sFrac2 = new VariableSeries<Double>(this);
			sPF = new VariableSeries<Double>(this);
			sPFF = new VariableSeries<Double>(this);
			sMIN = new VariableSeries<Double>(this);
			sMAX = new VariableSeries<Double>(this);
			pMIN = new VariableSeries<Double>(this);
			pMAX = new VariableSeries<Double>(this);

			iMACD = new Function.MACD(this);
			sMACD = new VariableSeries<Double>(this);

		}
		protected override void StartCalc()
		{
			iMACD.Price = Bars.Close;
			iMACD.FastLength= MACDFast;
			iMACD.SlowLength = MACDSlow;
		}


		protected override void CalcBar()
		{
			if (Bars.CurrentBar < MACDSlow)
				return;

			sMACD.Value = iMACD[0];

			sMIN.Value = MinValMACD(Period);			
			sMAX.Value = MaxValMACD(Period);
			sFrac1.Value = sMAX[0] - sMIN[0] > 0 ? 100 * (sMACD.Value - sMIN[0]) / (sMAX[0] - sMIN[0]) : sFrac1[1];
			sPF.Value = Bars.CurrentBar <= 1 ? sFrac1[0] : sPF[1] + (Factor * (sFrac1[0] - sPF[1]));
			pMIN.Value = MinVal(sPF, Period);
			pMAX.Value = MaxVal(sPF, Period);			
			sFrac2.Value = pMAX[0] - pMIN[0] > 0 ? 100 * (sPF[0] - pMIN[0]) / (pMAX[0] - pMIN[0]) : sFrac2[1];
			//sPFF.Value = Bars.CurrentBar <= MACDSlow+1 ? sFrac2[0] : sPFF[1] + (Factor * (sFrac2[0] - sPFF[1]));
			sPFF.Value = Bars.CurrentBar <= MACDSlow ? sFrac2[0] : sPFF[1] + (Factor * (sFrac2[0] - sPFF[1]));

			plot1.Set(sPFF[0]);
			plot2.Set(100);
			plot3.Set(70);
			plot4.Set(30);
			plot5.Set(0);

		}

		private double MinValMACD(int length)
		{
			double min = sMACD[0];
			for (int i = 0; i < length; ++i)
			{
				if (sMACD[i] < min)
					min = sMACD[i];
			}
			return min;
		}

		private double MaxValMACD(int length)
		{
			double max = sMACD[0];
			for (int i = 0; i < length; ++i)
			{
				if (sMACD[i] > max)
					max = sMACD[i];
			}
			return max;
		}

		private double MinVal(VariableSeries<Double> data, int length)
		{
			double min = data[0];
			for (int i = 0; i < length; ++i)
			{
				if (data[i] < min)
					min = data[i];
			}
			return min;
		}

		private double MaxVal(VariableSeries<Double> data, int length)
		{
			double max = data[0];
			for (int i = 0; i < length; ++i)
			{
				if (data[i] > max)
					max = data[i];
			}
			return max;
		}
	}
}
