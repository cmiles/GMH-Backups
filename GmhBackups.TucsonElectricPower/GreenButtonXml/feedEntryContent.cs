namespace Pw02.GreenButtonXml;

/// <remarks/>
[Serializable()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
public partial class feedEntryContent
{

    private LocalTimeParameters localTimeParametersField;

    private ElectricPowerUsageSummary electricPowerUsageSummaryField;

    private IntervalBlock[] intervalBlockField;

    private ReadingType readingTypeField;

    private object meterReadingField;

    private UsagePoint usagePointField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://naesb.org/espi")]
    public LocalTimeParameters LocalTimeParameters
    {
        get
        {
            return this.localTimeParametersField;
        }
        set
        {
            this.localTimeParametersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://naesb.org/espi")]
    public ElectricPowerUsageSummary ElectricPowerUsageSummary
    {
        get
        {
            return this.electricPowerUsageSummaryField;
        }
        set
        {
            this.electricPowerUsageSummaryField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("IntervalBlock", Namespace = "http://naesb.org/espi")]
    public IntervalBlock[] IntervalBlock
    {
        get
        {
            return this.intervalBlockField;
        }
        set
        {
            this.intervalBlockField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://naesb.org/espi")]
    public ReadingType ReadingType
    {
        get
        {
            return this.readingTypeField;
        }
        set
        {
            this.readingTypeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://naesb.org/espi")]
    public object MeterReading
    {
        get
        {
            return this.meterReadingField;
        }
        set
        {
            this.meterReadingField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://naesb.org/espi")]
    public UsagePoint UsagePoint
    {
        get
        {
            return this.usagePointField;
        }
        set
        {
            this.usagePointField = value;
        }
    }
}