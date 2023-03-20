namespace Mimic3Sharp.Rhubarb;

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class rhubarbResultMetadata
{

    private string soundFileField;

    private decimal durationField;

    /// <remarks/>
    public string soundFile
    {
        get
        {
            return this.soundFileField;
        }
        set
        {
            this.soundFileField = value;
        }
    }

    /// <remarks/>
    public decimal duration
    {
        get
        {
            return this.durationField;
        }
        set
        {
            this.durationField = value;
        }
    }
}

