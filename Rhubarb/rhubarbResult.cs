namespace Mimic3Sharp.Rhubarb;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class rhubarbResult
{

    private rhubarbResultMetadata metadataField;

    private List<rhubarbResultMouthCue> mouthCuesField;

    /// <remarks/>
    public rhubarbResultMetadata metadata
    {
        get
        {
            return this.metadataField;
        }
        set
        {
            this.metadataField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("mouthCue", IsNullable = false)]
    public List<rhubarbResultMouthCue> mouthCues
    {
        get
        {
            return this.mouthCuesField;
        }
        set
        {
            this.mouthCuesField = value;
        }
    }
}

