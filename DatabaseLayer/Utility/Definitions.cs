namespace DatabaseLayer.Utility;

using DatabaseModel.Models;

// Tables (entity types) do not have an Unique Identifier. This class defines a Unique Identifier for the entity types.
public static class Definitions
{
	public static readonly Guid Compartment        = new("0db49924-66ba-44d8-8fd2-8c68531d7240");
	public static readonly Guid Parameter          = new("eab90c34-6bf8-487c-8a86-72b54c4963a8");
	public static readonly Guid AnalysisPackage    = new("d87cdf80-e8c3-4c9f-8631-bb3e39ca462f");
	public static readonly Guid AttributeType      = new("9339995d-743f-45bc-b70f-246cc2b9ca00");
	public static readonly Guid Literature         = new("7903eddf-f885-43bb-9feb-54c8a425341f");
	public static readonly Guid MethodCategory     = new("e25cc886-06ab-4d8e-9f23-5e289a250da4");
	public static readonly Guid Quantity           = new("fd93e7d9-d724-4cc3-9713-871e543a794e");
	public static readonly Guid Unit               = new("af7e4606-b5ab-4199-979d-504a11bc8111");
	public static readonly Guid Project            = new("918e4a34-2330-42af-87a7-c7902315d86c");
	public static readonly Guid Note               = new("2fec5d89-65ae-40b9-9ee6-d77ea425de8d");
	public static readonly Guid Method             = new("bd911e45-6025-4bb2-9f11-3beb2a25f628");
	public static readonly Guid MeasurementObject  = new("8e5f54cc-cf59-43eb-9516-3368099ed877");
	public static readonly Guid MonitoringNetwork  = new("caee443b-2b55-483f-9202-2a087f3b6cf7");
	public static readonly Guid Relation           = new("3b5f5b5c-0b5f-4b5f-9b5f-5b5f5b5f5b5f");
	public static readonly Guid Organisation       = new("899d5a20-25bd-4b41-bc4c-a077dc2bd47d");
	public static readonly Guid MeasurementPackage = new("d4a3e44d-2244-44ef-97fb-fb28bd6d2360");
	public static readonly Guid TaxonType          = new("86672dd9-a442-4655-a579-ce3e19dacc9f");
	public static readonly Guid TaxonGroup         = new("2d1cf6f1-ab38-49a4-a2e9-b9fcf2061ea6");
	public static readonly Guid Purpose            = new("002120b6-6abc-417f-9c8e-bf3fb7bc680b");
	public static readonly Guid Ecotope            = new("f7793783-e5cd-4123-95c0-f3bce217de4e");
	public static readonly Guid WaterType          = new("35a17f79-c84f-4ab5-8bba-e6f75a5da5c4");
	public static readonly Guid Analyst            = new("064cf1dc-0a4b-4f51-8b90-dd6fb5646c6b");
	public static readonly Guid Sampler            = new("b0745f21-4b16-4e2d-aaf2-563454ef5ea8");
	public static readonly Guid Checker            = new("650791f2-2568-4a15-b188-31b2215dc851");

	// Map the definitions to their codes, names and descriptions.
	public static readonly Dictionary<Guid, Reference> References = new()
	{
		{ Compartment, new Reference { Id        = Compartment, Code        = "Compartment", Description        = "Compartment" } },
		{ Parameter, new Reference { Id          = Parameter, Code          = "Parameter", Description          = "Parameter" } },
		{ AnalysisPackage, new Reference { Id    = AnalysisPackage, Code    = "AnalysisPackage", Description    = "AnalysisPackage" } },
		{ WaterType, new Reference { Id          = WaterType, Code          = "Watertype", Description          = "Watertype" } },
		{ AttributeType, new Reference { Id      = AttributeType, Code      = "AttributeType", Description      = "AttributeType" } },
		{ Literature, new Reference { Id         = Literature, Code         = "Literature", Description         = "Literature" } },
		{ MethodCategory, new Reference { Id     = MethodCategory, Code     = "MethodCategory", Description     = "MethodCategory" } },
		{ Quantity, new Reference { Id           = Quantity, Code           = "Quantity", Description           = "Quantity" } },
		{ Unit, new Reference { Id               = Unit, Code               = "Unit", Description               = "Unit" } },
		{ Project, new Reference { Id            = Project, Code            = "Project", Description            = "Project" } },
		{ Note, new Reference { Id               = Note, Code               = "Note", Description               = "Note" } },
		{ Method, new Reference { Id             = Method, Code             = "Method", Description             = "Method" } },
		{ MeasurementObject, new Reference { Id  = MeasurementObject, Code  = "MeasurementObject", Description  = "MeasurementObject" } },
		{ MonitoringNetwork, new Reference { Id  = MonitoringNetwork, Code  = "MonitoringNetwork", Description  = "MonitoringNetwork" } },
		{ Relation, new Reference { Id           = Relation, Code           = "Relation", Description           = "Relation" } },
		{ Organisation, new Reference { Id       = Organisation, Code       = "Organisation", Description       = "Organisation" } },
		{ MeasurementPackage, new Reference { Id = MeasurementPackage, Code = "MeasurementPackage", Description = "MeasurementPackage" } },
		{ Sampler, new Reference { Id            = Sampler, Code            = "Sampler", Description            = "Sampling organisation" } },
		{ Analyst, new Reference { Id            = Analyst, Code            = "Analyst", Description            = "Analyzing organisation" } },
		{ Checker, new Reference { Id            = Checker, Code            = "Checker", Description            = "Validating organisation" } },
		{ Ecotope, new Reference { Id            = Checker, Code            = "Ecotope", Description            = "Ecotope" } }
	};

	public static readonly Dictionary<string, string> ParameterTranslators = new()
	{
		{ "OM", "Recordingmethod" },
		{ "MM", "Samplingmethod" },
		{ "AM", "Analysismethod" },
		{ "taxontype", "Taxontype"},
		{ "taxongroup", "Taxongroup" },
		{ "statistics", "Statistics" },
		{ "HT", "Habitat" },
		{ "ID", "Individuals"},
		{ "KD", "Graindiameter" },
		{ "KG", "Grainsizefraction" },
		{ "KO", "Qualityassessment" },
		{ "LK", "Lengthclass" },
		{ "LS", "Lifestage" },
		{ "LV", "Lifeform" },
		{ "MB.CM", "Widthclasscm"},
		{ "MB.MM", "Widthclassmm"},
		{ "ML.CM", "Lengthclasscm"},
		{ "ML.MM", "Lengthclassmm"},
		{ "MP", "Measurementposition" },
		{ "SD", "Sediment"},
		{ "ST", "Statistics"},
		{ "VV", "Appearance" },
		{ "WT", "Valuationtechnique" },
	};
}