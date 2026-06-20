using System;
using System.ComponentModel;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTests.CoreObjectsTests;

[TestClass]
public class DefaultsTesting
{
    [TestMethod]
    public void ObservablePointSetsCoordinateFromXY()
    {
        var point = new ObservablePoint(3, 5);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 3) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 5) < 0.001);
    }

    [TestMethod]
    public void ObservablePointCoordinateIsEmptyWhenXIsNull()
    {
        var point = new ObservablePoint(null, 5);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ObservablePointCoordinateIsEmptyWhenYIsNull()
    {
        var point = new ObservablePoint(3, null);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ObservablePointUpdatesCoordinateOnPropertyChange()
    {
        var point = new ObservablePoint(1, 2);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 1) < 0.001);

        point.X = 10;
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 10) < 0.001);

        point.Y = 20;
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 20) < 0.001);
    }

    [TestMethod]
    public void ObservablePointRaisesPropertyChanged()
    {
        var point = new ObservablePoint(1, 2);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.X = 10;
        point.Y = 20;

        Assert.IsTrue(changedProperties.Contains("X"));
        Assert.IsTrue(changedProperties.Contains("Y"));
    }

    [TestMethod]
    public void ObservablePointDefaultConstructorSetsEmpty()
    {
        var point = new ObservablePoint();
        Assert.IsTrue(point.Coordinate.IsEmpty);
        Assert.IsTrue(point.X is null);
        Assert.IsTrue(point.Y is null);
    }

    [TestMethod]
    public void ObservableValueSetsCoordinateFromValue()
    {
        var value = new ObservableValue(42);

        Assert.IsTrue(!value.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(value.Coordinate.PrimaryValue - 42) < 0.001);
    }

    [TestMethod]
    public void ObservableValueCoordinateIsEmptyWhenNull()
    {
        var value = new ObservableValue(null);
        Assert.IsTrue(value.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ObservableValueUpdatesCoordinateOnPropertyChange()
    {
        var value = new ObservableValue(10);
        Assert.IsTrue(Math.Abs(value.Coordinate.PrimaryValue - 10) < 0.001);

        value.Value = 99;
        Assert.IsTrue(Math.Abs(value.Coordinate.PrimaryValue - 99) < 0.001);
    }

    [TestMethod]
    public void ObservableValueRaisesPropertyChanged()
    {
        var value = new ObservableValue(1);
        var raised = false;
        value.PropertyChanged += (s, e) => raised = true;

        value.Value = 2;
        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ObservableValueBecomesEmptyWhenSetToNull()
    {
        var value = new ObservableValue(10);
        Assert.IsTrue(!value.Coordinate.IsEmpty);

        value.Value = null;
        Assert.IsTrue(value.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void DateTimePointSetsCoordinateFromDateAndValue()
    {
        var dt = new DateTime(2023, 6, 15);
        var point = new DateTimePoint(dt, 100);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - dt.Ticks) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 100) < 0.001);
    }

    [TestMethod]
    public void DateTimePointCoordinateIsEmptyWhenValueIsNull()
    {
        var point = new DateTimePoint(DateTime.Now, null);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void DateTimePointUpdatesCoordinateOnPropertyChange()
    {
        var dt1 = new DateTime(2023, 1, 1);
        var dt2 = new DateTime(2024, 1, 1);
        var point = new DateTimePoint(dt1, 50);

        point.DateTime = dt2;
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - dt2.Ticks) < 0.001);

        point.Value = 200;
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 200) < 0.001);
    }

    [TestMethod]
    public void DateTimePointRaisesPropertyChanged()
    {
        var point = new DateTimePoint(DateTime.Now, 10);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.DateTime = DateTime.Now.AddDays(1);
        point.Value = 20;

        Assert.IsTrue(changedProperties.Contains("DateTime"));
        Assert.IsTrue(changedProperties.Contains("Value"));
    }

    [TestMethod]
    public void TimeSpanPointSetsCoordinateFromTimeSpanAndValue()
    {
        var ts = TimeSpan.FromHours(3);
        var point = new TimeSpanPoint(ts, 55);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - ts.Ticks) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 55) < 0.001);
    }

    [TestMethod]
    public void TimeSpanPointCoordinateIsEmptyWhenValueIsNull()
    {
        var point = new TimeSpanPoint(TimeSpan.FromMinutes(10), null);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void TimeSpanPointRaisesPropertyChanged()
    {
        var point = new TimeSpanPoint(TimeSpan.Zero, 10);
        var raised = false;
        point.PropertyChanged += (s, e) => raised = true;

        point.TimeSpan = TimeSpan.FromSeconds(30);
        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ObservablePolarPointSetsCoordinate()
    {
        var point = new ObservablePolarPoint(45, 10);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 45) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 10) < 0.001);
    }

    [TestMethod]
    public void ObservablePolarPointCoordinateIsEmptyWhenAngleIsNull()
    {
        var point = new ObservablePolarPoint(null, 10);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ObservablePolarPointCoordinateIsEmptyWhenRadiusIsNull()
    {
        var point = new ObservablePolarPoint(45, null);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ObservablePolarPointUpdatesCoordinate()
    {
        var point = new ObservablePolarPoint(0, 0);
        point.Angle = 90;
        point.Radius = 5;

        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 90) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 5) < 0.001);
    }

    [TestMethod]
    public void ObservablePolarPointRaisesPropertyChanged()
    {
        var point = new ObservablePolarPoint(0, 0);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.Angle = 180;
        point.Radius = 15;

        Assert.IsTrue(changedProperties.Contains("Angle"));
        Assert.IsTrue(changedProperties.Contains("Radius"));
    }

    [TestMethod]
    public void WeightedPointSetsCoordinate()
    {
        var point = new WeightedPoint(1, 2, 3);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - 1) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 2) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.TertiaryValue - 3) < 0.001);
    }

    [TestMethod]
    public void WeightedPointCoordinateIsEmptyWhenXIsNull()
    {
        var point = new WeightedPoint(null, 2, 3);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void WeightedPointCoordinateIsEmptyWhenYIsNull()
    {
        var point = new WeightedPoint(1, null, 3);
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void WeightedPointWeightDefaultsToZeroWhenNull()
    {
        var point = new WeightedPoint(1, 2, null);
        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.TertiaryValue - 0) < 0.001);
    }

    [TestMethod]
    public void WeightedPointRaisesPropertyChanged()
    {
        var point = new WeightedPoint(0, 0, 0);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.X = 5;
        point.Y = 10;
        point.Weight = 15;

        Assert.IsTrue(changedProperties.Contains("X"));
        Assert.IsTrue(changedProperties.Contains("Y"));
        Assert.IsTrue(changedProperties.Contains("Weight"));
    }

    [TestMethod]
    public void FinancialPointSetsCoordinate()
    {
        var date = new DateTime(2023, 1, 15);
        var point = new FinancialPoint(date, 150, 100, 140, 90);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - date.Ticks) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 150) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.TertiaryValue - 100) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.QuaternaryValue - 140) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.QuinaryValue - 90) < 0.001);
    }

    [TestMethod]
    public void FinancialPointUpdatesCoordinateOnPropertyChange()
    {
        var point = new FinancialPoint(DateTime.Now, 150, 100, 140, 90);

        point.High = 200;
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 200) < 0.001);

        point.Open = 110;
        Assert.IsTrue(Math.Abs(point.Coordinate.TertiaryValue - 110) < 0.001);

        point.Close = 180;
        Assert.IsTrue(Math.Abs(point.Coordinate.QuaternaryValue - 180) < 0.001);

        point.Low = 80;
        Assert.IsTrue(Math.Abs(point.Coordinate.QuinaryValue - 80) < 0.001);
    }

    [TestMethod]
    public void FinancialPointRaisesPropertyChanged()
    {
        var point = new FinancialPoint(DateTime.Now, 100, 90, 95, 85);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.High = 110;
        point.Open = 100;
        point.Close = 105;
        point.Low = 80;
        point.Date = DateTime.Now.AddDays(1);

        Assert.IsTrue(changedProperties.Contains("High"));
        Assert.IsTrue(changedProperties.Contains("Open"));
        Assert.IsTrue(changedProperties.Contains("Close"));
        Assert.IsTrue(changedProperties.Contains("Low"));
        Assert.IsTrue(changedProperties.Contains("Date"));
    }

    [TestMethod]
    public void FinancialPointISetsCoordinate()
    {
        var point = new FinancialPointI(150, 100, 140, 90);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 150) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.TertiaryValue - 100) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.QuaternaryValue - 140) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.QuinaryValue - 90) < 0.001);
    }

    [TestMethod]
    public void FinancialPointIRaisesPropertyChanged()
    {
        var point = new FinancialPointI(100, 90, 95, 85);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.High = 110;
        point.Open = 100;
        point.Close = 105;
        point.Low = 80;

        Assert.IsTrue(changedProperties.Contains("High"));
        Assert.IsTrue(changedProperties.Contains("Open"));
        Assert.IsTrue(changedProperties.Contains("Close"));
        Assert.IsTrue(changedProperties.Contains("Low"));
    }

    [TestMethod]
    public void BoxValueSetsCoordinate()
    {
        var box = new BoxValue(100, 75, 25, 0, 50);

        Assert.IsTrue(!box.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(box.Coordinate.PrimaryValue - 100) < 0.001);
        Assert.IsTrue(Math.Abs(box.Coordinate.TertiaryValue - 75) < 0.001);
        Assert.IsTrue(Math.Abs(box.Coordinate.QuaternaryValue - 25) < 0.001);
        Assert.IsTrue(Math.Abs(box.Coordinate.QuinaryValue - 0) < 0.001);
        Assert.IsTrue(Math.Abs(box.Coordinate.SenaryValue - 50) < 0.001);
    }

    [TestMethod]
    public void BoxValueRaisesPropertyChanged()
    {
        var box = new BoxValue(100, 75, 25, 0, 50);
        var changedProperties = new System.Collections.Generic.List<string>();
        box.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        box.Max = 110;
        box.ThirdQuartile = 80;
        box.FirtQuartile = 30;
        box.Min = 5;
        box.Median = 55;

        Assert.IsTrue(changedProperties.Contains("Max"));
        Assert.IsTrue(changedProperties.Contains("ThirdQuartile"));
        Assert.IsTrue(changedProperties.Contains("FirtQuartile"));
        Assert.IsTrue(changedProperties.Contains("Min"));
        Assert.IsTrue(changedProperties.Contains("Median"));
    }

    [TestMethod]
    public void ErrorPointSetsCoordinate()
    {
        var point = new ErrorPoint(10, 20, 1, 2, 3, 4);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xi - 1) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xj - 2) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yi - 3) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yj - 4) < 0.001);
    }

    [TestMethod]
    public void ErrorPointSymmetricConstructor()
    {
        var point = new ErrorPoint(10, 20, 5, 3);

        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xi - 5) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xj - 5) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yi - 3) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yj - 3) < 0.001);
    }

    [TestMethod]
    public void ErrorPointCoordinateIsEmptyWhenXIsNull()
    {
        var point = new ErrorPoint(10, 20, 1, 2, 3, 4);
        point.X = null;
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ErrorPointCoordinateIsEmptyWhenYIsNull()
    {
        var point = new ErrorPoint(10, 20, 1, 2, 3, 4);
        point.Y = null;
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ErrorPointRaisesPropertyChanged()
    {
        var point = new ErrorPoint(10, 20, 1, 2, 3, 4);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.X = 15;
        point.Y = 25;
        point.XErrorI = 2;
        point.XErrorJ = 3;
        point.YErrorI = 4;
        point.YErrorJ = 5;

        Assert.IsTrue(changedProperties.Contains("X"));
        Assert.IsTrue(changedProperties.Contains("Y"));
        Assert.IsTrue(changedProperties.Contains("XErrorI"));
        Assert.IsTrue(changedProperties.Contains("XErrorJ"));
        Assert.IsTrue(changedProperties.Contains("YErrorI"));
        Assert.IsTrue(changedProperties.Contains("YErrorJ"));
    }

    [TestMethod]
    public void ErrorValueSetsCoordinate()
    {
        var errorVal = new ErrorValue(50, 5, 10);

        Assert.IsTrue(!errorVal.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(errorVal.Coordinate.PrimaryValue - 50) < 0.001);
        Assert.IsTrue(Math.Abs(errorVal.Coordinate.PointError.Yi - 5) < 0.001);
        Assert.IsTrue(Math.Abs(errorVal.Coordinate.PointError.Yj - 10) < 0.001);
    }

    [TestMethod]
    public void ErrorValueSymmetricConstructor()
    {
        var errorVal = new ErrorValue(50, 7);

        Assert.IsTrue(Math.Abs(errorVal.Coordinate.PointError.Yi - 7) < 0.001);
        Assert.IsTrue(Math.Abs(errorVal.Coordinate.PointError.Yj - 7) < 0.001);
    }

    [TestMethod]
    public void ErrorValueCoordinateIsEmptyWhenNull()
    {
        var errorVal = new ErrorValue(50, 5, 10);
        errorVal.Y = null;
        Assert.IsTrue(errorVal.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ErrorValueRaisesPropertyChanged()
    {
        var errorVal = new ErrorValue(50, 5, 10);
        var raised = false;
        errorVal.PropertyChanged += (s, e) => raised = true;

        errorVal.Y = 60;
        Assert.IsTrue(raised);
    }

    [TestMethod]
    public void ErrorDateTimePointSetsCoordinate()
    {
        var dt = new DateTime(2023, 6, 1);
        var point = new ErrorDateTimePoint(dt, 100, 1, 2, 3, 4);

        Assert.IsTrue(!point.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(point.Coordinate.PrimaryValue - 100) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.SecondaryValue - dt.Ticks) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xi - 1) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xj - 2) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yi - 3) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yj - 4) < 0.001);
    }

    [TestMethod]
    public void ErrorDateTimePointSymmetricConstructor()
    {
        var dt = new DateTime(2023, 6, 1);
        var point = new ErrorDateTimePoint(dt, 100, 5, 3);

        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xi - 5) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Xj - 5) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yi - 3) < 0.001);
        Assert.IsTrue(Math.Abs(point.Coordinate.PointError.Yj - 3) < 0.001);
    }

    [TestMethod]
    public void ErrorDateTimePointCoordinateIsEmptyWhenYIsNull()
    {
        var point = new ErrorDateTimePoint(DateTime.Now, 100, 1, 2, 3, 4);
        point.Y = null;
        Assert.IsTrue(point.Coordinate.IsEmpty);
    }

    [TestMethod]
    public void ErrorDateTimePointRaisesPropertyChanged()
    {
        var point = new ErrorDateTimePoint(DateTime.Now, 100, 1, 2, 3, 4);
        var changedProperties = new System.Collections.Generic.List<string>();
        point.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

        point.DateTime = DateTime.Now.AddDays(1);
        point.Y = 200;
        point.XErrorI = 10;
        point.XErrorJ = 20;
        point.YErrorI = 30;
        point.YErrorJ = 40;

        Assert.IsTrue(changedProperties.Contains("DateTime"));
        Assert.IsTrue(changedProperties.Contains("Y"));
        Assert.IsTrue(changedProperties.Contains("XErrorI"));
        Assert.IsTrue(changedProperties.Contains("XErrorJ"));
        Assert.IsTrue(changedProperties.Contains("YErrorI"));
        Assert.IsTrue(changedProperties.Contains("YErrorJ"));
    }

    [TestMethod]
    public void MappedChartEntityHasEmptyCoordinateByDefault()
    {
        var entity = new MappedChartEntity();
        Assert.IsTrue(entity.Coordinate.IsEmpty);
        Assert.IsTrue(entity.MetaData is null);
    }

    [TestMethod]
    public void MappedChartEntityCanSetCoordinate()
    {
        var entity = new MappedChartEntity();
        entity.Coordinate = new Coordinate(10, 20);

        Assert.IsTrue(!entity.Coordinate.IsEmpty);
        Assert.IsTrue(Math.Abs(entity.Coordinate.SecondaryValue - 10) < 0.001);
        Assert.IsTrue(Math.Abs(entity.Coordinate.PrimaryValue - 20) < 0.001);
    }
}
