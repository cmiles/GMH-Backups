namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://naesb.org/espi")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://naesb.org/espi", IsNullable = false)]
public partial class ElectricPowerUsageSummary
{

    private ElectricPowerUsageSummaryBillingPeriod billingPeriodField;

    private ElectricPowerUsageSummaryOverallConsumptionLastPeriod overallConsumptionLastPeriodField;

    private ElectricPowerUsageSummaryCurrentBillingPeriodOverAllConsumption currentBillingPeriodOverAllConsumptionField;

    private byte qualityOfReadingField;

    private uint statusTimeStampField;

    /// <remarks/>
    public ElectricPowerUsageSummaryBillingPeriod billingPeriod
    {
        get
        {
            return this.billingPeriodField;
        }
        set
        {
            this.billingPeriodField = value;
        }
    }

    /// <remarks/>
    public ElectricPowerUsageSummaryOverallConsumptionLastPeriod overallConsumptionLastPeriod
    {
        get
        {
            return this.overallConsumptionLastPeriodField;
        }
        set
        {
            this.overallConsumptionLastPeriodField = value;
        }
    }

    /// <remarks/>
    public ElectricPowerUsageSummaryCurrentBillingPeriodOverAllConsumption currentBillingPeriodOverAllConsumption
    {
        get
        {
            return this.currentBillingPeriodOverAllConsumptionField;
        }
        set
        {
            this.currentBillingPeriodOverAllConsumptionField = value;
        }
    }

    /// <remarks/>
    public byte qualityOfReading
    {
        get
        {
            return this.qualityOfReadingField;
        }
        set
        {
            this.qualityOfReadingField = value;
        }
    }

    /// <remarks/>
    public uint statusTimeStamp
    {
        get
        {
            return this.statusTimeStampField;
        }
        set
        {
            this.statusTimeStampField = value;
        }
    }
}